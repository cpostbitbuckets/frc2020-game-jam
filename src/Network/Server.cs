using Godot;
using System;

/// <summary>
/// A server for managing the game
/// </summary>
public class Server : Node
{

    public bool Started { get; private set; } = false;
    public int Day { get; private set; } = 1;

    private Timer dayTimer;

    /// <summary>
    /// Server is a singleton
    /// </summary>
    private static Server instance;
    public static Server Instance
    {
        get
        {
            return instance;
        }
    }

    Server()
    {
        instance = this;
    }

    public override void _Ready()
    {
        dayTimer = GetNode<Timer>("DayTimer");
        // signals for when a player connects to us
        GetTree().Connect("network_peer_connected", this, nameof(OnPlayerConnected));
        GetTree().Connect("network_peer_disconnected", this, nameof(OnPlayerDisconnected));

        dayTimer.WaitTime = Constants.SecondsPerDay;
        dayTimer.Connect("timeout", this, nameof(OnDayTimerTimeout));
    }

    # region Player Join/Leave Events

    private void OnPlayerConnected(int id)
    {
        if (this.IsServer())
        {
            // if we are the server, we know a new player has connected
            GD.Print($"Server: Player {id} connected to server.");
            Signals.PublishPlayerJoinedEvent(id);
        }
    }

    private void OnPlayerDisconnected(int id)
    {
        if (this.IsServer())
        {
            // if we are the server, we know a new player has connected
            GD.Print($"Server: Player {id} disconnected from server.");
            Signals.PublishPlayerLeftEvent(id);
        }
    }

    #endregion

    #region Game State Changes

    /// <summary>
    /// Host a new game, starting a server
    /// </summary>
    public void HostGame()
    {
        var peer = new NetworkedMultiplayerENet();
        var error = peer.CreateServer(3000, 5);
        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to create network server: Error: {error.ToString()}");
            return;
        }
        GetTree().NetworkPeer = peer;
        GD.Print("Hosting new game");
    }

    /// <summary>
    /// Close the connection to all clients
    /// </summary>
    public void CloseConnection()
    {
        var peer = GetTree().NetworkPeer as NetworkedMultiplayerENet;
        if (peer != null && peer.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected)
        {
            GD.Print("Closing connection");
            peer.CloseConnection();
        }
        GetTree().NetworkPeer = null;
    }

    public void BeginGame()
    {
        if (this.IsServer())
        {
            // Send some post start game stuff
        }
    }

    public void PostBeginGame()
    {
        if (this.IsServerOrSinglePlayer())
        {
            Started = true;
            dayTimer.Start();
        }
    }

    #endregion

    #region Game Events

    void OnDayTimerTimeout()
    {
        Day++;
        Signals.PublishDayPassedEvent(Day);
    }

    #endregion
}
