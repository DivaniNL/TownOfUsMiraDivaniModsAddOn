using System;
using System.Collections;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using TMPro;
using UnityEngine;

namespace DivaniMods.Patches;

/// <summary>
/// Keeps <see cref="MedicRole.Shielded"/> aligned with <see cref="MedicShieldModifier"/> holders and refreshes the role tab
/// (same flow as <see cref="TownOfUsEventHandlers.IntroEndEventHandler"/>).
/// </summary>
public static class MedicShieldStolenPatch
{
    private static bool s_afterMurderVictimWasLocalMedicShield;

    /// <summary>
    /// Ruthless (and any path that skips <c>RpcMedicShieldAttacked</c>) still clears the medic via <c>AfterMurder</c>,
    /// but the task/role tab text is not refreshed. Capture shielded victim before TOU clears <see cref="MedicRole.Shielded"/>.
    /// </summary>
    [RegisterEvent(-200)]
    public static void AfterMurderCaptureIfVictimWasOurShield(AfterMurderEvent evt)
    {
        s_afterMurderVictimWasLocalMedicShield = false;
        var victim = evt.Target;
        if (victim == null)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || !local.AmOwner || local.Data?.Role is not MedicRole medic)
        {
            return;
        }

        var shielded = medic.Shielded;
        if (shielded != null && shielded.PlayerId == victim.PlayerId)
        {
            s_afterMurderVictimWasLocalMedicShield = true;
        }
    }

    [RegisterEvent(200)]
    public static void AfterMurderRefreshMedicTabIfNeeded(AfterMurderEvent _)
    {
        if (!s_afterMurderVictimWasLocalMedicShield)
        {
            return;
        }

        s_afterMurderVictimWasLocalMedicShield = false;
        RebuildMedicShieldedFromModifiers();
        TryRefreshLocalMedicRoleTab();
        Coroutines.Start(CoDeferredSyncAndRefresh());
    }

    [RegisterEvent(1000)]
    public static void OnRoundStartResyncMedicShielded(RoundStartEvent _)
    {
        RebuildMedicShieldedFromModifiers();
        TryRefreshLocalMedicRoleTab();
        Coroutines.Start(CoDeferredSyncAndRefresh());
    }

    /// <summary>
    /// Called from Thief steal RPC after the Medic shield has been moved to the thief (all clients).
    /// </summary>
    public static void ApplyStolenMedicShield(PlayerControl? medicSourceFromModifier, PlayerControl thief)
    {
        if (thief == null)
        {
            return;
        }

        if (medicSourceFromModifier != null && medicSourceFromModifier.GetRole<MedicRole>() is { } medicRole)
        {
            medicRole.Shielded = thief;
            DivaniPlugin.Instance.Log.LogInfo(
                $"MedicShieldStolenPatch: Set Shielded -> {thief.Data?.PlayerName} (medic {medicSourceFromModifier.Data?.PlayerName})");
        }

        RebuildMedicShieldedFromModifiers();
        TryRefreshLocalMedicRoleTab();
        Coroutines.Start(CoDeferredSyncAndRefresh());
    }

    private static IEnumerator CoDeferredSyncAndRefresh()
    {
        yield return null;
        RebuildMedicShieldedFromModifiers();
        TryRefreshLocalMedicRoleTab();
        DivaniPlugin.Instance.Log.LogInfo("MedicShieldStolenPatch: Deferred sync + tab refresh");
    }

    public static void RebuildMedicShieldedFromModifiers()
    {
        try
        {
            var shields = ModifierUtils.GetActiveModifiers<MedicShieldModifier>().ToList();

            foreach (var medicRole in CustomRoleUtils.GetActiveRolesOfType<MedicRole>())
            {
                var medic = medicRole.Player;
                if (medic == null)
                {
                    continue;
                }

                var match = shields.FirstOrDefault(m =>
                    m.Medic != null && m.Medic.PlayerId == medic.PlayerId);

                medicRole.Shielded = match?.Player;
            }
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogError($"MedicShieldStolenPatch: Rebuild failed: {ex.Message}");
        }
    }

    private static void TryRefreshLocalMedicRoleTab()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || !local.AmOwner || local.Data?.Role is not MedicRole medicRole)
        {
            return;
        }

        RefreshRoleTabLikeIntroEnd(medicRole);
    }

    private static void RefreshRoleTabLikeIntroEnd(MedicRole medicRole)
    {
        try
        {
            if (!HudManager.InstanceExists || HudManager.Instance == null)
            {
                return;
            }

            var hud = HudManager.Instance;
            hud.SetHudActive(false);
            hud.SetHudActive(true);

            if (hud.TaskStuff == null)
            {
                return;
            }

            var panel = TownOfUsEventHandlers.TryGetRoleTab();
            var local = PlayerControl.LocalPlayer;
            if (local?.Data?.Role is not ICustomRole roleAsCustom || panel == null)
            {
                return;
            }

            panel.open = true;

            var tabHeader = panel.tab != null
                ? panel.tab.gameObject.GetComponentInChildren<TextMeshPro>()
                : null;

            var taskPanelTransform = hud.TaskStuff.transform.FindChild("TaskPanel");
            if (taskPanelTransform == null)
            {
                panel.SetTaskText(medicRole.SetTabText().ToString());
                return;
            }

            var ogPanel = taskPanelTransform.gameObject.GetComponent<TaskPanelBehaviour>();
            if (ogPanel == null)
            {
                panel.SetTaskText(medicRole.SetTabText().ToString());
                return;
            }

            if (tabHeader != null && tabHeader.text != roleAsCustom.RoleName)
            {
                tabHeader.text = roleAsCustom.RoleName;
            }

            var y = ogPanel.taskText.textBounds.size.y + 1;
            panel.closedPosition = new Vector3(ogPanel.closedPosition.x, ogPanel.open ? y + 0.2f : 2f,
                ogPanel.closedPosition.z);
            panel.openPosition = new Vector3(ogPanel.openPosition.x, ogPanel.open ? y : 2f, ogPanel.openPosition.z);

            panel.SetTaskText(medicRole.SetTabText().ToString());
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogError($"MedicShieldStolenPatch: RefreshRoleTab failed: {ex.Message}");
        }
    }
}
