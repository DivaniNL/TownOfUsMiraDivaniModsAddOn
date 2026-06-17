using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
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
    public override string Name => "Call UAV";
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
        OverrideName("UAV Active");
    }

    public override void OnEffectEnd()
    {
        var player = PlayerControl.LocalPlayer;
        if (player != null && player.HasModifier<UAVActiveModifier>())
        {
            player.RpcRemoveModifier<UAVActiveModifier>();
            RpcUavEnd(player);
        }

        OverrideName("Call UAV");
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
            ShowTimer(caller, "<b><color=#7CC85A>UAV Active</color></b>", duration);
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
            ShowTimer(caller, "<b><color=#7CC85A>Friendly UAV Active</color></b>", duration);
            if (opts.NotifyOthers)
            {
                Notify("<b><color=#7CC85A>Friendly UAV overhead</color></b>");
            }
        }
        else
        {
            // Enemies always see the timer and hear the sound.
            PlaySound(DivaniAssets.UavEnemySound.LoadAsset());
            ShowTimer(caller, "<b><color=#FF4040>Enemy UAV Active</color></b>", duration);
            if (opts.NotifyOthers)
            {
                Notify("<b><color=#FF4040>Enemy UAV overhead</color></b>");
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
            Notify("<b><color=#B37575>UAV signal lost</color></b>");
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
