# Divani Mods v1.3.5 (Short release notes)

Support for Among Us:17.4 and Town of Us Mira:1.7.0 + Monster + Small fixes

> [!NOTE]
> This version does not work with Town of Us Mira versions lower than 1.7.0

## Added

### Added Role: Monster (Neutral Killing)

Eat nearby players to trap them in your belly. If you make it to the next meeting, everyone you've eaten is killed for real. If you die first, they're released instead.

## General changes

- Added Support for Among Us:17.4 and Town of Us Mira:1.7.0

## Bugfixes:

### Frag

- Fixed a bug where the first receiver of the Frag could pass it back to the origin.

### Sniper

- Fixed a visual bug where the cause of dead was always "Killed", even when killed by a Neutral Killing

### Retributionist

- Fixed a visual bug where the cause of dead was sometimes "Killed" after being killed.

### Plague Doctor

- Fixed a bug where the "can win" check only happened at meeting start, not at round start aswell.

## Role/Modifier Changes:

### Thief

- Made the maximum amount of stealable modifiers 15, default 5
- Made shy non-stealable

### Shuffle

- Shuffle now works better with players that cannot be interacted with
- Shuffle now shuffles Medusa stoned bodies if they are not fully stoned yet, after that they won't be moved.

### Plague Doctor

- Fallen crewmates during the round are no longer removed from the list, but their infection state is frozen now instead.

### Recruiter

- If Recruiter's target dies during the meeting it was chosen, the charge is refunded now.
- Added a button for the Recruiter to also change roles after a succesful recruit attempt

Full notes: <https://github.com/DivaniNL/TownOfUsMiraDivaniModsAddOn/releases/tag/v1.3.5>
