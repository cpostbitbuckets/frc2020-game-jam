using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Leaderboard : Control
{
    public override void _Ready()
    {
        var font = new DynamicFont();
        font.FontData = ResourceLoader.Load<DynamicFontData>("res://assets/TechTreeFont.ttf");
        font.Size = 28;

        for (var i = 1; i < 6; i++)
        {
            var nodePath = GetNodePath(i);
            GetNode<Label>($"{nodePath}/Name").AddFontOverride("font", font);
            GetNode<Label>($"{nodePath}/Score").AddFontOverride("font", font);
            GetNode<Label>($"{nodePath}/Raw").AddFontOverride("font", font);
            GetNode<Label>($"{nodePath}/Science").AddFontOverride("font", font);
            GetNode<Label>($"{nodePath}/Power").AddFontOverride("font", font);
        }

        Signals.PlayerUpdatedEvent += OnPlayerUpdated;

        SetLeaderboardRows();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Signals.PlayerUpdatedEvent -= OnPlayerUpdated;
    }

    void SetLeaderboardRows()
    {
        // get a list of players sorted by score
        var sortedPlayers = new List<PlayerData>(PlayersManager.Instance.Players);
        sortedPlayers.Sort((p1, p2) => p2.Score.CompareTo(p1.Score));

        for (var i = 1; i < 6; i++)
        {
            var player = sortedPlayers[i - 1];
            var nodePath = GetNodePath(i);
            GetNode<Label>($"{nodePath}/Name").Modulate = player.Color;
            GetNode<Label>($"{nodePath}/Name").Text = player.Name;
            GetNode<Label>($"{nodePath}/Score").Text = player.Score.ToString();
            GetNode<Label>($"{nodePath}/Raw").Text = player.Resources.Raw.ToString();
            GetNode<Label>($"{nodePath}/Science").Text = player.Resources.Science.ToString();
            GetNode<Label>($"{nodePath}/Power").Text = player.Resources.Power.ToString();
        }
    }

    string GetNodePath(int playerNum)
    {
        return $"Background/Leaderboard/Player{playerNum}/Stats";
    }

    // when the player is updated, update our view
    void OnPlayerUpdated(PlayerData player)
    {
        SetLeaderboardRows();
    }

}
