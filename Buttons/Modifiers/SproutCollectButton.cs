using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Game.Crewmate;
using DivaniMods.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace DivaniMods.Buttons.Modifiers;

public class SproutCollectButton : TownOfUsTargetButton<DeadBody>
{
    public override string Name => "Collect";
    public override float Cooldown => 0.001f;
    public override float EffectDuration => 0f;
    public override int MaxUses => 1;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.SproutCollectButton;
    public override Color TextOutlineColor => SproutModifier.SproutColor;
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    private static readonly HashSet<string> ExcludedNamespaces = new(StringComparer.OrdinalIgnoreCase)
    {
        "TownOfUs.Modifiers.Neutral",
        "TownOfUs.Modifiers.Impostor",
        "TownOfUs.Modifiers.Game.Neutral",
        "TownOfUs.Modifiers.Game.Impostor",
        "TownOfUs.Modifiers.HnsCrewmate",
        "TownOfUs.Modifiers.HnsImpostor",
        "TownOfUs.Modifiers.HnsGame"
    };

    public override bool Enabled(RoleBehaviour? role)
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        return player.HasModifier<SproutModifier>();
    }

    public override DeadBody? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);
    }

    public override bool IsTargetValid(DeadBody? target)
    {
        return target != null;
    }

    public override void SetOutline(bool active)
    {
        if (Target == null || PlayerControl.LocalPlayer.HasDied()) return;

        foreach (var renderer in Target.bodyRenderers)
        {
            renderer.SetOutline(active ? new Color?(TextOutlineColor) : null);
        }
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        if (!player.HasModifier<SproutModifier>()) return false;
        return base.CanUse();
    }

    protected override void OnClick()
    {
        if (MeetingHud.Instance || ExileController.Instance) return;

        var collector = PlayerControl.LocalPlayer;
        if (collector == null || Target == null) return;
        if (!collector.HasModifier<SproutModifier>()) return;

        var deadPlayer = MiscUtils.PlayerById(Target.ParentId);
        var random = new System.Random();

        uint chosenId = 0;
        uint sourceId = 0;
        var targetId = deadPlayer?.PlayerId ?? byte.MaxValue;
        var fromBody = false;

        if (deadPlayer != null)
        {
            var collectible = GetCollectibleModifierIds(deadPlayer, collector);
            if (collectible.Count > 0)
            {
                chosenId = collectible[random.Next(collectible.Count)];
                sourceId = chosenId;
                fromBody = true;
            }
            else
            {
                sourceId = PickDestroyableModifierId(deadPlayer, random);
            }
        }

        if (chosenId == 0)
        {
            chosenId = PickRandomGivableId(collector, random);
        }

        RpcSproutCollect(collector, chosenId, fromBody, targetId, sourceId);
    }

    private static List<uint> GetCollectibleModifierIds(PlayerControl deadPlayer, PlayerControl collector)
    {
        var collectorIds = collector.GetModifiers<BaseModifier>()
            .Select(m => m.TypeId)
            .ToHashSet();

        return deadPlayer.GetModifiers<BaseModifier>()
            .Where(m => !IsExcluded(m) &&
                        !IsLover(m) &&
                        IsFactionValidForCrew(m) &&
                        !collectorIds.Contains(m.TypeId))
            .Select(m => m.TypeId)
            .Distinct()
            .ToList();
    }

    private static uint PickDestroyableModifierId(PlayerControl deadPlayer, System.Random random)
    {
        var ids = deadPlayer.GetModifiers<BaseModifier>()
            .Where(m => m is GameModifier &&
                        !m.HideOnUi &&
                        !IsShieldModifier(m) &&
                        !IsLover(m) &&
                        !IsNonUniversalVisualModifier(m) &&
                        !(m is SproutModifier) &&
                        !m.GetType().Name.StartsWith("Test", StringComparison.OrdinalIgnoreCase))
            .Select(m => m.TypeId)
            .Distinct()
            .ToList();

        if (ids.Count == 0) return 0;
        return ids[random.Next(ids.Count)];
    }

    private static uint PickRandomGivableId(PlayerControl collector, System.Random random)
    {
        var existingIds = collector.GetModifiers<BaseModifier>()
            .Select(m => m.TypeId)
            .ToHashSet();

        var availableIds = new List<uint>();

        foreach (var modifier in ModifierManager.Modifiers)
        {
            if (modifier is not GameModifier) continue;
            if (modifier.HideOnUi) continue;
            if (modifier is ExcludedGameModifier) continue;
            if (modifier is AllianceGameModifier) continue;
            if (IsShieldModifier(modifier)) continue;
            if (modifier is IVisualAppearance) continue;
            if (IsLover(modifier)) continue;
            if (modifier.GetType().Name.StartsWith("Test", StringComparison.OrdinalIgnoreCase)) continue;

            var modNamespace = modifier.GetType().Namespace ?? "null";
            if (ExcludedNamespaces.Any(ns => modNamespace.StartsWith(ns, StringComparison.OrdinalIgnoreCase))) continue;
            if (!IsFactionValidForCrew(modifier)) continue;

            var typeId = ModifierManager.GetModifierTypeId(modifier.GetType());
            if (typeId.HasValue && !existingIds.Contains(typeId.Value))
            {
                availableIds.Add(typeId.Value);
            }
        }

        if (availableIds.Count == 0) return 0;
        return availableIds[random.Next(availableIds.Count)];
    }

    private static bool IsExcluded(BaseModifier modifier)
    {
        if (modifier is ExcludedGameModifier) return true;
        if (modifier.GetType().Name == "MagicMirrorModifier") return true;
        if (IsShieldModifier(modifier)) return true;
        if (modifier.HideOnUi) return true;
        if (IsNonUniversalVisualModifier(modifier)) return true;
        if (modifier.GetType().Name.StartsWith("Test", StringComparison.OrdinalIgnoreCase)) return true;

        var modNamespace = modifier.GetType().Namespace;
        if (modNamespace != null && ExcludedNamespaces.Any(ns => modNamespace.StartsWith(ns, StringComparison.OrdinalIgnoreCase)))
            return true;

        return false;
    }

    private static bool IsLover(BaseModifier modifier)
    {
        return modifier.GetType().Name == "LoverModifier";
    }

    private static bool IsShieldModifier(BaseModifier modifier)
    {
        var type = modifier.GetType();
        while (type != null && type != typeof(object))
        {
            if (type.Name == "BaseShieldModifier") return true;
            type = type.BaseType;
        }
        return false;
    }

    private static bool IsNonUniversalVisualModifier(BaseModifier modifier)
    {
        if (modifier is not IVisualAppearance) return false;
        return modifier.GetType().Namespace != "TownOfUs.Modifiers.Game.Universal";
    }

    private static bool IsFactionValidForCrew(BaseModifier modifier)
    {
        var factionName = modifier switch
        {
            TouGameModifier tgm => tgm.FactionType.ToString(),
            UniversalGameModifier ugm => ugm.FactionType.ToString(),
            _ => null,
        };

        if (factionName == null) return true;

        if (factionName.Contains("Impostor", StringComparison.OrdinalIgnoreCase)) return false;
        if (factionName.Contains("Assailant", StringComparison.OrdinalIgnoreCase)) return false;
        if (factionName.Contains("Neutral", StringComparison.OrdinalIgnoreCase)) return false;
        if (factionName.Contains("NonCrew", StringComparison.OrdinalIgnoreCase)) return false;
        if (factionName.Contains("Hider", StringComparison.OrdinalIgnoreCase)) return false;
        if (factionName.Contains("Seeker", StringComparison.OrdinalIgnoreCase)) return false;
        if (factionName.Contains("External", StringComparison.OrdinalIgnoreCase)) return false;

        return true;
    }

    [MethodRpc((uint)DivaniRpcCalls.SproutCollect)]
    public static void RpcSproutCollect(PlayerControl collector, uint chosenId, bool fromBody, byte targetId, uint sourceId)
    {
        if (collector == null) return;

        // Remove Sprout first so a button modifier can be gained without the
        // one-button-per-player rule rejecting it.
        collector.RemoveModifier<SproutModifier>();

        if (sourceId != 0 && targetId != byte.MaxValue)
        {
            MiscUtils.PlayerById(targetId)?.RemoveModifier(sourceId, null);
        }

        if (chosenId == 0)
        {
            if (collector == PlayerControl.LocalPlayer)
            {
                Notify("<b><color=#7CC85A>No new modifiers available!</color></b>");
            }
            return;
        }

        collector.AddModifier(chosenId);

        var modifierType = ModifierManager.GetModifierType(chosenId);
        var modifierTypeName = modifierType?.Name ?? "Unknown";

        var added = collector.GetModifiers<BaseModifier>().FirstOrDefault(m => m.TypeId == chosenId);
        var displayName = modifierTypeName.Replace("Modifier", string.Empty);
        if (added != null && !string.IsNullOrEmpty(added.ModifierName) && added.ModifierName != modifierTypeName)
        {
            displayName = added.ModifierName;
        }

        if (collector == PlayerControl.LocalPlayer)
        {
            var prefix = fromBody ? "Collected" : "Gained";
            Notify($"<b><color=#7CC85A>{prefix} {displayName}!</color></b>");
            ButtonRefresher.RefreshAllButtons();
        }
    }

    private static void Notify(string message)
    {
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            message,
            Color.white,
            new Vector3(0f, 1f, -20f));
    }
}
