using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public class Dialog_DefPicker : Window
    {
        private readonly string title;
        private readonly List<Def> defs;
        private readonly Action<Def> onPicked;
        private readonly Action onAllJobs;
        private string filter = "";
        private Vector2 scroll;

        public override Vector2 InitialSize => new Vector2(420f, 520f);

        public Dialog_DefPicker(string title, List<Def> defs, Action<Def> onPicked, Action onAllJobs = null)
        {
            this.title = title;
            this.defs = defs ?? new List<Def>();
            this.onPicked = onPicked;
            this.onAllJobs = onAllJobs;
            doCloseX = true;
            draggable = true;
            absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 28f), title);
            Text.Font = GameFont.Small;
            filter = Widgets.TextField(new Rect(0f, 32f, inRect.width, 28f), filter);

            string f = filter == null ? "" : filter.Trim();
            List<Def> shown = new List<Def>();
            for (int i = 0; i < defs.Count; i++)
            {
                Def d = defs[i];
                if (d == null) continue;
                WorkTypeDef work = d as WorkTypeDef;
                bool hit;
                if (work != null)
                    hit = DefLabel.WorkMatches(work, f);
                else
                {
                    string label = DefLabel.Of(d);
                    hit = f.Length == 0
                        || label.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0
                        || d.defName.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0;
                }
                if (hit)
                    shown.Add(d);
            }
            shown.Sort((a, b) => string.Compare(DefLabel.Of(a), DefLabel.Of(b), StringComparison.OrdinalIgnoreCase));

            bool showAll = onAllJobs != null && DefLabel.AllJobsMatches(f);
            Rect view = new Rect(0f, 0f, inRect.width - 20f, (shown.Count + (showAll ? 1 : 0)) * 28f);
            Widgets.BeginScrollView(new Rect(0f, 68f, inRect.width, inRect.height - 68f), ref scroll, view);
            float y = 0f;
            if (showAll)
            {
                if (Widgets.ButtonText(new Rect(0f, y, view.width, 26f), "AWP_AllJobs".Translate()))
                {
                    onAllJobs();
                    Close();
                }
                y += 28f;
            }
            for (int i = 0; i < shown.Count; i++)
            {
                Def d = shown[i];
                if (Widgets.ButtonText(new Rect(0f, y, view.width, 26f), DefLabel.Of(d)))
                {
                    onPicked?.Invoke(d);
                    Close();
                }
                y += 28f;
            }
            Widgets.EndScrollView();
        }
    }
}
