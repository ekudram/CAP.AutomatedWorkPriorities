using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public class Dialog_JobRules : Window
    {
        private readonly WorkTypeDef work;
        private Vector2 scroll;

        public override Vector2 InitialSize => new Vector2(640f, 420f);

        public Dialog_JobRules(WorkTypeDef work)
        {
            this.work = work;
            doCloseX = true;
            draggable = true;
            absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            string title = work == null ? "AWP_AllJobs".Translate() : DefLabel.OfWork(work);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 160f, 28f), title);
            Text.Font = GameFont.Small;

            GameComponent_AWP data = GameComponent_AWP.Get;
            if (Widgets.ButtonText(new Rect(inRect.width - 150f, 0f, 150f, 28f), "AWP_AddRule".Translate()) && data != null)
            {
                var r = new AssignmentRule
                {
                    id = "user-" + Rand.Int,
                    workTypeDefName = work == null ? "*" : work.defName,
                    condition = RuleConditionKind.Always,
                    action = RuleActionKind.Ban
                };
                data.rules.Add(r);
                Find.WindowStack.Add(new Dialog_RuleEditor(r));
            }

            List<AssignmentRule> rules = Collect(data, work);
            if (rules.Count == 0)
            {
                Widgets.Label(new Rect(0f, 36f, inRect.width, 28f), "AWP_NoRulesForJob".Translate());
                return;
            }

            Rect view = new Rect(0f, 0f, inRect.width - 20f, rules.Count * 32f + 8f);
            Widgets.BeginScrollView(new Rect(0f, 36f, inRect.width, inRect.height - 36f), ref scroll, view);
            float y = 0f;
            AssignmentRule toDelete = null;
            for (int i = 0; i < rules.Count; i++)
            {
                AssignmentRule r = rules[i];
                Widgets.CheckboxLabeled(new Rect(0f, y, 28f, 28f), "", ref r.enabled);
                Widgets.Label(new Rect(32f, y, view.width - 170f, 28f), r.Summary());
                if (Widgets.ButtonText(new Rect(view.width - 160f, y, 70f, 26f), "Edit"))
                    Find.WindowStack.Add(new Dialog_RuleEditor(r));
                if (Widgets.ButtonText(new Rect(view.width - 80f, y, 70f, 26f), "Delete"))
                    toDelete = r;
                y += 32f;
            }
            Widgets.EndScrollView();
            if (toDelete != null && data != null)
                data.rules.Remove(toDelete);
        }

        public static List<AssignmentRule> Collect(GameComponent_AWP data, WorkTypeDef work)
        {
            var list = new List<AssignmentRule>();
            if (data == null || data.rules == null)
                return list;
            for (int i = 0; i < data.rules.Count; i++)
            {
                AssignmentRule r = data.rules[i];
                if (r == null) continue;
                if (work == null)
                {
                    if (r.TargetsAllJobs())
                        list.Add(r);
                }
                else if (r.AppliesTo(work))
                    list.Add(r);
            }
            return list;
        }
    }
}
