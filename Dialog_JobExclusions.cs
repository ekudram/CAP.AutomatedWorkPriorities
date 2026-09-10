using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public class Dialog_JobExclusions : Window
    {
        private readonly WorkTypeDef work;
        private Vector2 scroll;

        public override Vector2 InitialSize => new Vector2(400f, 480f);

        public Dialog_JobExclusions(WorkTypeDef work)
        {
            this.work = work;
            doCloseX = true;
            draggable = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            GameComponent_AWP data = GameComponent_AWP.Get;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 28f), "AWP_JobExclude".Translate(work.labelShort));
            if (data == null || Find.CurrentMap == null)
                return;
            List<Pawn> pawns = new List<Pawn>(Find.CurrentMap.mapPawns.FreeColonistsSpawned);
            pawns.Sort((a, b) => string.Compare(a.LabelShort, b.LabelShort, StringComparison.OrdinalIgnoreCase));
            Rect view = new Rect(0f, 0f, inRect.width - 20f, pawns.Count * 28f);
            Widgets.BeginScrollView(new Rect(0f, 32f, inRect.width, inRect.height - 32f), ref scroll, view);
            float y = 0f;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                bool ex = data.IsJobExcluded(p, work);
                Widgets.CheckboxLabeled(new Rect(0f, y, view.width, 26f), p.LabelShortCap, ref ex);
                data.SetJobExcluded(p, work, ex);
                y += 28f;
            }
            Widgets.EndScrollView();
        }
    }
}
