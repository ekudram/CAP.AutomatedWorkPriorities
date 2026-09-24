using RimWorld;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public static class DefLabel
    {
        public static string Of(Def def)
        {
            if (def == null)
                return "(null)";
            TraitDef trait = def as TraitDef;
            if (trait != null)
                return OfTrait(trait);
            WorkTypeDef work = def as WorkTypeDef;
            if (work != null)
                return OfWork(work);
            string label = def.label;
            if (!label.NullOrEmpty())
                return label.CapitalizeFirst();
            return def.defName;
        }

        public static string OfWork(WorkTypeDef work)
        {
            if (work == null)
                return "(null)";
            if (!work.labelShort.NullOrEmpty())
                return work.labelShort.CapitalizeFirst();
            if (!work.pawnLabel.NullOrEmpty())
                return work.pawnLabel.CapitalizeFirst();
            if (!work.label.NullOrEmpty())
                return work.label.CapitalizeFirst();
            return work.defName;
        }

        public static bool WorkMatches(WorkTypeDef work, string filter)
        {
            if (filter.NullOrEmpty())
                return true;
            if (work == null)
                return false;
            if (OfWork(work).IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (!work.pawnLabel.NullOrEmpty() && work.pawnLabel.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (work.defName.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            return false;
        }

        public static bool AllJobsMatches(string filter)
        {
            if (filter.NullOrEmpty())
                return true;
            string all = "AWP_AllJobs".Translate();
            return all.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string OfTrait(TraitDef trait)
        {
            if (trait == null)
                return "(null)";
            if (trait.degreeDatas != null && trait.degreeDatas.Count > 0)
            {
                for (int i = 0; i < trait.degreeDatas.Count; i++)
                {
                    TraitDegreeData d = trait.degreeDatas[i];
                    if (d != null && !d.label.NullOrEmpty())
                        return d.label.CapitalizeFirst();
                }
            }
            if (!trait.label.NullOrEmpty())
                return trait.label.CapitalizeFirst();
            return trait.defName;
        }
    }
}
