using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    [HarmonyPatch(typeof(MainTabWindow_Work), nameof(MainTabWindow_Work.DoWindowContents))]
    public static class Patch_WorkTab
    {
        [HarmonyPostfix]
        public static void Postfix(Rect rect)
        {
            GameComponent_AWP data = GameComponent_AWP.Get;
            float w = 128f;
            float x = rect.xMax - 16f - w * 3f - 8f;
            float y = rect.y + 4f;

            bool on = data != null && data.enabled;
            Rect onRect = new Rect(x, y, w, 26f);
            if (AWPUi.Toggle(onRect, on, on ? "AWP_ModOn".Translate() : "AWP_ModOff".Translate(), "AWP_TipAuto".Translate()))
            {
                if (data != null)
                    data.enabled = !data.enabled;
            }

            Rect refreshRect = new Rect(x + w + 4f, y, w, 26f);
            if (Widgets.ButtonText(refreshRect, "AWP_Refresh".Translate()))
                WorkAssigner.Refresh();
            TooltipHandler.TipRegion(refreshRect, "AWP_TipRefresh".Translate());

            Rect setRect = new Rect(x + (w + 4f) * 2f, y, w, 26f);
            if (Widgets.ButtonText(setRect, "AWP_Settings".Translate()))
                Find.WindowStack.Add(new Dialog_AWPSettings());
            TooltipHandler.TipRegion(setRect, "AWP_TipSettings".Translate());
        }
    }
}
