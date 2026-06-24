using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace DivaniMods.Assets;

public static class DivaniAssets
{
    private const string ShortPath = "DivaniMods.Resources";
    private const string IconPath = "DivaniMods.Resources.Icons";
    
    // Button sprites (115 ppu)
    public static LoadableAsset<Sprite> PlacePortalButton { get; } = new LoadableResourceAsset($"{ShortPath}.PortalUse.png", 100);
    public static LoadableAsset<Sprite> UsePortalButton { get; } = new LoadableResourceAsset($"{ShortPath}.PortalUse.png", 100);
    public static LoadableAsset<Sprite> PickpocketButton { get; } = new LoadableResourceAsset($"{ShortPath}.PickPocketButton.png");
    public static LoadableAsset<Sprite> PlagueDoctorVentButton { get; } = new LoadableResourceAsset($"{ShortPath}.PDVent.png");
    public static LoadableAsset<Sprite> DemolitionistVentButton { get; } = new LoadableResourceAsset($"{ShortPath}.DemolitionistVent.png");
    public static LoadableAsset<Sprite> FragVentButton { get; } = new LoadableResourceAsset($"{ShortPath}.FragVentButton.png");
    public static LoadableAsset<Sprite> FragGiveButton { get; } = new LoadableResourceAsset($"{ShortPath}.FragGive.png");
    public static LoadableAsset<Sprite> FragPassButton { get; } = new LoadableResourceAsset($"{ShortPath}.FragPass.png");
    public static LoadableAsset<Sprite> DeadlockLockdownButton { get; } = new LoadableResourceAsset($"{ShortPath}.DeadlockLockdown.png");
    public static LoadableAsset<Sprite> DemolitionistPlantButton { get; } = new LoadableResourceAsset($"{ShortPath}.DemolitionistPlant.png");
    public static LoadableAsset<Sprite> DemolitionistDefuseButton { get; } = new LoadableResourceAsset($"{ShortPath}.DemolitionistDefuse.png");
    public static LoadableAsset<Sprite> SentinelPlaceBeaconButton { get; } = new LoadableResourceAsset($"{ShortPath}.BeaconPlace.png");
    public static LoadableAsset<Sprite> DomesmithPlaceDomeButton { get; } = new LoadableResourceAsset($"{ShortPath}.DomesmithPlaceDome.png");
    public static LoadableAsset<Sprite> PlagueDoctorInfectButton { get; } = new LoadableResourceAsset($"{ShortPath}.PlagueDoctorInfect.png");
    public static LoadableAsset<Sprite> SproutCollectButton { get; } = new LoadableResourceAsset($"{ShortPath}.Collect.png", 100);
    public static LoadableAsset<Sprite> MosquitoStingButton { get; } = new LoadableResourceAsset($"{ShortPath}.MosquitoSting.png");
    public static LoadableAsset<Sprite> DuelistDuelButton { get; } = new LoadableResourceAsset($"{ShortPath}.DuelistDuel.png");
    public static LoadableAsset<Sprite> DuelStrikeButton { get; } = new LoadableResourceAsset($"{ShortPath}.DuelStrikeButton.png");
    public static LoadableAsset<Sprite> VengefulSoulRevengeButton { get; } = new LoadableResourceAsset($"{ShortPath}.VengefulSoulRevenge.png");
    public static LoadableAsset<Sprite> ShuffleButton { get; } = new LoadableResourceAsset($"{ShortPath}.ShuffleButton.png",100);
    public static LoadableAsset<Sprite> UavButton { get; } = new LoadableResourceAsset($"{ShortPath}.UAVAirMap.png", 100);
    public static LoadableAsset<Sprite> CupidMatchmakeButton { get; } = new LoadableResourceAsset($"{ShortPath}.CupidMatchmake.png");
    public static LoadableAsset<Sprite> CupidProtectButton { get; } = new LoadableResourceAsset($"{ShortPath}.CupidProtect.png");
    public static LoadableAsset<Sprite> CupidProtectOneButton { get; } = new LoadableResourceAsset($"{ShortPath}.CupidProtectOne.png");
    public static LoadableAsset<Sprite> CupidProtectTwoButton { get; } = new LoadableResourceAsset($"{ShortPath}.CupidProtectTwo.png");
    // Role icons (200 ppu)
    public static LoadableAsset<Sprite> ThiefIcon { get; } = new LoadableResourceAsset($"{IconPath}.Thief.png", 200);
    public static LoadableAsset<Sprite> DeadlockIcon { get; } = new LoadableResourceAsset($"{IconPath}.Deadlock.png", 200);
    public static LoadableAsset<Sprite> CouncillorIcon { get; } = new LoadableResourceAsset($"{IconPath}.Councillor.png", 200);
    public static LoadableAsset<Sprite> PortalmakerIcon { get; } = new LoadableResourceAsset($"{IconPath}.PortalMaker.png", 200);
    public static LoadableAsset<Sprite> FragIcon { get; } = new LoadableResourceAsset($"{IconPath}.Frag.png", 200);
    public static LoadableAsset<Sprite> SilencerIcon { get; } = new LoadableResourceAsset($"{IconPath}.Silencer.png", 200);
    public static LoadableAsset<Sprite> PlagueDoctorIcon { get; } = new LoadableResourceAsset($"{IconPath}.PlagueDoctor.png", 200);
    public static LoadableAsset<Sprite> InnocentIcon { get; } = new LoadableResourceAsset($"{IconPath}.Innocent.png", 200);
    public static LoadableAsset<Sprite> OpportunistIcon { get; } = new LoadableResourceAsset($"{IconPath}.Opportunist.png", 200);
    public static LoadableAsset<Sprite> RecruiterIcon { get; } = new LoadableResourceAsset($"{IconPath}.Recruiter.png");
    public static LoadableAsset<Sprite> SentinelIcon { get; } = new LoadableResourceAsset($"{IconPath}.Sentinel.png", 200);
    public static LoadableAsset<Sprite> DemolitionistIcon { get; } = new LoadableResourceAsset($"{IconPath}.Demolitionist.png", 200);
    public static LoadableAsset<Sprite> DomesmithIcon { get; } = new LoadableResourceAsset($"{IconPath}.Domesmith.png", 200);
    public static LoadableAsset<Sprite> SummonerIcon { get; } = new LoadableResourceAsset($"{IconPath}.Summoner.png", 200);
    public static LoadableAsset<Sprite> RevenantIcon { get; } = new LoadableResourceAsset($"{IconPath}.Revenant.png", 200);
    public static LoadableAsset<Sprite> MosquitoIcon { get; } = new LoadableResourceAsset($"{IconPath}.Mosquito.png", 200);
    public static LoadableAsset<Sprite> DuelistIcon { get; } = new LoadableResourceAsset($"{IconPath}.Duellist.png", 200);
    public static LoadableAsset<Sprite> ClockstopperIcon { get; } = new LoadableResourceAsset($"{IconPath}.Clockstopper.png", 200);
    public static LoadableAsset<Sprite> RetributionistIcon { get; } = new LoadableResourceAsset($"{IconPath}.Retributionist.png", 200);
    public static LoadableAsset<Sprite> CupidIcon { get; } = new LoadableResourceAsset($"{IconPath}.Cupid.png", 200);

