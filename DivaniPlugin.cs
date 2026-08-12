using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Modules.Localization;
using DivaniMods.Patches;
using DivaniMods.Patches.WinConditions;
using TownOfUs.Patches;
using DivaniMods.Utilities;
using MiraAPI.Roles;
using DivaniMods.Roles.Impostor.ImpostorPower;

namespace DivaniMods;

[BepInPlugin(Id, "Divani Mods", Version)]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency("mira.api")]
[BepInDependency("auavengers.tou.mira", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(
    "com.edgetel.perfectcomms",
    BepInDependency.DependencyFlags.SoftDependency)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public class DivaniPlugin : BasePlugin, IMiraPlugin
{
    public string GetAbbreviatedModName() =>
        $"<b><color={DivaniCreditsColorPatch.CreditsColor}>DM</color></b>";

    public const string Id = "com.divani.mods";
    public const string Version = "1.3.7";
    
    public static DivaniPlugin Instance { get; private set; } = null!;
    public new ManualLogSource Log => base.Log;
    
    public Harmony Harmony { get; } = new(Id);
    public string OptionsTitleText => "Divani Mods";
    
    public ConfigFile GetConfigFile() => Config;
    
    public override void Load()
    {
        Instance = this;
        Harmony.PatchAll();
        DemolitionistPatches.Register(Log);
        DemolitionistNumpad.Register(Harmony, Log);
        FragileTownOfUsButtonPatch.Initialize(Harmony);
        NullifiedPatch.Initialize(Harmony);
        SniperSerialKillerKill.Initialize(Harmony);
        DutchMemeSoundpackPatch.Register(Harmony);
        VersionDisplay.Register();
        DivaniModAnnouncementPatch.EnsureLoaded();
        DivaniLocale.Register();
        DivaniWikiTermsPatch.RegisterLocale();
        WinConditionRegistry.Register(new BetrayerWinCondition());
        WinConditionRegistry.Register(new ThiefQuotaDrawWinCondition());
        WinConditionRegistry.Register(new InnocentLoverWinCondition());
        Log.LogInfo($"Divani Mods v{Version} loaded successfully!");
        if (!IL2CPPChainloader.Instance.Plugins.ContainsKey(
                "com.edgetel.perfectcomms"))
            return;
        PerfectCommsVoiceIntegration.Register();
    }
}
