using MiraAPI.Modifiers;

namespace DivaniMods.Modifiers.Crewmate.CrewmateKilling;

public sealed class CursedModifier : BaseModifier
{
    private int _savedEmergencies;

    public override string ModifierName => "Cursed";
    public override bool HideOnUi => true;
    public override bool Unique => true;

    public override bool? CanVent()
    {
        return false;
    }

    public override void OnActivate()
    {
        if (Player == null)
        {
            return;
        }

        _savedEmergencies = Player.RemainingEmergencies;
        Player.RemainingEmergencies = 0;

        if (Player.AmOwner && Player.inVent && Vent.currentVent != null)
        {
            Player.MyPhysics.RpcExitVent(Vent.currentVent.Id);
        }
    }

    public override void FixedUpdate()
    {
        if (Player != null)
        {
            Player.RemainingEmergencies = 0;
        }
    }

    public override void OnDeactivate()
    {
        if (Player != null)
        {
            Player.RemainingEmergencies = _savedEmergencies;
        }
    }
}
