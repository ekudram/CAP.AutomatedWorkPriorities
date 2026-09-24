using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public class AWPRuleDef : Def
    {
        public string workTypeDefName = "*";
        public RuleConditionKind condition = RuleConditionKind.Always;
        public string conditionDefName;
        public int skillMin;
        public int skillMax = 20;
        public float capacityMax = 1f;
        public RuleActionKind action = RuleActionKind.Ban;
        public int setPriority;
        public RuleWho who = RuleWho.All;
        public bool defaultEnabled = true;

        public AssignmentRule ToRule()
        {
            return new AssignmentRule
            {
                id = defName,
                enabled = defaultEnabled,
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
    }
}
