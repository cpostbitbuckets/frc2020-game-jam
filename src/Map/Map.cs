using Godot;
using System;
using System.Collections.Generic;

public class Map : Node2D
{
    GameBuilding instancedScene;

    public List<Territory> Territories { get; private set; }

    private AudioStreamPlayer click;
    private AudioStreamPlayer asteroidTerritoryStrike;

    public override void _Ready()
    {
        click = GetNode<AudioStreamPlayer>("Click");
        asteroidTerritoryStrike = GetNode<AudioStreamPlayer>("AsteroidTerritoryStrike");

        Signals.GameBuildingSelectedEvent += OnGameBuildingSelected;
        Signals.GameBuildingCancelledEvent += OnGameBuildingCancelled;
        Signals.GameBuildingPlacedEvent += OnGameBuildingPlaced;
        Signals.AsteroidImpactEvent += OnAsteroidImpact;

        var buildings = this.GetAllNodesOfType<GameBuilding>();
        BuildingsManager.Instance.InitBuildings(buildings);

        // load in all territories
        Territories = this.GetAllNodesOfType<Territory>();

        // Tell all the AI Players on this map about the territories
        // so they can use it to make choices
        var aiPlayers = this.GetAllNodesOfType<AIPlayer>();
        aiPlayers.ForEach(p => p.Territories = Territories);
    }

    public override void _ExitTree()
    {
        Signals.GameBuildingSelectedEvent -= OnGameBuildingSelected;
        Signals.GameBuildingCancelledEvent -= OnGameBuildingCancelled;
        Signals.GameBuildingPlacedEvent -= OnGameBuildingPlaced;
        Signals.AsteroidImpactEvent -= OnAsteroidImpact;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (instancedScene != null)
        {
            instancedScene.Position = GetLocalMousePosition();
            if (instancedScene.Placeable)
            {
                Input.SetMouseMode(Input.MouseMode.Hidden);
                Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
            }
            else
            {
                Input.SetMouseMode(Input.MouseMode.Visible);
                Input.SetDefaultCursorShape(Input.CursorShape.Forbidden);
            }
        }
        else
        {
            Input.SetMouseMode(Input.MouseMode.Visible);
            Input.SetDefaultCursorShape(Input.CursorShape.Forbidden);
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsActionPressed("left_click") && instancedScene != null && instancedScene.Placeable)
        {
            click.Play(.2f);
            var position = GetLocalMousePosition();
            var buildingId = BuildingsManager.Instance.GetNextId();
            Signals.PublishGameBuildingPlacedEvent(buildingId, PlayersManager.Instance.Me.Num, instancedScene.Type, position);
            RemoteSignals.PublishGameBuildingPlacedEvent(buildingId, PlayersManager.Instance.Me.Num, instancedScene.Type, position);

            // free the scene
            OnGameBuildingCancelled();
        }
    }


    #region Event Handlers

    private void OnGameBuildingSelected(GameBuildingType type)
    {
        // hide the mouse because the building becomes our mouse
        Input.SetMouseMode(Input.MouseMode.Hidden);

        var scene = ResourceLoader.Load<PackedScene>(type.GetScenePath());
        instancedScene = (GameBuilding)scene.Instance();
        instancedScene.NewlySpawned = true;
        instancedScene.PlayerNum = PlayersManager.Instance.Me.Num;
        instancedScene.Type = type;
        AddChild(instancedScene);
    }

    private void OnGameBuildingCancelled()
    {
        if (instancedScene != null)
        {
            instancedScene.QueueFree();
            instancedScene = null;
        }
    }

    private void OnGameBuildingPlaced(string buildingId, int playerNum, GameBuildingType type, Vector2 position)
    {
        var scene = ResourceLoader.Load<PackedScene>(type.GetScenePath());
        GameBuilding building = (GameBuilding)scene.Instance();
        building.PlayerNum = playerNum;
        building.Position = position;
        building.BuildingId = buildingId;
        building.Active = true;
        AddChild(building);

        PlayersManager.Instance.GetPlayer(playerNum).AddScore(ScoreType.BuildingBuilt);
    }

    private void OnAsteroidImpact(int asteroidId, Vector2 impactPoint, int explosionRadius)
    {
        var explosionArea = new Area2D();
        var shape = new CircleShape2D();
        shape.Radius = explosionRadius;

        var collision = new CollisionShape2D();
        collision.Shape = shape;

        explosionArea.AddChild(collision);
        explosionArea.GlobalPosition = impactPoint;
        CallDeferred(nameof(ImpactDeferred), explosionArea);
    }

    private void ImpactDeferred(Area2D explosionArea)
    {
        AddChild(explosionArea);
        explosionArea.Connect("area_entered", this, nameof(OnImpactRegistered), new Godot.Collections.Array(new Area2D[] { explosionArea }));
    }

    private void OnImpactRegistered(Area2D target, Area2D explosionArea)
    {
        // go through each overlapping area for the explosion
        foreach (Node node in explosionArea.GetOverlappingAreas())
        {
            // if we are overlapping a territoryArea, destroy all its buildings
            if (node is TerritoryArea territoryArea)
            {
                // bye bye poor buildings
                territoryArea.GetBuildings().ForEach(b => b.QueueFree());

                // play some dramatic sounds so the user knows we were struck
                asteroidTerritoryStrike.Play();

                // All of our TerritoryAreas should have a Territory as a child that we
                // can mark as destroyed. Territories are weird because they are a CollisionPolygon2D in the 
                // editor for easy drawing, but at runtime they are reorganized like this:
                // - TerritoryArea
                //   - Territory
                //     - Polygon2D
                //     - Center
                //     - Smoke
                if (territoryArea.GetChildCount() > 0)
                {
                    // mark this as destroyed so the smoke effect will be visible
                    // but don't publish this Destroyed event unless it's the first time
                    // the territory becomes destroyed
                    if (territoryArea.GetChild(0) is Territory territory && territory.Type != TerritoryType.Destroyed)
                    {
                        territory.Type = TerritoryType.Destroyed;
                        Signals.PublishTerritoryDestroyedEvent(territory);
                    }
                }
            }
        }

        // free our explosion area
        explosionArea.QueueFree();
    }

    #endregion
}
