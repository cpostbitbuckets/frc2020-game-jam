using Godot;
using System;

public class Main : MarginContainer
{

    WindowDialog hostWindow;
    WindowDialog joinWindow;
    LineEdit hostEdit;
    LineEdit hostPortEdit;
    LineEdit joinPortEdit;

    private bool joining = false;

    public override void _Ready()
    {
        Signals.PlayerUpdatedEvent += OnPlayerUpdated;

        FindNode("ExitButton").Connect("pressed", this, nameof(OnExitButtonPressed));
        FindNode("NewGameButton").Connect("pressed", this, nameof(OnNewGameButtonPressed));
    }

    void OnExitButtonPressed()
    {
        GetTree().Quit();
    }

    void OnNewGameButtonPressed()
    {
        GetTree().ChangeScene("res://src/World.tscn");
    }

    void OnPlayerUpdated(PlayerData player)
    {
        if (joining && this.IsClient() && player.NetworkId == this.GetNetworkId())
        {
            GetTree().ChangeScene("res://src/Screens/Lobby.tscn");
        }
    }
}
