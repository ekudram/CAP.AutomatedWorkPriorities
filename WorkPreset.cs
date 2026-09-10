using System.Collections.Generic;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public class WorkPreset : IExposable
    {
        public string id;
        public string name = "New preset";
        public bool shipped;
        public List<WorkTypeConfig> jobs = new List<WorkTypeConfig>();
        public List<AssignmentRule> rules = new List<AssignmentRule>();

        public void ExposeData()
        {
            Scribe_Values.Look(ref id, "id");
            Scribe_Values.Look(ref name, "name", "New preset");
            Scribe_Values.Look(ref shipped, "shipped");
            Scribe_Collections.Look(ref jobs, "jobs", LookMode.Deep);
            Scribe_Collections.Look(ref rules, "rules", LookMode.Deep);
            if (jobs == null) jobs = new List<WorkTypeConfig>();
            if (rules == null) rules = new List<AssignmentRule>();
        }

        public WorkPreset Clone()
        {
            var p = new WorkPreset { id = id, name = name, shipped = shipped };
            if (jobs != null)
            {
                for (int i = 0; i < jobs.Count; i++)
                    p.jobs.Add(jobs[i].Clone());
            }
            if (rules != null)
            {
                for (int i = 0; i < rules.Count; i++)
                    p.rules.Add(rules[i].Clone());
            }
            return p;
        }

        public static WorkPreset FromCurrentWorld(string newId, string newName)
        {
            var p = new WorkPreset { id = newId, name = newName };
            GameComponent_AWP data = GameComponent_AWP.Get;
            if (data == null)
            {
                p.rules.AddRange(DefaultRules.Create());
                return p;
            }
            foreach (var kv in data.jobSettings)
                p.jobs.Add(kv.Value.Clone());
            for (int i = 0; i < data.rules.Count; i++)
                p.rules.Add(data.rules[i].Clone());
            return p;
        }

        public void ApplyTo(GameComponent_AWP data)
        {
            if (data == null) return;
            data.lastAppliedPresetId = id;
            if (data.tierOverrides != null)
                data.tierOverrides.Clear();
            data.jobSettings.Clear();

            if (shipped)
            {
                List<WorkTypeDef> all = DefDatabase<WorkTypeDef>.AllDefsListForReading;
                for (int i = 0; i < all.Count; i++)
                {
                    WorkTypeDef def = all[i];
                    if (def == null) continue;
                    WorkTypeConfig cfg = WorkTypeConfig.DefaultFor(def, id);
                    cfg.overrideTier = 0;
                    data.jobSettings[def.defName] = cfg;
                }
            }
            else
            {
                for (int i = 0; i < jobs.Count; i++)
                {
                    WorkTypeConfig c = jobs[i].Clone();
                    if (!string.IsNullOrEmpty(c.workTypeDefName))
                        data.jobSettings[c.workTypeDefName] = c;
                }
            }

            data.rules.Clear();
            for (int i = 0; i < rules.Count; i++)
                data.rules.Add(rules[i].Clone());
            if (data.rules.Count == 0)
                data.rules.AddRange(DefaultRules.Create());
            data.RebuildJobList();
        }
    }
}
