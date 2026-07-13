using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using DivaniMods.Options;
using TownOfUs.Assets;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class TasklisttwoModifier : BaseModifier
{
    public override string ModifierName => "Task list two";
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.OverworkedIcon;
    public override bool HideOnUi => true;


var lt = OptionsGroupSingleton<OverworkedOptions>.Instance.ExtraLongTasks.Value;
var st = OptionsGroupSingleton<OverworkedOptions>.Instance.ExtraShortTasks.Value;
var ct = OptionsGroupSingleton<OverworkedOptions>.Instance.ExtraCommonTasks.Value;
var overworkedRole = CustomRoleUtils.GetActiveRolesOfType<OverworkedRole>().FirstOrDefault(x => x.IsLover(Player));

PlayerTask.GetOrCreateTask<LongTask>(overworkedRole, lt);

PlayerTask.GetOrCreateTask<ShortTask>(overworkedRole, st);

PlayerTask.GetOrCreateTask<CommonTask>(overworkedRole, ct);

public static string TaskInfo(this PlayerControl player)
    {
        var completed = player.myTasks.ToArray().Count(x => x.IsComplete);
        var totalTasks = player.myTasks.ToArray()
            .Count(x => !PlayerTask.TaskIsEmergency(x) && !x.TryCast<ImportantTextTask>());

if (completed = totaltasks)
    {
            winType = 1;
            GameHistory.WinningFaction =
                $"<color=#{Palette.CrewmateBlue.ToHtmlStringRGBA()}>{TouLocale.Get("CrewmateWin")}</color>";
     }
}