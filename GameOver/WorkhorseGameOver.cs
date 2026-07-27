using System.Linq;
using MiraAPI.GameEnd;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DivaniMods.GameOver;

public sealed class WorkhorseGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        if (winners == null || winners.Length == 0)
        {
            return false;
        }

        return winners.Any(w => w?.Object != null && w.Object.Data?.Role is WorkhorseRole &&
                                w.Object.HasModifier<EgotistModifier>());
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        var winColor = WorkhorseRole.WorkhorseColor;
        var winText = "Workhorse Wins";

        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, winColor);
        GameHistory.WinningFaction = $"<color=#{winColor.ToHtmlStringRGBA()}>{winText}</color>";

        var text = Object.Instantiate(endGameManager.WinText);
        text.text = $"<size=4>{winText}!</size>";
        text.color = winColor;

        var pos = endGameManager.WinText.transform.localPosition;
        pos.y = 1.5f;
        pos += Vector3.down * 0.15f;
        text.transform.localScale = new Vector3(1f, 1f, 1f);
        text.transform.position = pos;
    }
}