    // Modifier icons (200 ppu)
    public static LoadableAsset<Sprite> MementoIcon { get; } = new LoadableResourceAsset($"{IconPath}.Memento.png", 200);
    public static LoadableAsset<Sprite> BlindspotIcon { get; } = new LoadableResourceAsset($"{IconPath}.Blindspot.png", 200);
    public static LoadableAsset<Sprite> FragileIcon { get; } = new LoadableResourceAsset($"{IconPath}.Fragile.png", 200);
    public static LoadableAsset<Sprite> ShuffleIcon { get; } = new LoadableResourceAsset($"{IconPath}.Shuffle.png", 200);
    public static LoadableAsset<Sprite> MisvoteIcon { get; } = new LoadableResourceAsset($"{IconPath}.Misvote.png", 200);
    public static LoadableAsset<Sprite> SniperIcon { get; } = new LoadableResourceAsset($"{IconPath}.Sniper.png", 200);
    public static LoadableAsset<Sprite> BearTrapIcon { get; } = new LoadableResourceAsset($"{IconPath}.Beartrap.png", 200);
    public static LoadableAsset<Sprite> SkilledIcon { get; } = new LoadableResourceAsset($"{IconPath}.Skilled.png", 200);
    public static LoadableAsset<Sprite> StrongIcon { get; } = new LoadableResourceAsset($"{IconPath}.Strong.png", 200);
    public static LoadableAsset<Sprite> BloodyIcon { get; } = new LoadableResourceAsset($"{IconPath}.Bloody.png", 200);
    public static LoadableAsset<Sprite> RuthlessIcon { get; } = new LoadableResourceAsset($"{IconPath}.Ruthless.png", 200);
    public static LoadableAsset<Sprite> NullifiedIcon { get; } = new LoadableResourceAsset($"{IconPath}.Nullified.png", 200);
    public static LoadableAsset<Sprite> SproutIcon { get; } = new LoadableResourceAsset($"{IconPath}.Sprout.png", 200);
    public static LoadableAsset<Sprite> ObfuscatorIcon { get; } = new LoadableResourceAsset($"{IconPath}.Obfuscator.png", 200);
    public static LoadableAsset<Sprite> CunctatorIcon { get; } = new LoadableResourceAsset($"{IconPath}.Cunctator.png", 200);
    public static LoadableAsset<Sprite> IncompetentIcon { get; } = new LoadableResourceAsset($"{IconPath}.Incompetent.png", 200);
    public static LoadableAsset<Sprite> ArmoredIcon { get; } = new LoadableResourceAsset($"{IconPath}.Armored.png", 200);
    public static LoadableAsset<Sprite> UavIcon { get; } = new LoadableResourceAsset($"{IconPath}.UAV.png", 200);
    // Audio clips (16000hz)
    public static LoadableAsset<AudioClip> FragileBreak { get; } = new LoadableAudioResourceAsset($"{ShortPath}.FragileBreak.wav");
    public static LoadableAsset<AudioClip> PlagueDoctorIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.PlagueDoctorIntro.wav");
    public static LoadableAsset<AudioClip> InfectSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.InfectSound.wav");
    public static LoadableAsset<AudioClip> ThiefIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.ThiefIntro.wav");
    public static LoadableAsset<AudioClip> PortalMakerIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.PortalMakerIntro.wav");
    public static LoadableAsset<AudioClip> DeadlockIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.DeadlockIntro.wav");
    public static LoadableAsset<AudioClip> CouncillorIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.CouncillorIntro.wav");
    public static LoadableAsset<AudioClip> PlacePortalSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.PlacePortalSound.wav");
    public static LoadableAsset<AudioClip> FragHeartbeat { get; } = new LoadableAudioResourceAsset($"{ShortPath}.FragHeartbeat.wav");
    public static LoadableAsset<AudioClip> FragIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.FragIntro.wav");
    public static LoadableAsset<AudioClip> SilencerIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.SilencerIntro.wav");
    public static LoadableAsset<AudioClip> FragGiveSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.FragGive.wav");
    public static LoadableAsset<AudioClip> BearTrapActivateSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.BearTrapActivate.wav");
    public static LoadableAsset<AudioClip> OpportunistIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.OpportunistIntro.wav");
    public static LoadableAsset<AudioClip> DemolitionistIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.DemolitionistIntro.wav");
    public static LoadableAsset<AudioClip> DemolitionistExplosionSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.DemolitionistExplosion.wav");
    public static LoadableAsset<AudioClip> DomesmithIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.DomesmithIntro.wav");
    public static LoadableAsset<AudioClip> SummonerIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.SummonerIntro.wav");
    public static LoadableAsset<AudioClip> SentinelIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.SentinelIntro.wav");
    public static LoadableAsset<AudioClip> RecruiterIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.RecruiterIntro.wav");
    public static LoadableAsset<AudioClip> MosquitoIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.MosquitoIntro.wav");
    public static LoadableAsset<AudioClip> CunctatorIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.CunctatorIntro.wav");
    public static LoadableAsset<AudioClip> MosquitoSwatSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.MosquitoSwat.wav");
    public static LoadableAsset<AudioClip> ObfuscatorIntro { get; } = new LoadableAudioResourceAsset($"{ShortPath}.ObfuscatorIntro.wav");
    public static LoadableAsset<AudioClip> InnocentIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.InnocentIntro.wav");
    public static LoadableAsset<AudioClip> DuelistIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.DuelistIntro.wav");
    public static LoadableAsset<AudioClip> ClockstopperIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.ClockstopperIntro.wav");
    public static LoadableAsset<AudioClip> RetributionistIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.RetributionistIntro.wav");
    public static LoadableAsset<AudioClip> UavFriendlySound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.UAVFriendly.wav");
    public static LoadableAsset<AudioClip> UavEnemySound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.UAVEnemy.wav");
    public static LoadableAsset<AudioClip> UavEndSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.UAVEnd.wav");
    public static LoadableAsset<AudioClip> CupidIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.CupidIntro.wav");
    // Dutch Meme Soundpack door SFX - used by DutchMemeSoundpackPatch to replace
    // the vanilla door open/close audio clips when the matching lobby toggle is on.
    public static LoadableAsset<AudioClip> DutchDoorOpen { get; } = new LoadableAudioResourceAsset($"{ShortPath}.DoorOpen.wav");
    public static LoadableAsset<AudioClip> DutchDoorClose { get; } = new LoadableAudioResourceAsset($"{ShortPath}.DoorClose.wav");
    
