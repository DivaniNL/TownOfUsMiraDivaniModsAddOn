using System.Linq;
using System.Text;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Universal;
using DivaniMods.Options;
using DivaniMods.Patches;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Universal;

public sealed class MementoRevealModifier(RoleBehaviour role) : BaseRevealModifier
{
    public override string ModifierName => "Memento Revealed";

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    private static MementoRevealMode Mode =>
        (MementoRevealMode)OptionGroupSingleton<MementoOptions>.Instance.RevealMode.Value;

    private RoleBehaviour? RevealedRole
    {
        get
        {
            if (Player == null)
            {
                return role;
            }

            var fromDict = MementoModifier.ResolveRoleBeforeDeath(Player.PlayerId);
            if (fromDict != null)
            {
                return fromDict;
            }

            var fromHistory = Player.GetRoleWhenAlive();
            if (fromHistory != null)
            {
                return fromHistory;
            }

            return role;
        }
    }

    public override RoleBehaviour? ShownRole
    {
        get => RevealedRole;
        set { }
    }

    public override bool RevealRole
    {
        get => Mode == MementoRevealMode.Role;
        set { }
    }

    // Only living local viewers see the Memento reveal; the dead (incl. the holder
    // themselves) get the unmodified vanilla / TouMira display.
    public override bool Visible
    {
        get => PlayerControl.LocalPlayer != null && !PlayerControl.LocalPlayer.HasDied();
        set { }
    }

    public override string ExtraRoleText
    {
        get
        {
            var r = RevealedRole;
            if (r == null)
            {
                return string.Empty;
            }

            return Mode switch
            {
                MementoRevealMode.Alignment =>
                    $"{MementoPatch.FactionColor(r).ToTextColor()}{MementoPatch.AlignmentName(r)}</color>",
                MementoRevealMode.Faction =>
                    $"{MementoPatch.FactionColor(r).ToTextColor()}{MementoPatch.FactionName(r)}</color>",
                _ => string.Empty,
            };
        }
        set { }
    }

    public override string ExtraNameText
    {
        get => OptionGroupSingleton<MementoOptions>.Instance.ShowHeldModifiers.Value && Player != null
            ? HeldModifiersText(Player)
            : string.Empty;
        set { }
    }

    private static string HeldModifiersText(PlayerControl pc)
    {
        var modifiers = pc.GetModifiers<GameModifier>()
            .Where(x => x is not ExcludedGameModifier)
            .OrderBy(x => x.ModifierName)
            .ToList();

        var builder = new StringBuilder();
        var first = true;
        foreach (var modifier in modifiers)
        {
            builder.Append(first ? "\n<size=55%>(" : ", ");
            first = false;
            var color = MiscUtils.GetModifierColour(modifier);
            builder.Append($"{color.ToTextColor()}{modifier.ModifierName}</color>");
        }

        if (first)
        {
            return string.Empty;
        }

        builder.Append(")</size>");
        return builder.ToString();
    }
}
