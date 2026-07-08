using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Utilities;
using TownOfUs.Assets;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Modules.Anims;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Universal;

public sealed class TacticalInsertionModifier : UniversalGameModifier, IColoredModifier, IWikiDiscoverable, IButtonModifier
{
    public static readonly Color TacticalColor = new Color32(0, 255, 0, 255);

    public override string ModifierName => "Tactical Insertion";
    public override string LocaleKey => "TacticalInsertion";
    public override string IntroInfo => "Mark a spot, respawn there after the next meeting!";
    public override ModifierFaction FactionType => ModifierFaction.UniversalUtility;
    public override Color FreeplayFileColor => TacticalColor;
    public Color ModifierColor => TacticalColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.TacticalInsertionIcon;

    [HideFromIl2Cpp] public Vector2? MarkedLocation { get; set; }
    [HideFromIl2Cpp] public GameObject? MarkObject { get; set; }
    public bool UsedThisRound { get; set; }

    public override string GetDescription() =>
        "Mark your current position. After the next meeting you respawn at the marked spot.";

    public string GetAdvancedDescription() =>
        "Mark your current position. After the next meeting you respawn at the marked spot instead of the meeting area." +
        MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Tactical Insertion", "Mark your current position to respawn there after the next meeting.", DivaniAssets.TacticalInsertionButton)
    ];

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.TacticalInsertionChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.TacticalInsertionAmount.Value;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) &&
            !ModifierExclusions.ConflictsWithOwned(role.Player, this) &&
            !role.Player.GetModifierComponent().HasModifier<GameModifier>(true, x => x is IButtonModifier);
    }

    private int _usesRemaining = -1;

    public int UsesRemaining
    {
        get
        {
            if (_usesRemaining < 0)
            {
                _usesRemaining = (int)OptionGroupSingleton<TacticalInsertionOptions>.Instance.Uses.Value;
            }

            return _usesRemaining;
        }
        set => _usesRemaining = value;
    }

    public override void OnActivate()
    {
        _usesRemaining = (int)OptionGroupSingleton<TacticalInsertionOptions>.Instance.Uses.Value;
    }

    [MethodRpc((uint)DivaniRpcCalls.TacticalInsertionMark)]
    public static void RpcMark(PlayerControl player, float x, float y)
    {
        player.GetModifier<TacticalInsertionModifier>()?.Mark(new Vector2(x, y));
    }

    public void Mark(Vector2 position)
    {
        ClearMark();

        MarkedLocation = position;
        UsedThisRound = true;

        var local = PlayerControl.LocalPlayer;
        var localIsDead = local != null && local.Data != null && local.Data.IsDead;

        if (Player.AmOwner || localIsDead)
        {
            MarkObject = AnimStore.SpawnAnimAtPlayer(Player, TouAssets.EscapistMarkPrefab.LoadAsset());
            MarkObject.transform.localPosition = new Vector3(position.x, position.y + 0.3f, 0.1f);

            foreach (var rend in MarkObject.GetComponentsInChildren<SpriteRenderer>())
            {
                rend.material.color = TacticalColor;
            }
        }
    }

    public void ClearMark()
    {
        if (MarkObject != null)
        {
            MarkObject.Destroy();
            MarkObject = null;
        }

        MarkedLocation = null;
    }

    public void OnRoundStart()
    {
        if (MiscUtils.GetCurrentMap == ExpandedMapNames.Airship)
        {
            return;
        }

        if (Player.AmOwner && !Player.HasDied() && MarkedLocation.HasValue)
        {
            var location = MarkedLocation.Value;
            Player.NetTransform.RpcSnapTo(location);

            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                "<b><color=#00FF00>You spawned at the location of your Tactical Insertion</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.TacticalInsertionIcon.LoadAsset());

            if (ModCompatibility.IsSubmerged())
            {
                ModCompatibility.ChangeFloor(Player.GetTruePosition().y > -7);
                ModCompatibility.CheckOutOfBoundsElevator(PlayerControl.LocalPlayer);
            }
        }

        ClearMark();
        UsedThisRound = false;
    }

    public override void OnDeactivate()
    {
        ClearMark();
    }
}
