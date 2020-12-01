using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class RPC : Node
{

    private String LogPrefix { get => this.IsServer() ? "Server:" : "Client:"; }

    /// <summary>
    /// RPC is a singleton
    /// </summary>
    private static RPC instance;
    public static RPC Instance
    {
        get
        {
            return instance;
        }
    }

    RPC()
    {
        instance = this;
    }

    public override void _Ready()
    {
        // when a player is updated, notify other players
        Signals.PlayerJoinedEvent += OnPlayerJoined;
    }

    public override void _ExitTree()
    {
        Signals.PlayerJoinedEvent -= OnPlayerJoined;
    }

    private void OnPlayerJoined(int networkId)
    {
        if (this.IsServer() && networkId != 1)
        {
            // tell the new player about our players
            SendPlayersUpdated(PlayersManager.Instance.Players, networkId);
        }
    }

    #region Connection RPC Calls

    #endregion

    #region Player RPC Calls

    public void SendMessage(string message)
    {
        var playerMessage = new PlayerMessage(PlayersManager.Instance.Me.Num, message);
        GD.Print($"{LogPrefix} Sending Message {playerMessage}");
        Rpc(nameof(Message), playerMessage.ToArray());
    }

    [RemoteSync]
    public void Message(Godot.Collections.Array data)
    {
        var message = new PlayerMessage().FromArray(data);

        GD.Print($"{LogPrefix} Received PlayerMessage {message} from {GetTree().GetRpcSenderId()}");

        // notify listeners that we have updated PlayerData
        Signals.PublishPlayerMessageEvent(message);
    }

    public void SendPlayerUpdated(PlayerData player)
    {
        // send our peers an update of a player
        GD.Print($"{LogPrefix} Notifying clients about player update: {player}");
        Rpc(nameof(PlayerUpdated), player.ToArray());
    }

    [Remote]
    public void PlayerUpdated(Godot.Collections.Array data)
    {
        var player = new PlayerData().FromArray(data);
        player.FromArray(data);

        GD.Print($"{LogPrefix} Received PlayerUpdated event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

        // notify listeners that we have updated PlayerData
        Signals.PublishPlayerUpdatedEvent(player);

    }

    /// <summary>
    /// Called by the server to notify all clients about new player data
    /// </summary>
    /// <param name="players"></param>
    /// <param name="networkId"></param>
    public void SendPlayersUpdated(List<PlayerData> players, int networkId = 0)
    {
        // servers listen for signals and notify clients
        if (this.IsServer())
        {
            var playersArray = new Godot.Collections.Array(players.Select(p => p.ToArray()));
            if (networkId == 0)
            {
                GD.Print($"{LogPrefix} Sending all players to all clients");
                // we are a server, tell the clients we have a player update
                Rpc(nameof(PlayersUpdated), playersArray);
            }
            else
            {
                GD.Print($"{LogPrefix} Sending players to {networkId}");
                // we are a server, tell the clients we have a player update
                RpcId(networkId, nameof(PlayersUpdated), playersArray);
            }
        }
        else
        {
            GD.PrintErr("A client tried to send a list of all players over Rpc");
        }
    }

    /// <summary>
    /// Method called by the server whenever a client needs to know about player updates
    /// </summary>
    /// <param name="data"></param>
    [Remote]
    public void PlayersUpdated(Godot.Collections.Array data)
    {
        var players = new PlayerData[data.Count];
        for (int i = 0; i < data.Count; i++)
        {
            var playerData = data[i] as Godot.Collections.Array;
            if (playerData != null)
            {
                var player = new PlayerData().FromArray(playerData);

                GD.Print($"{LogPrefix} Received PlayerUpdated event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

                // notify listeners that we have updated PlayerData
                Signals.PublishPlayerUpdatedEvent(player);
            }
            else
            {
                GD.PrintErr("Failed to convert array of player arrays in PlayerData: " + data[i].ToString());
            }
        }
    }
    #endregion

    /// <summary>
    /// Sent by the server to notify players the game is started
    /// </summary>
    /// <param name="networkId"></param>
    public void SendPostStartGame(int networkId = 0)
    {
        if (networkId == 0)
        {
            Rpc(nameof(PostStartGame));
        }
        else
        {
            RpcId(networkId, nameof(PostStartGame));
        }
    }

    [RemoteSync]
    public void PostStartGame()
    {
        Signals.PublishPostStartGameEvent();
    }

    #region Game Events

    public void SendDayPassed(int day)
    {
        Rpc(nameof(DayPassed), day);
    }

    [Remote]
    public void DayPassed(int day)
    {
        Signals.PublishDayPassedEvent(day);
    }

    public void SendAsteroidIncomingEvent(Vector2 globalPosition, int asteroidStrength, FallingAsteroid asteroid)
    {
        Rpc(nameof(AsteroidIncomingEvent), globalPosition, asteroidStrength, asteroid.ToArray());
    }

    [Remote]
    public void AsteroidIncomingEvent(Vector2 globalPosition, int asteroidStrength, Godot.Collections.Array asteroidData)
    {
        FallingAsteroid asteroid = new FallingAsteroid().FromArray(asteroidData);
        Signals.PublishAsteroidIncomingEvent(globalPosition, asteroidStrength, asteroid);
    }

    public void SendAsteroidTimeEstimate(int asteroidId, int size, float timeToImpact)
    {
        Rpc(nameof(AsteroidTimeEstimate), asteroidId, size, timeToImpact);
    }

    [Remote]
    public void AsteroidTimeEstimate(int asteroidId, int size, float timeToImpact)
    {
        Signals.PublishAsteroidTimeEstimateEvent(asteroidId, size, timeToImpact);
    }

    public void SendAsteroidWaveStarted(int wave, int waves)
    {
        Rpc(nameof(AsteroidWaveStarted), wave, waves);
    }

    [Remote]
    public void AsteroidWaveStarted(int wave, int waves)
    {
        Signals.PublishAsteroidWaveStartedEvent(wave, waves);
    }

    public void SendAsteroidWaveTimerUpdated(float timeLeft)
    {
        Rpc(nameof(AsteroidWaveTimerUpdated), timeLeft);
    }

    [Remote]
    public void AsteroidWaveTimerUpdated(float timeLeft)
    {
        Signals.PublishAsteroidWaveTimerUpdatedEvent(timeLeft);
    }

    #endregion
}
