using UnityEngine;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public static class AWPUi
    {
        private static readonly Color On = new Color(0.45f, 0.85f, 0.45f);
        private static readonly Color Off = new Color(0.9f, 0.4f, 0.4f);

        public static bool Toggle(Rect rect, bool on, string label, string tip)
        {
            Color old = GUI.color;
            GUI.color = on ? On : Off;
            bool clicked = Widgets.ButtonText(rect, label);
            GUI.color = old;
            if (!tip.NullOrEmpty())
                TooltipHandler.TipRegion(rect, tip);
            return clicked;
        }
    }
}
