using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public class PreviewRow
    {
        public Pawn pawn;
        public WorkTypeDef work;
        public int oldPriority;
        public int newPriority;
        public string reason;
    }

    public static class WorkAssigner
    {
        public static bool IsRefreshing;

        public static void Refresh()
        {
            List<PreviewRow> rows = Compute(true);
            GameComponent_AWP data = GameComponent_AWP.Get;
            if (data == null) return;
            data.lastRefreshTick = Find.TickManager != null ? Find.TickManager.TicksGame : 0;
            data.lastRefreshChanged = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rows.Count; i++)
            {
                PreviewRow r = rows[i];
                if (r.oldPriority != r.newPriority)
                {
                    data.lastRefreshChanged++;
                    if (sb.Length < 800)
                        sb.AppendLine(r.pawn.LabelShort + " " + r.work.labelShort + " " + r.oldPriority + "→" + r.newPriority + " (" + r.reason + ")");
                }
            }
            data.lastRefreshLog = data.lastRefreshChanged + " changes.\n" + sb;
        }

        public static List<PreviewRow> Preview()
        {
            return Compute(false);
        }

        private static List<PreviewRow> Compute(bool apply)
        {
            var result = new List<PreviewRow>();
            GameComponent_AWP data = GameComponent_AWP.Get;
            if (data == null || !data.enabled)
                return result;
            Map map = Find.CurrentMap;
            if (map == null)
                return result;

            List<Pawn> colonists = new List<Pawn>();
            List<Pawn> all = map.mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < all.Count; i++)
            {
                Pawn p = all[i];
                if (p == null || p.Dead || p.workSettings == null)
                    continue;
                if (p.DevelopmentalStage == DevelopmentalStage.Baby || p.DevelopmentalStage == DevelopmentalStage.Newborn)
                    continue;
                if (data.IsPawnExcluded(p))
                    continue;
                colonists.Add(p);
            }

            if (apply)
                IsRefreshing = true;
            try
            {
                List<WorkTypeDef> types = DefDatabase<WorkTypeDef>.AllDefsListForReading;
                for (int t = 0; t < types.Count; t++)
                {
                    WorkTypeDef work = types[t];
                    if (work == null)
                        continue;
                    AssignOne(data, work, colonists, apply, result);
                }
            }
            finally
            {
                if (apply)
                    IsRefreshing = false;
            }
            return result;
        }

        private static void AssignOne(GameComponent_AWP data, WorkTypeDef work, List<Pawn> colonists, bool apply, List<PreviewRow> rows)
        {
            WorkTypeConfig cfg = data.GetJob(work);
            if (cfg == null || !cfg.included)
                return;

            List<Pawn> eligible = new List<Pawn>();
            List<Pawn> banned = new List<Pawn>();
            for (int i = 0; i < colonists.Count; i++)
            {
                Pawn p = colonists[i];
                if (p.WorkTypeIsDisabled(work) || p.WorkTagIsDisabled(work.workTags))
                    continue;
                if (data.IsJobExcluded(p, work))
                    continue;
                if (data.IsPinned(p, work))
                    continue;
                if (IsHardExclude(data, p, work))
                    banned.Add(p);
                else
                    eligible.Add(p);
            }

            for (int i = 0; i < banned.Count; i++)
                SetPrio(banned[i], work, 0, apply, rows, "ban");

            WorkTier tier = WorkTierCatalog.GetTier(work);

            bool hasRequire = false;
            for (int i = 0; i < data.rules.Count; i++)
            {
                if (data.rules[i].enabled && data.rules[i].action == RuleActionKind.Require && data.rules[i].AppliesTo(work))
                    hasRequire = true;
            }

            List<Pawn> primaryPool = eligible;
            if (hasRequire)
            {
                primaryPool = new List<Pawn>();
                for (int i = 0; i < eligible.Count; i++)
                {
                    if (IsAction(data, eligible[i], work, RuleActionKind.Require) || !FailsAnyRequire(data, eligible[i], work))
                    {
                        if (MatchesAnyRequire(data, eligible[i], work))
                            primaryPool.Add(eligible[i]);
                    }
                }
            }

            primaryPool.Sort((a, b) => Score(b, work, cfg, data).CompareTo(Score(a, work, cfg, data)));

            int slots;
            if (tier == WorkTier.AlwaysHigh || (cfg.usePercentage && cfg.percentage >= 0.999f))
                slots = eligible.Count;
            else if (cfg.usePercentage)
                slots = (int)(primaryPool.Count * cfg.percentage + 0.5f);
            else
                slots = cfg.count;
            if (slots < 0) slots = 0;
            if (slots > primaryPool.Count) slots = primaryPool.Count;

            HashSet<int> primaryIds = new HashSet<int>();
            for (int i = 0; i < slots; i++)
                primaryIds.Add(primaryPool[i].thingIDNumber);

            for (int i = 0; i < eligible.Count; i++)
            {
                Pawn p = eligible[i];
                int prio;
                string reason;
                AssignmentRule set = FindSet(data, p, work);
                if (set != null)
                {
                    prio = ClampPrio(set.setPriority);
                    reason = "set:" + set.id;
                }
                else if (primaryIds.Contains(p.thingIDNumber))
                {
                    prio = ClampPrio(cfg.primaryPriority <= 0 && tier == WorkTier.AlwaysHigh ? 1 : cfg.primaryPriority);
                    reason = "primary";
                }
                else
                {
                    prio = ClampPrio(cfg.backupPriority);
                    if (tier == WorkTier.Everyone && prio == 0)
                        prio = 4;
                    reason = "backup";
                }

                if (data.forceEmergencyPriorities && IsEmergency(work) && !IsHardExclude(data, p, work))
                {
                    prio = 1;
                    reason = "emergency";
                }

                SetPrio(p, work, prio, apply, rows, reason);
            }
        }

        private static bool IsEmergency(WorkTypeDef work)
        {
            WorkTier t = WorkTierCatalog.GetTier(work);
            return t == WorkTier.AlwaysHigh;
        }

        private static void SetPrio(Pawn pawn, WorkTypeDef work, int prio, bool apply, List<PreviewRow> rows, string reason)
        {
            int old = 0;
            try { old = pawn.workSettings.GetPriority(work); }
            catch { }
            if (apply)
                pawn.workSettings.SetPriority(work, prio);
            rows.Add(new PreviewRow
            {
                pawn = pawn,
                work = work,
                oldPriority = old,
                newPriority = prio,
                reason = reason
            });
        }

        private static int ClampPrio(int p)
        {
            if (p < 0) return 0;
            if (p > 4) return 4;
            return p;
        }

        private static bool IsHardExclude(GameComponent_AWP data, Pawn pawn, WorkTypeDef work)
        {
            for (int i = 0; i < data.rules.Count; i++)
            {
                if (data.rules[i].IsHardExclude(pawn, work))
                    return true;
            }
            return false;
        }

        private static bool IsAction(GameComponent_AWP data, Pawn pawn, WorkTypeDef work, RuleActionKind kind)
        {
            for (int i = 0; i < data.rules.Count; i++)
            {
                AssignmentRule r = data.rules[i];
                if (r.action == kind && r.Matches(pawn, work))
                    return true;
            }
            return false;
        }

        private static bool MatchesAnyRequire(GameComponent_AWP data, Pawn pawn, WorkTypeDef work)
        {
            bool any = false;
            for (int i = 0; i < data.rules.Count; i++)
            {
                AssignmentRule r = data.rules[i];
                if (!r.enabled || r.action != RuleActionKind.Require || !r.AppliesTo(work))
                    continue;
                any = true;
                if (r.Matches(pawn, work))
                    return true;
            }
            return !any;
        }

        private static bool FailsAnyRequire(GameComponent_AWP data, Pawn pawn, WorkTypeDef work)
        {
            return !MatchesAnyRequire(data, pawn, work);
        }

        private static AssignmentRule FindSet(GameComponent_AWP data, Pawn pawn, WorkTypeDef work)
        {
            for (int i = 0; i < data.rules.Count; i++)
            {
                AssignmentRule r = data.rules[i];
                if (r.action == RuleActionKind.Set && r.Matches(pawn, work))
                    return r;
            }
            return null;
        }

        private static float Score(Pawn pawn, WorkTypeDef work, WorkTypeConfig cfg, GameComponent_AWP data)
        {
            float skill = 0f;
            float passion = 0f;
            if (pawn.skills != null && work.relevantSkills != null)
            {
                for (int i = 0; i < work.relevantSkills.Count; i++)
                {
                    SkillRecord rec = pawn.skills.GetSkill(work.relevantSkills[i]);
                    if (rec == null) continue;
                    if (rec.Level > skill) skill = rec.Level;
                    if (rec.passion == Passion.Major) passion = 8f;
                    else if (rec.passion == Passion.Minor && passion < 3f) passion = 3f;
                }
            }
            float prefer = 0f;
            for (int i = 0; i < data.rules.Count; i++)
            {
                AssignmentRule r = data.rules[i];
                if (r.action == RuleActionKind.Prefer && r.Matches(pawn, work))
                    prefer += 10f;
            }
            if (pawn.health?.capacities != null)
            {
                float manip = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation);
                if (manip < 0.5f)
                    skill *= manip + 0.1f;
            }
            return skill + passion * cfg.passionWeight + prefer;
        }
    }
}
