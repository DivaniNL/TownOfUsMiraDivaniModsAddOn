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
using DivaniMods.Roles.Neutral.NeutralKilling;
using DivaniMods.Utilities;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Events;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Localization;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace DivaniMods.Buttons.Crewmate.CrewmatePower;

public class PickpocketButton : TownOfUsButton
{
    public override string Name => "Pickpocket";
    public override float Cooldown => OptionGroupSingleton<ThiefOptions>.Instance.PickpocketCooldown.Value;
    public override float EffectDuration => OptionGroupSingleton<ThiefOptions>.Instance.PickpocketDuration.Value;
    public override int MaxUses => (int)OptionGroupSingleton<ThiefOptions>.Instance.MaxStolenModifiers.Value;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.PickpocketButton;
    public float Distance => OptionGroupSingleton<ThiefOptions>.Instance.PickpocketRange.Value * 1.5f;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => new Color(0.5f, 0.3f, 0.1f);
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;

    private byte _capturedTargetId;
    private bool _stealFragBomb;
    private PlayerControl? _target;

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

    private static readonly string[] AllowedNamespacePrefixes =
    {
        "TownOfUs",
        "DivaniMods",
    };

    private static bool IsAllowedSource(BaseModifier modifier)
    {
        var ns = modifier.GetType().Namespace;
        if (ns == null) return false;
        return AllowedNamespacePrefixes.Any(p => ns.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }



    public override bool Enabled(RoleBehaviour? role)
    {
        return role is ThiefRole;
    }

    private PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance, true);
    }

    private void SetOutline(bool active)
    {
        if (_target == null) return;
        _target.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Color.yellow));
    }

    private static bool IsTargetValid(PlayerControl? target)
    {
        if (target == null) return false;
        if (target.Data == null || target.Data.IsDead) return false;
        if (target == PlayerControl.LocalPlayer) return false;
        return true;
    }

    private void ResetTarget()
    {
        SetOutline(false);
        _target = null;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        if (player.Data.Role is not ThiefRole thief) return false;

        if (MeetingHud.Instance || ExileController.Instance)
        {
            ResetTarget();
            return false;
        }

        var usesLeft = thief.MaxStolenModifiers - thief.StolenModifierIds.Count;
        SetUses(usesLeft);

        // Manual targeting (non-target button): pick closest, refresh outline each frame.
        var newTarget = GetTarget();
        if (newTarget != _target) SetOutline(false);
        _target = IsTargetValid(newTarget) ? newTarget : null;
        SetOutline(true);

        // Bomb snatch is always allowed even at max stolen modifiers because
        // it doesn't consume a slot — keep the button live when a holder is nearby.
        if (!thief.CanStealMore)
        {
            if (_target == null || !FragBombState.IsHolder(_target.PlayerId)) return false;
        }

        if (EffectActive) return false;

        return base.CanUse() && _target != null;
    }

    public override void ClickHandler()
    {
        if (!CanClick() || PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() ||
            PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return;
        }

        OnClick();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || _target == null) return;
        if (player.Data.Role is not ThiefRole thief) return;
        if (EffectActive) return;

        _stealFragBomb = FragBombState.IsHolder(_target.PlayerId);
        if (!_stealFragBomb && !thief.CanStealMore)
        {
            return;
        }

        _capturedTargetId = _target.PlayerId;
        var targetName = _target.Data?.PlayerName ?? "them";
        var delay = EffectDuration;

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#804D1A>Pickpocketing {targetName} in {delay:0.#}s...</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.ThiefIcon.LoadAsset());

        if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            OnEffectEnd();
        }
    }

    public override void OnEffectEnd()
    {
        var thief = PlayerControl.LocalPlayer;
        if (thief == null || thief.Data == null || thief.Data.IsDead)
        {
            ResetTarget();
            return;
        }

        // A meeting interrupting the steal cancels it — no steal resolves mid-meeting.
        if (MeetingHud.Instance || ExileController.Instance)
        {
            ResetTarget();
            return;
        }

        var target = GetPlayerById(_capturedTargetId);
        if (target == null || !IsTargetValid(target))
        {
            ResetTarget();
            return;
        }

        PerformSteal(thief, target);
        ResetTarget();
    }

    private static PlayerControl? GetPlayerById(byte id)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player != null && player.PlayerId == id)
            {
                return player;
            }
        }

        return null;
    }

    private void PerformSteal(PlayerControl thief, PlayerControl target)
    {
        if (_stealFragBomb)
        {
            FragBombState.PlayGivePassSoundLocal();
            FragBombButton.RpcPassBomb(thief, thief.PlayerId, target.PlayerId, 0f, 0f);
            RpcNotifyFragStolen(thief, target.PlayerId);
            return;
        }

        if (thief.Data?.Role is not ThiefRole thiefRole || !thiefRole.CanStealMore)
        {
            return;
        }

        var targetModifiers = GetTargetModifiers(target);

        if (thief.HasModifier<LoverModifier>())
        {
            targetModifiers = targetModifiers.Where(m => m is not LoverModifier).ToList();
        }

        var random = new System.Random();

        if (targetModifiers.Count > 0)
        {
            var thiefHasButtonModifier = HasButtonModifier(thief);
            var stolen = PickTargetModifier(targetModifiers, random, preferNonButtonModifier: thiefHasButtonModifier, thief: thief);
            var canUseModifier = CanThiefUseModifier(stolen, thief);

            uint fallbackRandomId = 0;
            if (!canUseModifier)
            {
                fallbackRandomId = PickRandomGivableId(thief, random, allowButtonModifiers: !thiefHasButtonModifier);
            }

            RpcStealModifier(thief, target.PlayerId, stolen.TypeId, canUseModifier, fallbackRandomId);
        }
        else
        {
            var chosenId = PickRandomGivableId(thief, random, allowButtonModifiers: !HasButtonModifier(thief));
            RpcGiveRandomModifier(thief, chosenId);
        }
    }
    
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
            .Where(m => IsAllowedSource(m) &&
                        !IsExcludedFromStealing(m) &&
                        !IsVisualModifier(m))
            .ToList();
    }

    private static bool IsExcludedFromStealing(BaseModifier modifier)
    {
        if (modifier is ExcludedGameModifier)
            return true;
        
        if (modifier.GetType().Name == "MagicMirrorModifier")
            return true;

        if (modifier.GetType().Name == "KnightedModifier")
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

        if (!IsAllowedSource(modifier))
            return false;

        var modNamespace = modifier.GetType().Namespace;
        if (modNamespace != null && ExcludedNamespaces.Any(ns => modNamespace.StartsWith(ns, StringComparison.OrdinalIgnoreCase)))
            return false;
        
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

            if (modifier is AllianceGameModifier)
                continue;

            if (modTypeName.StartsWith("Test", StringComparison.OrdinalIgnoreCase))
                continue;
            
            if (!IsAllowedSource(modifier))
                continue;

            if (ExcludedNamespaces.Any(ns => modNamespace.StartsWith(ns, StringComparison.OrdinalIgnoreCase)))
                continue;

            if (modifier is IVisualAppearance)
                continue;

            if (!allowButtonModifiers && IsButtonModifier(modifier))
                continue;
            
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
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.ThiefIcon.LoadAsset());
        }
        
        if (canUseModifier)
        {
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
                        TownOfUsColors.Lover,
                        new Vector3(0f, 1f, -20f),
                        spr: TouModifierIcons.Lover.LoadAsset());
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
                var stoleLover = isStealingLover && loverPartner != null;
                var stolenMsg = stoleLover
                    ? $"<b><color=#FF66CC>Stole Lover! You are now in love with {loverPartner!.Data.PlayerName}!</color></b>"
                    : $"<b><color=#804D1A>Stole/Gained {displayName}!</color></b>";
                MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                    stolenMsg,
                    stoleLover ? TownOfUsColors.Lover : Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: (stoleLover ? TouModifierIcons.Lover : DivaniAssets.ThiefIcon).LoadAsset());
                
                ButtonRefresher.RefreshAllButtons();
            }
            
            
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
    
    private static IEnumerator HeartbreakOldLoverCoroutine(PlayerControl victim)
    {
        yield return new WaitForSeconds(0.25f);
        
        if (victim == null || victim.Data == null || victim.Data.IsDead)
        {
            yield break;
        }
        
        
        var inMeeting = MeetingHud.Instance || ExileController.Instance;
        var heartbreakText = TouLocale.Get("DiedToHeartbreak");
        
        DeathHandlerModifier.UpdateDeathHandlerImmediate(
            victim,
            heartbreakText,
            DeathEventHandlers.CurrentRound,
            inMeeting ? DeathHandlerOverride.SetFalse : DeathHandlerOverride.SetTrue,
            lockInfo: DeathHandlerOverride.SetTrue);
        
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
            var showAnim = !MeetingHud.Instance && !ExileController.Instance;
            var flags = MurderResultFlags.DecisionByHost | MurderResultFlags.Succeeded;
            victim.CustomMurder(victim, flags, false, showAnim, false, showAnim, false);
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.FragStolenNotify)]
    public static void RpcNotifyFragStolen(PlayerControl thief, byte targetId)
    {
        if (thief == PlayerControl.LocalPlayer)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                "<b><color=#e8a87c>You stole the Frag!</color></b>",
                FragRole.FragColor,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.FragIcon.LoadAsset());
        }

        if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId == targetId)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                "<b><color=#e8a87c>Your Frag was stolen by the Thief!</color></b>",
                FragRole.FragColor,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.FragIcon.LoadAsset());
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.GiveRandomModifier)]
    public static void RpcGiveRandomModifier(PlayerControl thief, uint chosenId)
    {
        ApplyGivenModifier(thief, chosenId, prefix: "Stole/Gained");
    }
    
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
                    new Vector3(0f, 1f, -20f),
                    spr: DivaniAssets.ThiefIcon.LoadAsset());
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
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.ThiefIcon.LoadAsset());
            
            ButtonRefresher.RefreshAllButtons();
        }
        
    }
}
