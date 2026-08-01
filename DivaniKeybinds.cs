using MiraAPI.Keybinds;
using Rewired;

namespace DivaniMods;

[RegisterCustomKeybinds]
public static class DivaniKeybinds
{
    public static MiraKeybind TeleportPortal1 { get; } = new("Teleport To Portal 1", KeyboardKeyCode.Alpha1);

    public static MiraKeybind TeleportPortal2 { get; } = new("Teleport To Portal 2", KeyboardKeyCode.Alpha2);

    public static MiraKeybind MarkPlayer { get; } = new("Mark Player", KeyboardKeyCode.N);
}
