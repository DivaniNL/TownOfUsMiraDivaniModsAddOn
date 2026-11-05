using MiraAPI.Utilities.Assets;
using UnityEngine;
using DivaniMods.Options;

namespace DivaniMods.Interfaces;

public interface INeutralEvilWinOutcomeRole
{
    bool ReachedWinCondition { get; }

    NeutralEvilWinOutcome WinOutcome { get; }

    NeutralEvilWinOutcome EffectiveWinOutcome { get; }

    bool AboutToTorment { get; set; }

    bool HasKilled { get; set; }

    Color RoleColor { get; }

    LoadableAsset<Sprite> WinIcon { get; }
}
