using Godot;
using System;

[Tool]
public class Territory : CollisionPolygon2D
{
    public string TypeName { get => nameof(Territory); }

    public static Color[] playerColors = new Color[] {
        Colors.Black,
        new Color("c33232"),
        new Color("1f8ba7"),
        new Color("43a43e"),
        new Color("8d29cb"),
        new Color("b88628")
    };

    // PlayerColors PlayerColors { get; set; }

    #region Exports

    [Export(PropertyHint.Range, "0, 5")]
    public int TerritoryOwner
    {
        get => territoryOwner;
        set
        {
            territoryOwner = value;
            if (playerColors != null && territoryOwner >= 0 && territoryOwner < playerColors.Length)
            {
                Color = playerColors[TerritoryOwner];
            }
        }
    }

    [Export]
    public TerritoryType Type
    {
        get => type; set
        {
            type = value;
            if (polygon2D != null && smoke != null)
            {
                polygon2D.Color = GetPolygonColor();
                if (Type == TerritoryType.Destroyed && smoke != null)
                {
                    smoke.Visible = true;
                }
                else
                {
                    smoke.Visible = false;
                }
            }
        }
    }
    private TerritoryType type = TerritoryType.Normal;

    private int territoryOwner = 0;

    #endregion

    private Color color;
    private Color highlightColor = Colors.White;
    public Color Color
    {
        get => color; set
        {
            color = value;
            if (polygon2D != null)
            {
                polygon2D.Color = GetPolygonColor();
            }
        }
    }

    private Polygon2D polygon2D;

    public Vector2 Center { get; set; }

    private Node2D smoke;

    private Area2D area2d;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // PlayerColors = ResourceLoader.Load<PlayerColors>("res://assets/PlayerColorsMono.tres");

        // set the color to the current owner's color
        Color = playerColors[TerritoryOwner];
        highlightColor = Color.Lightened(.2f);

        // setup the polygon 2D to match our CollisionPolygon2D
        polygon2D = GetNode<Polygon2D>("Polygon2D");
        polygon2D.Color = GetPolygonColor();
        polygon2D.Polygon = Polygon;

        smoke = GetNode<Node2D>("Smoke");
        area2d = GetNode<Area2D>("Area2D");

        // wire up some mouse events
        area2d.Connect("mouse_entered", this, nameof(OnArea2DMouseEntered));
        area2d.Connect("mouse_exited", this, nameof(OnArea2DMouseExited));

        if (!Engine.EditorHint)
        {
            CallDeferred("ReparentArea2D");

            // move our local Position2D to the computed center of this
            // node and update the Center value to it's GlobalPosition
            Position2D center = GetNode<Position2D>("Center");
            center.Position = CalculateCenter();
            Center = center.GlobalPosition;
            smoke.GlobalPosition = Center;
            GetNode<Sprite>("Sprite").GlobalPosition = Center;
        }
    }

    private void ReparentArea2D()
    {
        // make this area2d our parent so we can create a polygon2d in the editor, but
        // use an Area2D with a polygon shape and collision box in game

        RemoveChild(area2d);
        GetParent().AddChild(area2d);
        GetParent().RemoveChild(this);
        area2d.AddChild(this);

    }

    private Color GetPolygonColor()
    {
        switch (Type)
        {
            case TerritoryType.Resource:
                return Color.Lightened(.3f);
            case TerritoryType.Destroyed:
                return Colors.OrangeRed;
            case TerritoryType.Normal:
            default:
                return Color;

        }
    }

    private Vector2 CalculateCenter()
    {
        var minX = float.PositiveInfinity;
        var minY = float.PositiveInfinity;
        var maxX = float.NegativeInfinity;
        var maxY = float.NegativeInfinity;

        foreach (var vector in Polygon)
        {
            if (vector.x < minX)
            {
                minX = vector.x;
            }
            if (vector.y < minY)
            {
                minY = vector.y;
            }
            if (vector.x > maxX)
            {
                maxX = vector.x;
            }
            if (vector.y > maxY)
            {
                maxY = vector.y;
            }
        }
        return new Vector2((maxX - minX) / 2 + minX, (maxY - minY) / 2 + minY);
    }

    #region Events

    private void OnArea2DMouseEntered()
    {
        polygon2D.Color = highlightColor;
        // GetNode<Sprite>("Sprite").Show();
    }

    private void OnArea2DMouseExited()
    {
        polygon2D.Color = GetPolygonColor();
        // GetNode<Sprite>("Sprite").Hide();
    }

    #endregion

}
