using Godot;
using System;
using System.Collections.Generic;

public class PlayersManager : Node
{
    /// <summary>
    /// Data for all the players in the game
    /// </summary>
    /// <typeparam name="PlayerData"></typeparam>
    /// <returns></returns>
    public List<PlayerData> Players { get; } = new List<PlayerData>();

    /// <summary>
    /// Messages from ourselves and other players
    /// </summary>
    /// <typeparam name="PlayerMessage"></typeparam>
    /// <returns></returns>
    public List<PlayerMessage> Messages { get; } = new List<PlayerMessage>();

    public Dictionary<int, PlayerData> PlayersByNetworkId { get; } = new Dictionary<int, PlayerData>();

    private PlayerData me;
    public PlayerData Me
    {
        get
        {
            if (me == null)
            {
                if (GetTree().HasNetworkPeer())
                {
                    me = Players.Find(p => p.NetworkId == GetTree().GetNetworkUniqueId());
                }
                else
                {
                    // no network, we are player one
                    me = Players[0];
                }
            }
            return me;
        }
    }

    /// <summary>
    /// PlayersManager is a singleton
    /// </summary>
    private static PlayersManager instance;
    public static PlayersManager Instance
    {
        get
        {
            return instance;
        }
    }

    PlayersManager()
    {
        instance = this;
    }

    public override void _Ready()
    {
        SetupPlayers();
        // Subscribe to some player joined/left events
        Signals.PlayerJoinedEvent += OnPlayerJoined;
        Signals.PlayerLeftEvent += OnPlayerLeft;
        Signals.PlayerUpdatedEvent += OnPlayerUpdated;
        Signals.PlayerMessageEvent += OnPlayerMessage;
    }

    public override void _ExitTree()
    {
        Signals.PlayerJoinedEvent -= OnPlayerJoined;
        Signals.PlayerLeftEvent -= OnPlayerLeft;
        Signals.PlayerUpdatedEvent -= OnPlayerUpdated;
        Signals.PlayerMessageEvent -= OnPlayerMessage;
    }

    public void Reset()
    {
        Players.Clear();
        PlayersByNetworkId.Clear();
        Messages.Clear();
        SetupPlayers();
    }

    /// <summary>
    /// Setup the initial players list with players based on color
    /// </summary>
    public void SetupPlayers()
    {
        string[] names = new string[Constants.Names.Length];
        Constants.Names.CopyTo(names, 0);

        var rng = new Random();
        rng.Shuffle(names);

        for (var i = 0; i < Constants.NumPlayers; i++)
        {
            var num = i + 1;
            Players.Add(new PlayerData
            {
                Num = num,
                Name = names[i],
                Color = Constants.PlayerColors[num],
                AIControlled = true,
                Ready = true,
                Resources = {
                    Raw = Constants.StartingResources.Raw,
                    Power = Constants.StartingResources.Power,
                    Science = Constants.StartingResources.Science,
                }
            });

            if (GameSettings.Instance.Easy)
            {
                Players[i].Resources.Raw *= 2;
                Players[i].Resources.Power *= 2;
            }

            Signals.PublishPlayerUpdatedEvent(Players[i]);
        }
    }

    #region Event Listeners

    /// <summary>
    /// A player has joined, find them a match in our players list and notify any
    /// listeners that we have a new human player
    /// </summary>
    /// <param name="networkId"></param>
    private void OnPlayerJoined(int networkId)
    {
        // find an AI controlled player
        var emptyPlayer = Players.Find(p => p.AIControlled);
        if (emptyPlayer != null)
        {
            // claim this player for the network user
            emptyPlayer.AIControlled = false;
            emptyPlayer.Ready = false;
            emptyPlayer.NetworkId = networkId;

            GD.Print($"{emptyPlayer} joined and is added to the player registry");
            Signals.PublishPlayerUpdatedEvent(emptyPlayer);
        }
        else
        {
            GD.PrintErr($"Player with networkId {networkId} tried to join, but we couldn't find any empty player slots!");
        }
    }

    void OnPlayerMessage(PlayerMessage message)
    {
        Messages.Add(message);
    }

    public PlayerData GetPlayer(int playerNum)
    {
        if (playerNum > 0 && playerNum <= Players.Count)
        {
            return Players[playerNum - 1];
        }
        return null;
    }

    public PlayerData GetNetworkPlayer(int networkId)
    {
        return Players.Find(p => p.NetworkId == networkId);
    }

    /// <summary>
    /// If a player leaves, remove them from our registry and notify any listeners that we
    /// have a new AI player
    /// </summary>
    /// <param name="networkId"></param>
    private void OnPlayerLeft(int networkId)
    {
        var player = Players.Find(p => p.NetworkId == networkId);
        if (player != null)
        {
            GD.Print($"{player} left and will be removed from the player registry");
            player.AIControlled = true;
            player.NetworkId = 0;
            Signals.PublishPlayerUpdatedEvent(player);
        }
        else
        {
            GD.PrintErr($"Player with networkId {networkId} left the game, but they werne't in our player registry!");
        }
    }

    private void OnPlayerUpdated(PlayerData player)
    {
        var existingPlayer = GetPlayer(player.Num);
        if (existingPlayer != null)
        {
            existingPlayer.From(player);
            // GD.Print($"{player} updated in player registry");
        }
    }

    #endregion

}
