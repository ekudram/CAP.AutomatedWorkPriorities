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
            string label = def.label;
            if (!label.NullOrEmpty())
                return label.CapitalizeFirst();
            return def.defName;
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
