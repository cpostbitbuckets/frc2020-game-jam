using Godot;
using System;

public class LaserArea : Area2D
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

    Sprite sprite;

    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Sprite");
        shape = new CircleShape2D();
        collision = new CollisionShape2D();

        shape.Radius = radius;
        collision.Shape = shape;
        AddChild(collision);

        ResizeRadius();
    }

    private void ResizeRadius()
    {
        shape.Radius = radius;
        sprite.Scale = new Vector2(radius * 2 / 512, radius * 2 / 512);
    }

}
