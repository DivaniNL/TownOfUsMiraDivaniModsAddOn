# Divani Mods v1.2.9
Cupid and Nullified join Divani Mods + Bugfixes and Quality of Life updates
## Added

### Added Role: Cupid (Neutral Benign)

Use Matchmake to make two people fall in love next round! You can Bestow your lovers to protect them once they are in love.
You will win if your lvoers survive in the end.
Cupid will change into a configured role or die after lover's death (setting)

### Added Modifier: Nullified: (Impostor Passive)

You are immune to kill debuffs (Bait, Frosty, Diseased, Bloody, Aftermath, Noisemaker, Bear Trap). If configures, this also silences a Celebrity meeting announcement

## Description changes

- Innocent's role description now explains what a Wildcard does
- Mosquito's role description now explains how it can be swatted

## Quality of Life Updates


### Demolitionist
- Now shows the completed and required succesful sabotages tally next to the role name for the role holder and dead shipmates.

### Duelist
- Now shows the current wins and losses, along side with the win requirement+loss threshold next to the role name for the role holder and dead shipmates

### Opportunist
- Now shows the gained and required votes tally next to the role name for the role holder and dead shipmates.

### Retributionist
- If the Retributionist can only revenge once, show a box or checkmarked to represent its revenge state: Revenge pending/Revenge complete

## Bugfixes:

### Demolitionist

- Fixed a bug which can cause a sabotage being started if the meeting is called during the arming stage of the sabotage
- Fixed a bug where a Impostor sabotage can be started if the sabotage is started during the arming stage of the sabotage. Now it will just cancel the Demolitionist sabotage if an impostor calls a sabotage during the arming stage of a Demolitionist sabotage

### Duelist

- Fixed a bug where a solo win did not trigger when it won a 1v1 which completed its requirement

### Memento

- Fixed a bug where a Memento's revealed role information stayed visible after being revived by a Time Lord

### Opportunist
- Fixed a bug where vote/tally tracking sometimes only worked on the host.

### Retributionist

- Fixed a bug where kills that happen during meeting triggered revenge (Frag, Assassain)
- Fixed a bug where if multiple revenges were allowed, the kill button would only be usable once. Fixed this by making it infinite use.
- Fixed a bug where a Retribuitionist which was revived due to timelord would not be able to Revenge anymore. Now this charge is refunded in this case.
- Fixed a bug where lover teammates were not revived when "Both Lovers Die And Revive Together" is on in the general Mira settings

### Sentinel

- Fixed a bug where it records activity if people are teleported to the spawn room (Cafetaria on Skeld)
- Fixed a bug which records activity of people already in the room when a beacon is placed

### Thief

- Fixed a bug where a lover Thief was able to break the Lovers state when stealing from it's lover teammate

## Role/Modifier Changes:

### Frag

- Made sure that a Frag holder only dies in meeting if the bomb is actually armed. This caused some confusion to some players.

### Incompetent

- Now should properly be unguessable by anyone

### Innocent

- Added a option to make the taunted killer not able to report the innocent's body

### Retributionist

- Made sure a Retributionist is no longer able to be assigned Postmortem modifiers 

### UAV

- If "Notify Players of UAV" is disabled, enemies were always alerted (intended). To avoid confusion with this setting, setting "Notify Players of UAV" to false also silences notifications for enemy teams.