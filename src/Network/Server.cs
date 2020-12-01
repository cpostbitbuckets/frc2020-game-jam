using Godot;
using System;

/// <summary>
/// A server for managing the game
/// </summary>
public class Server : Node
{

    public bool Started { get; private set; } = false;
    public int Day { get; private set; } = 1;

    /// <summary>
    /// If any player isn't ready to start, the server isn't ready
    /// </summary>
    /// <returns></returns>
    public bool ReadyToStart { get => PlayersManager.Instance.Players.Find(p => !p.Ready) == null; }


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

        // listen for events to notify clients
        Signals.AsteroidIncomingEvent += OnAsteroidIncoming;
        Signals.AsteroidWaveTimerUpdatedEvent += OnAsteroidWaveTimerUpdated;
        Signals.AsteroidWaveStartedEvent += OnAsteroidWaveStarted;
        Signals.AsteroidTimeEstimateEvent += OnAsteroidTimeEstimate;
    }

    public override void _ExitTree()
    {
        dayTimer.Stop();
    }

    public void Reset()
    {
        dayTimer.Stop();
        Day = 0;
        Started = false;
    }

    # region Player Join/Leave Events

    void OnPlayerConnected(int id)
    {
        if (this.IsServer())
        {
            // if we are the server, we know a new player has connected
            GD.Print($"Server: Player {id} connected to server.");
            Signals.PublishPlayerJoinedEvent(id);
            var player = PlayersManager.Instance.GetNetworkPlayer(id);
            RPC.Instance.SendMessage($"{player} has joined the game");
        }
    }

    void OnPlayerDisconnected(int id)
    {
        if (this.IsServer())
        {
            // if we are the server, we know a new player has connected
            GD.Print($"Server: Player {id} disconnected from server.");
            var player = PlayersManager.Instance.GetNetworkPlayer(id);
            RPC.Instance.SendMessage($"{player} has left the game");
            Signals.PublishPlayerLeftEvent(id);
        }
    }

    #endregion

    #region Game State Changes

    /// <summary>
    /// Host a new game, starting a server
    /// </summary>
    public void HostGame(int port = 3000)
    {
        var peer = new NetworkedMultiplayerENet();
        var error = peer.CreateServer(port, Constants.NumPlayers);
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

    #endregion

    #region Game State Changes

    public void BeginGame()
    {
        if (this.IsServer())
        {
            // join our own game
            Signals.PublishPlayerJoinedEvent(GetTree().GetNetworkUniqueId());
        }
    }

    public void PostBeginGame()
    {
        // for single player games, we control player 1
        if (this.IsSinglePlayer())
        {
            PlayersManager.Instance.Players[0].AIControlled = false;
        }
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

        if (this.IsServer())
        {
            RPC.Instance.SendDayPassed(Day);
            RPC.Instance.SendPlayersUpdated(PlayersManager.Instance.Players);
        }
    }

    void OnAsteroidTimeEstimate(int asteroidId, int size, float timeToImpact)
    {
        if (this.IsServer())
        {
            RPC.Instance.SendAsteroidTimeEstimate(asteroidId, size, timeToImpact);
        }
    }

    void OnAsteroidWaveStarted(int wave, int waves)
    {
        if (this.IsServer())
        {
            RPC.Instance.SendAsteroidWaveStarted(wave, waves);
        }
    }

    void OnAsteroidWaveTimerUpdated(float timeLeft)
    {
        if (this.IsServer())
        {
            RPC.Instance.SendAsteroidWaveTimerUpdated(timeLeft);
        }
    }

    void OnAsteroidIncoming(Vector2 globalPosition, int asteroidStrength, FallingAsteroid asteroid)
    {
        if (this.IsServer())
        {
            RPC.Instance.SendAsteroidIncomingEvent(globalPosition, asteroidStrength, asteroid);
        }
    }
    #endregion
}
