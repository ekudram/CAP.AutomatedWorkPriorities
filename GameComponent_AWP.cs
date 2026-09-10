using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public class GameComponent_AWP : GameComponent
    {
        public bool enabled = true;
        public bool dailyRefresh = true;
        public bool forceEmergencyPriorities = true;
        public int lastCheckDay = -1;
        public int lastRefreshTick = -1;
        public int lastRefreshChanged;
        public string lastAppliedPresetId = "normal";
        public string lastRefreshLog = "";
        public List<string> excludedPawnIds = new List<string>();
        public List<string> pinnedKeys = new List<string>();
        public Dictionary<string, WorkTypeConfig> jobSettings = new Dictionary<string, WorkTypeConfig>();
        public List<string> jobExcludeKeys = new List<string>();
        public Dictionary<string, int> tierOverrides = new Dictionary<string, int>();
        public List<AssignmentRule> rules = new List<AssignmentRule>();

        public GameComponent_AWP(Game game)
        {
        }

        public static GameComponent_AWP Get
        {
            get
            {
                if (Current.Game == null)
                    return null;
                return Current.Game.GetComponent<GameComponent_AWP>();
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enabled, "enabled", true);
            Scribe_Values.Look(ref dailyRefresh, "dailyRefresh", true);
            Scribe_Values.Look(ref forceEmergencyPriorities, "forceEmergencyPriorities", true);
            Scribe_Values.Look(ref lastCheckDay, "lastCheckDay", -1);
            Scribe_Values.Look(ref lastRefreshTick, "lastRefreshTick", -1);
            Scribe_Values.Look(ref lastRefreshChanged, "lastRefreshChanged");
            Scribe_Values.Look(ref lastAppliedPresetId, "lastAppliedPresetId", "normal");
            Scribe_Values.Look(ref lastRefreshLog, "lastRefreshLog", "");
            Scribe_Collections.Look(ref excludedPawnIds, "excludedPawnIds", LookMode.Value);
            Scribe_Collections.Look(ref pinnedKeys, "pinnedKeys", LookMode.Value);
            Scribe_Collections.Look(ref jobSettings, "jobSettings", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref jobExcludeKeys, "jobExcludeKeys", LookMode.Value);
            Scribe_Collections.Look(ref tierOverrides, "tierOverrides", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref rules, "rules", LookMode.Deep);
            if (excludedPawnIds == null) excludedPawnIds = new List<string>();
            if (pinnedKeys == null) pinnedKeys = new List<string>();
            if (jobSettings == null) jobSettings = new Dictionary<string, WorkTypeConfig>();
            if (jobExcludeKeys == null) jobExcludeKeys = new List<string>();
            if (tierOverrides == null) tierOverrides = new Dictionary<string, int>();
            if (rules == null) rules = new List<AssignmentRule>();
        }

        public override void FinalizeInit()
        {
            List<AssignmentRule> defaults = DefaultRules.Create();
            if (rules.Count == 0)
            {
                rules.AddRange(defaults);
                return;
            }
            for (int i = 0; i < defaults.Count; i++)
            {
                AssignmentRule d = defaults[i];
                bool found = false;
                for (int j = 0; j < rules.Count; j++)
                {
                    if (rules[j].id == d.id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    rules.Add(d);
            }
            RebuildJobList();
        }

        public void RebuildJobList()
        {
            if (jobSettings == null)
                jobSettings = new Dictionary<string, WorkTypeConfig>();
            var next = new Dictionary<string, WorkTypeConfig>();
            List<WorkTypeDef> all = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            for (int i = 0; i < all.Count; i++)
            {
                WorkTypeDef def = all[i];
                if (def == null) continue;
                WorkTypeConfig cfg;
                if (jobSettings.TryGetValue(def.defName, out cfg) && cfg != null)
                    next[def.defName] = cfg;
                else
                {
                    cfg = WorkTypeConfig.DefaultFor(def, lastAppliedPresetId);
                    cfg.overrideTier = 0;
                    next[def.defName] = cfg;
                }
            }
            jobSettings = next;
        }

        public override void GameComponentTick()
        {
            if (!enabled || !dailyRefresh)
                return;
            if (Find.TickManager.TicksGame % 2500 != 0)
                return;
            int day = GenDate.DaysPassed;
            if (day <= lastCheckDay)
                return;
            lastCheckDay = day;
            WorkAssigner.Refresh();
        }

        public WorkTypeConfig GetJob(WorkTypeDef def)
        {
            if (def == null)
                return null;
            WorkTypeConfig cfg;
            if (!jobSettings.TryGetValue(def.defName, out cfg) || cfg == null)
            {
                cfg = WorkTypeConfig.DefaultFor(def, lastAppliedPresetId);
                jobSettings[def.defName] = cfg;
            }
            return cfg;
        }

        public bool IsPawnExcluded(Pawn pawn)
        {
            return pawn != null && excludedPawnIds != null && excludedPawnIds.Contains(pawn.ThingID);
        }

        public void SetPawnExcluded(Pawn pawn, bool exclude)
        {
            if (pawn == null) return;
            if (exclude)
            {
                if (!excludedPawnIds.Contains(pawn.ThingID))
                    excludedPawnIds.Add(pawn.ThingID);
            }
            else
                excludedPawnIds.Remove(pawn.ThingID);
        }

        public static string JobExcludeKey(Pawn pawn, WorkTypeDef work)
        {
            return work.defName + "|" + pawn.ThingID;
        }

        public bool IsJobExcluded(Pawn pawn, WorkTypeDef work)
        {
            return pawn != null && work != null && jobExcludeKeys.Contains(JobExcludeKey(pawn, work));
        }

        public void SetJobExcluded(Pawn pawn, WorkTypeDef work, bool exclude)
        {
            if (pawn == null || work == null) return;
            string key = JobExcludeKey(pawn, work);
            if (exclude)
            {
                if (!jobExcludeKeys.Contains(key))
                    jobExcludeKeys.Add(key);
            }
            else
                jobExcludeKeys.Remove(key);
        }

        public static string PinKey(Pawn pawn, WorkTypeDef work)
        {
            return pawn.ThingID + "|" + work.defName;
        }

        public bool IsPinned(Pawn pawn, WorkTypeDef work)
        {
            return pawn != null && work != null && pinnedKeys.Contains(PinKey(pawn, work));
        }

        public void SetPinned(Pawn pawn, WorkTypeDef work, bool pin)
        {
            if (pawn == null || work == null) return;
            string key = PinKey(pawn, work);
            if (pin)
            {
                if (!pinnedKeys.Contains(key))
                    pinnedKeys.Add(key);
            }
            else
                pinnedKeys.Remove(key);
        }
    }
}