    // Portal on map (200 ppu)
    public static LoadableAsset<Sprite> PortalSprite { get; } = new LoadableResourceAsset($"{ShortPath}.PortalSprite.png", 200);

    // Beacon on map (550 ppu – source image is ~1024px so high ppu keeps it small in-game)
    public static LoadableAsset<Sprite> BeaconSprite { get; } = new LoadableResourceAsset($"{ShortPath}.BeaconAsset.png", 550);

    // Meeting nameplate toggles:
    public static LoadableAsset<Sprite> RecruitMeetingCrewmate { get; } =
        new LoadableResourceAsset($"{ShortPath}.RecruitMeetingCrewmate.png", 440f);
    public static LoadableAsset<Sprite> RecruitMeetingImpostor { get; } =
        new LoadableResourceAsset($"{ShortPath}.RecruitMeetingImpostor.png", 440f);
    public static LoadableAsset<Sprite> SummonerMeetingActive { get; } =
        new LoadableResourceAsset($"{ShortPath}.SummonerMeetingActive.png", 440f);
    public static LoadableAsset<Sprite> SummonerMeetingInactive { get; } =
        new LoadableResourceAsset($"{ShortPath}.SummonerMeetingInactive.png", 440f);
    public static LoadableAsset<Sprite> ObfuscateActive { get; } =
        new LoadableResourceAsset($"{ShortPath}.ObfuscateActive.png", 300f);
    public static LoadableAsset<Sprite> ObfuscateInactive { get; } =
        new LoadableResourceAsset($"{ShortPath}.ObfuscateDisabled.png", 300f);

