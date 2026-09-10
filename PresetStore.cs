using System;
using System.Collections.Generic;
using System.IO;
using Verse;

namespace CAP.AutomatedWorkPriorities
{
    public static class PresetStore
    {
        public static List<WorkPreset> Library = new List<WorkPreset>();

        public static string Folder => Path.Combine(GenFilePaths.ConfigFolderPath, "CAP_AutomatedWorkPriorities", "presets");

        public static void Load()
        {
            Library.Clear();
            try
            {
                Directory.CreateDirectory(Folder);
                string[] files = Directory.GetFiles(Folder, "*.xml");
                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        WorkPreset p = null;
                        Scribe.loader.InitLoading(files[i]);
                        Scribe_Deep.Look(ref p, "preset");
                        Scribe.loader.FinalizeLoading();
                        if (p != null && !string.IsNullOrEmpty(p.id))
                            Library.Add(p);
                    }
                    catch (Exception e)
                    {
                        Log.Warning("[AWP] Failed to load preset " + files[i] + ": " + e.Message);
                        Scribe.ForceStop();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("[AWP] Preset load failed: " + e);
            }

            EnsureShipped("normal", "Normal");
            EnsureShipped("crisis", "Crisis");
            EnsureShipped("construction", "Construction");
        }

        private static void EnsureShipped(string id, string name)
        {
            if (FindById(id) == null)
                Library.Add(BuildShipped(id, name));
        }

        public static WorkPreset FindById(string id)
        {
            for (int i = 0; i < Library.Count; i++)
            {
                if (Library[i].id == id)
                    return Library[i];
            }
            return null;
        }

        public static void Save(WorkPreset preset)
        {
            if (preset == null || string.IsNullOrEmpty(preset.id))
                return;
            WriteFile(preset, Path.Combine(Folder, SafeFile(preset.id) + ".xml"));
            WorkPreset existing = FindById(preset.id);
            if (existing != null)
                Library.Remove(existing);
            Library.Add(preset);
        }

        public static void Export(WorkPreset preset, string path)
        {
            WriteFile(preset, path);
        }

        public static WorkPreset Import(string path)
        {
            WorkPreset p = null;
            try
            {
                Scribe.loader.InitLoading(path);
                Scribe_Deep.Look(ref p, "preset");
                Scribe.loader.FinalizeLoading();
            }
            catch (Exception e)
            {
                Log.Error("[AWP] Import failed: " + e);
                Scribe.ForceStop();
                return null;
            }
            if (p == null) return null;
            if (string.IsNullOrEmpty(p.id))
                p.id = "import-" + DateTime.UtcNow.Ticks;
            p.shipped = false;
            Save(p);
            return p;
        }

        public static void Delete(WorkPreset preset)
        {
            if (preset == null || preset.shipped)
                return;
            Library.Remove(preset);
            string path = Path.Combine(Folder, SafeFile(preset.id) + ".xml");
            if (File.Exists(path))
                File.Delete(path);
        }

        private static void WriteFile(WorkPreset preset, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Folder);
            try
            {
                Scribe.saver.InitSaving(path, "AWPPreset");
                WorkPreset p = preset;
                Scribe_Deep.Look(ref p, "preset");
                Scribe.saver.FinalizeSaving();
            }
            catch (Exception e)
            {
                Log.Error("[AWP] Failed to save preset: " + e);
                Scribe.ForceStop();
            }
        }

        private static string SafeFile(string id)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                id = id.Replace(c, '_');
            return id;
        }

        public static WorkPreset BuildShipped(string id, string name)
        {
            var p = new WorkPreset { id = id, name = name, shipped = true };
            p.rules.AddRange(DefaultRules.Create());
            List<WorkTypeDef> all = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            if (all != null)
            {
                for (int i = 0; i < all.Count; i++)
                    p.jobs.Add(WorkTypeConfig.DefaultFor(all[i], id));
            }
            return p;
        }

        public static string ExportPath(WorkPreset p)
        {
            return Path.Combine(Folder, "export-" + SafeFile(p.id) + ".xml");
        }
    }
}
