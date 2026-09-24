using RimWorld;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public enum RuleWho
    {
        All,
        Colonist,
        Slave
    }

    public enum RuleActionKind
    {
        Ban,
        Only,
        Set,
        Prefer,
        Require
    }

    public enum RuleConditionKind
    {
        Always,
        Trait,
        Gene,
        PassionMajor,
        PassionMinor,
        PassionAny,
        SkillAtLeast,
        SkillRange,
        Hediff,
        CapacityBelow,
        LifeStage
    }

    public class AssignmentRule : IExposable
    {
        public string id;
        public bool enabled = true;
        public string workTypeDefName;
        public RuleConditionKind condition = RuleConditionKind.Always;
        public string conditionDefName;
        public int skillMin;
        public int skillMax = 20;
        public float capacityMax = 0.5f;
        public RuleActionKind action = RuleActionKind.Set;
        public int setPriority;
        public RuleWho who = RuleWho.All;

        public void ExposeData()
        {
            Scribe_Values.Look(ref id, "id");
            Scribe_Values.Look(ref enabled, "enabled", true);
            Scribe_Values.Look(ref workTypeDefName, "workTypeDefName");
            Scribe_Values.Look(ref condition, "condition", RuleConditionKind.Always);
            Scribe_Values.Look(ref conditionDefName, "conditionDefName");
            Scribe_Values.Look(ref skillMin, "skillMin");
            Scribe_Values.Look(ref skillMax, "skillMax", 20);
            Scribe_Values.Look(ref capacityMax, "capacityMax", 0.5f);
            Scribe_Values.Look(ref action, "action", RuleActionKind.Set);
            Scribe_Values.Look(ref setPriority, "setPriority");
            Scribe_Values.Look(ref who, "who", RuleWho.All);
        }

        public AssignmentRule Clone()
        {
            return new AssignmentRule
            {
                id = id,
                enabled = enabled,
                workTypeDefName = workTypeDefName,
                condition = condition,
                conditionDefName = conditionDefName,
                skillMin = skillMin,
                skillMax = skillMax,
                capacityMax = capacityMax,
                action = action,
                setPriority = setPriority,
                who = who
            };
        }

        public bool AppliesTo(WorkTypeDef work)
        {
            if (work == null) return false;
            if (string.IsNullOrEmpty(workTypeDefName) || workTypeDefName == "*")
                return true;
            return work.defName == workTypeDefName;
        }

        public bool AppliesToPawn(Pawn pawn)
        {
            if (pawn == null) return false;
            if (who == RuleWho.All) return true;
            bool slave = pawn.IsSlaveOfColony;
            if (who == RuleWho.Slave) return slave;
            return pawn.IsColonist && !slave;
        }

        public bool IsHardExclude(Pawn pawn, WorkTypeDef work)
        {
            if (!enabled || pawn == null || !AppliesTo(work) || !AppliesToPawn(pawn))
                return false;
            bool cond = ConditionHolds(pawn, work);
            if (action == RuleActionKind.Ban)
                return cond;
            if (action == RuleActionKind.Only)
                return !cond;
            return false;
        }

        public bool Matches(Pawn pawn, WorkTypeDef work)
        {
            if (!enabled || pawn == null || !AppliesTo(work) || !AppliesToPawn(pawn))
                return false;
            return ConditionHolds(pawn, work);
        }

        private bool ConditionHolds(Pawn pawn, WorkTypeDef work)
        {
            switch (condition)
            {
                case RuleConditionKind.Always:
                    return true;
                case RuleConditionKind.Trait:
                    TraitDef trait = string.IsNullOrEmpty(conditionDefName) ? null : DefDatabase<TraitDef>.GetNamedSilentFail(conditionDefName);
                    return trait != null && pawn.story?.traits != null && pawn.story.traits.HasTrait(trait);
                case RuleConditionKind.Gene:
                    if (!ModsConfig.BiotechActive || pawn.genes == null || string.IsNullOrEmpty(conditionDefName))
                        return false;
                    GeneDef gene = DefDatabase<GeneDef>.GetNamedSilentFail(conditionDefName);
                    return gene != null && pawn.genes.HasActiveGene(gene);
                case RuleConditionKind.PassionMajor:
                    return HasPassion(pawn, work, Passion.Major);
                case RuleConditionKind.PassionMinor:
                    return HasPassion(pawn, work, Passion.Minor);
                case RuleConditionKind.PassionAny:
                    return HasPassion(pawn, work, Passion.Minor) || HasPassion(pawn, work, Passion.Major);
                case RuleConditionKind.SkillAtLeast:
                    return BestSkill(pawn, work) >= skillMin;
                case RuleConditionKind.SkillRange:
                    int lv = BestSkill(pawn, work);
                    return lv >= skillMin && lv <= skillMax;
                case RuleConditionKind.Hediff:
                    if (pawn.health?.hediffSet == null || string.IsNullOrEmpty(conditionDefName))
                        return false;
                    HediffDef hd = DefDatabase<HediffDef>.GetNamedSilentFail(conditionDefName);
                    return hd != null && pawn.health.hediffSet.HasHediff(hd);
                case RuleConditionKind.CapacityBelow:
                    if (pawn.health?.capacities == null || string.IsNullOrEmpty(conditionDefName))
                        return false;
                    PawnCapacityDef cap = DefDatabase<PawnCapacityDef>.GetNamedSilentFail(conditionDefName);
                    if (cap == null) return false;
                    return pawn.health.capacities.GetLevel(cap) < capacityMax;
                case RuleConditionKind.LifeStage:
                    return MatchesLifeStage(pawn, conditionDefName);
                default:
                    return false;
            }
        }

        private static bool HasPassion(Pawn pawn, WorkTypeDef work, Passion want)
        {
            if (work.relevantSkills == null || work.relevantSkills.Count == 0 || pawn.skills == null)
                return false;
            for (int i = 0; i < work.relevantSkills.Count; i++)
            {
                SkillRecord rec = pawn.skills.GetSkill(work.relevantSkills[i]);
                if (rec != null && rec.passion == want)
                    return true;
            }
            return false;
        }

        private static bool MatchesLifeStage(Pawn pawn, string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            DevelopmentalStage stage = pawn.DevelopmentalStage;
            if (name.Equals("Child", System.StringComparison.OrdinalIgnoreCase))
                return stage == DevelopmentalStage.Child;
            if (name.Equals("Adult", System.StringComparison.OrdinalIgnoreCase))
                return stage == DevelopmentalStage.Adult;
            if (name.Equals("Baby", System.StringComparison.OrdinalIgnoreCase))
                return stage == DevelopmentalStage.Baby || stage == DevelopmentalStage.Newborn;
            return false;
        }

        private static int BestSkill(Pawn pawn, WorkTypeDef work)
        {
            if (pawn.skills == null || work.relevantSkills == null || work.relevantSkills.Count == 0)
                return 0;
            int best = 0;
            for (int i = 0; i < work.relevantSkills.Count; i++)
            {
                SkillRecord rec = pawn.skills.GetSkill(work.relevantSkills[i]);
                if (rec != null && rec.Level > best)
                    best = rec.Level;
            }
            return best;
        }

        public string Summary()
        {
            string job = JobLabel();
            string cond;
            switch (condition)
            {
                case RuleConditionKind.Trait: cond = "trait " + conditionDefName; break;
                case RuleConditionKind.Gene: cond = "gene " + conditionDefName; break;
                case RuleConditionKind.PassionMajor: cond = "major passion"; break;
                case RuleConditionKind.PassionMinor: cond = "minor passion"; break;
                case RuleConditionKind.PassionAny: cond = "any passion"; break;
                case RuleConditionKind.SkillAtLeast: cond = "skill ≥ " + skillMin; break;
                case RuleConditionKind.SkillRange: cond = "skill " + skillMin + "-" + skillMax; break;
                case RuleConditionKind.Hediff: cond = "hediff " + conditionDefName; break;
                case RuleConditionKind.CapacityBelow: cond = (conditionDefName ?? "capacity") + " < " + capacityMax.ToString("0.00"); break;
                case RuleConditionKind.LifeStage: cond = "lifestage " + (conditionDefName ?? "?"); break;
                default: cond = "always"; break;
            }
            string act;
            switch (action)
            {
                case RuleActionKind.Ban: act = "Ban"; break;
                case RuleActionKind.Only: act = "Only"; break;
                case RuleActionKind.Prefer: act = "Prefer"; break;
                case RuleActionKind.Require: act = "Require"; break;
                default: act = "Set P" + setPriority; break;
            }
            string whoLabel = "";
            if (who == RuleWho.Colonist) whoLabel = " [" + "AWP_WhoColonist".Translate() + "]";
            else if (who == RuleWho.Slave) whoLabel = " [" + "AWP_WhoSlave".Translate() + "]";
            return (enabled ? "" : "[off] ") + act + " " + job + whoLabel + " if " + cond;
        }

        public bool TargetsAllJobs()
        {
            return string.IsNullOrEmpty(workTypeDefName) || workTypeDefName == "*";
        }

        public string JobLabel()
        {
            if (TargetsAllJobs())
                return "AWP_AllJobs".Translate();
            WorkTypeDef def = DefDatabase<WorkTypeDef>.GetNamedSilentFail(workTypeDefName);
            if (def != null)
                return DefLabel.OfWork(def);
            return workTypeDefName;
        }
    }
}
