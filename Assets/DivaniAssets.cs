using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace DivaniMods.Assets;

public static class DivaniAssets
{
    private const string ShortPath = "DivaniMods.Resources";
    private const string IconPath = "DivaniMods.Resources.Icons";
    
    // Button sprites (115 ppu)
    public static LoadableAsset<Sprite> PlacePortalButton { get; } = new LoadableResourceAsset($"{ShortPath}.PortalSprite.png", 115);
    public static LoadableAsset<Sprite> UsePortalButton { get; } = new LoadableResourceAsset($"{ShortPath}.PortalSprite.png", 115);
    public static LoadableAsset<Sprite> PickpocketButton { get; } = new LoadableResourceAsset($"{ShortPath}.PickPocketButton.png");
    public static LoadableAsset<Sprite> PlagueDoctorVentButton { get; } = new LoadableResourceAsset($"{ShortPath}.PDVent.png");
    public static LoadableAsset<Sprite> DemolitionistVentButton { get; } = new LoadableResourceAsset($"{ShortPath}.DemolitionistVent.png");
    public static LoadableAsset<Sprite> FragGiveButton { get; } = new LoadableResourceAsset($"{ShortPath}.FragGive.png", 250f);
    public static LoadableAsset<Sprite> FragPassButton { get; } = new LoadableResourceAsset($"{ShortPath}.FragPass.png", 250f);
    public static LoadableAsset<Sprite> DeadlockLockdownButton { get; } = new LoadableResourceAsset($"{IconPath}.Deadlock.png", 300f);
    public static LoadableAsset<Sprite> DemolitionistPlantButton { get; } = new LoadableResourceAsset($"{ShortPath}.DemolitionistPlant.png");
    public static LoadableAsset<Sprite> DemolitionistDefuseButton { get; } = new LoadableResourceAsset($"{ShortPath}.DemolitionistDefuse.png");
    public static LoadableAsset<Sprite> SentinelPlaceBeaconButton { get; } = new LoadableResourceAsset($"{IconPath}.Sentinel.png", 250f);
    public static LoadableAsset<Sprite> DomesmithPlaceDomeButton { get; } = new LoadableResourceAsset($"{ShortPath}.DomesmithPlaceDome.png");
    public static LoadableAsset<Sprite> PlagueDoctorInfectButton { get; } = new LoadableResourceAsset($"{ShortPath}.PlagueDoctorInfect.png");
    public static LoadableAsset<Sprite> ShuffleAbilityButton { get; } = new LoadableResourceAsset($"{IconPath}.Shuffle.png", 250f);
    public static LoadableAsset<Sprite> SproutCollectButton { get; } = new LoadableResourceAsset($"{ShortPath}.Collect.png", 100);

    // Role icons (200 ppu)
    public static LoadableAsset<Sprite> ThiefIcon { get; } = new LoadableResourceAsset($"{IconPath}.Thief.png", 200);
    public static LoadableAsset<Sprite> DeadlockIcon { get; } = new LoadableResourceAsset($"{IconPath}.Deadlock.png", 200);
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

    // Modifier icons (200 ppu)
    public static LoadableAsset<Sprite> MementoIcon { get; } = new LoadableResourceAsset($"{IconPath}.Memento.png", 200);
    public static LoadableAsset<Sprite> BlindspotIcon { get; } = new LoadableResourceAsset($"{IconPath}.Blindspot.png", 200);
    public static LoadableAsset<Sprite> FragileIcon { get; } = new LoadableResourceAsset($"{IconPath}.Fragile.png", 200);
    public static LoadableAsset<Sprite> ShuffleIcon { get; } = new LoadableResourceAsset($"{IconPath}.Shuffle.png", 200);
    public static LoadableAsset<Sprite> MisvoteIcon { get; } = new LoadableResourceAsset($"{IconPath}.Misvote.png", 200);
    public static LoadableAsset<Sprite> SniperIcon { get; } = new LoadableResourceAsset($"{IconPath}.Sniper.png", 200);
    public static LoadableAsset<Sprite> BearTrapIcon { get; } = new LoadableResourceAsset($"{IconPath}.Beartrap.png", 200);
    public static LoadableAsset<Sprite> BloodyIcon { get; } = new LoadableResourceAsset($"{IconPath}.Bloody.png", 200);
    public static LoadableAsset<Sprite> RuthlessIcon { get; } = new LoadableResourceAsset($"{IconPath}.Ruthless.png", 200);
    public static LoadableAsset<Sprite> SproutIcon { get; } = new LoadableResourceAsset($"{IconPath}.Sprout.png", 200);
    
    // Audio clips (16000hz)
    public static LoadableAsset<AudioClip> FragileBreak { get; } = new LoadableAudioResourceAsset($"{ShortPath}.FragileBreak.wav");
    public static LoadableAsset<AudioClip> PlagueDoctorIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.PlagueDoctorIntro.wav");
    public static LoadableAsset<AudioClip> InfectSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.InfectSound.wav");
    public static LoadableAsset<AudioClip> ThiefIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.ThiefIntro.wav");
    public static LoadableAsset<AudioClip> PortalMakerIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.PortalMakerIntro.wav");
    public static LoadableAsset<AudioClip> DeadlockIntroSound { get; } = new LoadableAudioResourceAsset($"{ShortPath}.DeadlockIntro.wav");
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

    // Announcement badge
    public static LoadableAsset<Sprite> ModNewsLogo { get; } =
        new LoadableResourceAsset($"{ShortPath}.Banners.DivaniModNewsLogo.png", 220f);
}
