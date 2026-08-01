using DivaniMods.Assets;
using DivaniMods.Options;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmateInvestigative;

public sealed class ChameleonRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color ChameleonColor = new Color32(122, 220, 193, 255);

    public DoomableType DoomHintType => DoomableType.Hunter;
    public string LocaleKey => "Chameleon";
    public string RoleName => "Chameleon";
    public string RoleDescription => "Camouflage to sneakily get around and gather info!";
    public string RoleLongDescription => "Use your Camouflage ability to turn invisible and gather information.";

        public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());
    

    public Color RoleColor => ChameleonColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Canouflage", "Turn invisible for a short period of time.", DivaniAssets.MageIllusionButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<ChameleonOptions>.Instance.CanVent,
        Icon = DivaniAssets.ChameleonIcon,
        IntroSound = TouAudio.PhantomIntroSound
    };
        
}