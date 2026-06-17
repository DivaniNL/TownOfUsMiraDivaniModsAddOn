using DivaniMods.Networking.Crewmate.CrewmateKilling;
using DivaniMods.Roles.Crewmate.CrewmateAfterlife;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using TownOfUs.Assets;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DivaniMods.Modifiers.Crewmate.CrewmateKilling;

public sealed class RevengeTimerModifier(float time) : TimedModifier
{
    private Image? revengeBar;
    private TMPro.TextMeshProUGUI? revengeText;
    private GameObject? revengeUI;
    private float soundTimer = 1f;

    public override string ModifierName => "Revenge";
    public override float Duration => time;
    public override bool AutoStart => true;
    public override bool HideOnUi => true;
    public override bool RemoveOnComplete => true;

    public override string GetDescription()
    {
        var roundedTime = (int)System.Math.Round(System.Math.Max(TimeRemaining, 0), 0);
        var textColor = roundedTime switch
        {
            > 10 => Color.green,
            > 5 => Color.yellow,
            _ => Color.red
        };
        return $"{textColor.ToTextColor()}<size=80%>{roundedTime}s</size></color>";
    }

    public override void OnActivate()
    {
        base.OnActivate();

        if (!Player.AmOwner)
        {
            return;
        }

        revengeUI = Object.Instantiate(TouAssets.ScatterUI.LoadAsset(), HudManager.Instance.transform);
        revengeUI.transform.localPosition = new Vector3(-3.22f, 2.26f, -10f);
        revengeUI.SetActive(false);

        revengeText = revengeUI.transform.FindChild("ScatterCanvas").FindChild("ScatterText").gameObject
            .GetComponent<TMPro.TextMeshProUGUI>();
        revengeText.text = $"Revenge: {Duration}s";
        revengeText.gameObject.SetActive(false);

        revengeBar = revengeUI.transform.FindChild("ScatterCanvas").FindChild("ScatterBar").gameObject
            .GetComponent<Image>();
        revengeBar.fillAmount = 1f;

        var revengeIcon = revengeUI.transform.FindChild("ScatterCanvas").FindChild("ScatterIcon").gameObject
            .GetComponent<Image>();
        revengeIcon.sprite = Player.Data.Role.RoleIconSolid;
    }

    public override void OnMeetingStart()
    {
        TimeRemaining = Duration;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Player == null || !Player.AmOwner || revengeUI == null)
        {
            return;
        }

        if (!TimerActive || Player.Data.Role is not VengefulSoulRole || MeetingHud.Instance)
        {
            soundTimer = 1f;
            TimeRemaining = Duration;
            revengeUI.SetActive(false);
            revengeText?.gameObject.SetActive(false);
            return;
        }

        var roundedTime = (int)System.Math.Round(System.Math.Max(TimeRemaining, 0f), 0f);
        var textColor = roundedTime switch
        {
            > 10 => Color.green,
            > 5 => Color.yellow,
            _ => Color.red
        };

        if (revengeText != null)
        {
            revengeText.text = $"Revenge: {textColor.ToTextColor()}{roundedTime}s</color>";
        }

        if (revengeBar != null)
        {
            revengeBar.fillAmount = System.Math.Clamp(TimeRemaining / Duration, 0f, 1f);
            revengeBar.color = textColor;
        }

        if (roundedTime <= 11f)
        {
            soundTimer -= Time.fixedDeltaTime;
            if (soundTimer <= 0f)
            {
                var num = roundedTime / 10f;
                var pitch = 1.5f - num / 2f;
                SoundManager.Instance.PlaySoundImmediate(
                    GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideCountdownSFX, false, 1f, pitch,
                    SoundManager.Instance.SfxChannel);
                soundTimer = 1f;
            }
        }

        revengeUI.SetActive(true);
        revengeText?.gameObject.SetActive(true);
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        soundTimer = 1f;
        revengeText?.gameObject.SetActive(false);

        if (revengeUI != null)
        {
            revengeUI.SetActive(false);
            Object.Destroy(revengeUI);
        }
    }

    public override void OnTimerComplete()
    {
        if (Player != null && Player.AmOwner && Player.Data.Role is VengefulSoulRole)
        {
            RetributionistRpc.RpcRevengeFailed(Player);
        }
    }
}
