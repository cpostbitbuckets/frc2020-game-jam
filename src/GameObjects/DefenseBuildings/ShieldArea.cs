using Godot;
using System;

public class ShieldArea : Area2D
{
    public float Radius
    {
        get => radius;
        set
        {
            if (radius != value)
            {
                radius = value;
                ResizeRadius();
            }
        }
    }
    private float radius = 256.0f;

    CircleShape2D shape;

    CollisionShape2D collision;

    private Shield parent;

    public Sprite Sprite { get; private set; }

    public bool Active { get; set; }

    public int? Health { get => parent?.Health; }

    public override void _Ready()
    {
        Sprite = GetNode<Sprite>("Sprite");
        shape = new CircleShape2D();
        collision = new CollisionShape2D();
        parent = GetParent<Shield>();

        shape.Radius = radius;
        collision.Shape = shape;
        AddChild(collision);

        ResizeRadius();
    }

    private void ResizeRadius()
    {
        shape.Radius = radius;
        Sprite.Scale = new Vector2(radius * 2 / 512, radius * 2 / 512);
    }

    public void Damage(int damage)
    {
        parent.Damage(damage);
        ClientSignals.PublishShieldDamagedEvent(parent.BuildingId, damage);
    }
}
