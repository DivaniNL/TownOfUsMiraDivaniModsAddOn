using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace DivaniMods.Assets;

public static class DivaniAssets
{
    private const string ShortPath = "DivaniMods.Resources";
    
    // Button sprites (115 ppu)
    public static LoadableAsset<Sprite> ShuffleButton { get; } = new LoadableResourceAsset($"{ShortPath}.ShuffleButton.png", 115);
    public static LoadableAsset<Sprite> LockdownButton { get; } = new LoadableResourceAsset($"{ShortPath}.LockdownButton.png", 115);
    public static LoadableAsset<Sprite> PlacePortalButton { get; } = new LoadableResourceAsset($"{ShortPath}.PortalSprite.png", 115);
    public static LoadableAsset<Sprite> UsePortalButton { get; } = new LoadableResourceAsset($"{ShortPath}.PortalSprite.png", 115);
    public static LoadableAsset<Sprite> InfectButton { get; } = new LoadableResourceAsset($"{ShortPath}.InfectButton.png", 115);
    public static LoadableAsset<Sprite> PlagueDoctorVentButton { get; } = new LoadableResourceAsset($"{ShortPath}.PDVent.png", 1024);
    // Frag button art is 512x512; lower ppu = larger on HUD (Lockdown uses 115).
    public static LoadableAsset<Sprite> FragGiveButton { get; } = new LoadableResourceAsset($"{ShortPath}.FragGive.png", 360);
    public static LoadableAsset<Sprite> FragPassButton { get; } = new LoadableResourceAsset($"{ShortPath}.FragPass.png", 360);

    // Role icons (550 ppu)
    public static LoadableAsset<Sprite> ThiefIcon { get; } = new LoadableResourceAsset($"{ShortPath}.ThiefIcon.png", 550);
    public static LoadableAsset<Sprite> ThiefButton { get; } = new LoadableResourceAsset($"{ShortPath}.ThiefIcon.png", 550);
    public static LoadableAsset<Sprite> DeadlockIcon { get; } = new LoadableResourceAsset($"{ShortPath}.DeadlockIcon.png", 550);
    public static LoadableAsset<Sprite> PortalmakerIcon { get; } = new LoadableResourceAsset($"{ShortPath}.PortalmakerIcon.png", 550);
    public static LoadableAsset<Sprite> RuthlessIcon { get; } = new LoadableResourceAsset($"{ShortPath}.RuthlessIcon.png", 550);
    // These source images are 512x512; lower ppu makes them match larger role icons in settings.
    public static LoadableAsset<Sprite> FragIcon { get; } = new LoadableResourceAsset($"{ShortPath}.FragIcon.png", 275);
    public static LoadableAsset<Sprite> SilencerIcon { get; } = new LoadableResourceAsset($"{ShortPath}.SilencerIcon.png", 275);
    // Role screen icon uses a lower ppu so the 150x150 source renders at a comparable
    // size to the other role icons (which are 700-1024 px at 550 ppu).
    public static LoadableAsset<Sprite> PlagueDoctorIcon { get; } = new LoadableResourceAsset($"{ShortPath}.PlagueDoctorIcon.png", 115);
    public static LoadableAsset<Sprite> InnocentIcon { get; } = new LoadableResourceAsset($"{ShortPath}.InnocentIcon.png", 115);

    // Sentinel icon - used for role icon and button. Lower ppu to match small source image size.
    public static LoadableAsset<Sprite> SentinelIcon { get; } = new LoadableResourceAsset($"{ShortPath}.SentinelIcon.png", 115);

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

    // Dutch Meme Soundpack door SFX - used by DutchMemeSoundpackPatch to replace
    // the vanilla door open/close audio clips when the matching lobby toggle is on.
    public static LoadableAsset<AudioClip> DutchDoorOpen { get; } = new LoadableAudioResourceAsset($"{ShortPath}.DoorOpen.wav");
    public static LoadableAsset<AudioClip> DutchDoorClose { get; } = new LoadableAudioResourceAsset($"{ShortPath}.DoorClose.wav");

    // Modifier icons (550 ppu for modifiers - same as role icons)
    public static LoadableAsset<Sprite> BlindspotIcon { get; } = new LoadableResourceAsset($"{ShortPath}.BlindspotIcon.png", 550);
    public static LoadableAsset<Sprite> FragileIcon { get; } = new LoadableResourceAsset($"{ShortPath}.FragileIcon.png", 550);
    public static LoadableAsset<Sprite> ShuffleIcon { get; } = new LoadableResourceAsset($"{ShortPath}.ShuffleButton.png", 550);
    public static LoadableAsset<Sprite> MisvoteIcon { get; } = new LoadableResourceAsset($"{ShortPath}.MisvoteIcon.png", 550);
    
    // Portal on map (200 ppu)
    public static LoadableAsset<Sprite> PortalSprite { get; } = new LoadableResourceAsset($"{ShortPath}.PortalSprite.png", 200);

    // Beacon on map (550 ppu – source image is ~1024px so high ppu keeps it small in-game)
    public static LoadableAsset<Sprite> BeaconSprite { get; } = new LoadableResourceAsset($"{ShortPath}.BeaconAsset.png", 550);

    // Biohazard icon shown next to fully-infected players in meetings (PD-only view).
    public static LoadableAsset<Sprite> InfectedIcon { get; } = new LoadableResourceAsset($"{ShortPath}.InfectedIcon.png", 300);
}
