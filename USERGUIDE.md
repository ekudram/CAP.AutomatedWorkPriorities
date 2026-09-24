# [CAP] Automated Work Priorities — User guide

RimWorld **1.6**. Requires **Harmony**. Load **after** Harmony, Biotech (if you have it), and **[FSF] Complex Jobs** (if you use it). Do **not** run with Automated Work Assignment — both write the same Work grid.

On load the log should show: `[CAP] Automated Work Priorities loaded`

The settings window remembers **position and size** in RimWorld ModSettings (not the colony save).

---

## Work tab

Three buttons (hover for tooltips):

| Button | What it does |
|--------|----------------|
| **Auto: ON/OFF** (green / red) | When ON, the mod writes priorities and **re-applies at dawn**. Same control as Settings → Auto. |
| **Refresh** | Apply Jobs, Rules, and the current preset **now**. |
| **Settings** | Open this window. |

---

## Settings header

| Button | What it does |
|--------|----------------|
| **Auto: ON/OFF** | Same as the Work tab Auto. Dawn refresh is included. |
| **Emergency: ON/OFF** | When ON, Firefighter, Doctor, Childcare, Patient, and Bed rest are forced to **P1** for capable pawns. **Ban / Only** still win. |

---

## Jobs

Search (magnifying glass) filters by job name, including Complex Jobs **labelShort** (Nurse, Surgeon — not `FSFNurse`).

Columns:

| Column | Meaning |
|--------|---------|
| **Job** | Work type |
| **Auto** | This job is managed |
| **Fill** | How many get **Priority**. `#` = count, `%` = share of eligible. Hover for the tip. Passion weight is the slider under Fill. |
| **Priority** | Work number for Fill slots. **1** first, **4** last, **0** off. Left click up, right click down. |
| **Backup** | Work number for everyone else. Same click rules. |
| **Tier** | T1 Always high, T2 Passion, T3 Everyone. Left next, right previous. |
| **Pawns** | Exclude people from **this job** only |
| **Rules** | Opens that job’s rules (includes **All Jobs** rules). **Add rule** creates a rule for this job. |

Tiers:

1. **Always high** — Firefighter, Doctor, Childcare, Patient, Bed rest, Nurse, Surgeon  
2. **Passion** — jobs with a skill (cooking, mining, Scan, …)  
3. **Everyone** — no skill/passion (Basic, haul, clean, refine, …)

---

## Rules

Grouped by job: **All Jobs** first, then Work-tab order (Firefighter, Patient, Doctor, Nurse, …). Search filters those groups.

**Add rule** opens the editor. Job picker: **All Jobs** is the first row (no extra All Jobs button).

### Who

**All**, **Colonist**, or **Slave**. Colonist = free colonists. Slave = colony slaves. All = both (default).

### Actions

| Action | Effect |
|--------|--------|
| **Ban** | Matching pawns get **0** (off the job) |
| **Only** | Pawns who **fail** the condition get 0 (inverse Ban). Example: Surgeon, skill 14–20, Only. |
| **Set** | Force a priority 0–4 |
| **Prefer** | Score bonus when filling slots |
| **Require** | Only matches fill **Fill** slots; others still get **Backup** |

Use Ban or Only to take someone off a job. Require does not.

### Conditions

Always, trait, gene, major / minor / any passion, skill at least, skill range, hediff, capacity below, lifestage (Child / Adult).

Trait, gene, hediff, and job lists use a **search** picker.

Shipped defaults (turn off if you disagree): Pyromaniac and Fire Terror off Firefighter; major passion prefer; Doctor medicine 8 Require; children banned from dangerous jobs.

**Infants** never appear on the Work tab and are never assigned.

---

## Presets

Built-in: **Normal**, **Crisis**, **Construction**. **Apply** copies into **this** colony.

**Save current as new preset** writes to:

`Config/CAP_AutomatedWorkPriorities/presets/`

That folder is **cross-save** (not inside the `.rws`). **Export** / **Import** use files in that folder.

Applying a built-in preset rebuilds every job row and clears tier overrides.

---

## Pawns

Search by name. Check = **included in automation**. Unchecked = excluded.

List: **adults first**, then children, A–Z. Slaves of the colony are listed. Babies are not.

---

## Preview

**Refresh** runs assignment and shows the change log on this tab (no extra window).

---

## Tips

- Work numbers: **1** is done first. **0** is disabled.  
- Complex Jobs: search **Nurse** / **Surgeon**, not defNames.  
- Do not stack with other mods that also `SetPriority` the Work grid.
