# [CAP] Automated Work Priorities — User guide

Requires **Harmony**. Load **after** Harmony, Biotech (if present), and **[FSF] Complex Jobs** (if you use it). Do **not** run with Automated Work Assignment.

On load the log should show: `[CAP] Automated Work Priorities loaded`

## Work tab

- **Auto: ON/OFF** (green / red) — stop writing priorities
- **Refresh** — assign now (also each new day if that option is on)
- **Clear pins** — unpin Shift-clicked cells
- **Settings**

**Shift-click** a priority cell to pin it so Refresh will not overwrite that pawn+job.

## Settings

**Jobs** — three tiers. Auto, Fill (# or %), Priority, Backup, Tier, Pawns (per-job exclude), Rules (hover).

**Rules** — grouped by job (All Jobs first, then Work-tab order). Ban / Only / Set / Prefer / Require.

**Presets** — Normal / Crisis / Construction plus your saves in `Config/CAP_AutomatedWorkPriorities/presets/` (cross-save).

**Pawns** — search; check = included in automation; adults first.

**Preview** — old → new priority and why.

## Tiers

1. Always high — Firefighter, Doctor, Childcare, Patient, Bed rest, Nurse, Surgeon
2. Passion — jobs with a skill (including Scan)
3. Everyone — no skill/passion (Basic, haul, clean, …)

## Tips

- **Only** = inverse Ban (e.g. Surgeon skill 14–20 Only)
- **Require** still gives others backup; use Ban/Only to take someone off a job
- Infants are never assigned
- Children can be banned from dangerous jobs by default rules
