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
            float x = rect.xMax - 16f - w * 4f - 12f;
            float y = rect.y + 4f;

            bool on = data != null && data.enabled;
            string onLabel = on ? "AWP_ModOn".Translate() : "AWP_ModOff".Translate();
            Rect onRect = new Rect(x, y, w, 26f);
            Color old = GUI.color;
            GUI.color = on ? new Color(0.45f, 0.85f, 0.45f) : new Color(0.9f, 0.4f, 0.4f);
            bool clicked = Widgets.ButtonText(onRect, onLabel);
            GUI.color = old;
            if (clicked && data != null)
                data.enabled = !data.enabled;
            TooltipHandler.TipRegion(onRect, data == null ? "" : "AWP_LastRefresh".Translate(data.lastRefreshChanged, data.lastRefreshLog ?? ""));

            Rect refreshRect = new Rect(x + w + 4f, y, w, 26f);
            if (Widgets.ButtonText(refreshRect, "AWP_Refresh".Translate()))
                WorkAssigner.Refresh();
            TooltipHandler.TipRegion(refreshRect, data == null ? "" : "AWP_LastRefresh".Translate(data.lastRefreshChanged, data.lastRefreshLog ?? ""));

            int pins = data != null && data.pinnedKeys != null ? data.pinnedKeys.Count : 0;
            if (Widgets.ButtonText(new Rect(x + (w + 4f) * 2f, y, w, 26f), "AWP_ClearPins".Translate(pins)))
            {
                if (data != null)
                    data.pinnedKeys.Clear();
            }

            if (Widgets.ButtonText(new Rect(x + (w + 4f) * 3f, y, w, 26f), "AWP_Settings".Translate()))
                Find.WindowStack.Add(new Dialog_AWPSettings());
        }
    }

    [HarmonyPatch(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.SetPriority))]
    public static class Patch_SetPriority
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn_WorkSettings __instance, WorkTypeDef w, int priority)
        {
            try
            {
                if (WorkAssigner.IsRefreshing)
                    return;
                if (Current.ProgramState != ProgramState.Playing)
                    return;
                if (!(Find.UIRoot is UIRoot_Play))
                    return;
                if (!(Find.MainTabsRoot?.OpenTab?.TabWindow is MainTabWindow_Work))
                    return;
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                    return;
                GameComponent_AWP data = GameComponent_AWP.Get;
                if (data == null || !data.enabled)
                    return;
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                if (pawn == null || w == null)
                    return;
                if (!pawn.IsColonistPlayerControlled)
                    return;
                data.SetPinned(pawn, w, true);
            }
            catch
            {
            }
        }
    }
}
