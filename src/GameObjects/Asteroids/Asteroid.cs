using Godot;
using System;

public class Asteroid : Area2D
{
    public int? Id { get => parent?.Id; }
    public bool? Destroyed { get => parent?.Destroyed; }

    FallingAsteroid parent;

    public override void _Ready()
    {
        parent = GetParent<FallingAsteroid>();

    }

    public void Damage(int damage)
    {
        GetParent<FallingAsteroid>()?.Damage(damage);
    }
}
