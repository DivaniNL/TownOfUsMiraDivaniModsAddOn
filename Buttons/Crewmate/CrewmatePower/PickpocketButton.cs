using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Buttons.Neutral.NeutralKilling;
using DivaniMods.Options;
using DivaniMods.Patches;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using DivaniMods.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Events;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Localization;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace DivaniMods.Buttons.Crewmate.CrewmatePower;

public class PickpocketButton : TownOfUsTargetButton<PlayerControl>
{
    public override string Name => "Pickpocket";
    public override float Cooldown => OptionGroupSingleton<ThiefOptions>.Instance.PickpocketCooldown;
    public override float EffectDuration => 0f;
    public override int MaxUses => (int)OptionGroupSingleton<ThiefOptions>.Instance.MaxStolenModifiers;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.PickpocketButton;
    public override float Distance => OptionGroupSingleton<ThiefOptions>.Instance.PickpocketRange * 1.5f;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => new Color(0.5f, 0.3f, 0.1f);
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    
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
        
        // Bomb snatch is always allowed even at max stolen modifiers because
        // it doesn't consume a slot — keep the button live when a holder is nearby.
        if (!thief.CanStealMore)
        {
            var nearby = GetTarget();
            if (nearby == null || !FragBombState.IsHolder(nearby.PlayerId)) return false;
        }
        
