using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using DivaniMods.Modifiers.Crewmate.CrewmatePower;
using DivaniMods.Modules;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using TownOfUs.Modifiers;

namespace DivaniMods.Events.Crewmate.CrewmatePower;

public static class OverworkedEvents
{
    public static string TaskInfo(this PlayerControl player)
    {
        var completed = player.myTasks.ToArray().Count(x => x.IsComplete);
        var totaltasks = player.myTasks.ToArray().Count(x => !PlayerTask.TaskIsEmergency(x) && !x.TryCast<ImportantTaskText>());

        if (completed = totaltasks)
        {
            RpcAddModifier<TasklisttwoModifier>();
        }
        else void
    }
}