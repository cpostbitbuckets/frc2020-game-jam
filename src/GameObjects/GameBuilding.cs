using Godot;
using System;
using System.Collections.Generic;

public abstract class GameBuilding : Area2D
{
    /// <summary>
    /// the index of the player who owns this building
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "1,5")]
    public int PlayerNum { get; set; } = 1;

    [Export]
    public virtual Boolean Active { get; set; }

    /// <summary>
    /// The type of game building we are
    /// </summary>
    /// <value></value>
    public virtual GameBuildingType Type { get => type; set { if (AllowedTypes.Contains(value)) { type = value; } } }
    private GameBuildingType type = GameBuildingType.None;

    protected abstract HashSet<GameBuildingType> AllowedTypes { get; }

    public Boolean IsDefenseBuilding { get => Type == GameBuildingType.Laser || Type == GameBuildingType.Missile || Type == GameBuildingType.Shield || Type == GameBuildingType.Radar; }
    public Boolean IsResourceBuilding { get => Type == GameBuildingType.Mine || Type == GameBuildingType.PowerPlant || Type == GameBuildingType.ScienceLab; }

    public Boolean Placeable { get; private set; }

    /// <summary>
    /// True if this building isn't placed yet, but is being moved by the mouse
    /// </summary>
    /// <value></value>
    public Boolean NewlySpawned { get; set; }

    public String BuildingId { get; set; }

    private CollisionShape2D collisionShape;

    public override void _Ready()
    {
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        Connect("tree_exited", this, nameof(OnFreed));
    }

    /// <summary>
    /// 
    /// </summary>
    public override void _Draw()
    {
        if (NewlySpawned)
        {
            var radius = 0f;
            if (collisionShape.Shape is CircleShape2D circleShape)
            {
                radius = circleShape.Radius;
            }
            else if (collisionShape.Shape is CapsuleShape2D capsuleShape)
            {
                radius = capsuleShape.Radius;
            }
            DrawArc(collisionShape.Position, radius, Mathf.Deg2Rad(0), Mathf.Deg2Rad(359), 100, Colors.LightBlue);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        // ignore existing buildings
        if (!NewlySpawned)
        {
            return;
        }

        // if we can't afford it, we can't place it
        if (!PlayersManager.Instance.Me.CanAffordBuilding(Type))
        {
            Placeable = false;
            return;
        }

        // Go through each area overlapping our area. If we are only overlapping a territory of
        // the type we can place on, place it
        var placeable = false;
        foreach (Area2D area in GetOverlappingAreas())
        {
            var territoryArea = area as TerritoryArea;

            if (territoryArea != null)
            {
                // we are overlapping a territory, see if we can place it
                // note, if we are overlapping both a normal and resource territory, we will be
                // able to place.
                if (IsDefenseBuilding && territoryArea.Territory.Type == TerritoryType.Normal)
                {
                    placeable = true;
                }
                else if (IsResourceBuilding && territoryArea.Territory.Type == TerritoryType.Resource)
                {
                    placeable = true;
                }
            }
            else
            {
                // we are overlapping an area that isn't a territory, we can't place
                placeable = false;
            }
        }

        Placeable = placeable;
    }

    /// <summary>
    /// We have been freed, disable this node
    /// </summary>
    protected virtual void OnFreed()
    {
        Active = false;
    }

}
