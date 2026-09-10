using System.Collections.Generic;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public static class DefaultRules
    {
        public static List<AssignmentRule> Create()
        {
            var list = new List<AssignmentRule>();
            List<AWPRuleDef> defs = DefDatabase<AWPRuleDef>.AllDefsListForReading;
            if (defs != null)
            {
                for (int i = 0; i < defs.Count; i++)
                    list.Add(defs[i].ToRule());
            }
            if (list.Count == 0)
            {
                list.Add(new AssignmentRule
                {
                    id = "ban-pyromaniac-firefighter",
                    workTypeDefName = "Firefighter",
                    condition = RuleConditionKind.Trait,
                    conditionDefName = "Pyromaniac",
                    action = RuleActionKind.Ban
                });
                list.Add(new AssignmentRule
                {
                    id = "ban-fireterror-firefighter",
                    workTypeDefName = "Firefighter",
                    condition = RuleConditionKind.Gene,
                    conditionDefName = "FireTerror",
                    action = RuleActionKind.Ban
                });
                list.Add(new AssignmentRule
                {
                    id = "prefer-major-passion",
                    workTypeDefName = "*",
                    condition = RuleConditionKind.PassionMajor,
                    action = RuleActionKind.Prefer
                });
                list.Add(new AssignmentRule
                {
                    id = "require-doctor-medicine",
                    workTypeDefName = "Doctor",
                    condition = RuleConditionKind.SkillAtLeast,
                    skillMin = 8,
                    action = RuleActionKind.Require
                });
                AddChildBan(list, "Firefighter");
                AddChildBan(list, "Hunting");
                AddChildBan(list, "Mining");
                AddChildBan(list, "Doctor");
                AddChildBan(list, "Construction");
            }
            return list;
        }

        private static void AddChildBan(List<AssignmentRule> list, string work)
        {
            list.Add(new AssignmentRule
            {
                id = "ban-child-" + work.ToLowerInvariant(),
                workTypeDefName = work,
                condition = RuleConditionKind.LifeStage,
                conditionDefName = "Child",
                action = RuleActionKind.Ban
            });
        }
    }
}
