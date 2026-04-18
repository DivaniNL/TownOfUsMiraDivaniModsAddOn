using Reactor.Utilities;

namespace DivaniMods.Patches;

public static class VersionDisplay
{
    public static void Register()
    {
        ReactorCredits.Register<DivaniPlugin>(ReactorCredits.AlwaysShow);
    }
}
