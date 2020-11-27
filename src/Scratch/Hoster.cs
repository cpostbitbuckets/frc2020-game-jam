using Godot;
using System;

public class Hoster : Control
{
    Client client;

    Server server;

    Button hostButton;
    Button joinButton;
    Button resetButton;

    LineEdit nameLineEdit;

    public override void _Ready()
    {
        hostButton = FindNode("HostButton") as Button;
        joinButton = FindNode("JoinButton") as Button;
        resetButton = FindNode("ResetButton") as Button;
        nameLineEdit = FindNode("NameLineEdit") as LineEdit;
        hostButton.Connect("pressed", this, nameof(OnHostButtonPressed));
        joinButton.Connect("pressed", this, nameof(OnJoinButtonPressed));
        resetButton.Connect("pressed", this, nameof(OnResetButtonPressed));
        nameLineEdit.Connect("text_entered", this, nameof(OnTextEntered));

        client = (Client)FindNode("Client");
        server = (Server)FindNode("Server");

        PlayersManager.Instance.Players.ForEach(p => OnPlayerUpdated(p));
        Signals.PlayerUpdatedEvent += OnPlayerUpdated;
    }

    public void OnHostButtonPressed()
    {
        server.HostGame();
        // even servers need players sometimes
        Signals.PublishPlayerJoinedEvent(GetTree().GetNetworkUniqueId());

        // update buttons
        hostButton.Text = "Hosting";
        joinButton.Disabled = true;
        hostButton.Disabled = true;
        resetButton.Disabled = false;
    }

    public void OnJoinButtonPressed()
    {
        client.JoinGame("127.0.0.1", 3000);
        joinButton.Text = "Joined";
        joinButton.Disabled = true;
        hostButton.Disabled = true;
        resetButton.Disabled = false;

    }

    public void OnResetButtonPressed()
    {
        client.CloseConnection();
        hostButton.Text = "Host";
        joinButton.Text = "Join";
        joinButton.Disabled = false;
        hostButton.Disabled = false;
    }

    public void OnTextEntered(string newText)
    {
        var player = PlayersManager.Instance.Me;
        if (player != null)
        {
            GD.Print("Client: Adding network id to player name.");
            player.Name = nameLineEdit.Text;

            // notify our listeners that we have an updated player
            Signals.PublishPlayerUpdatedEvent(player);

            // notify any network peers that we have an updated player
            RemoteSignals.PublishPlayerUpdatedEvent(player);
        }

    }


    private void OnPlayerUpdated(PlayerData player)
    {
        Label playerLabel = FindNode("Player" + player.Num) as Label;
        playerLabel.Text = player.ToString();
        playerLabel.Modulate = player.Color;
    }


}
