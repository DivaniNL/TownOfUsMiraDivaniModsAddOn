# Divani Mods &mdash; v1.1.0

Divani Mods 1.1.0 adds new roles, new modifiers, keybind support, and several
gameplay fixes and polish updates for
[Town Of Us &ndash; Mira](https://github.com/AU-Avengers/TOU-Mira).

## Added

### Roles
- **Frag** (Impostor Killing) &mdash; Starts a hot-potato time bomb that can
  be passed between players before it explodes.
- **Silencer** (Impostor Killing) &mdash; Cuts meeting voting time with each
  kill, down to a configurable minimum voting time.
- **Innocent** (Neutral Evil) &mdash; Taunts another player into killing them,
  then wins if that killer is voted out in the next meeting.
- **Opportunist** (Neutral Outlier) &mdash; Collects votes from targets they
  vote for and wins after reaching the configured vote threshold.

### Modifiers
- **Bear Trap** (Crewmate Postmortem) &mdash; Freezes the killer after the
  Bear Trap holder dies and prevents them from reporting the body while frozen.
- **Sniper** (Neutral Killing modifier) &mdash; Lets Neutral Killing roles kill
  from farther away without teleporting to the target.

### Controls
- Added keybind support for Divani Mods buttons.

## Changes

- **Ruthless** now kills through First Death Shield.
- Fixed some task behaviours when **Lockdown** is called.
- **Misvote** now also counts a random vote if the player does not vote.
- **Immovable** now works with portals and **Shuffle**.
- **Shuffle** button is now hidden when dead.
- **Thief** no longer receives more than one button modifier.
- **Shuffle** is now correctly listed as a button modifier.
- Improved modifier options so they are more grouped, matching source
  TOU Mira more closely.
- Added icons from Atony (Mira Dev).

## Compatibility

- Requires Among Us (Steam).
- Requires [BepInEx IL2CPP](https://builds.bepinex.dev/projects/bepinex_be) 6.x.
- Requires [Reactor](https://github.com/NuclearPowered/Reactor),
  [MiraAPI](https://github.com/All-Of-Us-Mods/MiraAPI), and
  [TOU&ndash;Mira](https://github.com/AU-Avengers/TOU-Mira).
