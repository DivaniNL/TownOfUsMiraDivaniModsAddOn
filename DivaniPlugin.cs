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
using DivaniMods.Patches;

namespace DivaniMods;

[BepInPlugin(Id, "Divani Mods", Version)]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency("mira.api")]
[BepInDependency("auavengers.tou.mira", BepInDependency.DependencyFlags.SoftDependency)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public class DivaniPlugin : BasePlugin, IMiraPlugin
{
    public const string Id = "com.divani.mods";
    public const string Version = "1.1.0";
    
    public static DivaniPlugin Instance { get; private set; } = null!;
    public new ManualLogSource Log => base.Log;
    
    public Harmony Harmony { get; } = new(Id);
    public string OptionsTitleText => "Divani Mods";
    
    public ConfigFile GetConfigFile() => Config;
    
    public override void Load()
    {
        Instance = this;
        Harmony.PatchAll();
        FragileTownOfUsButtonPatch.Initialize(Harmony);
        RuthlessEventHandler.Initialize(Harmony);
        DutchMemeSoundpackPatch.Register(Harmony);
        VersionDisplay.Register();
        Log.LogInfo("Divani Mods loaded successfully!");
    }
}
