using RimWorld;
using System.Collections.Generic;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public enum WorkTier
    {
        AlwaysHigh = 1,
        Passion = 2,
        Everyone = 3
    }

    public static class WorkTierCatalog
    {
        private static readonly HashSet<string> AlwaysHigh = new HashSet<string>
        {
            "Firefighter", "Doctor", "Childcare", "Patient", "BedRest",
            "FSFNurse", "FSFSurgeon"
        };

        private static readonly HashSet<string> Passion = new HashSet<string>
        {
            "Cooking", "Hunting", "Growing", "Mining", "Construction",
            "Smithing", "Tailoring", "Art", "Crafting", "Research",
            "Warden", "Handling", "FSFScan",
            "FSFButcher", "FSFTraining", "FSFHarvesting", "FSFDrilling",
            "FSFDrugs", "FSFMachining", "FSFFabrication", "FSFRepair", "FSFDeconstruct"
        };

        private static readonly HashSet<string> Everyone = new HashSet<string>
        {
            "Hauling", "Cleaning", "PlantCutting", "Basic",
            "FSFHauling", "FSFDeliver", "FSFTransport", "FSFRefining", "FSFProduction",
            "FSFSmelt", "FSFStoneCut", "FSFRearming", "FSFCremating", "FSFBasic"
        };

        public static WorkTier GetTier(WorkTypeDef def)
        {
            if (def == null) return WorkTier.Everyone;
            GameComponent_AWP data = GameComponent_AWP.Get;
            int ov;
            if (data != null && data.tierOverrides != null && data.tierOverrides.TryGetValue(def.defName, out ov))
            {
                if (ov >= 1 && ov <= 3)
                    return (WorkTier)ov;
            }

            WorkTypeConfig cfg = null;
            if (data != null)
                data.jobSettings.TryGetValue(def.defName, out cfg);
            if (cfg != null && cfg.overrideTier >= 1 && cfg.overrideTier <= 3)
                return (WorkTier)cfg.overrideTier;

            WorkTypeTierDef xml = DefDatabase<WorkTypeTierDef>.GetNamedSilentFail("AWP_Tier_" + def.defName);
            if (xml != null)
                return xml.tier;

            string n = def.defName;
            if (AlwaysHigh.Contains(n)) return WorkTier.AlwaysHigh;
            if (Everyone.Contains(n)) return WorkTier.Everyone;
            if (Passion.Contains(n)) return WorkTier.Passion;

            if (def.labelShort != null && def.labelShort.Equals("Basic", System.StringComparison.OrdinalIgnoreCase))
                return WorkTier.Everyone;
            if (def.labelShort != null && def.labelShort.Equals("Scan", System.StringComparison.OrdinalIgnoreCase))
                return WorkTier.Passion;

            if (def.relevantSkills != null && def.relevantSkills.Count > 0)
                return WorkTier.Passion;

            return WorkTier.Everyone;
        }

        public static WorkTier Cycle(WorkTier current)
        {
            int n = (int)current + 1;
            if (n > 3) n = 1;
            return (WorkTier)n;
        }

        public static WorkTier CyclePrev(WorkTier current)
        {
            int n = (int)current - 1;
            if (n < 1) n = 3;
            return (WorkTier)n;
        }

        public static string TierLabel(WorkTier tier)
        {
            switch (tier)
            {
                case WorkTier.AlwaysHigh: return "AWP_TierAlwaysHigh".Translate();
                case WorkTier.Passion: return "AWP_TierPassion".Translate();
                default: return "AWP_TierEveryone".Translate();
            }
        }

        public static string Short(WorkTier tier)
        {
            switch (tier)
            {
                case WorkTier.AlwaysHigh: return "T1";
                case WorkTier.Passion: return "T2";
                default: return "T3";
            }
        }
    }
}
