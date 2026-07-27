# Divani Mods v1.3.4 (Short release notes)

## Reworks

### Reworked Role: Opportunist (Neutral Outlier -> Neutral Evil)

- New option: "Max Votes Collected Per Meeting". Default set to 5 votes. 

### Reworked Role: Recruiter (Added the hidden role: Recruit)

- Now changes the role of the recruited shipmate to a specific role (Vanilla imp -> Recruit)
- This Recruit behaves the same as Traitor. Pick a role from 3 non-Imp Power roles.

### Reworked Role: Thief (Crewmate Power -> Neutral Killing)

Yeah okay, you guys were right. Thief is too evil as a Crewmate.

## General changes

- Fixed a bug where multiple Divani Mods Impostor modifiers could be assigned to one impostor (only 1 imp modifier in total is the standard in TOU)

## Bugfixes:

### Duelist

- Fixed a bug which made Duelist not leave victorious correctly like Inquisitor

### Retributionist

- Fixed a bug where a Vengeful Soul could see the game chat
- Fixed a bug where ambushing a Retributionist made the Ambusher invisible for the remainder of the round.


### Watcher

- Fixed a bug where the initial charges were not applied correctly (no use at start).
- Fixed a bug where Watcher plays gunshots on ghosts moving if "Ghostwalkers Must Freeze" is set to false

## Role/Modifier Changes:


### Duelist

- Duelist tie window reduced (0.15 -> 0.10)

### Frag

- Made Veterans on alert die to Frag now, as this can be abused
- Added a setting to make Cleric defuse arming and active Frags


### Watcher

- Watcher Red Light kills no longer trigger Bait reports
- Watcher Red Light now only makes an alerted Veteran protect itself, not strike back

### Workhorse

- Crewpostor Workhorse now makes the Impostors win
- Egotist Workhorse generates a "Workhorse Win" containing the Ego Workhorse, all Impostors and Neutral Killers


## Coming in 1.3.5
- Support for Town of Us Mira 1.7.0
- A setting for a max amount of total modifiers

Full notes: <https://github.com/DivaniNL/TownOfUsMiraDivaniModsAddOn/releases/tag/v1.3.4>