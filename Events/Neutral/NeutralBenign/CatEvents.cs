using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralBenign;
using TownOfUs;
using TownOfUs.Buttons;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Localization;
using TownOfUs.Networking;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Events.Neutral.NeutralBenign;

public static class CatEvents
{
    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent @event)
 {  
    if (!AmongUsClient.Instance.AmHost) return;

    var cat = @event.Target;
    var killerRole = @event.Source.Data.Role.Role;
    
    @event.cancel();


    RoleManager.Instance.Setrole(cat, killerRole);
 }
}
