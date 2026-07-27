# Divani Mods v1.3.4 - Thief, Opportunist and Recruiter reworks

> [!NOTE]
> Not guaranteed to work on Town Of Us versions newer than 1.6.2.

[Download v1.3.4](https://github.com/DivaniNL/TownOfUsMiraDivaniModsAddOn/releases/tag/v1.3.4)
[Wiki](https://github.com/DivaniNL/TownOfUsMiraDivaniModsAddOn/wiki)

## Reworks

**Opportunist** (Neutral Outlier -> Neutral Evil)
It ends games, so it fits better as a Neutral Evil. Wildcard now defaults to off, and a new "Max Votes Collected Per Meeting" option (default 5) stops early wins in big lobbies.

**Recruiter** (added the hidden role: Recruit)
The recruited shipmate now becomes **Recruit** (Impostor Power) instead of a vanilla Impostor. Recruit works like Traitor: pick from 3 non-Impostor-Power roles.

**Thief** (Crewmate Power -> Neutral Killing)
You were right, Thief was too evil as a Crewmate. Now has a kill button and an optional vent button, can steal some Impostor and Neutral modifiers (only Sniper for now), can no longer steal Crewpostor or Egotist, and the Pickpocket range setting is gone.

## Bugfixes
- **Armored**: modifier is no longer removed when the shield breaks, so it shows in the end-game summary.
- **Cupid**: a lover disconnecting no longer makes Cupid change role or die.
- **Duelist**: now leaves victorious correctly, like Inquisitor.
- **Mole**: vents respect Plumber actions (Block, Flush); dead players no longer see the Mole Vent button.
- **Retributionist**: Vengeful Soul can no longer see game chat; ambushing no longer leaves the Ambusher invisible; no revenge after a Hunter kill; visual modifiers (Mini, Giant) re-apply after a revive; winning the 1v1 no longer ends in a Draw.
- **Watcher**: initial charges apply correctly (also fixed for Mosquito and Deadlock); no more gunshots on moving ghosts when "Ghostwalkers Must Freeze" is off.
- Only one Divani Mods Impostor modifier can be assigned per Impostor.
- The Terminology symbols explanation shows up alongside other extension mods.

## Changes
- **Armored**: now resets the killer's buttons to the full cooldown instead of the short one.
- **Duelist**: tie window 0.15s -> 0.10s; Duelers are excluded from others in PerfectComms and hear each other map-wide; new icon and role color; cannot duel players holding the first death shield.
- **Frag**: Veterans on alert now die to Frag; new setting to let Cleric defuse arming and active Frags.
- **Innocent**: always solo wins (no longer with Impostors in the top 4); makes their lover partner win too; new setting to break through shields (Ruthless-like); can no longer roll Armored or Memento; target symbol also shows to players who die in the same round.
- **Mage**: Shock Shield kills display as "Zapped"; added "Crewmates" to the Shock Shield visibility option; new option to make interactions die to the Shock Shield (like Veteran).
- **Retributionist**: Vengeful Souls no longer hear dead players via PerfectComms; revenge button moved to the left of the screen; Vengeful Soul speed minimum 1.0 -> 0.9 and default 1.05 -> 1.00; no revenge on a killer holding the first death shield.
- **Ruthless**: removed the option to break through the first death shield; it no longer happens.
- **Watcher**: Red Light kills no longer trigger Bait reports; an alerted Veteran only protects itself instead of striking back.
- **Workhorse**: Crewpostor Workhorse makes the Impostors win; Egotist Workhorse creates a "Workhorse Win" with the Ego Workhorse, all Impostors and Neutral Killers.
- Added a wiki settings tab showing the state of the general Divani Mods settings.

## Coming in 1.3.5
- Support for Town of Us Mira 1.7.0
- A setting to have a max amount of total modifiers
---

This is the short version. For the full release notes, with every option and detail per role, check the [v1.3.4 release page](https://github.com/DivaniNL/TownOfUsMiraDivaniModsAddOn/releases/tag/v1.3.4) or the [wiki](https://github.com/DivaniNL/TownOfUsMiraDivaniModsAddOn/wiki).
