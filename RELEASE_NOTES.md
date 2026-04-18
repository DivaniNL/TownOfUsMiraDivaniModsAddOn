# Divani Mods &mdash; v1.0.0

Initial public release of Divani Mods, a content pack for
[Town Of Us &ndash; Mira](https://github.com/AU-Avengers/TOU-Mira).

## New Roles

### Impostor
- **Deadlock** &mdash; Disables all crewmate tasks for a short window with a
  *Lockdown* ability. Gains additional lockdown charges per kill. Timer
  pauses during meetings and ejections, and is hidden from the HUD while a
  meeting is open.

### Crewmate
- **Thief** &mdash; Pickpockets nearby players to steal their modifiers (or
  gives them a random modifier if they have none). Stolen modifiers are
  fully synchronised and show up correctly in the end&ndash;game summary.
- **Portalmaker** &mdash; Places two portals on the map. Once both are
  placed, any living player can teleport between them.

### Neutral Killing
- **Plague Doctor** &mdash; Wins by infecting every living player. Has a
  direct *Infect* ability and passive infection spread between infected
  players and their neighbours. Plays a custom intro voice line and an
  infect SFX on ability use.

## New Modifiers

- **Blindspot** (Crewmate Utility) &mdash; Camera lights do not activate
  when this player uses cameras.
- **Fragile** (Universal) &mdash; Chance to break and die instantly when
  any other player interacts with you. The killer hears a glass&ndash;break
  sound effect. The break chance is shown inline in the in&ndash;game
  modifier description.
- **Ruthless** (Impostor Utility) &mdash; Kills by this Impostor bypass
  Medic shields, Guardian Angel protection, and Survivor vests.
- **Shuffle** (Universal) &mdash; Teleports every living player to a random
  other player's position. Dead players (ghosts) are never shuffled; dead
  bodies optionally are.

## Audio

- Added role intro sounds for **Thief**, **Portalmaker**, **Deadlock**, and
  **Plague Doctor**.
- Added an **Infect** SFX for the Plague Doctor ability.
- Added a glass&ndash;break SFX played on the killer's client when a
  Fragile player breaks.

## Fixes & Polish

- Fix Thief end&ndash;game summary showing the wrong set of modifiers
  compared to what the Thief actually has in&ndash;game. Random modifier
  selection is now deterministic across clients.
- Fix Lockdown timer counting down during meetings / ejections; it now
  pauses and is hidden from the HUD while a meeting or exile is active.
- Fix Shuffle teleporting dead / disconnected players. Ghosts are now
  excluded in both the sender and receiver code paths.
- Line&ndash;broke the long descriptions for the **Deadlock** and
  **Plague Doctor** roles so they render cleanly on the role screen.
- Tuned the **Plague Doctor** role icon size on the role screen (icon
  sprite ppu lowered so the 150&times;150 source matches the other
  role icons).

## Compatibility

- Requires Among Us (Steam).
- Requires [BepInEx IL2CPP](https://builds.bepinex.dev/projects/bepinex_be) 6.x.
- Requires [Reactor](https://github.com/NuclearPowered/Reactor),
  [MiraAPI](https://github.com/All-Of-Us-Mods/MiraAPI), and
  [TOU&ndash;Mira](https://github.com/AU-Avengers/TOU-Mira).
