using MiraAPI.GameEnd;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using DivaniMods.Roles;
using UnityEngine;

namespace DivaniMods.GameOver;

public sealed class StalkerGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        if (StalkerRole.WinningStalker == null)
        {
            return false;
        }

        return StalkerRole.TriggerStalkerWin;
    }

    public override bool BeforeEndGameSetup(EndGameManager endGameManager)
    {
        if (StalkerRole.WinningStalker?.Data == null)
        {
            return true;
        }

        var stalkerData = StalkerRole.WinningStalker.Data;

        EndGameResult.CachedWinners.Clear();
        EndGameResult.CachedWinners.Add(new CachedPlayerData(stalkerData));

        DivaniPlugin.Instance.Log.LogInfo($"StalkerGameOver: Added Stalker ({stalkerData.PlayerName}) as sole winner.");
        return true;
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.BackgroundBar.material.SetColor(Shader.PropertyToID("_Color"), StalkerRole.StalkerColor);

        var text = UnityEngine.Object.Instantiate(endGameManager.WinText);
        text.text = "Stalker Wins!";
        text.color = StalkerRole.StalkerColor;

        var colorHex = StalkerRole.StalkerColor.ToHtmlStringRGBA();
        GameHistory.WinningFaction = $"<color=#{colorHex}>Stalker Wins</color>";

        var pos = endGameManager.WinText.transform.localPosition;
        pos.y = 1.5f;
        pos += Vector3.down * 0.15f;
        text.transform.localScale = new Vector3(1f, 1f, 1f);
        text.transform.position = pos;
        text.text = $"<size=4>{text.text}</size>";
    }
}
