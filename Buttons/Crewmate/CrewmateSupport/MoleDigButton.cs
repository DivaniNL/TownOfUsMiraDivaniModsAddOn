using System;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DivaniMods.Buttons.Crewmate.CrewmateSupport;

public sealed class MoleDigButton : TownOfUsRoleButton<MoleRole>
{
    public override string Name => "Dig";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => MoleRole.MoleColor;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<MoleOptions>.Instance.DigCooldown + MapCooldown, 5f, 120f);

    public override float EffectDuration =>
        OptionGroupSingleton<MoleOptions>.Instance.VentVisibility is MoleVentVisibility.Immediate
            ? OptionGroupSingleton<MoleOptions>.Instance.DigDelay.Value + 0.001f
            : 0.001f;

    public override int MaxUses => (int)OptionGroupSingleton<MoleOptions>.Instance.MaxVents;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.MoleDigButton;

    public Vector2 VentSize { get; set; }
    public Vector3? SavedPos { get; set; }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        var vents = Object.FindObjectsOfType<Vent>();

        if (vents.Count > 0)
        {
            VentSize = Vector2.Scale(vents[0].GetComponent<BoxCollider2D>().size, vents[0].transform.localScale) *
                       0.75f;
        }
    }

    public override bool CanUse()
    {
        if (!base.CanUse())
        {
            return false;
        }

        var hits = Physics2D.OverlapBoxAll(PlayerControl.LocalPlayer.transform.position, VentSize, 0);

        hits = hits.Where(c =>
            (c.name.Contains("Vent") || c.name.Contains("Door") || !c.isTrigger) && c.gameObject.layer != 8 &&
            c.gameObject.layer != 5).ToArray();

        var noConflict = !PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.Collider,
            PlayerControl.LocalPlayer.Collider.bounds.center, PlayerControl.LocalPlayer.transform.position,
            Constants.ShipAndAllObjectsMask,
            false);

        return hits.Count == 0 && noConflict;
    }

    protected override void OnClick()
    {
        SavedPos = PlayerControl.LocalPlayer.transform.position;
    }

    public override void OnEffectEnd()
    {
        base.OnEffectEnd();

        if (SavedPos == null)
        {
            return;
        }

        var visibility = OptionGroupSingleton<MoleOptions>.Instance.VentVisibility;

        // After Next Meeting: queue silently, no sound/anim/vent until the meeting ends.
        if (visibility == MoleVentVisibility.AfterNextMeeting)
        {
            Role.PendingVents.Add(SavedPos.Value);

            var roomName = MiscUtils.GetRoomName(SavedPos.Value);
            var hex = ColorUtility.ToHtmlStringRGB(MoleRole.MoleColor);
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>Your placed vent at {roomName} will activate after the next meeting.</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.MoleDigButton.LoadAsset());

            SavedPos = null;
            return;
        }

        var immediate = visibility == MoleVentVisibility.Immediate;

        MoleRole.RpcPlaceVent(PlayerControl.LocalPlayer, GetNextVentId(), SavedPos.Value, SavedPos.Value.z, immediate);
        TouAudio.PlaySound(TouAudio.MineSound);
        SavedPos = null;
    }

    public static int GetNextVentId()
    {
        var id = 0;

        while (true)
        {
            if (ShipStatus.Instance.AllVents.All(v => v.Id != id))
            {
                return id;
            }

            id++;
        }
    }
}
