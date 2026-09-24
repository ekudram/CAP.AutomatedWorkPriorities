using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public class Dialog_RuleEditor : Window
    {
        private readonly AssignmentRule rule;

        public override Vector2 InitialSize => new Vector2(480f, 420f);

        public Dialog_RuleEditor(AssignmentRule rule)
        {
            this.rule = rule;
            doCloseX = true;
            draggable = true;
            absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            list.CheckboxLabeled("Enabled", ref rule.enabled);
            if (list.ButtonTextLabeled("Who", WhoLabel(rule.who)))
            {
                Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>
                {
                    new FloatMenuOption("AWP_WhoAll".Translate(), () => rule.who = RuleWho.All),
                    new FloatMenuOption("AWP_WhoColonist".Translate(), () => rule.who = RuleWho.Colonist),
                    new FloatMenuOption("AWP_WhoSlave".Translate(), () => rule.who = RuleWho.Slave)
                }));
            }
            if (list.ButtonTextLabeled("Job", rule.JobLabel()))
            {
                var listDefs = new List<Def>();
                List<WorkTypeDef> all = DefDatabase<WorkTypeDef>.AllDefsListForReading;
                for (int i = 0; i < all.Count; i++)
                    listDefs.Add(all[i]);
                Find.WindowStack.Add(new Dialog_DefPicker("Job", listDefs, def => rule.workTypeDefName = def.defName, () => rule.workTypeDefName = "*"));
            }
            if (list.ButtonTextLabeled("Condition", rule.condition.ToString()))
            {
                var opts = new List<FloatMenuOption>();
                foreach (RuleConditionKind k in Enum.GetValues(typeof(RuleConditionKind)))
                {
                    RuleConditionKind local = k;
                    opts.Add(new FloatMenuOption(local.ToString(), () => rule.condition = local));
                }
                Find.WindowStack.Add(new FloatMenu(opts));
            }
            if (list.ButtonTextLabeled("Action", rule.action.ToString()))
            {
                var opts = new List<FloatMenuOption>();
                foreach (RuleActionKind k in Enum.GetValues(typeof(RuleActionKind)))
                {
                    RuleActionKind local = k;
                    opts.Add(new FloatMenuOption(local.ToString(), () => rule.action = local));
                }
                Find.WindowStack.Add(new FloatMenu(opts));
            }
            if (rule.action == RuleActionKind.Set)
            {
                string buf = rule.setPriority.ToString();
                list.TextFieldNumericLabeled("Priority", ref rule.setPriority, ref buf, 0, 4);
            }
            if (rule.condition == RuleConditionKind.SkillAtLeast || rule.condition == RuleConditionKind.SkillRange)
            {
                string b1 = rule.skillMin.ToString();
                list.TextFieldNumericLabeled("Skill min", ref rule.skillMin, ref b1, 0, 20);
            }
            if (rule.condition == RuleConditionKind.SkillRange)
            {
                string b2 = rule.skillMax.ToString();
                list.TextFieldNumericLabeled("Skill max", ref rule.skillMax, ref b2, 0, 20);
            }
            if (rule.condition == RuleConditionKind.CapacityBelow)
            {
                rule.capacityMax = list.Slider(rule.capacityMax, 0f, 1f);
                if (list.ButtonText(rule.conditionDefName ?? "Consciousness"))
                    PickCapacity();
            }
            if (rule.condition == RuleConditionKind.Trait && list.ButtonText(TraitButtonLabel()))
                PickTrait();
            if (rule.condition == RuleConditionKind.Gene && list.ButtonText(rule.conditionDefName ?? "Pick gene"))
                PickGene();
            if (rule.condition == RuleConditionKind.Hediff && list.ButtonText(rule.conditionDefName ?? "Pick hediff"))
                PickHediff();
            if (rule.condition == RuleConditionKind.LifeStage && list.ButtonText(rule.conditionDefName ?? "Child"))
            {
                Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>
                {
                    new FloatMenuOption("Child", () => rule.conditionDefName = "Child"),
                    new FloatMenuOption("Adult", () => rule.conditionDefName = "Adult")
                }));
            }
            list.Label(rule.Summary());
            if (list.ButtonText("OK"))
                Close();
            list.End();
        }

        private static string WhoLabel(RuleWho who)
        {
            if (who == RuleWho.Colonist) return "AWP_WhoColonist".Translate();
            if (who == RuleWho.Slave) return "AWP_WhoSlave".Translate();
            return "AWP_WhoAll".Translate();
        }

        private string TraitButtonLabel()
        {
            if (string.IsNullOrEmpty(rule.conditionDefName))
                return "Pick trait";
            TraitDef t = DefDatabase<TraitDef>.GetNamedSilentFail(rule.conditionDefName);
            return t == null ? rule.conditionDefName : DefLabel.OfTrait(t);
        }

        private void PickTrait()
        {
            var list = new List<Def>();
            foreach (TraitDef d in DefDatabase<TraitDef>.AllDefsListForReading)
                list.Add(d);
            Find.WindowStack.Add(new Dialog_DefPicker("Trait", list, def => rule.conditionDefName = def.defName));
        }

        private void PickGene()
        {
            var list = new List<Def>();
            if (ModsConfig.BiotechActive)
            {
                foreach (GeneDef d in DefDatabase<GeneDef>.AllDefsListForReading)
                    list.Add(d);
            }
            Find.WindowStack.Add(new Dialog_DefPicker("Gene", list, def => rule.conditionDefName = def.defName));
        }

        private void PickHediff()
        {
            var list = new List<Def>();
            foreach (HediffDef d in DefDatabase<HediffDef>.AllDefsListForReading)
            {
                if (d.label.NullOrEmpty() && d.defName.NullOrEmpty())
                    continue;
                list.Add(d);
            }
            Find.WindowStack.Add(new Dialog_DefPicker("Hediff", list, def => rule.conditionDefName = def.defName));
        }

        private void PickCapacity()
        {
            var opts = new List<FloatMenuOption>();
            foreach (PawnCapacityDef d in DefDatabase<PawnCapacityDef>.AllDefsListForReading)
            {
                PawnCapacityDef local = d;
                opts.Add(new FloatMenuOption(local.label, () => rule.conditionDefName = local.defName));
            }
            Find.WindowStack.Add(new FloatMenu(opts));
        }
    }
}
