using Godot;
using System;

[Tool]
public class PlayerResources : VBoxContainer
{
    Label raw;
    Label power;
    Label science;

    string rawFormat;
    string powerFormat;
    string scienceFormat;

    public override void _Ready()
    {
        raw = GetNode<Label>("RawContainer/Label");
        power = GetNode<Label>("PowerContainer/Label");
        science = GetNode<Label>("ScienceContainer/Label");

        rawFormat = raw.Text;
        powerFormat = power.Text;
        scienceFormat = science.Text;

        Signals.PlayerUpdatedEvent += OnPlayerUpdated;

        // setup color
        var player = PlayersManager.Instance.Me;
        Modulate = player.Color;
        GetNode<TextureRect>("RawContainer/TextureRect").Modulate = player.Color;
        GetNode<TextureRect>("PowerContainer/TextureRect").Modulate = player.Color;
        GetNode<TextureRect>("ScienceContainer/TextureRect").Modulate = player.Color;

        OnPlayerUpdated(player);
    }

    private void OnPlayerUpdated(PlayerData player)
    {
        if (player.Num == PlayersManager.Instance.Me.Num)
        {
            raw.Text = string.Format(rawFormat, player.Resources.Raw);
            power.Text = string.Format(powerFormat, player.Resources.Power);
            science.Text = string.Format(scienceFormat, player.Resources.Science);
        }
    }
}
