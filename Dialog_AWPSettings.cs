using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public class Dialog_AWPSettings : Window
    {
        private enum Tab { Jobs, Rules, Presets, Pawns, Preview }

        private Tab tab = Tab.Jobs;
        private Vector2 scroll;
        private string newPresetName = "My preset";
        private WorkTier filterTier = WorkTier.AlwaysHigh;
        private string importName = "import.xml";
        private string pawnSearch = "";

        public override Vector2 InitialSize => new Vector2(1120f, 680f);

        public Dialog_AWPSettings()
        {
            doCloseX = true;
            absorbInputAroundWindow = false;
            draggable = true;
            resizeable = true;
            closeOnClickedOutside = false;
        }

        public override void PostOpen()
        {
            base.PostOpen();
            GameComponent_AWP data = GameComponent_AWP.Get;
            if (data != null)
                data.RebuildJobList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 32f), "AWP_WindowTitle".Translate());
            Text.Font = GameFont.Small;

            GameComponent_AWP data = GameComponent_AWP.Get;
            float y = 36f;
            if (data != null)
            {
                float col = inRect.width / 3f;
                Widgets.CheckboxLabeled(new Rect(0f, y, col - 8f, 24f), "AWP_EnableMod".Translate(), ref data.enabled);
                Widgets.CheckboxLabeled(new Rect(col, y, col - 8f, 24f), "AWP_DailyRefresh".Translate(), ref data.dailyRefresh);
                Rect emRect = new Rect(col * 2f, y, col - 8f, 24f);
                Widgets.CheckboxLabeled(emRect, "AWP_ForceEmergency".Translate(), ref data.forceEmergencyPriorities);
                TooltipHandler.TipRegion(emRect, "AWP_ForceEmergencyTip".Translate());
                y += 22f;
                Widgets.Label(new Rect(0f, y, inRect.width, 22f), "AWP_LastRefresh".Translate(data.lastRefreshChanged, data.lastRefreshLog ?? ""));
            }
            else
                Widgets.Label(new Rect(0f, y, inRect.width, 24f), "AWP_LoadSaveFirst".Translate());

            y += 26f;
            List<TabRecord> records = new List<TabRecord>
            {
                new TabRecord("AWP_TabJobs".Translate(), () => tab = Tab.Jobs, tab == Tab.Jobs),
                new TabRecord("AWP_TabRules".Translate(), () => tab = Tab.Rules, tab == Tab.Rules),
                new TabRecord("AWP_TabPresets".Translate(), () => tab = Tab.Presets, tab == Tab.Presets),
                new TabRecord("AWP_TabPawns".Translate(), () => tab = Tab.Pawns, tab == Tab.Pawns),
                new TabRecord("AWP_TabPreview".Translate(), () => tab = Tab.Preview, tab == Tab.Preview)
            };
            TabDrawer.DrawTabs(new Rect(0f, y + 28f, inRect.width, 1f), records);
            y += 36f;
            Rect body = new Rect(0f, y, inRect.width, inRect.height - y);
            switch (tab)
            {
                case Tab.Jobs: DrawJobs(body, data); break;
                case Tab.Rules: DrawRules(body, data); break;
                case Tab.Presets: DrawPresets(body, data); break;
                case Tab.Pawns: DrawPawns(body, data); break;
                default: DrawPreviewLog(body, data); break;
            }
        }

        private void DrawJobs(Rect rect, GameComponent_AWP data)
        {
            float btnW = (rect.width - 200f) / 3f;
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, btnW, 24f), WorkTierCatalog.TierLabel(WorkTier.AlwaysHigh)))
                filterTier = WorkTier.AlwaysHigh;
            if (Widgets.ButtonText(new Rect(rect.x + btnW + 4f, rect.y, btnW, 24f), WorkTierCatalog.TierLabel(WorkTier.Passion)))
                filterTier = WorkTier.Passion;
            if (Widgets.ButtonText(new Rect(rect.x + (btnW + 4f) * 2f, rect.y, btnW, 24f), WorkTierCatalog.TierLabel(WorkTier.Everyone)))
                filterTier = WorkTier.Everyone;
            if (data != null && Widgets.ButtonText(new Rect(rect.xMax - 170f, rect.y, 165f, 24f), "AWP_ClearPins".Translate(data.pinnedKeys.Count)))
                data.pinnedKeys.Clear();

            const float xJob = 0f;
            const float wJob = 130f;
            const float xAuto = 132f;
            const float wAuto = 40f;
            const float xFill = 176f;
            const float wFill = 200f;
            const float xPrio = 384f;
            const float wPrio = 56f;
            const float xBack = 444f;
            const float wBack = 56f;
            const float xTier = 504f;
            const float wTier = 40f;
            const float xPawns = 548f;
            const float wPawns = 64f;
            const float xRules = 618f;
            float wRules = rect.width - xRules - 24f;
            if (wRules < 80f) wRules = 80f;

            float headerY = rect.y + 28f;
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Widgets.DrawLineHorizontal(rect.x, headerY + 22f, rect.width - 16f);
            GUI.color = Color.white;
            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(xJob, headerY, wJob, 22f), "AWP_ColJob".Translate());
            Widgets.Label(new Rect(xAuto, headerY, wAuto, 22f), "AWP_ColAuto".Translate());
            Rect fillHead = new Rect(xFill, headerY, wFill, 22f);
            Widgets.Label(fillHead, "AWP_ColFill".Translate());
            TooltipHandler.TipRegion(fillHead, "AWP_ColFillTip".Translate());
            Widgets.Label(new Rect(xPrio, headerY, wPrio, 22f), "AWP_ColPriority".Translate());
            Widgets.Label(new Rect(xBack, headerY, wBack, 22f), "AWP_ColBackup".Translate());
            Widgets.Label(new Rect(xTier, headerY, wTier, 22f), "AWP_ColTier".Translate());
            Widgets.Label(new Rect(xPawns, headerY, wPawns, 22f), "AWP_ColPawns".Translate());
            Widgets.Label(new Rect(xRules, headerY, wRules, 22f), "AWP_ColRules".Translate());
            Text.Font = GameFont.Small;

            List<WorkTypeDef> defs = new List<WorkTypeDef>();
            List<WorkTypeDef> all = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            for (int i = 0; i < all.Count; i++)
            {
                if (WorkTierCatalog.GetTier(all[i]) == filterTier)
                    defs.Add(all[i]);
            }
            defs.Sort((a, b) => string.Compare(a.labelShort, b.labelShort, StringComparison.OrdinalIgnoreCase));

            const float rowH = 52f;
            Rect view = new Rect(0f, 0f, rect.width - 20f, defs.Count * rowH + 8f);
            Widgets.BeginScrollView(new Rect(rect.x, rect.y + 54f, rect.width, rect.height - 54f), ref scroll, view);
            float y = 0f;
            for (int i = 0; i < defs.Count; i++)
            {
                WorkTypeDef def = defs[i];
                Widgets.Label(new Rect(xJob, y, wJob, 24f), def.labelShort.CapitalizeFirst());
                if (data != null)
                {
                    WorkTypeConfig cfg = data.GetJob(def);
                    Widgets.Checkbox(new Vector2(xAuto + 8f, y + 3f), ref cfg.included);
                    if (Widgets.ButtonText(new Rect(xFill, y, 36f, 24f), cfg.usePercentage ? "%" : "#"))
                        cfg.usePercentage = !cfg.usePercentage;
                    if (cfg.usePercentage)
                    {
                        cfg.percentage = Widgets.HorizontalSlider(new Rect(xFill + 40f, y + 4f, 120f, 22f), cfg.percentage, 0f, 1f);
                        Widgets.Label(new Rect(xFill + 162f, y, 50f, 24f), ((int)(cfg.percentage * 100f)) + "%");
                    }
                    else
                    {
                        string buf = cfg.count.ToString();
                        Widgets.TextFieldNumeric(new Rect(xFill + 40f, y, 50f, 24f), ref cfg.count, ref buf, 0, 99);
                    }
                    if (Widgets.ButtonText(new Rect(xPrio, y, wPrio, 24f), "P" + cfg.primaryPriority))
                        cfg.primaryPriority = (cfg.primaryPriority + 1) % 5;
                    if (Widgets.ButtonText(new Rect(xBack, y, wBack, 24f), "B" + cfg.backupPriority))
                        cfg.backupPriority = (cfg.backupPriority + 1) % 5;
                    if (Widgets.ButtonText(new Rect(xTier, y, wTier, 24f), WorkTierCatalog.Short(WorkTierCatalog.GetTier(def))))
                    {
                        WorkTier next = WorkTierCatalog.Cycle(WorkTierCatalog.GetTier(def));
                        data.tierOverrides[def.defName] = (int)next;
                        cfg.overrideTier = (int)next;
                    }
                    if (Widgets.ButtonText(new Rect(xPawns, y, wPawns, 24f), "AWP_ExcludeShort".Translate()))
                        Find.WindowStack.Add(new Dialog_JobExclusions(def));
                    cfg.passionWeight = Widgets.HorizontalSlider(new Rect(xFill, y + 26f, 150f, 22f), cfg.passionWeight, 0f, 3f);
                    Widgets.Label(new Rect(xFill + 154f, y + 26f, 70f, 22f), cfg.passionWeight.ToString("0.0") + "x");
                    Rect rulesRect = new Rect(xRules, y, wRules, 48f);
                    string rulesText;
                    string rulesTip;
                    JobRulesSummary(data, def, out rulesText, out rulesTip);
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(rulesRect, rulesText);
                    Text.Font = GameFont.Small;
                    TooltipHandler.TipRegion(rulesRect, rulesTip);
                }
                y += rowH;
            }
            Widgets.EndScrollView();
        }

        private static void JobRulesSummary(GameComponent_AWP data, WorkTypeDef def, out string text, out string tip)
        {
            text = "—";
            tip = "AWP_NoRulesForJob".Translate();
            if (data == null || data.rules == null || def == null)
                return;
            int n = 0;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < data.rules.Count; i++)
            {
                AssignmentRule r = data.rules[i];
                if (r == null || !r.enabled || !r.AppliesTo(def))
                    continue;
                n++;
                if (sb.Length > 0)
                    sb.AppendLine();
                sb.Append(r.Summary());
            }
            if (n == 0)
                return;
            text = n == 1 ? "AWP_OneRule".Translate() : "AWP_NRules".Translate(n);
            tip = sb.ToString();
        }

        private void DrawRules(Rect rect, GameComponent_AWP data)
        {
            if (data == null)
            {
                Widgets.Label(rect, "AWP_NoGame".Translate());
                return;
            }
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, 140f, 28f), "AWP_AddRule".Translate()))
            {
                var r = new AssignmentRule
                {
                    id = "user-" + Rand.Int,
                    workTypeDefName = "Firefighter",
                    condition = RuleConditionKind.Trait,
                    conditionDefName = "Pyromaniac",
                    action = RuleActionKind.Ban
                };
                data.rules.Add(r);
                Find.WindowStack.Add(new Dialog_RuleEditor(r));
            }
            if (Widgets.ButtonText(new Rect(rect.x + 150f, rect.y, 140f, 28f), "AWP_TabPreview".Translate()))
                Find.WindowStack.Add(new Dialog_Preview());

            List<string> groupKeys = new List<string>();
            Dictionary<string, List<AssignmentRule>> groups = new Dictionary<string, List<AssignmentRule>>();
            List<AssignmentRule> allJobs = new List<AssignmentRule>();
            for (int i = 0; i < data.rules.Count; i++)
            {
                AssignmentRule r = data.rules[i];
                if (r.TargetsAllJobs())
                    allJobs.Add(r);
                else
                {
                    string key = r.workTypeDefName ?? "";
                    List<AssignmentRule> list;
                    if (!groups.TryGetValue(key, out list))
                    {
                        list = new List<AssignmentRule>();
                        groups[key] = list;
                    }
                    list.Add(r);
                }
            }

            List<WorkTypeDef> ordered = new List<WorkTypeDef>(DefDatabase<WorkTypeDef>.AllDefsListForReading);
            ordered.Sort((a, b) => b.naturalPriority.CompareTo(a.naturalPriority));

            float contentH = 8f;
            if (allJobs.Count > 0)
                contentH += 26f + allJobs.Count * 32f;
            for (int i = 0; i < ordered.Count; i++)
            {
                List<AssignmentRule> list;
                if (groups.TryGetValue(ordered[i].defName, out list) && list.Count > 0)
                    contentH += 26f + list.Count * 32f;
            }

            Rect view = new Rect(0f, 0f, rect.width - 20f, contentH);
            Widgets.BeginScrollView(new Rect(rect.x, rect.y + 34f, rect.width, rect.height - 34f), ref scroll, view);
            float y = 0f;
            AssignmentRule toDelete = null;
            if (allJobs.Count > 0)
                DrawRuleGroup(view.width, ref y, "AWP_AllJobs".Translate(), allJobs, ref toDelete);
            for (int i = 0; i < ordered.Count; i++)
            {
                List<AssignmentRule> list;
                if (!groups.TryGetValue(ordered[i].defName, out list) || list.Count == 0)
                    continue;
                string title = ordered[i].labelShort.CapitalizeFirst();
                DrawRuleGroup(view.width, ref y, title, list, ref toDelete);
            }
            Widgets.EndScrollView();
            if (toDelete != null)
                data.rules.Remove(toDelete);
        }

        private static void DrawRuleGroup(float width, ref float y, string title, List<AssignmentRule> rules, ref AssignmentRule toDelete)
        {
            Widgets.DrawHighlight(new Rect(0f, y, width, 24f));
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(4f, y, width - 8f, 24f), title);
            y += 26f;
            for (int i = 0; i < rules.Count; i++)
            {
                AssignmentRule r = rules[i];
                Widgets.CheckboxLabeled(new Rect(8f, y, 28f, 28f), "", ref r.enabled);
                Widgets.Label(new Rect(40f, y, width - 190f, 28f), r.Summary());
                if (Widgets.ButtonText(new Rect(width - 160f, y, 70f, 26f), "Edit"))
                    Find.WindowStack.Add(new Dialog_RuleEditor(r));
                if (Widgets.ButtonText(new Rect(width - 80f, y, 70f, 26f), "Delete"))
                    toDelete = r;
                y += 32f;
            }
        }

        private void DrawPresets(Rect rect, GameComponent_AWP data)
        {
            if (PresetStore.Library.Count == 0)
                PresetStore.Load();

            float y = rect.y;
            newPresetName = Widgets.TextField(new Rect(rect.x, y, 200f, 28f), newPresetName);
            if (Widgets.ButtonText(new Rect(rect.x + 210f, y, 200f, 28f), "AWP_SavePreset".Translate()))
            {
                string id = "user-" + DateTime.UtcNow.Ticks;
                WorkPreset p = WorkPreset.FromCurrentWorld(id, newPresetName);
                PresetStore.Save(p);
                Messages.Message("Saved preset " + p.name, MessageTypeDefOf.TaskCompletion, false);
            }
            importName = Widgets.TextField(new Rect(rect.x + 420f, y, 180f, 28f), importName);
            if (Widgets.ButtonText(new Rect(rect.x + 605f, y, 80f, 28f), "AWP_Import".Translate()))
            {
                string path = Path.Combine(PresetStore.Folder, importName);
                if (File.Exists(path))
                    PresetStore.Import(path);
                else
                    Messages.Message("File not found: " + path, MessageTypeDefOf.RejectInput, false);
            }
            y += 36f;

            Rect view = new Rect(0f, 0f, rect.width - 20f, PresetStore.Library.Count * 36f);
            Widgets.BeginScrollView(new Rect(rect.x, y, rect.width, rect.height - 40f), ref scroll, view);
            float ly = 0f;
            for (int i = 0; i < PresetStore.Library.Count; i++)
            {
                WorkPreset p = PresetStore.Library[i];
                Widgets.Label(new Rect(0f, ly, 180f, 28f), p.name + (p.shipped ? " (built-in)" : ""));
                if (data != null && Widgets.ButtonText(new Rect(185f, ly, 150f, 26f), "AWP_ApplyPreset".Translate()))
                {
                    if (p.jobs.Count == 0)
                        p = PresetStore.BuildShipped(p.id, p.name);
                    p.ApplyTo(data);
                    WorkAssigner.Refresh();
                    Messages.Message("Applied " + p.name, MessageTypeDefOf.TaskCompletion, false);
                }
                if (Widgets.ButtonText(new Rect(340f, ly, 80f, 26f), "AWP_Export".Translate()))
                {
                    string path = PresetStore.ExportPath(p);
                    PresetStore.Export(p, path);
                    Messages.Message(path, MessageTypeDefOf.TaskCompletion, false);
                }
                if (!p.shipped && Widgets.ButtonText(new Rect(425f, ly, 80f, 26f), "AWP_DeletePreset".Translate()))
                {
                    PresetStore.Delete(p);
                    break;
                }
                ly += 34f;
            }
            Widgets.EndScrollView();
        }

        private void DrawPawns(Rect rect, GameComponent_AWP data)
        {
            if (data == null || Find.CurrentMap == null)
            {
                Widgets.Label(rect, "AWP_NoGame".Translate());
                return;
            }
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, 180f, 26f), "AWP_ClearPins".Translate(data.pinnedKeys.Count)))
                data.pinnedKeys.Clear();
            pawnSearch = Widgets.TextField(new Rect(rect.x + 190f, rect.y, 280f, 26f), pawnSearch ?? "");

            List<Pawn> pawns = new List<Pawn>();
            List<Pawn> all = Find.CurrentMap.mapPawns.FreeColonistsSpawned;
            string f = pawnSearch == null ? "" : pawnSearch.Trim();
            for (int i = 0; i < all.Count; i++)
            {
                Pawn p = all[i];
                if (p == null) continue;
                if (p.DevelopmentalStage == DevelopmentalStage.Baby || p.DevelopmentalStage == DevelopmentalStage.Newborn)
                    continue;
                if (f.Length > 0)
                {
                    bool hit = p.LabelShort.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0
                        || p.Name.ToStringFull.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!hit)
                        continue;
                }
                pawns.Add(p);
            }
            pawns.Sort(ComparePawnsForTab);

            Rect view = new Rect(0f, 0f, rect.width - 20f, pawns.Count * 30f);
            Widgets.BeginScrollView(new Rect(rect.x, rect.y + 30f, rect.width, rect.height - 30f), ref scroll, view);
            float y = 0f;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                bool included = !data.IsPawnExcluded(p);
                int years = p.ageTracker != null ? p.ageTracker.AgeBiologicalYears : 0;
                string status = included ? "AWP_IncludedInAuto".Translate() : "AWP_ExcludedFromAuto".Translate();
                string label = p.LabelShortCap + " (" + p.DevelopmentalStage + ", " + years + ") — " + status;
                Widgets.CheckboxLabeled(new Rect(0f, y, view.width, 28f), label, ref included);
                data.SetPawnExcluded(p, !included);
                y += 30f;
            }
            Widgets.EndScrollView();
        }

        private static int ComparePawnsForTab(Pawn a, Pawn b)
        {
            int sa = a.DevelopmentalStage == DevelopmentalStage.Adult ? 0 : 1;
            int sb = b.DevelopmentalStage == DevelopmentalStage.Adult ? 0 : 1;
            int c = sa.CompareTo(sb);
            if (c != 0) return c;
            return string.Compare(a.LabelShort, b.LabelShort, StringComparison.OrdinalIgnoreCase);
        }

        private void DrawPreviewLog(Rect rect, GameComponent_AWP data)
        {
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, 180f, 28f), "AWP_OpenPreview".Translate()))
                Find.WindowStack.Add(new Dialog_Preview());
            string log = data != null ? data.lastRefreshLog : "";
            Widgets.TextArea(new Rect(rect.x, rect.y + 34f, rect.width, rect.height - 34f), log ?? "", true);
        }
    }
}
