using HarmonyLib;
using UnityEngine;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public class AWPModSettings : ModSettings
    {
        public float winX = -1f;
        public float winY = -1f;
        public float winW = 780f;
        public float winH = 630f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref winX, "winX", -1f);
            Scribe_Values.Look(ref winY, "winY", -1f);
            Scribe_Values.Look(ref winW, "winW", 780f);
            Scribe_Values.Look(ref winH, "winH", 630f);
        }

        public Rect ClampToScreen(Rect r)
        {
            float w = Mathf.Clamp(r.width, 640f, UI.screenWidth);
            float h = Mathf.Clamp(r.height, 400f, UI.screenHeight);
            float x = Mathf.Clamp(r.x, 0f, Mathf.Max(0f, UI.screenWidth - w));
            float y = Mathf.Clamp(r.y, 0f, Mathf.Max(0f, UI.screenHeight - h));
            return new Rect(x, y, w, h);
        }
    }

    public class AWPMod : Mod
    {
        public static AWPModSettings Settings;

        public AWPMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<AWPModSettings>();
            var harmony = new Harmony("captolamia.automatedworkpriorities");
            harmony.PatchAll();
            LongEventHandler.ExecuteWhenFinished(PresetStore.Load);
            Log.Message("[CAP] Automated Work Priorities loaded");
        }

        public override string SettingsCategory()
        {
            return "AWP_SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (Widgets.ButtonText(new Rect(inRect.x, inRect.y, 280f, 32f), "AWP_Settings".Translate()))
                Find.WindowStack.Add(new Dialog_AWPSettings());
        }
    }
}
