using HarmonyLib;
using UnityEngine;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public class AWPMod : Mod
    {
        public AWPMod(ModContentPack content) : base(content)
        {
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
