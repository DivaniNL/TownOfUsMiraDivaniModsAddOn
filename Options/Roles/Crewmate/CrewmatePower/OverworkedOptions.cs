using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmatePower;

namespace DivaniMods.Options;

public class OverworkedOptions : AbstractOptionGroup<OverworkedRole>
{
    public override string GroupName => "Overworked";

    public ModdedNumberOption ExtraLongTasks { get; } = new(
        "Extra Long tasks", 1f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ExtraShortTasks { get; } = new(
        "Extra Short tasks", 1f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ExtraCommonTasks { get; } = new(
        "Extra Common tasks", 1f, 1f, 5f, 1f, MiraNumberSuffixes.None);
}