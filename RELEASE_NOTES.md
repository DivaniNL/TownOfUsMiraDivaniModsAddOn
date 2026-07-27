# Divani Mods v1.3.4
Reworked roles, balance changes, bug fixes and more

> [!NOTE]
> This version is not guaranteed to work on Town of Us versions newer than 1.6.2

## Reworks

### Reworked Role: Opportunist (Neutral Outlier -> Neutral Evil)

As it ends games, it fits more as a Neutral Evil.
Also added some balancing changes to it:

- Wildcard option default value is now false
- New option: "Max Votes Collected Per Meeting". Default set to 5 votes. With big lobbies this will not make the Opportunist win that early anymore. For example, if the votes needed are set to 20 and max votes to 5, the earliest the Opportunist can win now is meeting 4 (if the stars align)

### Reworked Role: Recruiter (Added the hidden role: Recruit)

- Now changes the role of the recruited shipmate to a specific role (Vanilla imp -> Recruit)
This role is an Impostor Power
- This Recruit behaves the same as Traitor. Pick a role from 3 non-Imp Power roles.

### Reworked Role: Thief (Crewmate Power -> Neutral Killing)

Yeah okay, you guys were right. Thief is too evil as a Crewmate.

- Now has a kill button and an optional vent button.
- Can steal some Impostor modifiers and neutral modifiers (only Sniper at the moment)
- Won't be able to steal Crewpostor or Egotist anymore
- Removed the Pickpocket range setting

## General changes

- Fixed a bug where multiple Divani Mods Impostor modifiers could be assigned to one impostor (only 1 imp modifier in total is the standard in TOU)
- Fixed a bug where the Terminology Divani Mods symbols explanation could not be seen when used with other extension mods
- Added a tab to the settings part of the wiki which shows the state of the general settings of Divani Mods

## Bugfixes:

### Armored

- Fixed a bug where the Armored modifier was removed after the shield breaks and was not visible in the end game summary. Now only the hidden modifier is removed.

### Cupid

- Fixed a bug where a lover disconnecting caused the Cupid to change role or die.

### Duelist

- Fixed a bug which made Duelist not leave victorious correctly like Inquisitor

### Mole

- Fixed a bug where the Mole vents ignored actions done by the Plumber (Block, Flush)
- Fixed a bug where dead people could see the Mole Vent Button

### Retributionist

- Fixed a bug where a Vengeful Soul could see the game chat
- Fixed a bug where ambushing a Retributionist made the Ambusher invisible for the remainder of the round.
- Fixed a bug causing the Retributionist to start a revenge after being killed by the Hunter
- Fixed a bug which made Visual modifiers (Mini, Giant) not being re-applied after a revive (Successful revenge)
- Fixed a bug where winning the 1v1 as the Vengeful Soul would result in a Draw

### Watcher

- Fixed a bug where the initial charges were not applied correctly (no use at start). If I have to believe the code this could also happen to Mosquito and Deadlock. Now not anymore.
- Fixed a bug where Watcher plays gunshots on ghosts moving if "Ghostwalkers Must Freeze" is set to false

## Role/Modifier Changes:

### Armored

- Armored now resets the killer's buttons to the full timer instead of the short one.

### Duelist

- Duelist tie window reduced (0.15 -> 0.10)
- Duelers are excluded from others in PerfectComms (also Duelers can hear each other on the entire map)
- Duelist has a changed icon and role color
- Duelist can no longer Duel players holding the first death shield

### Frag

- Made Veterans on alert die to Frag now, as this can be abused
- Added a setting to make Cleric defuse arming and active Frags

### Innocent

- Innocent now always solo wins, no longer together with imps in the top 4. (rewarding to stay alive long)
- Innocent now makes their lover partner win after winning. (Do you want to take the risk??)
- Innocent now has a setting to make a killer go through your shield (Ruthless like)
- Innocent can no longer be assigned Armored and Memento modifiers
- Innocent target symbol is now also shown to people who die in the same round as the Innocent (QoL for the lovers case)
I know this could affect the game if a ghost sees this and gets revived, but that is fine. Let me know if you people want this reverted.

### Mage

- If killed by a Shock Shield, it will now display as "Zapped"
- Added "Crewmates" to the option for who can see the Shock Shield
- Added option to make interactions also be killed by the Shock Shield (like Veteran)

### Retributionist

- Vengeful Souls no longer hear dead players via PerfectComms
- Moved the revenge button to the left side of the screen (otherwise while spamming you could instantly report as these buttons sit at the same spot most of the time)
- The speed for the Vengeful Soul can now be set to a lower minimum speed (1.0 -> 0.9)
- The default setting for the Vengeful Soul speed is also lowered (1.05 -> 1.00)
- If a killer with a first death shield kills you, you will no longer be able to revenge on that person.

### Ruthless

- Removed the option to break through first death shield (this will no longer happen)

### Watcher

- Watcher Red Light kills no longer trigger Bait reports
- Watcher Red Light now only makes an alerted Veteran protect itself, not strike back

### Workhorse

- Crewpostor Workhorse now makes the Impostors win
- Egotist Workhorse generates a "Workhorse Win" containing the Ego Workhorse, all Impostors and Neutral Killers
