using Godot;
using System;

/// <summary>
/// A server for managing the game
/// </summary>
public class Server : Node
{

    public override void _Ready()
    {

        // signals for when a player connects to us
        GetTree().Connect("network_peer_connected", this, nameof(OnPlayerConnected));
        GetTree().Connect("network_peer_disconnected", this, nameof(OnPlayerDisconnected));
    }

    private void OnPlayerConnected(int id)
    {
        if (GetTree().IsNetworkServer())
        {
            // if we are the server, we know a new player has connected
            GD.Print($"Server: Player {id} connected to server.");
            Signals.PublishPlayerJoinedEvent(id);
        }
    }

    private void OnPlayerDisconnected(int id)
    {
        if (GetTree().IsNetworkServer())
        {
            // if we are the server, we know a new player has connected
            GD.Print($"Server: Player {id} disconnected from server.");
            Signals.PublishPlayerLeftEvent(id);
        }
    }

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
}
