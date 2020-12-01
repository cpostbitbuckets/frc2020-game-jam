using Godot;
using System;
using System.Collections.Generic;

public class Lobby : MarginContainer
{
    TextEdit chat;
    LineEdit chatMessage;
    Button startGameButton;

    public override void _Ready()
    {
        chat = (TextEdit)FindNode("Chat");
        chatMessage = (LineEdit)FindNode("ChatMessage");
        startGameButton = (Button)FindNode("StartGameButton");

        Signals.PreStartGameEvent += OnPreStartGame;
        Signals.PostStartGameEvent += OnPostStartGame;
        Signals.PlayerMessageEvent += OnPlayerMessage;
        Signals.PlayerUpdatedEvent += OnPlayerUpdated;

        chatMessage.Connect("text_entered", this, nameof(OnChatMessageTextEntered));
        FindNode("BackButton").Connect("pressed", this, nameof(OnBackButtonPressed));
        FindNode("ReadyButton").Connect("pressed", this, nameof(OnReadyButtonPressed));
        startGameButton.Connect("pressed", this, nameof(OnStartGameButtonPressed));

        if (this.IsServer())
        {
            startGameButton.Visible = true;
            CheckStartGameButton();
        }

        // Add any player messages when we come into the lobby
        PlayersManager.Instance.Messages.ForEach(m => AddPlayerMessage(m));
    }

    public override void _ExitTree()
    {
        Signals.PreStartGameEvent -= OnPreStartGame;
        Signals.PostStartGameEvent -= OnPostStartGame;
        Signals.PlayerMessageEvent -= OnPlayerMessage;
        Signals.PlayerUpdatedEvent -= OnPlayerUpdated;
    }

    void CheckStartGameButton()
    {
        startGameButton.Disabled = !Server.Instance.ReadyToStart;
    }

    void AddPlayerMessage(PlayerMessage message)
    {
        var player = PlayersManager.Instance.GetPlayer(message.PlayerNum);
        var host = player.Num == 1 ? "Host - " : "";
        chat.Text += $"\n{host}{player.Name}: {message.Message}";
    }

    #region Event Handlers

    void OnChatMessageTextEntered(string newText)
    {
        RPC.Instance.SendMessage(newText);
        chatMessage.Text = "";
    }

    void OnPreStartGame(List<PlayerData> players)
    {
        // Tell the server we are ready
        // RPC.Instance.SendReadyToStart();
    }

    void OnPostStartGame()
    {
        GetTree().ChangeScene("res://src/World.tscn");
    }

    void OnPlayerMessage(PlayerMessage message)
    {
        AddPlayerMessage(message);
    }

    void OnPlayerUpdated(PlayerData player)
    {
        CheckStartGameButton();
    }

    void OnBackButtonPressed()
    {
        if (this.IsServer())
        {
            Server.Instance.CloseConnection();
        }
        else
        {
            Client.Instance.CloseConnection();
        }

        PlayersManager.Instance.Reset();
        Server.Instance.Reset();
        GetTree().ChangeScene("res://src/Main.tscn");
    }

    void OnReadyButtonPressed()
    {
        var me = PlayersManager.Instance.Me;
        me.Ready = !me.Ready;

        Signals.PublishPlayerUpdatedEvent(me, notifyPeers: true);
    }

    void OnStartGameButtonPressed()
    {
        GD.Print("Server: All players ready, starting the game!");
        RPC.Instance.SendPostStartGame();
    }

    #endregion
}
