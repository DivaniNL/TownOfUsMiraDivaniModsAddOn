using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles;
using System.Collections;
using UnityEngine;

namespace DivaniMods.Buttons;

public class LockdownButton : CustomActionButton
{
    public override string Name => "Lockdown";
    public override float Cooldown => OptionGroupSingleton<DeadlockOptions>.Instance.LockdownCooldown;
    public override float EffectDuration => OptionGroupSingleton<DeadlockOptions>.Instance.LockdownDuration;
    public override int MaxUses => (int)OptionGroupSingleton<DeadlockOptions>.Instance.InitialCharges;
    public override LoadableAsset<Sprite>? Sprite => DivaniAssets.LockdownButton;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => Palette.ImpostorRed;
    
    public static bool IsLockdownActive { get; private set; }
    public static float LockdownTimeRemaining { get; private set; }
    public static LockdownButton? Instance { get; private set; }
    
    public static int ChargesPerKill => (int)OptionGroupSingleton<DeadlockOptions>.Instance.ChargesPerKill;
    
    private int _currentCharges = -1;
    
    public int CurrentCharges
    {
        get
        {
            if (_currentCharges < 0)
            {
                _currentCharges = MaxUses;
            }
            return _currentCharges;
        }
        set
        {
            _currentCharges = value;
            SetUses(value);
        }
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        return role is DeadlockRole;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        
        SetUses(CurrentCharges);
        
        bool hasCharges = CurrentCharges > 0 || MaxUses == 0;
        return hasCharges && ((Timer <= 0 && !EffectActive) || (EffectActive && Timer <= EffectDuration - 2f));
    }
    
    public override bool IsEffectCancellable() => Timer <= EffectDuration - 2f;

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
        
        if (EffectActive)
        {
            Timer = Cooldown;
            EffectActive = false;
        }
        else if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            Timer = Cooldown;
        }
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;
        
        if (!EffectActive)
        {
            if (MaxUses > 0)
            {
                CurrentCharges--;
            }
            
            var duration = OptionGroupSingleton<DeadlockOptions>.Instance.LockdownDuration;
            DivaniPlugin.Instance.Log.LogInfo($"Deadlock: Starting lockdown for {duration} seconds (Charges: {CurrentCharges})");
            RpcStartLockdown(player, duration);
        }
        else
        {
            DivaniPlugin.Instance.Log.LogInfo("Deadlock: Ending lockdown early");
            RpcEndLockdown(player);
        }
    }

    public override void OnEffectEnd()
    {
        if (!IsLockdownActive) return;
        
        RpcEndLockdown(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)DivaniRpcCalls.StartLockdown)]
    public static void RpcStartLockdown(PlayerControl source, float duration)
    {
        DivaniPlugin.Instance.Log.LogInfo($"Deadlock RPC: Lockdown started by {source.Data.PlayerName} for {duration}s");
        StartLockdownLocal(duration);
    }
    
    [MethodRpc((uint)DivaniRpcCalls.EndLockdown)]
    public static void RpcEndLockdown(PlayerControl source)
    {
        DivaniPlugin.Instance.Log.LogInfo($"Deadlock RPC: Lockdown ended by {source.Data.PlayerName}");
        EndLockdownLocal();
    }

    private static void StartLockdownLocal(float duration)
    {
        IsLockdownActive = true;
        LockdownTimeRemaining = duration;
        
        KickPlayersFromTasks();
        
        Coroutines.Start(LockdownTimerCoroutine(duration));
        
        if (Instance != null)
        {
            Instance.OverrideName("UNLOCK");
        }
    }
    
    private static void EndLockdownLocal()
    {
        IsLockdownActive = false;
        LockdownTimeRemaining = 0;
        
        if (Instance != null)
        {
            Instance.OverrideName("LOCKDOWN");
            
            if (Instance.Button != null)
            {
                Coroutines.Start(ShakeButton(Instance.Button));
            }
        }
    }

    private static IEnumerator LockdownTimerCoroutine(float duration)
    {
        LockdownTimeRemaining = duration;
        
        while (LockdownTimeRemaining > 0 && IsLockdownActive)
        {
            // Pause countdown during meetings / ejection so lockdown resumes for
            // the remaining time once gameplay restarts.
            if (MeetingHud.Instance == null && ExileController.Instance == null)
            {
                LockdownTimeRemaining -= Time.deltaTime;
            }
            yield return null;
        }
        
        if (IsLockdownActive)
        {
            EndLockdownLocal();
            
            if (Instance != null)
            {
                Instance.Timer = Instance.Cooldown;
                Instance.EffectActive = false;
            }
        }
    }
    
    private static IEnumerator ShakeButton(ActionButton button)
    {
        var originalPosition = button.transform.localPosition;
        float elapsed = 0f;
        float shakeDuration = 0.3f;
        float shakeIntensity = 0.1f;
        
        while (elapsed < shakeDuration)
        {
            float x = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            float y = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            button.transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        button.transform.localPosition = originalPosition;
    }

    private static void KickPlayersFromTasks()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;
        
        if (localPlayer.Data.Role.IsImpostor) return;
        
        var minigame = Minigame.Instance;
        if (minigame != null)
        {
            DivaniPlugin.Instance.Log.LogInfo("Deadlock: Kicking player from current task");
            minigame.Close();
        }
    }

    public static void ResetLockdown()
    {
        IsLockdownActive = false;
        LockdownTimeRemaining = 0;
        Instance?.ResetCharges();
    }
    
    public void AddCharges(int amount)
    {
        if (amount <= 0) return;
        
        CurrentCharges += amount;
        DivaniPlugin.Instance.Log.LogInfo($"Deadlock: Added {amount} charge(s). Total: {CurrentCharges}");
    }
    
    public void ResetCharges()
    {
        _currentCharges = -1;
    }
}
