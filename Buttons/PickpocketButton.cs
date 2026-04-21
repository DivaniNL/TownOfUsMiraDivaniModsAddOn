using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Patches;
using DivaniMods.Roles;
using DivaniMods.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace DivaniMods.Buttons;

public class PickpocketButton : CustomActionButton<PlayerControl>
{
    public override string Name => "Pickpocket";
    public override float Cooldown => OptionGroupSingleton<ThiefOptions>.Instance.PickpocketCooldown;
    public override float EffectDuration => 0f;
    public override int MaxUses => (int)OptionGroupSingleton<ThiefOptions>.Instance.MaxStolenModifiers;
    public override LoadableAsset<Sprite>? Sprite => DivaniAssets.ThiefButton;
    public override float Distance => OptionGroupSingleton<ThiefOptions>.Instance.PickpocketRange * 1.5f;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => new Color(0.5f, 0.3f, 0.1f);
    
    private static readonly HashSet<string> ExcludedNamespaces = new(StringComparer.OrdinalIgnoreCase)
    {
        "TownOfUs.Modifiers.Neutral",
        "TownOfUs.Modifiers.Impostor",
        "TownOfUs.Modifiers.Game.Neutral",
        "TownOfUs.Modifiers.Game.Impostor",
        "TownOfUs.Modifiers.Game.Alliance",
        "TownOfUs.Modifiers.HnsCrewmate",
        "TownOfUs.Modifiers.HnsImpostor",
        "TownOfUs.Modifiers.HnsGame"
    };
    
    

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is ThiefRole;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance, true);
    }

    public override void SetOutline(bool active)
    {
        if (Target == null) return;
        Target.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Color.yellow));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (target == null) return false;
        if (target.Data == null || target.Data.IsDead) return false;
        if (target == PlayerControl.LocalPlayer) return false;
        return true;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        if (player.Data.Role is not ThiefRole thief) return false;
        
        var usesLeft = thief.MaxStolenModifiers - thief.StolenModifierIds.Count;
        SetUses(usesLeft);
        
        if (!thief.CanStealMore) return false;
        
        return base.CanUse();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || Target == null) return;
        
        if (player.Data.Role is not ThiefRole thief) return;
        if (!thief.CanStealMore)
        {
            DivaniPlugin.Instance.Log.LogInfo("Thief: Already at max stolen modifiers!");
            return;
        }
        
        var targetModifiers = GetTargetModifiers(Target);
        var random = new System.Random();
        
        if (targetModifiers.Count > 0)
        {
            var stolen = targetModifiers[random.Next(targetModifiers.Count)];
            var canUseModifier = CanThiefUseModifier(stolen, player.Data.Role);
            
            // Pre-pick the fallback random id on the sender so every client applies the
            // same result. Without this, each client ran its own System.Random() and the
            // thief's end-screen modifier list could diverge from what they actually hold.
            uint fallbackRandomId = 0;
            if (!canUseModifier)
            {
                fallbackRandomId = PickRandomGivableId(player, random);
            }
            
            DivaniPlugin.Instance.Log.LogInfo($"Thief: Stealing {stolen.ModifierName} from {Target.Data.PlayerName} (can use: {canUseModifier}, fallback: {fallbackRandomId})");
            RpcStealModifier(player, Target.PlayerId, stolen.TypeId, canUseModifier, fallbackRandomId);
        }
        else
        {
            var chosenId = PickRandomGivableId(player, random);
            DivaniPlugin.Instance.Log.LogInfo($"Thief: No modifiers on {Target.Data.PlayerName}, giving random modifier id={chosenId}");
            RpcGiveRandomModifier(player, chosenId);
        }
        
        ResetTarget();
    }
    
    /// <summary>
    /// Sender-side pick so all clients apply the same modifier. Returns 0 when the thief
    /// already owns every givable modifier.
    /// </summary>
    private static uint PickRandomGivableId(PlayerControl thief, System.Random random)
    {
        var givableIds = GetGivableModifierIds();
        var existingIds = thief.GetModifiers<BaseModifier>()
            .Select(m => m.TypeId)
            .ToHashSet();
        var availableIds = givableIds.Where(id => !existingIds.Contains(id)).ToList();
        if (availableIds.Count == 0) return 0;
        return availableIds[random.Next(availableIds.Count)];
    }

    private static List<BaseModifier> GetTargetModifiers(PlayerControl target)
    {
        return target.GetModifiers<BaseModifier>()
            .Where(m => !IsExcludedFromStealing(m) && !IsNonStealableVisualModifier(m))
            .ToList();
    }

    private static bool IsExcludedFromStealing(BaseModifier modifier)
    {
        if (modifier is ExcludedGameModifier)
            return true;
        
        if (modifier.GetType().Name == "MagicMirrorModifier")
            return true;
        
        if (IsShieldModifier(modifier))
            return false;
        
        if (modifier.HideOnUi)
            return true;
        
        return false;
    }
    
    private static bool IsShieldModifier(BaseModifier modifier)
    {
        var type = modifier.GetType();
        while (type != null && type != typeof(object))
        {
            if (type.Name == "BaseShieldModifier")
                return true;
            type = type.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Il2CppInterop often uses wrapper types, so <c>is MedicShieldModifier</c> fails. Match by type name like Harmony/TOU patches do.
    /// </summary>
    private static bool IsMedicShieldModifierIl2Cpp(BaseModifier modifier)
    {
        var type = modifier.GetType();
        while (type != null && type != typeof(object))
        {
            if (type.Name == "MedicShieldModifier")
                return true;
            type = type.BaseType;
        }
        return false;
    }
    
    private static bool CanThiefUseModifier(BaseModifier modifier, RoleBehaviour? thiefRole)
    {
        var modNamespace = modifier.GetType().Namespace;
        if (modNamespace != null && ExcludedNamespaces.Any(ns => modNamespace.StartsWith(ns, StringComparison.OrdinalIgnoreCase)))
            return false;
        
        // Reject faction-locked modifiers that belong to a side the Thief isn't on.
        // IsModifierValidOn is NOT usable here because TouGameModifier's base implementation
        // also encodes the "one TouGameModifier per player" assignment rule, which would
        // falsely reject Shuffle/etc. after the thief already holds one stolen modifier.
        if (!IsFactionValidForThief(modifier))
            return false;
        
        return true;
    }
    
    /// <summary>
    /// True when the modifier's <c>FactionType</c> fits a crew-aligned Thief. Namespaces alone
    /// don't cover modifiers like DoubleShot that live in the generic
    /// <c>TownOfUs.Modifiers.Game</c> namespace but are marked as AssailantUtility.
    /// Works across all three mods: TownOfUsMira uses <see cref="TouGameModifier"/> and
    /// <see cref="UniversalGameModifier"/>; TouMiraRolesExtension uses UniversalGameModifier
    /// plus TimedModifier/BaseModifier (which are already filtered by HideOnUi); DivaniMods
    /// uses TouGameModifier. Both base classes expose a <c>ModifierFaction FactionType</c>.
    /// </summary>
    private static bool IsFactionValidForThief(BaseModifier modifier)
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

    private static bool IsVisualModifier(BaseModifier modifier)
    {
        return modifier is IVisualAppearance;
    }
    
    private static bool IsUniversalVisualModifier(BaseModifier modifier)
    {
        if (!(modifier is IVisualAppearance))
            return false;
        
        var modNamespace = modifier.GetType().Namespace;
        return modNamespace != null && modNamespace == "TownOfUs.Modifiers.Game.Universal";
    }
    
    private static bool IsNonStealableVisualModifier(BaseModifier modifier)
    {
        if (!(modifier is IVisualAppearance))
            return false;
        
        return !IsUniversalVisualModifier(modifier);
    }

    private static List<uint> GetGivableModifierIds()
    {
        var result = new List<uint>();
        
        foreach (var modifier in ModifierManager.Modifiers)
        {
            if (modifier is not GameModifier gm) continue;
            
            var modName = modifier.ModifierName;
            var modNamespace = modifier.GetType().Namespace ?? "null";
            var modTypeName = modifier.GetType().Name;
            
            if (modifier.HideOnUi)
                continue;
            
            if (modifier is ExcludedGameModifier)
                continue;
            
            if (IsShieldModifier(modifier))
                continue;
            
            if (modTypeName.StartsWith("Test", StringComparison.OrdinalIgnoreCase))
                continue;
            
            if (ExcludedNamespaces.Any(ns => modNamespace.StartsWith(ns, StringComparison.OrdinalIgnoreCase)))
                continue;
            
            if (modifier is IVisualAppearance)
                continue;
            
            // Skip faction-locked modifiers that aren't appropriate for a crew-aligned
            // Thief. Covers addon impostor modifiers (e.g. Ruthless in DivaniMods.*) and
            // TOU modifiers that sit in the generic TownOfUs.Modifiers.Game namespace but
            // are flagged with an impostor/assailant/neutral FactionType (e.g. DoubleShot).
            if (!IsFactionValidForThief(modifier))
                continue;
            
            var modType = modifier.GetType();
            var typeId = ModifierManager.GetModifierTypeId(modType);
            if (typeId.HasValue)
            {
                result.Add(typeId.Value);
            }
        }
        
        DivaniPlugin.Instance.Log.LogInfo($"Thief: Found {result.Count} givable modifiers");
        return result;
    }

    private static PlayerControl? GetShieldSourcePlayer(BaseModifier modifier)
    {
        var modType = modifier.GetType();
        DivaniPlugin.Instance.Log.LogInfo($"GetShieldSourcePlayer: Checking type {modType.Name}");
        
        var propertyNames = new[] { "Medic", "Cleric", "Mirrorcaster" };
        
        foreach (var propName in propertyNames)
        {
            var prop = modType.GetProperty(propName);
            if (prop != null)
            {
                DivaniPlugin.Instance.Log.LogInfo($"GetShieldSourcePlayer: Found property {propName}");
                try
                {
                    var value = prop.GetValue(modifier);
                    DivaniPlugin.Instance.Log.LogInfo($"GetShieldSourcePlayer: Value is {value?.GetType().Name ?? "null"}");
                    if (value is PlayerControl pc)
                    {
                        DivaniPlugin.Instance.Log.LogInfo($"GetShieldSourcePlayer: Returning player {pc.Data?.PlayerName ?? "unknown"}");
                        return pc;
                    }
                }
                catch (Exception ex)
                {
                    DivaniPlugin.Instance.Log.LogError($"GetShieldSourcePlayer: Error getting {propName}: {ex.Message}");
                }
            }
        }
        
        DivaniPlugin.Instance.Log.LogInfo("GetShieldSourcePlayer: No source player found");
        return null;
    }

    [MethodRpc((uint)DivaniRpcCalls.StealModifier)]
    public static void RpcStealModifier(PlayerControl thief, byte targetId, uint modifierTypeId, bool canUseModifier, uint fallbackRandomId)
    {
        var target = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p != null && p.PlayerId == targetId);
        
        if (target == null)
        {
            DivaniPlugin.Instance.Log.LogError("Thief RPC: Target not found!");
            return;
        }
        
        var modifier = target.GetModifiers<BaseModifier>()
            .FirstOrDefault(m => m.TypeId == modifierTypeId);
        
        if (modifier == null)
        {
            DivaniPlugin.Instance.Log.LogError("Thief RPC: Modifier not found on target!");
            return;
        }
        
        var modifierName = modifier.ModifierName;
        var modifierTypeName = modifier.GetType().Name;
        
        var displayName = modifierName;
        if (string.IsNullOrEmpty(displayName) || displayName == modifierTypeName)
        {
            displayName = modifierTypeName.Replace("Modifier", "");
        }
        
        var shieldSourcePlayer = GetShieldSourcePlayer(modifier);
        var wasMedicShield = IsMedicShieldModifierIl2Cpp(modifier);
        
        target.RemoveModifier(modifierTypeId, null);
        
        if (target == PlayerControl.LocalPlayer)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#804D1A>Your {displayName} modifier was stolen!</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f));
        }
        
        if (canUseModifier)
        {
            if (shieldSourcePlayer != null)
            {
                thief.AddModifier(modifierTypeId, shieldSourcePlayer);
            }
            else
            {
                thief.AddModifier(modifierTypeId);
            }
            
            if (wasMedicShield)
            {
                MedicShieldStolenPatch.ApplyStolenMedicShield(shieldSourcePlayer, thief);
            }
            
            if (thief.Data.Role is ThiefRole thiefRole)
            {
                thiefRole.StolenModifierIds.Add(modifierTypeId);
            }
            
            if (thief == PlayerControl.LocalPlayer)
            {
                MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                    $"<b><color=#804D1A>Stole/Gained {displayName}!</color></b>",
                    Color.white,
                    new Vector3(0f, 1f, -20f));
                
                ButtonRefresher.RefreshAllButtons();
            }
            
            DivaniPlugin.Instance.Log.LogInfo($"Thief: {thief.Data.PlayerName} stole {displayName} from {target.Data.PlayerName}");
        }
        else
        {
            DivaniPlugin.Instance.Log.LogInfo($"Thief: {thief.Data.PlayerName} destroyed {displayName} from {target.Data.PlayerName}, giving random modifier (id={fallbackRandomId}) instead");
            ApplyGivenModifier(thief, fallbackRandomId, prefix: "Stole/Gained");
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.GiveRandomModifier)]
    public static void RpcGiveRandomModifier(PlayerControl thief, uint chosenId)
    {
        ApplyGivenModifier(thief, chosenId, prefix: "Gained");
    }
    
    /// <summary>
    /// Applies a sender-chosen modifier id on every client so the end-screen / role tab
    /// view agrees with the thief's local stolen list.
    /// </summary>
    private static void ApplyGivenModifier(PlayerControl thief, uint chosenId, string prefix)
    {
        if (chosenId == 0)
        {
            DivaniPlugin.Instance.Log.LogWarning("Thief: No new modifiers available!");
            if (thief == PlayerControl.LocalPlayer)
            {
                MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                    $"<b><color=#804D1A>No new modifiers available!</color></b>",
                    Color.white,
                    new Vector3(0f, 1f, -20f));
            }
            return;
        }
        
        thief.AddModifier(chosenId);
        
        if (thief.Data.Role is ThiefRole thiefRole)
        {
            thiefRole.StolenModifierIds.Add(chosenId);
        }
        
        var modifierType = ModifierManager.GetModifierType(chosenId);
        var modifierTypeName = modifierType?.Name ?? "Unknown";
        
        var addedModifier = thief.GetModifiers<BaseModifier>()
            .FirstOrDefault(m => m.TypeId == chosenId);
        
        var displayName = modifierTypeName;
        if (addedModifier != null)
        {
            var modName = addedModifier.ModifierName;
            if (!string.IsNullOrEmpty(modName) && modName != modifierTypeName)
            {
                displayName = modName;
            }
            else
            {
                displayName = modifierTypeName.Replace("Modifier", "");
            }
        }
        else
        {
            displayName = modifierTypeName.Replace("Modifier", "");
        }
        
        if (thief == PlayerControl.LocalPlayer)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#804D1A>{prefix} {displayName}!</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f));
            
            ButtonRefresher.RefreshAllButtons();
        }
        
        DivaniPlugin.Instance.Log.LogInfo($"Thief: {thief.Data.PlayerName} gained modifier {displayName}");
    }
}
