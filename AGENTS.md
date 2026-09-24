# Automated Work Priorities — Grok Build notes (RimWorld 1.6)

Standalone Captolamia mod. **Not RICS.** Do not copy RICS Logger, `[RICS]` logs, delivery pipelines, or chat commands.

Assigns colonist work priorities from named presets and rules (Ban / Only / Set / Prefer / Require). Shift-click a Work cell to pin it.

## Trees

- Mod root: `Mods\[CAP] Automated Work Priorities\`
- `packageId`: `Captolamia.AutomatedWorkPriorities`
- Namespace: `CAP.AutomatedWorkPriorities`
- Harmony id: `captolamia.automatedworkpriorities` (`AWPMod` → `PatchAll`)
- DLL: `1.6/Assemblies/CAP_AutomatedWorkPriorities.dll` (csproj output; extra Harmony copies are deleted after build)
- Load after Harmony, Biotech, `[FSF] Complex Jobs`. Do **not** run with Automated Work Assignment.

## Version (ask first)

**Current work: 1.0.2 (Pre-Release).** Stay on 1.0.2 unless Captolamia says to bump.

Source of truth: parent `About/About.xml` `<modVersion>` (now `1.0.2`). There is **no** `VersionHistory.cs`.

Local notes (gitignored): `Source/VERSION_HISTORY.md`.

- **Ask** before changing the number.
- **Same version:** append short notes to `VERSION_HISTORY.md`.
- **New version:** update About `<modVersion>` and add a heading in `VERSION_HISTORY.md`. Date line is **Pre-Release** until a real ship date.

## Logging

Use `Log.Message` / `Log.Warning` / `Log.Error` with a `[CAP]` prefix (see `AWPMod`). No custom logger.

## Translations

Keyed XML is **not** `string.Format`. Use `{0}`, `{1}` — never `{0:N0}` / `{0:F1}` (colon is a grammar role and drops the value).

This mod’s keys live only at **mod root:** `Languages/English/Keyed/AWP_Keys.xml` (no Source Languages copy).

## Code

- Core: `WorkAssigner`, `GameComponent_AWP`, `PresetStore` (RimWorld Config, cross-save), `WorkPreset`, `AssignmentRule`, dialogs under `Dialog_*.cs`.
- Harmony: Prefix/Postfix first. Work tab buttons are a postfix on `MainTabWindow_Work.DoWindowContents`.
- Prefer vanilla work types / pawn work settings. Read 1.6 source: `...\RimWorld\Source\RIMWORLD 1.6\Assembly-CSharp\` (`Verse\`, `RimWorld\`). Defs: `...\RimWorld\Data\`.
- Null-check; do not crash the game. Match existing naming (`AWP_*` keys, short methods).
- No `dynamic` with Newtonsoft (not used here — do not add it).
