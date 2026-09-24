# Installation

## Requirements

- RimWorld **1.6**
- **Harmony** (`brrainz.harmony`)

## Load order

1. Harmony  
2. Biotech (if you have it)  
3. [FSF] Complex Jobs (if you use it)  
4. **[CAP] Automated Work Priorities**

## Do not stack

Do not run **Automated Work Assignment** at the same time. Both mods call `SetPriority` on the same grid.

## Build from source

Open `Source/CAP.AutomatedWorkPriorities.sln`. Release builds to:

`Mods/[CAP] Automated Work Priorities/1.6/Assemblies/CAP_AutomatedWorkPriorities.dll`
