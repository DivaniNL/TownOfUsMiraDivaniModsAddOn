# Divani Mods

An [Among Us](https://store.steampowered.com/app/945360/Among_Us/) mod that adds
new roles and modifiers on top of [Town Of Us &ndash; Mira](https://github.com/AU-Avengers/TOU-Mira).

Divani Mods is built on [BepInEx](https://github.com/BepInEx/BepInEx),
[Reactor](https://github.com/NuclearPowered/Reactor) and
[MiraAPI](https://github.com/All-Of-Us-Mods/MiraAPI), and depends on
[TOU-Mira](https://github.com/AU-Avengers/TOU-Mira) at runtime.

---

## Contents

| **Impostor Roles**    | **Crewmate Roles**          | **Neutral Roles**            | **Modifiers**           |
| --------------------- | --------------------------- | ---------------------------- | ----------------------- |
| [Deadlock](#deadlock) | [Thief](#thief)             | [Plague Doctor](#plague-doctor) | [Blindspot](#blindspot) |
|                       | [Portalmaker](#portalmaker) |                              | [Fragile](#fragile)     |
|                       |                             |                              | [Ruthless](#ruthless)   |
|                       |                             |                              | [Shuffle](#shuffle)     |

---

## Installation

1. Install the [Steam version of Among Us](https://store.steampowered.com/app/945360/Among_Us/).
2. Install [BepInEx IL2CPP 6.x](https://builds.bepinex.dev/projects/bepinex_be) for Among Us.
3. Drop the following DLLs into `Among Us/BepInEx/plugins/`:
   - `Reactor.dll`
   - `MiraAPI.dll`
   - `TownOfUsMira.dll` (plus the bundled `touhats.bundle` + `touhats.catalog`)
   - `DivaniMods.dll` &larr; *this mod*
4. Launch Among Us. All Divani Mods options appear under a **Divani Mods** tab
   in the lobby options.

If you want the simplest install, grab the latest `DivaniMods-vX.Y.Z.zip`
from [Releases](../../releases), extract it, and copy the `BepInEx` folder
over your Among Us folder.

---

# Roles

## Crewmate Roles

### Thief

#### **Team: Crewmates**

The Thief is a Crewmate that can steal modifiers from other players.
Use your **Pickpocket** ability on a nearby player to either take one of their
modifiers, or &mdash; if they have none &mdash; give them a random modifier.
Stolen modifiers are applied to the Thief and appear in the end&ndash;game
summary.

#### Game Options

| Name                  | Description                                                   | Type       | Default |
| --------------------- | ------------------------------------------------------------- | ---------- | ------- |
| Thief                 | The percentage probability of the Thief appearing             | Percentage | 0%      |
| Max Stolen Modifiers  | How many modifiers the Thief can hold at once                 | Number     | 2       |
| Pickpocket Cooldown   | The cooldown on the Pickpocket ability                        | Time       | 25s     |
| Pickpocket Range      | The range of the Pickpocket ability                           | Multiplier | 1x      |

---

### Portalmaker

#### **Team: Crewmates**

The Portalmaker is a Crewmate that can place two portals on the map.
Once both portals are placed, **any player** (including the Portalmaker)
can use them to teleport between the two locations.

#### Game Options

| Name                  | Description                                             | Type | Default |
| --------------------- | ------------------------------------------------------- | ---- | ------- |
| Portalmaker           | The percentage probability of the Portalmaker appearing | Percentage | 0% |
| Place Portal Cooldown | The cooldown on the place-portal button                 | Time | 20s     |
| Use Portal Cooldown   | The cooldown on the use-portal button                   | Time | 10s     |

---

## Impostor Roles

### Deadlock

#### **Team: Impostors**

The Deadlock is an Impostor that can temporarily disable all crewmate tasks.
During a **Lockdown**, crewmates cannot access or complete any tasks.
The Deadlock can also use a vanilla kill button, and gains more charges by
killing.

#### Game Options

| Name              | Description                                         | Type   | Default |
| ----------------- | --------------------------------------------------- | ------ | ------- |
| Deadlock          | The percentage probability of the Deadlock appearing | Percentage | 0% |
| Lockdown Duration | How long a single lockdown lasts                    | Time   | 10s     |
| Lockdown Cooldown | The cooldown between lockdowns                      | Time   | 45s     |
| Initial Charges   | How many lockdown charges the Deadlock starts with  | Number | 1       |
| Charges Per Kill  | Bonus lockdown charges granted per successful kill  | Number | 1       |

---

## Neutral Roles

### Plague Doctor

#### **Team: Neutral (Killing)**

The Plague Doctor is a Neutral role that wins by infecting every living
player. Use the **Infect** ability to directly infect a nearby player, or
rely on **passive spread**: infected players continuously infect anyone who
stands near them for long enough. The Plague Doctor wins when every other
living player is infected.

#### Game Options

| Name                    | Description                                                              | Type       | Default |
| ----------------------- | ------------------------------------------------------------------------ | ---------- | ------- |
| Plague Doctor           | The percentage probability of the Plague Doctor appearing                | Percentage | 0%      |
| Infect Cooldown         | The cooldown on the Infect ability                                       | Time       | 25s     |
| Max Direct Infections   | How many players the Plague Doctor can infect directly with the ability  | Number     | 2       |
| Infection Distance      | Range for passive infection spread                                       | Multiplier | 1.5x    |
| Infection Duration      | Seconds of proximity needed to passively infect someone                  | Time       | 5s      |
| Post-Meeting Immunity   | Grace period after meetings where passive spread is paused               | Time       | 10s     |
| Infect Killer On Death  | Whether killing the Plague Doctor also infects the killer                | Toggle     | True    |
| Can Win While Dead      | Whether the Plague Doctor still wins if they die before all are infected | Toggle     | True    |
| Can Use Vents           | Whether the Plague Doctor can use vents                                  | Toggle     | False   |

---

# Modifiers

## Blindspot

### **Team: Crewmate Utility**

A Crewmate with the Blindspot modifier does not cause the red camera indicator
to light up when they view cameras. Useful for sneakier information gathering.

### Game Options

| Name             | Description                                                       | Type       | Default |
| ---------------- | ----------------------------------------------------------------- | ---------- | ------- |
| Blindspot Amount | How many Blindspot modifiers are assigned each game               | Number     | 1       |
| Blindspot Chance | Per&ndash;assignment chance (rolled per slot up to Amount)        | Percentage | 50%     |

---

## Fragile

### **Team: Universal**

A Fragile player has a chance to break (die instantly) if any other player
interacts with them. The *Chance to Break* option controls how often an
interaction actually kills them. When they break, the player who triggered
the death hears a glass&ndash;breaking sound effect.

### Game Options

| Name              | Description                                                             | Type       | Default |
| ----------------- | ----------------------------------------------------------------------- | ---------- | ------- |
| Fragile Amount    | How many Fragile modifiers are assigned each game                       | Number     | 0       |
| Fragile Chance    | Per&ndash;assignment chance (rolled per slot up to Amount)              | Percentage | 50%     |
| Chance to Break   | Chance that an interaction actually breaks the Fragile player           | Percentage | 100%    |

---

## Ruthless

### **Team: Impostor Utility**

A Ruthless Impostor's kills bypass Medic shields, Guardian Angel protection,
and Survivor vests.

### Game Options

| Name             | Description                                                       | Type       | Default |
| ---------------- | ----------------------------------------------------------------- | ---------- | ------- |
| Ruthless Amount  | How many Ruthless modifiers are assigned each game                | Number     | 0       |
| Ruthless Chance  | Per&ndash;assignment chance (rolled per slot up to Amount)        | Percentage | 50%     |

---

## Shuffle

### **Team: Universal**

A Shuffle player can press a button to **teleport every living player to a
random other player's position**. Great for chaos and escape plays. Ghosts
are excluded from the shuffle; dead bodies can optionally be shuffled too.

### Game Options

| Name                | Description                                                 | Type       | Default |
| ------------------- | ----------------------------------------------------------- | ---------- | ------- |
| Shuffle Amount      | How many Shuffle modifiers are assigned each game           | Number     | 1       |
| Shuffle Chance      | Per&ndash;assignment chance (rolled per slot up to Amount)  | Percentage | 50%     |
| Shuffle Uses        | How many times a Shuffle holder can press the button        | Number     | 1       |
| Shuffle Cooldown    | Cooldown between Shuffle button uses                        | Time       | 30s     |
| Shuffle Dead Bodies | Whether dead bodies also get shuffled                       | Toggle     | False   |

---

# Building from Source

Divani Mods targets `net6.0` and expects a local Among Us + BepInEx
installation so MSBuild can resolve the interop DLLs.

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download).
2. Install Among Us + BepInEx + TOU-Mira somewhere locally.
3. Open `DivaniMods.csproj` and update `AmongUsPath` to point at that folder.
4. Run:

   ```powershell
   dotnet build DivaniMods.csproj -c Release
   ```

   The output DLL is written to `bin/Release/net6.0/DivaniMods.dll` and
   also copied into `<AmongUsPath>/BepInEx/plugins/` by the `CopyToPlugins`
   build target.

To create a release package locally (zip + bare DLL), run:

```powershell
./scripts/Package.ps1 -Version 1.0.0
```

This produces `release/DivaniMods.dll` and
`release/DivaniMods-v1.0.0.zip` matching the structure uploaded to
GitHub releases.

---

# Credits

- Built on top of [TOU-Mira](https://github.com/AU-Avengers/TOU-Mira) by the
  AU-Avengers team.
- Uses [MiraAPI](https://github.com/All-Of-Us-Mods/MiraAPI) and
  [Reactor](https://github.com/NuclearPowered/Reactor).
- Glass&ndash;break SFX from [Freesound](https://freesound.org/) (community).
