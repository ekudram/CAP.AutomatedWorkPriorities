using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public class WorkTypeConfig : IExposable
    {
        public string workTypeDefName;
        public bool included = true;
        public bool usePercentage;
        public int count = 1;
        public float percentage = 0.3f;
        public int primaryPriority = 2;
        public int backupPriority;
        public float passionWeight = 1f;
        public int overrideTier;

        public void ExposeData()
        {
            Scribe_Values.Look(ref workTypeDefName, "workTypeDefName");
            Scribe_Values.Look(ref included, "included", true);
            Scribe_Values.Look(ref usePercentage, "usePercentage");
            Scribe_Values.Look(ref count, "count", 1);
            Scribe_Values.Look(ref percentage, "percentage", 0.3f);
            Scribe_Values.Look(ref primaryPriority, "primaryPriority", 2);
            Scribe_Values.Look(ref backupPriority, "backupPriority", 0);
            Scribe_Values.Look(ref passionWeight, "passionWeight", 1f);
            Scribe_Values.Look(ref overrideTier, "overrideTier", 0);
        }

        public WorkTypeConfig Clone()
        {
            return new WorkTypeConfig
            {
                workTypeDefName = workTypeDefName,
                included = included,
                usePercentage = usePercentage,
                count = count,
                percentage = percentage,
                primaryPriority = primaryPriority,
                backupPriority = backupPriority,
                passionWeight = passionWeight,
                overrideTier = overrideTier
            };
        }

        public static WorkTypeConfig DefaultFor(WorkTypeDef def, string presetId = "normal")
        {
            WorkTier tier = WorkTierCatalog.GetTier(def);
            var cfg = new WorkTypeConfig { workTypeDefName = def.defName, included = true };
            ApplyPresetShape(cfg, def, presetId, tier);
            return cfg;
        }

        public static void ApplyPresetShape(WorkTypeConfig cfg, WorkTypeDef def, string presetId, WorkTier tier)
        {
            if (presetId == "crisis")
            {
                if (tier == WorkTier.AlwaysHigh || tier == WorkTier.Everyone)
                {
                    cfg.usePercentage = true;
                    cfg.percentage = 1f;
                    cfg.primaryPriority = 1;
                    cfg.backupPriority = tier == WorkTier.Everyone ? 2 : 1;
                }
                else if (tier == WorkTier.Passion)
                {
                    cfg.usePercentage = true;
                    cfg.percentage = 0.15f;
                    cfg.primaryPriority = 3;
                    cfg.backupPriority = 0;
                }
                else
                {
                    cfg.usePercentage = false;
                    cfg.count = 0;
                    cfg.primaryPriority = 3;
                    cfg.backupPriority = 4;
                }
                return;
            }

            if (presetId == "construction")
            {
                bool build = def.defName == "Construction" || def.defName == "FSFRepair" || def.defName == "FSFDeconstruct"
                    || def.defName == "Hauling" || def.defName == "FSFHauling" || def.defName == "Mining" || def.defName == "FSFDrilling";
                if (tier == WorkTier.AlwaysHigh)
                {
                    cfg.usePercentage = true;
                    cfg.percentage = 1f;
                    cfg.primaryPriority = 1;
                    cfg.backupPriority = 1;
                }
                else if (build)
                {
                    cfg.usePercentage = true;
                    cfg.percentage = 0.5f;
                    cfg.primaryPriority = 1;
                    cfg.backupPriority = 3;
                }
                else if (tier == WorkTier.Everyone)
                {
                    cfg.backupPriority = 4;
                    cfg.count = 0;
                    cfg.primaryPriority = 3;
                }
                else
                {
                    cfg.usePercentage = true;
                    cfg.percentage = 0.15f;
                    cfg.primaryPriority = 3;
                    cfg.backupPriority = 0;
                }
                return;
            }

            switch (tier)
            {
                case WorkTier.AlwaysHigh:
                    cfg.usePercentage = true;
                    cfg.percentage = 1f;
                    cfg.primaryPriority = 1;
                    cfg.backupPriority = 1;
                    break;
                case WorkTier.Passion:
                    cfg.usePercentage = true;
                    cfg.percentage = 0.25f;
                    cfg.primaryPriority = 2;
                    cfg.backupPriority = 0;
                    break;
                case WorkTier.Everyone:
                    cfg.usePercentage = false;
                    cfg.count = 0;
                    cfg.primaryPriority = 3;
                    cfg.backupPriority = 4;
                    break;
            }
        }
    }
}
