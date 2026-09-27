using MiraAPI.Roles;
using TownOfUs.Roles;

namespace DivaniMods.Interfaces;

public interface IDivaniRole : ITownOfUsRole
{
    string ICustomRole.IdPrefix => "DivaniMods.Role";

    string ICustomRole.IdPart
    {
        get
        {
            var typeName = GetType().Name;

            return typeName.EndsWith("Role", StringComparison.Ordinal)
                ? typeName[..^4]
                : typeName;
        }
    }
}