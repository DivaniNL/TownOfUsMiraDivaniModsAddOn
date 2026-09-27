using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using MiraAPI.Translation;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Game.Universal;
using DivaniMods.Options;
using DivaniMods.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Buttons.Modifiers;

public sealed class UAVButton : TownOfUsButton
{
    public override string Name => MiraLocaleManager.Get("DivaniMods.Modifier.UAV.Ability.CallUAV");
    public override Color TextOutlineColor => UAVModifier.UavColor;
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<UAVOptions>.Instance.UavCooldown.Value + MapCooldown, 5f, 120f);

    public override float EffectDuration => OptionGroupSingleton<UAVOptions>.Instance.UavDuration.Value;

    public override int MaxUses => (int)OptionGroupSingleton<UAVOptions>.Instance.UavUses.Value;

    public override LoadableAsset<Sprite> Sprite => DivaniAssets.UavButton;

    // Uses are tracked on the modifier (not the base button) because modifier
    // buttons get recreated, which would otherwise reset the base use counter.
    public override bool ZeroIsInfinite { get; set; } = true;

    public override bool Enabled(RoleBehaviour? role)
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            return false;
        }

        return player.HasModifier<UAVModifier>();
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            return false;
        }

        var modifier = player.GetModifier<UAVModifier>();
        if (modifier == null)
        {
            SetUses(0);
            return false;
        }

        SetUses(modifier.UsesRemaining);
        if (modifier.UsesRemaining <= 0)
        {
            return false;
        }

        return base.CanUse();
    }

    protected override void OnClick()
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        var player = PlayerControl.LocalPlayer;
        if (player == null)
        {
            return;
        }

        var modifier = player.GetModifier<UAVModifier>();
        if (modifier == null || modifier.UsesRemaining <= 0)
        {
            return;
        }

        modifier.UsesRemaining--;

        player.RpcAddModifier<UAVActiveModifier>();
        RpcUavCall(player);
        OverrideName(MiraLocaleManager.Get("DivaniMods.Modifier.UAV.Button.Active"));
    }

    public override void OnEffectEnd()
    {
        var player = PlayerControl.LocalPlayer;
        if (player != null && player.HasModifier<UAVActiveModifier>())
        {
            player.RpcRemoveModifier<UAVActiveModifier>();
            RpcUavEnd(player);
        }

        OverrideName(Name);
    }

    [MethodRpc((uint)DivaniRpcCalls.UavCall)]
    public static void RpcUavCall(PlayerControl caller)
    {
        if (caller == null)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null)
        {
            return;
        }

        var opts = OptionGroupSingleton<UAVOptions>.Instance;
        var duration = opts.UavDuration.Value;

        if (local.PlayerId == caller.PlayerId)
        {
            PlaySound(DivaniAssets.UavFriendlySound.LoadAsset());
            ShowTimer(caller, $"<b><color=#7CC85A>{MiraLocaleManager.Get("DivaniMods.Modifier.UAV.Button.Active")}</color></b>", duration);
            return;
        }

        if (AreFriendly(caller, local))
        {
            // Friendlies only get the heads-up when the share-vision option is on.
            if (!opts.FriendliesShareVision)
            {
                return;
            }

            PlaySound(DivaniAssets.UavFriendlySound.LoadAsset());
            ShowTimer(caller, $"<b><color=#7CC85A>{MiraLocaleManager.Get("DivaniMods.Modifier.UAV.Notification.FriendlyActive")}</color></b>", duration);
            if (opts.NotifyOthers)
            {
                Notify($"<b><color=#7CC85A>{MiraLocaleManager.Get("DivaniMods.Modifier.UAV.Notification.FriendlyOverhead")}</color></b>");
            }
        }
        else
        {
            if (opts.NotifyOthers)
            {
                PlaySound(DivaniAssets.UavEnemySound.LoadAsset());
                ShowTimer(caller, $"<b><color=#FF4040>{MiraLocaleManager.Get("DivaniMods.Modifier.UAV.Notification.EnemyActive")}</color></b>", duration);
                Notify($"<b><color=#FF4040>{MiraLocaleManager.Get("DivaniMods.Modifier.UAV.Notification.EnemyOverhead")}</color></b>");
            }
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.UavEnd)]
    public static void RpcUavEnd(PlayerControl caller)
    {
        if (caller == null)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null)
        {
            return;
        }

        DivaniTimers.Remove(TimerId(caller));

        if (local.PlayerId == caller.PlayerId)
        {
            PlaySound(DivaniAssets.UavEndSound.LoadAsset());
            Notify($"<b><color=#B37575>{MiraLocaleManager.Get("DivaniMods.Modifier.UAV.Notification.SignalLost")}</color></b>");
        }
    }

    public static bool AreFriendly(PlayerControl a, PlayerControl b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        if (a.IsImpostorAligned() && b.IsImpostorAligned())
        {
            return true;
        }

        if (a.IsCrewmate() && b.IsCrewmate())
        {
            return true;
        }

        if (a.IsNeutral() && b.IsNeutral())
        {
            return a.Data?.Role?.GetType() == b.Data?.Role?.GetType();
        }

        return false;
    }

    private static string TimerId(PlayerControl caller) => $"uav_{caller.PlayerId}";

    private static void ShowTimer(PlayerControl caller, string titleRich, float seconds)
    {
        DivaniTimers.Set(
            TimerId(caller),
            titleRich,
            DivaniAssets.UavIcon.LoadAsset(),
            seconds,
            useLocalTimeDelta: true,
            priority: DivaniTimers.DefaultPriority);
    }

    private static void PlaySound(AudioClip? clip)
    {
        if (clip == null || !SoundManager.Instance)
        {
            return;
        }

        try
        {
            SoundManager.Instance.PlaySound(clip, false, 1f);
        }
        catch (System.Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning($"UAV: sfx failed: {ex.Message}");
        }
    }

    private static void Notify(string message)
    {
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            message,
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.UavIcon.LoadAsset());
    }
}
