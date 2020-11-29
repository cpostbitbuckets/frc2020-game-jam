using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class RPC : Node
{

    private String LogPrefix { get => this.IsServer() ? "Server:" : "Client:"; }

    public override void _Ready()
    {
        // when a player is updated, notify other players
        Signals.PlayerJoinedEvent += OnPlayerJoined;
        RemoteSignals.PlayerUpdatedEvent += SendPlayerUpdated;

    }

    private void OnPlayerJoined(int networkId)
    {
        if (this.IsServer())
        {
            // tell the new player about our players
            SendPlayers(PlayersManager.Instance.Players, networkId);
        }
    }

    #region Connection RPC Calls

    #endregion

    #region Player RPC Calls
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

    public void SendPlayers(List<PlayerData> players, int networkId = 0)
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
}
