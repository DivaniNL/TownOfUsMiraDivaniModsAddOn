using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Buttons.Crewmate.CrewmatePower;
using DivaniMods.Buttons.Impostor.ImpostorKilling;
using DivaniMods.Buttons.Impostor.ImpostorSupport;
using DivaniMods.Buttons.Neutral.NeutralEvil;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using DivaniMods.Roles.Impostor.ImpostorKilling;
using DivaniMods.Roles.Impostor.ImpostorSupport;
using DivaniMods.Roles.Neutral.NeutralEvil;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Interfaces;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace DivaniMods.Modules;

public static class MageEnergize
{
    private static readonly List<bool> Pending = new();

    public static void ClearPending() => Pending.Clear();

    public static void QueuePending(bool isBuff)
    {
        Pending.Add(isBuff);
    }

    public static void ApplyPending()
    {
        if (Pending.Count == 0)
        {
            return;
        }

        var queued = Pending.ToList();
        Pending.Clear();

        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null || local.Data.IsDead)
        {
            return;
        }

        foreach (var isBuff in queued)
        {
            ApplyAndNotify(local, isBuff);
        }
    }

    public static void ApplyAfterDelay(PlayerControl target, bool isBuff, float delay)
    {
        Coroutines.Start(DelayedApply(target, isBuff, delay));
    }

    private static IEnumerator DelayedApply(PlayerControl target, bool isBuff, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (target == null || target.Data == null || target.Data.IsDead)
        {
            yield break;
        }

        ApplyAndNotify(target, isBuff);
    }

    public static void ApplyAndNotify(PlayerControl target, bool isBuff)
    {
        var ability = TryAdjustUses(target, isBuff);
        if (string.IsNullOrEmpty(ability))
        {
            return;
        }

        var msg = isBuff
            ? $"<b><color=#1586a2>The Mage has Energized you, giving you an additional {ability} use!</color></b>"
            : $"<b><color=#1586a2>The Mage has Energized you, zapping a {ability} use from you!</color></b>";

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            msg,
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.MageIcon.LoadAsset());
    }

    private static string? TryAdjustUses(PlayerControl target, bool isBuff)
    {
        var role = target.Data?.Role;
        if (role == null)
        {
            return null;
        }

        switch (role)
        {
            case PlagueDoctorRole:
            {
                if (!isBuff && PlagueDoctorRole.NumInfectionsRemaining <= 0)
                {
                    return null;
                }
                PlagueDoctorRole.NumInfectionsRemaining += isBuff ? 1 : -1;
                return ButtonName(CustomButtonSingleton<InfectButton>.Instance, "Infect");
            }

            case MosquitoRole:
            {
                var btn = CustomButtonSingleton<MosquitoStingButton>.Instance;
                if (btn == null || btn.MaxUses <= 0 || (!isBuff && btn.CurrentCharges <= 0))
                {
                    return null;
                }
                btn.CurrentCharges += isBuff ? 1 : -1;
                return ButtonName(btn, "Sting");
            }

            case DeadlockRole:
            {
                var btn = CustomButtonSingleton<LockdownButton>.Instance;
                if (btn == null || btn.MaxUses <= 0 || (!isBuff && btn.CurrentCharges <= 0))
                {
                    return null;
                }
                btn.CurrentCharges += isBuff ? 1 : -1;
                return ButtonName(btn, "Lockdown");
            }

            case VeteranRole vet:
            {
                var btn = CustomButtonSingleton<VeteranAlertButton>.Instance;
                if (!BumpButton(btn, isBuff))
                {
                    return null;
                }
                vet.Alerts += isBuff ? 1 : -1;
                return ButtonName(btn, "Alert");
            }

            case MageRole:
            {
                var btn = CustomButtonSingleton<MageSpellButton>.Instance;
                if (btn == null)
                {
                    return null;
                }

                var pool = new List<MageSpell>();
                if (IsAdjustable(btn.ShockShieldUsesLeft, isBuff)) pool.Add(MageSpell.ShockShield);
                if (IsAdjustable(btn.EnergizeUsesLeft, isBuff)) pool.Add(MageSpell.Energize);
                if (IsAdjustable(btn.IllusionUsesLeft, isBuff)) pool.Add(MageSpell.Illusion);
                if (pool.Count == 0)
                {
                    return null;
                }

                var pick = pool[UnityEngine.Random.RandomRangeInt(0, pool.Count)];
                var d = isBuff ? 1 : -1;
                switch (pick)
                {
                    case MageSpell.ShockShield: btn.ShockShieldUsesLeft += d; break;
                    case MageSpell.Energize: btn.EnergizeUsesLeft += d; break;
                    case MageSpell.Illusion: btn.IllusionUsesLeft += d; break;
                }

                btn.OverrideName(MageSpellButton.SpellNames[(int)btn.CurrentSpell]);
                return MageSpellButton.SpellNames[(int)pick];
            }

            case HerbalistRole:
            {
                var btn = CustomButtonSingleton<HerbalistAbilityHerbButton>.Instance;
                if (btn == null)
                {
                    return null;
                }

                var pool = new List<HerbAbilities>();
                if (IsAdjustable(btn.ExposeUsesLeft, isBuff)) pool.Add(HerbAbilities.Expose);
                if (IsAdjustable(btn.ConfuseUsesLeft, isBuff)) pool.Add(HerbAbilities.Confuse);
                if (IsAdjustable(btn.ProtectUsesLeft, isBuff)) pool.Add(HerbAbilities.Protect);
                if (pool.Count == 0)
                {
                    return null;
                }

                var pick = pool[UnityEngine.Random.RandomRangeInt(0, pool.Count)];
                var d = isBuff ? 1 : -1;
                switch (pick)
                {
                    case HerbAbilities.Expose: btn.ExposeUsesLeft += d; break;
                    case HerbAbilities.Confuse: btn.ConfuseUsesLeft += d; break;
                    case HerbAbilities.Protect: btn.ProtectUsesLeft += d; break;
                }

                btn.OverrideName(HerbalistAbilityHerbButton.ProtectionText[(int)btn.CurrentAbility]);
                return HerbalistAbilityHerbButton.ProtectionText[(int)pick];
            }
        }

        var names = new List<string>();
        foreach (var button in CustomButtonManager.Buttons)
        {
            if (button.Button == null || button.MaxUses <= 0 || !IsEnabledFor(button, role) || !IsRoleButton(button))
            {
                continue;
            }

            if (!BumpButton(button, isBuff))
            {
                continue;
            }

            var name = ButtonName(button, null);
            if (!string.IsNullOrEmpty(name) && !names.Contains(name!))
            {
                names.Add(name!);
            }
        }

        if (names.Count > 0)
        {
            return string.Join("/", names);
        }

        foreach (var mod in target.GetModifiers<BaseModifier>())
        {
            if (mod is IButtonModifier && TryAdjustModifierUses(mod, isBuff))
            {
                return mod.ModifierName;
            }
        }

        return null;
    }

    private static bool IsAdjustable(int uses, bool isBuff)
    {
        if (uses == -1 || uses == -2)
        {
            return false;
        }

        return isBuff || uses > 0;
    }

    private static string? ButtonName(CustomActionButton? button, string? fallback)
    {
        var name = button?.Name;
        return string.IsNullOrEmpty(name) ? fallback : name;
    }

    private static bool BumpButton(CustomActionButton? button, bool isBuff)
    {
        if (button == null || button.MaxUses <= 0 || (!isBuff && button.UsesLeft <= 0))
        {
            return false;
        }

        button.UsesLeft += isBuff ? 1 : -1;
        button.SetUses(button.UsesLeft);
        return true;
    }

    private static bool IsEnabledFor(CustomActionButton button, RoleBehaviour role)
    {
        try
        {
            return button.Enabled(role);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsRoleButton(CustomActionButton button)
    {
        try
        {
            return !button.Enabled(null);
        }
        catch
        {
            return false;
        }
    }

    private static bool TryAdjustModifierUses(BaseModifier modifier, bool isBuff)
    {
        var type = modifier.GetType();
        foreach (var name in new[] { "UsesRemaining", "UsesLeft", "Uses" })
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || prop.PropertyType != typeof(int) || !prop.CanRead || !prop.CanWrite)
            {
                continue;
            }

            var value = (int)prop.GetValue(modifier)!;
            if (!isBuff && value <= 0)
            {
                return false;
            }

            prop.SetValue(modifier, value + (isBuff ? 1 : -1));
            return true;
        }

        return false;
    }
}