    // Animation bundles
    public static readonly AssetBundle Bundle = AssetBundleManager.Load("divanimods-bundle");
    public static LoadableBundleAsset<GameObject> PortalPrefab { get; } = new("Portal.prefab", Bundle);
    // Announcement badge
    public static LoadableAsset<Sprite> ModNewsLogo { get; } =
        new LoadableResourceAsset($"{ShortPath}.Banners.DivaniModNewsLogo.png", 220f);

    // Local settings tab icon (lower ppu = larger sprite, ~100px)
    public static LoadableAsset<Sprite> LocalSettingsTabIcon { get; } =
        new LoadableResourceAsset($"{ShortPath}.Banners.DivaniModNewsLogo.png", 66f);

    // Role options screenshots
    public static LoadableAsset<Sprite> ThiefBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.ThiefBanner.png");
    public static LoadableAsset<Sprite> ClockstopperBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.ClockstopperBanner.png");
    public static LoadableAsset<Sprite> CunctatorBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.CunctatorBanner.png");
    public static LoadableAsset<Sprite> CupidBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.CupidBanner.png");
    public static LoadableAsset<Sprite> DeadlockBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.DeadlockBanner.png");
    public static LoadableAsset<Sprite> DemolitionistBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.DemolitionistBanner.png");
    public static LoadableAsset<Sprite> DomesmithBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.DomesmithBanner.png");
    public static LoadableAsset<Sprite> DuelistBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.DuelistBanner.png");
    public static LoadableAsset<Sprite> FragBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.FragBanner.png");
    public static LoadableAsset<Sprite> InnocentBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.InnocentBanner.png");
    public static LoadableAsset<Sprite> MosquitoBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.MosquitoBanner.png");
    public static LoadableAsset<Sprite> PortalmakerBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.PortalmakerBanner.png");
    public static LoadableAsset<Sprite> RetributionistBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.RetributionistBanner.png");
    public static LoadableAsset<Sprite> SentinelBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.SentinelBanner.png");
    public static LoadableAsset<Sprite> SilencerBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.SilencerBanner.png");
    public static LoadableAsset<Sprite> SummonerBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.OptionBanners.SummonerBanner.png");
}
