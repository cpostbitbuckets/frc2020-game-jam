using Godot;
using System;

/// <summary>
/// A client to connect to servers
/// </summary>
public class Client : Node
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // hook up to client specific network events
        GetTree().Connect("server_disconnected", this, nameof(OnServerDisconnected));
        GetTree().Connect("connected_to_server", this, nameof(OnConnectedToServer));
        GetTree().Connect("connection_failed", this, nameof(OnConnectionFailed));
    }

    /// <summary>
    /// Called when the server disconnects us, a client.
    /// </summary>
    public void OnServerDisconnected()
    {
        GD.Print("Client: server disconnected");
    }

    /// <summary>
    /// Called when the we have connected to a server
    /// </summary>
    public void OnConnectedToServer()
    {
        GD.Print("Client: connected to server");
    }

    /// <summary>
    /// Called when our client connection to the server fails
    /// </summary>
    public void OnConnectionFailed()
    {
        GD.Print("Client: connecting to server failed");
    }

    /// <summary>
    /// Join an existing game by address and port
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    public void JoinGame(String address, int port)
    {
        var peer = new NetworkedMultiplayerENet();
        var error = peer.CreateClient(address, port);

        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to connect to server: {address}:{port} Error: {error.ToString()}");
            return;
        }
        GetTree().NetworkPeer = peer;

        GD.Print($"Joined game at {address}:{port}");
    }

    /// <summary>
    /// Close the connection to a server or all clients
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
}
