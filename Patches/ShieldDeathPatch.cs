using System.Linq;
using System.Reflection;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(PlayerControl))]
public static class ShieldDeathPatch
{
    [HarmonyPatch(nameof(PlayerControl.Die))]
    [HarmonyPostfix]
    public static void DiePostfix(PlayerControl __instance)
    {
        if (__instance == null || __instance.Data == null) return;
        
        var deadPlayerName = __instance.Data.PlayerName;
        DivaniPlugin.Instance.Log.LogInfo($"ShieldDeathPatch: {deadPlayerName} died, checking for shields to remove");
        
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.IsDead) continue;
            
            var modifiers = player.GetModifiers<BaseModifier>().ToList();
            foreach (var modifier in modifiers)
            {
                if (!IsShieldModifier(modifier)) continue;
                
                var sourcePlayer = GetShieldSourcePlayer(modifier);
                if (sourcePlayer != null && sourcePlayer.PlayerId == __instance.PlayerId)
                {
                    DivaniPlugin.Instance.Log.LogInfo($"ShieldDeathPatch: Removing shield from {player.Data.PlayerName} because {deadPlayerName} (shield source) died");
                    player.RemoveModifier(modifier.TypeId, null);
                }
            }
        }
    }
    
    private static bool IsShieldModifier(BaseModifier modifier)
    {
        var type = modifier.GetType();
        while (type != null && type != typeof(object))
        {
            if (type.Name == "BaseShieldModifier")
                return true;
            type = type.BaseType;
        }
        return false;
    }
    
    private static PlayerControl? GetShieldSourcePlayer(BaseModifier modifier)
    {
        var modType = modifier.GetType();
        
        foreach (var prop in modType.GetProperties())
        {
            if (prop.Name == "Player" || prop.Name == "ModifierComponent")
                continue;
            
            if (prop.PropertyType == typeof(PlayerControl) || 
                prop.PropertyType.IsAssignableFrom(typeof(PlayerControl)))
            {
                try
                {
                    var value = prop.GetValue(modifier);
                    if (value is PlayerControl pc && pc != null)
                    {
                        return pc;
                    }
                }
                catch
                {
                }
            }
        }
        
        return null;
    }
}
