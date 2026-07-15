# Divani Mods v1.3.3
Added Workhorse and Betrayer + Bugfixes

> [!NOTE]
> This version is not guaranteed to work on Town of Us versions newer than 1.6.2

## Added

### Added Role: Workhorse (Crewmate Power)

A crewmate that gets a set number of extra tasks after they completed their original list. Completing this total list can give the crew an instant Crewmate win, but killers are alerted once you get closer to being done.


### Added Modifier: Betrayer (Alliance, Impostor only)

An Impostor alliance modifier that turns on their own team. They win like a Neutral Killer: be the last killer standing. Impostors will gain information about the Betrayer's identity when only a set number of people are alive or if one impostor is killed when you have more than 2 impostor aligned people. Betrayers never win with the impostors together.

## General changes

- Ordered the modifiers in the settings better

## Bugfixes:

### Armored

- Attacking Armored did not always set the short cooldown after a failed murder attempt

### Cupid

- Fixed a bug which caused the game thinking a lover was dead when they were only provisional lovers, causing the Cupid to change roles.

### Duelist

- Fixed a bug which made some skins visible when duellers return to the ship.

### Retributionist

- Fixed a bug where Cursed killers could still hop in a vent. 
(I tested without taking 'Max players alive when vents disable' into account)



## Role/Modifier Changes:

### Duelist

- Duelist duels can now result in a tie (0.15s window). Dropped the kill protection after the first successful click. This was not the right way to go.

### Demolitionist

- Added a local setting which disables the alternating colors on the flashes and arrows.
- Added a better description about what "Consoles" are.


### Mage
- Egotist and Crewpostor Mage are now added. Energize behaves differently when holding one of those alliance modifiers:
*Crewpostor Mage energizes:*

Crew → nerf
Impostors → buff
Crewpostor/Egotist crewmates → buff
Neutral Killing / Neutral Evil → nerf
Neutral Benign → option (EnergizeNeutralBenign: Buff / Debuff / None)


*Egotist Mage energizes:*

Crew → nerf
Impostors → buff
Crewpostor/Egotist crewmates → buff
Neutral Killing / Neutral Evil → buff ← only diff vs Crewpostor
Neutral Benign → option

### Mole

- Who can enter mole vents. Default changed to include all players
- Enable the host to allow the mole from getting more vents after completing tasks
- Added a cooldown for mole vent usage and a set time anyone can be in a mole vent (only for roles that normally cannot vent + Mole itself)

### Thief and Sprout

- Random giving will now only include modifiers set enabled by the host

### Retributionist

- Added option for it to continue game (Default false)


### Portalmaker

- Added better placements for the portals (Not near tasks, consoles, doors, walls)
- Made sure the regular Use button will overrule the portal button ( No more stuck behind polus doors)
- Made sure clicking on the portal will use the portal
- Made the behaviour with pet button more stable
- Added a cooldown on the use portal button.