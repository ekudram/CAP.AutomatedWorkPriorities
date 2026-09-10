using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public class Dialog_Preview : Window
    {
        private Vector2 scroll;
        private List<PreviewRow> rows;

        public override Vector2 InitialSize => new Vector2(720f, 520f);

        public Dialog_Preview()
        {
            doCloseX = true;
            draggable = true;
            resizeable = true;
            rows = WorkAssigner.Preview();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Widgets.Label(new Rect(0f, 0f, inRect.width, 28f), "AWP_PreviewTitle".Translate() + " (" + rows.Count + ")");
            if (Widgets.ButtonText(new Rect(inRect.width - 160f, 0f, 150f, 28f), "AWP_Refresh".Translate()))
            {
                WorkAssigner.Refresh();
                Close();
            }
            Rect view = new Rect(0f, 0f, inRect.width - 20f, rows.Count * 24f);
            Widgets.BeginScrollView(new Rect(0f, 36f, inRect.width, inRect.height - 36f), ref scroll, view);
            float y = 0f;
            for (int i = 0; i < rows.Count; i++)
            {
                PreviewRow r = rows[i];
                if (r.oldPriority == r.newPriority)
                    continue;
                Widgets.Label(new Rect(0f, y, view.width, 22f),
                    r.pawn.LabelShortCap + "  " + r.work.labelShort + "  " + r.oldPriority + " → " + r.newPriority + "  " + r.reason);
                y += 22f;
            }
            Widgets.EndScrollView();
        }
    }
}
