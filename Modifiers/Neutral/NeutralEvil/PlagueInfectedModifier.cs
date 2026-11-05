using MiraAPI.Modifiers;
using MiraAPI.Translation;
using DivaniMods.Roles.Neutral.NeutralEvil;

namespace DivaniMods.Modifiers.Neutral.NeutralEvil;

public sealed class PlagueInfectedModifier : BaseModifier
{
    public override string ModifierName => MiraLocaleManager.Get("DivaniMods.Modifier.PlagueInfected", "Plague Infected"); //Freeplay
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        base.OnActivate();
        PlagueDoctorRole.OnPlayerInfected(Player);
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        PlagueDoctorRole.OnPlayerCured(Player);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (Player == null || Player.Data == null || Player.Data.IsDead)
        {
            return;
        }

        if (MeetingHud.Instance || PlagueDoctorRole.MeetingFlag)
        {
            return;
        }

        if (PlagueDoctorRole.ImmunityTimer > 0f)
        {
            return;
        }

        PlagueDoctorRole.SpreadInfectionFrom(Player);
    }
}
