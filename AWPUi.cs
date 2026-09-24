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

        public static void SearchField(Rect rect, ref string text)
        {
            const float iconSize = 24f;
            Texture2D icon = ContentFinder<Texture2D>.Get("UI/Widgets/Search", false);
            float x = rect.x;
            if (icon != null)
            {
                float iy = rect.y + (rect.height - iconSize) / 2f;
                Widgets.DrawTextureFitted(new Rect(rect.x, iy, iconSize, iconSize), icon, 1f);
                x += iconSize + 4f;
            }
            text = Widgets.TextField(new Rect(x, rect.y, rect.xMax - x, rect.height), text ?? "");
        }

        /// <summary>1 = left click (up), -1 = right click (down), 0 = none.</summary>
        public static int ButtonStep(Rect rect, string label, string tip)
        {
            if (!tip.NullOrEmpty())
                TooltipHandler.TipRegion(rect, tip);
            if (Widgets.ButtonText(rect, label))
                return 1;
            if (Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                Event.current.Use();
                return -1;
            }
            return 0;
        }

        public static int WrapPriority(int value, int delta)
        {
            int n = value + delta;
            if (n > 4) n = 0;
            if (n < 0) n = 4;
            return n;
        }
    }
}
