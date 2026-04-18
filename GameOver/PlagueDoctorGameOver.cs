using MiraAPI.GameEnd;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using DivaniMods.Roles;
using UnityEngine;

namespace DivaniMods.GameOver;

public sealed class PlagueDoctorGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        // Verify that a Plague Doctor won
        if (PlagueDoctorRole.PlagueDoctorPlayer == null) return false;
        
        // Check if PD triggered the win
        return PlagueDoctorRole.TriggerPlagueDoctorWin;
    }

    public override bool BeforeEndGameSetup(EndGameManager endGameManager)
    {
        // Ensure PD is in the winners list (important when PD is dead)
        if (PlagueDoctorRole.PlagueDoctorPlayer?.Data == null) return true;
        
        var pdData = PlagueDoctorRole.PlagueDoctorPlayer.Data;
        
        // Clear winners and add only PD (solo win)
        EndGameResult.CachedWinners.Clear();
        
        var winnerData = new CachedPlayerData(pdData);
        EndGameResult.CachedWinners.Add(winnerData);
        
        DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctorGameOver: Added PD ({pdData.PlayerName}) to winners list. IsDead: {pdData.IsDead}");
        
        return true;
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        // Set background color
        endGameManager.BackgroundBar.material.SetColor(Shader.PropertyToID("_Color"), PlagueDoctorRole.PlagueDoctorColor);

        // Create the "Plague Doctor Wins!" subtitle text
        var text = UnityEngine.Object.Instantiate(endGameManager.WinText);
        text.text = "Plague Doctor Wins!";
        text.color = PlagueDoctorRole.PlagueDoctorColor;
        
        // Set the winning faction for game summary
        var colorHex = PlagueDoctorRole.PlagueDoctorColor.ToHtmlStringRGBA();
        GameHistory.WinningFaction = $"<color=#{colorHex}>Plague Doctor Wins</color>";

        // Position the text below "Victory"
        var pos = endGameManager.WinText.transform.localPosition;
        pos.y = 1.5f;
        pos += Vector3.down * 0.15f;
        text.transform.localScale = new Vector3(1f, 1f, 1f);
        text.transform.position = pos;
        text.text = $"<size=4>{text.text}</size>";
    }
}
