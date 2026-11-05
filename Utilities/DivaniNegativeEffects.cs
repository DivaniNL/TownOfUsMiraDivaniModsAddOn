using System;
using System.Collections.Generic;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Crewmate;
using DivaniMods.Modifiers.Neutral.NeutralEvil;

namespace DivaniMods.Utilities;

public static class DivaniNegativeEffects
{
    private static readonly List<Action<PlayerControl>> Removers =
    [
        Remover<BloodyKillerFootstepsModifier>(),
        Remover<PlagueInfectedModifier>(),
    ];

    public static void CleanseAll(PlayerControl player)
    {
        if (player == null)
        {
            return;
        }

        foreach (var remove in Removers)
        {
            remove(player);
        }
    }

    private static Action<PlayerControl> Remover<T>()
        where T : BaseModifier
    {
        return player =>
        {
            if (player.HasModifier<T>())
            {
                player.RpcRemoveModifier<T>();
            }
        };
    }
}
