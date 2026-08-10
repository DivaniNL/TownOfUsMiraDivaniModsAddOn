public class FreezeButton : CustomActionButton<PlayerControl>
{
    public override string Name => "Freeze";
    public override float Cooldown => 5f;
    public override LoadableAsset<Sprite> Sprite => DivaniModsAssets.DeathnoteNoteButton;
    public override bool PauseTimerInVent => true;
    public override maxuses => OptionGroupSingleton<DeathnoteRole>.Instance.NotesPerGame.value

    protected override void OnClick()
    {
        Target?.RpcAddModifier<NoteModifier>();
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Palette.ImpostorRed));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is DeathnoteRole;
    }
}