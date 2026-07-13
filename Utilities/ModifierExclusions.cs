using System.Collections.Generic;
using System.Linq;
using MiraAPI.Modifiers;

namespace DivaniMods.Utilities;

public static class ModifierExclusions
{
    private static readonly (string A, string B)[] ExclusivePairs =
    {
        ("SkilledModifier", "IncompetentModifier"),
        ("TacticalInsertionModifier", "ImmovableModifier"),
        ("BountyModifier", "DiseasedModifier"), 
    };

    public static IEnumerable<string> GetConflictingNames(string modifierTypeName)
    {
        foreach (var (a, b) in ExclusivePairs)
        {
            if (a == modifierTypeName) yield return b;
            else if (b == modifierTypeName) yield return a;
        }
    }

    public static bool ConflictsWithOwned(PlayerControl player, BaseModifier modifier)
    {
        return ConflictsWithOwned(player, modifier.GetType().Name);
    }

    public static bool ConflictsWithOwned(PlayerControl player, uint modifierTypeId)
    {
        var type = ModifierManager.GetModifierType(modifierTypeId);
        return type != null && ConflictsWithOwned(player, type.Name);
    }

    private static bool ConflictsWithOwned(PlayerControl player, string modifierTypeName)
    {
        var conflicts = GetConflictingNames(modifierTypeName).ToHashSet();
        if (conflicts.Count == 0) return false;
        return player.GetModifiers<BaseModifier>().Any(m => conflicts.Contains(m.GetType().Name));
    }
}
