## Bugfixes:


### Cunctator

- Fixed a bug where killing a Retributionist would still create a body after the delay.

### Duelist

- Fixed a bug which allowed duelist to reach it's win threshold earlier than intended

### Retributionist

- Fixed a bug where a Lover Retributionist being killed by its Lover teammate caused the Lover state to break. Now Killing your Lover teammate Retributionist will dissalow the Retributionist to Revenge (because there is noone to revenge)

### Revenant

- Fixed a bug where Revenant was able to kill its impostor teammates. This can now only happen during Camouflaged Comms (if Kill anyone during Comms is enabled)


### UAV

- Fixed a bug where a UAV sees people in Duel. Duellers now also only only see it's opponent if the duel starts with an active UAV

## Role/Modifier Changes:

### Domesmith (Addition)

- Domesmith circles are now blue (no more confusion with Trapper)

### Plague Doctor (Nerf)

- Plague Doctor can now no longer become a Neutral Afterlife role if the setting to allow Plague Doctor to win while dead is enabled.

### Sentinel (Buff)

- Added a option to see the names of dead bodies if they were ever present in your room. THis also still tracks a body even if it was cleaned/dragged after (default: false) 

### Ruthless (Refactor/Rework)

Ruthless' code has been cleaned to work in a more generic way. This changes the following:
- Pestillence is no longer killed by Ruthless killers (Nerf)
- Shield sources (Medic role holder etc.) now get alerted by the flash, even though their target still dies (Nerf)
- Veteran (And Mage Shock Shield) now make sure both the Ruthless attacker, as the Alerted shipmate, die (Buff)
- Mirrorcaster can now unleash after a Ruthless impostor killed the Magic Mirrored player