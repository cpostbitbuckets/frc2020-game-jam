using Godot;
using System;

public class Tester : Node2D
{
    public override void _Ready()
    {
        GetTree().Root.GetNode("Network").Call("host_game", true);
        GetTree().Root.GetNode("Server").Call("begin_game", true);
        GetTree().Root.GetNode("RPC").Call("send_ready_to_start", true);
        GetTree().Root.GetNode("RPC").Call("send_post_start_game");
        GetTree().ChangeScene("res://src/World.tscn");
    }

}
