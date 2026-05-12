using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace DivaniMods.Assets;

public static class DivaniAssets
{
    private const string ShortPath = "DivaniMods.Resources";
    private const string IconPath = "DivaniMods.Resources.Icons";
    
    // Button sprites (115 ppu)
    public static LoadableAsset<Sprite> ShuffleButton { get; } = new LoadableResourceAsset($"{ShortPath}.ShuffleButton.png", 115);
    public static LoadableAsset<Sprite> LockdownButton { get; } = new LoadableResourceAsset($"{ShortPath}.LockdownButton.png", 115);
    public static LoadableAsset<Sprite> PlacePortalButton { get; } = new LoadableResourceAsset($"{ShortPath}.PortalSprite.png", 115);
    public static LoadableAsset<Sprite> UsePortalButton { get; } = new LoadableResourceAsset($"{ShortPath}.PortalSprite.png", 115);
    public static LoadableAsset<Sprite> InfectButton { get; } = new LoadableResourceAsset($"{ShortPath}.InfectButton.png", 115);
    // Illustrated HUD art (padding / white stroke); 115 like vanilla makes it read huge—match Frag scale.
    public static LoadableAsset<Sprite> PickpocketButton { get; } = new LoadableResourceAsset($"{ShortPath}.PickPocketButton.png");
    public static LoadableAsset<Sprite> PlagueDoctorVentButton { get; } = new LoadableResourceAsset($"{ShortPath}.PDVent.png", 1024);
    // Frag button art is 512x512; lower ppu = larger on HUD (Lockdown uses 115).
    public static LoadableAsset<Sprite> FragGiveButton { get; } = new LoadableResourceAsset($"{ShortPath}.FragGive.png", 360);
    public static LoadableAsset<Sprite> FragPassButton { get; } = new LoadableResourceAsset($"{ShortPath}.FragPass.png", 360);

    public static LoadableAsset<Sprite> ThiefIcon { get; } = new LoadableResourceAsset($"{IconPath}.Thief.png", 200);
    public static LoadableAsset<Sprite> DeadlockIcon { get; } = new LoadableResourceAsset($"{IconPath}.Deadlock.png", 200);
    public static LoadableAsset<Sprite> PortalmakerIcon { get; } = new LoadableResourceAsset($"{IconPath}.Portalmaker.png", 200);
    public static LoadableAsset<Sprite> RuthlessIcon { get; } = new LoadableResourceAsset($"{IconPath}.Ruthless.png", 200);
    public static LoadableAsset<Sprite> FragIcon { get; } = new LoadableResourceAsset($"{IconPath}.Frag.png", 200);
    public static LoadableAsset<Sprite> SilencerIcon { get; } = new LoadableResourceAsset($"{IconPath}.Silencer.png", 200);
    public static LoadableAsset<Sprite> PlagueDoctorIcon { get; } = new LoadableResourceAsset($"{IconPath}.PlagueDoctor.png", 200);
    public static LoadableAsset<Sprite> InnocentIcon { get; } = new LoadableResourceAsset($"{IconPath}.Innocent.png", 200);
    public static LoadableAsset<Sprite> OpportunistIcon { get; } = new LoadableResourceAsset($"{IconPath}.Opportunist.png", 200);
    public static LoadableAsset<Sprite> SentinelIcon { get; } = new LoadableResourceAsset($"{IconPath}.Sentinel.png", 200);

    // Audio clips - loaded lazily by MiraAPI from embedded WAVs, same approach
    // TouMiraRolesExtension uses. Drop WAV files into Resources/ and embed them
    // in the csproj alongside the other resources.
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

    // Dutch Meme Soundpack door SFX - used by DutchMemeSoundpackPatch to replace
    // the vanilla door open/close audio clips when the matching lobby toggle is on.
    public static LoadableAsset<AudioClip> DutchDoorOpen { get; } = new LoadableAudioResourceAsset($"{ShortPath}.DoorOpen.wav");
    public static LoadableAsset<AudioClip> DutchDoorClose { get; } = new LoadableAudioResourceAsset($"{ShortPath}.DoorClose.wav");

    // Modifier icons (550 ppu for modifiers - same as role icons)
    public static LoadableAsset<Sprite> BlindspotIcon { get; } = new LoadableResourceAsset($"{IconPath}.Blindspot.png", 200);
    public static LoadableAsset<Sprite> FragileIcon { get; } = new LoadableResourceAsset($"{IconPath}.Fragile.png", 200);
    public static LoadableAsset<Sprite> ShuffleIcon { get; } = new LoadableResourceAsset($"{IconPath}.Shuffle.png", 200);
    public static LoadableAsset<Sprite> MisvoteIcon { get; } = new LoadableResourceAsset($"{IconPath}.Misvote.png", 200);
    public static LoadableAsset<Sprite> SniperIcon { get; } = new LoadableResourceAsset($"{IconPath}.Sniper.png", 200);
    
    // Portal on map (200 ppu)
    public static LoadableAsset<Sprite> PortalSprite { get; } = new LoadableResourceAsset($"{ShortPath}.PortalSprite.png", 200);

    // Beacon on map (550 ppu – source image is ~1024px so high ppu keeps it small in-game)
    public static LoadableAsset<Sprite> BeaconSprite { get; } = new LoadableResourceAsset($"{ShortPath}.BeaconAsset.png", 550);

    // Announcement badge: PPU between Mira default (~100, reads huge) and very high PPU (tiny).
    // TOU uses AuAvengersSprite at 290; we sit a bit under that plus slightly lower patch scale.
    public static LoadableAsset<Sprite> ModNewsLogo { get; } =
        new LoadableResourceAsset($"{ShortPath}.Banners.DivaniModNewsLogo.png", 220f);
}