        return base.CanUse();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || Target == null) return;
        
        if (player.Data.Role is not ThiefRole thief) return;
        
        // Frag bomb pickpocket: if the target is the current bomb holder, snatch
        // the bomb instead of stealing a modifier. Doesn't consume a stolen slot.
        if (FragBombState.IsHolder(Target.PlayerId))
        {
            FragBombState.PlayGivePassSoundLocal();
            FragBombButton.RpcPassBomb(player, player.PlayerId, Target.PlayerId, 0f, 0f);
            ResetTarget();
            return;
        }
        
        if (!thief.CanStealMore)
        {
            return;
        }
        
        var targetModifiers = GetTargetModifiers(Target);
        var random = new System.Random();
        
        if (targetModifiers.Count > 0)
        {
            var thiefHasButtonModifier = HasButtonModifier(player);
            var stolen = PickTargetModifier(targetModifiers, random, preferNonButtonModifier: thiefHasButtonModifier, thief: player);
            var canUseModifier = CanThiefUseModifier(stolen, player);
            
            // Pre-pick the fallback random id on the sender so every client applies the
            // same result. Without this, each client ran its own System.Random() and the
            // thief's end-screen modifier list could diverge from what they actually hold.
            uint fallbackRandomId = 0;
            if (!canUseModifier)
            {
                fallbackRandomId = PickRandomGivableId(player, random, allowButtonModifiers: !thiefHasButtonModifier);
            }
            
            RpcStealModifier(player, Target.PlayerId, stolen.TypeId, canUseModifier, fallbackRandomId);
        }
        else
        {
            var chosenId = PickRandomGivableId(player, random, allowButtonModifiers: !HasButtonModifier(player));
            RpcGiveRandomModifier(player, chosenId);
        }
        
        ResetTarget();
    }
    
    /// <summary>
    /// Sender-side pick so all clients apply the same modifier. Returns 0 when the thief
    /// already owns every givable modifier.
    /// </summary>
    private static BaseModifier PickTargetModifier(
        List<BaseModifier> targetModifiers,
        System.Random random,
        bool preferNonButtonModifier,
        PlayerControl thief)
    {
        var thiefModifierIds = thief.GetModifiers<BaseModifier>()
            .Select(m => m.TypeId)
            .ToHashSet();
        var nonDuplicateModifiers = targetModifiers
            .Where(m => !thiefModifierIds.Contains(m.TypeId))
            .ToList();
        var baseCandidates = nonDuplicateModifiers.Count > 0 ? nonDuplicateModifiers : targetModifiers;

        if (!preferNonButtonModifier)
        {
            return baseCandidates[random.Next(baseCandidates.Count)];
        }

        var nonButtonModifiers = baseCandidates.Where(x => !IsButtonModifier(x)).ToList();
        var candidates = nonButtonModifiers.Count > 0 ? nonButtonModifiers : baseCandidates;
        return candidates[random.Next(candidates.Count)];
    }

    private static uint PickRandomGivableId(PlayerControl thief, System.Random random, bool allowButtonModifiers)
    {
        var givableIds = GetGivableModifierIds(allowButtonModifiers);
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
            .Where(m => !IsExcludedFromStealing(m) &&
                        !IsNonStealableVisualModifier(m))
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
    
    private static bool CanThiefUseModifier(BaseModifier modifier, PlayerControl thief)
    {
        if (thief.GetModifiers<BaseModifier>().Any(m => m.TypeId == modifier.TypeId))
            return false;

        var modNamespace = modifier.GetType().Namespace;
        if (modNamespace != null && ExcludedNamespaces.Any(ns => modNamespace.StartsWith(ns, StringComparison.OrdinalIgnoreCase)))
            return false;
        
        // Reject faction-locked modifiers that belong to a side the Thief isn't on.
        // IsModifierValidOn is NOT usable here because TouGameModifier's base implementation
        // also encodes the "one TouGameModifier per player" assignment rule, which would
        // falsely reject Shuffle/etc. after the thief already holds one stolen modifier.
        if (!IsFactionValidForThief(modifier))
            return false;

        if (IsButtonModifier(modifier) && HasButtonModifier(thief))
            return false;
        
        // Cannot steal Lover when thief already a Lover (avoids double-pairing).
        if (modifier.GetType().Name == "LoverModifier" && thief.HasModifier<LoverModifier>())
            return false;
        
        return true;
    }

    private static bool HasButtonModifier(PlayerControl player)
    {
        return player.GetModifiers<BaseModifier>().Any(IsButtonModifier);
    }

    private static bool IsButtonModifier(BaseModifier modifier)
    {
        return modifier is IButtonModifier;
    }
    
    /// <summary>
    /// True when the modifier's <c>FactionType</c> fits a crew-aligned Thief. Namespaces alone
    /// don't cover modifiers like DoubleShot that live in the generic
    /// <c>TownOfUs.Modifiers.Game</c> namespace but are marked as AssailantUtility.
    /// Works across all three mods: TownOfUsMira uses <see cref="TouGameModifier"/> and
    /// <see cref="UniversalGameModifier"/>; TouMiraRolesExtension uses UniversalGameModifier
    /// plus TimedModifier/BaseModifier (which are already filtered by HideOnUi); DivaniMods
    /// uses TouGameModifier for alignment-specific modifiers and UniversalGameModifier for anyone.
    /// Both base classes expose a <c>ModifierFaction FactionType</c>.
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

    private static List<uint> GetGivableModifierIds(bool allowButtonModifiers)
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

            if (!allowButtonModifiers && IsButtonModifier(modifier))
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
        
        return result;
    }

    private static PlayerControl? GetShieldSourcePlayer(BaseModifier modifier)
    {
        var modType = modifier.GetType();
        
        var propertyNames = new[] { "Medic", "Cleric", "Mirrorcaster" };
        
        foreach (var propName in propertyNames)
        {
            var prop = modType.GetProperty(propName);
            if (prop != null)
            {
                try
                {
                    var value = prop.GetValue(modifier);
                    if (value is PlayerControl pc)
                    {
                        return pc;
                    }
                }
                catch (Exception ex)
                {
                    DivaniPlugin.Instance.Log.LogError($"GetShieldSourcePlayer: Error getting {propName}: {ex.Message}");
                }
            }
        }
        
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
        
        // Capture lover partner before remove.
        // TryGetModifier<LoverModifier> works on Il2Cpp wrappers (pattern match against
        // BaseModifier does not).
        PlayerControl? loverPartner = null;
        bool isStealingLover = false;
        if (target.TryGetModifier<LoverModifier>(out var existingLover) && existingLover.TypeId == modifierTypeId)
        {
            loverPartner = existingLover.OtherLover;
            isStealingLover = true;
        }
        
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
            // Lover path uses generic AddModifier<T> so we get the instance back to wire OtherLover
            // (uint AddModifier returns void; matches LoverModifier.RpcSetOtherLover pattern).
            if (isStealingLover && loverPartner != null)
            {
                var thiefLover = thief.AddModifier<LoverModifier>();
                if (thiefLover != null)
                {
                    thiefLover.OtherLover = loverPartner;
                    if (!thief.IsCrewmate() || !loverPartner.IsCrewmate())
                    {
                        thiefLover.ForceDisableTasks = true;
                    }
                }
                
                if (loverPartner.TryGetModifier<LoverModifier>(out var partnerLover))
                {
                    partnerLover.OtherLover = thief;
                    if (!thief.IsCrewmate() || !loverPartner.IsCrewmate())
                    {
                        partnerLover.ForceDisableTasks = true;
                    }
                }
                
                if (loverPartner == PlayerControl.LocalPlayer)
                {
                    MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                        $"<b><color=#FF66CC>You are now in love with {thief.Data.PlayerName}!</color></b>",
                        Color.white,
                        new Vector3(0f, 1f, -20f));
                }
            }
            else
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
            }
            
            if (thief.Data.Role is ThiefRole thiefRole)
            {
                thiefRole.StolenModifierIds.Add(modifierTypeId);
            }
            
            if (thief == PlayerControl.LocalPlayer)
            {
                var stolenMsg = isStealingLover && loverPartner != null
                    ? $"<b><color=#FF66CC>Stole Lover! You are now in love with {loverPartner.Data.PlayerName}!</color></b>"
                    : $"<b><color=#804D1A>Stole/Gained {displayName}!</color></b>";
                MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                    stolenMsg,
                    Color.white,
                    new Vector3(0f, 1f, -20f));
                
                ButtonRefresher.RefreshAllButtons();
            }
            
            
            // Heartbreak the old Lover (victim). Deferred via coroutine so the pair-swap
            // RPC body fully completes first; otherwise the kill cascade can fire while
            // victim still appears linked on some client and chain through to the thief.
            // Runs on EVERY client locally (no host gate) so each client spawns the body
            // and registers the death — same pattern as LoverEvents.PlayerDeathEventHandler.
            if (isStealingLover
                && OptionGroupSingleton<ThiefOptions>.Instance.StealingLoverHeartbreaksVictim
                && target != null
                && target.Data != null
                && !target.Data.IsDead)
            {
                Coroutines.Start(HeartbreakOldLoverCoroutine(target));
            }
        }
        else
        {
            if (isStealingLover && loverPartner != null && loverPartner.HasModifier<LoverModifier>())
            {
                loverPartner.RemoveModifier<LoverModifier>();
            }
            
            ApplyGivenModifier(thief, fallbackRandomId, prefix: "Stole/Gained");
        }
    }
    
    /// <summary>
    /// Waits a short delay so the Lover pair-swap (thief.AddModifier + partner.OtherLover swap)
    /// fully settles, then kills the old Lover with a Heartbroken cause on this client.
    /// Mirrors LoverEvents.PlayerDeathEventHandler pattern: every client runs this locally
    /// (no host gate) so each client spawns the body and shows the death.
    /// </summary>
    private static IEnumerator HeartbreakOldLoverCoroutine(PlayerControl victim)
    {
        yield return new WaitForSeconds(0.25f);
        
        if (victim == null || victim.Data == null || victim.Data.IsDead)
        {
            yield break;
        }
        
        
        var inMeeting = MeetingHud.Instance != null || ExileController.Instance != null;
        var heartbreakText = TouLocale.Get("DiedToHeartbreak");
        
        DeathHandlerModifier.UpdateDeathHandlerImmediate(
            victim,
            heartbreakText,
            DeathEventHandlers.CurrentRound,
            inMeeting ? DeathHandlerOverride.SetFalse : DeathHandlerOverride.SetTrue,
            lockInfo: DeathHandlerOverride.SetTrue);
        
        // UpdateDeathHandlerImmediate is async. CustomMurder must not run until CauseOfDeath
        // and LockInfo are written, or AfterMurderEventHandler overwrites with default "Killed"
        // on remote clients (only the victim's own client tended to win the race before).
        while (DeathHandlerModifier.IsAltCoroutineRunning)
        {
            yield return null;
        }
        
        if (victim.TryGetModifier<DeathHandlerModifier>(out var deathHandler))
        {
            deathHandler.CauseOfDeath = heartbreakText;
            deathHandler.RoundOfDeath = DeathEventHandlers.CurrentRound;
            deathHandler.DiedThisRound = !inMeeting;
            deathHandler.LockInfo = true;
        }
        
        if (inMeeting)
        {
            victim.Exiled();
        }
        else
        {
            var showAnim = MeetingHud.Instance == null && ExileController.Instance == null;
            var flags = MurderResultFlags.DecisionByHost | MurderResultFlags.Succeeded;
            victim.CustomMurder(victim, flags, false, showAnim, false, showAnim, false);
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
        
    }
}
