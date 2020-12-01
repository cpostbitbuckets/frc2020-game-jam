using Godot;
using System;

public class FallingAsteroid : Node2D
{
    [Export]
    public int BaseSpeed { get; set; } = 100;

    [Export]
    public int ExplosionRadius { get; set; } = 64;

    [Export]
    public int BaseDistance { get; set; } = 1000;

    [Export]
    public int MaxHealth { get; set; } = 100;

    [Export]
    public int Size { get; set; } = 0;

    // the id of this asteroid. Used for keeping asteroids in sync over
    // the network
    public int Id { get; set; }
    public bool Destroyed { get; set; } = false;

    public float Distance { get; set; }
    public float Speed { get; set; }
    public Vector2 ImpactVector { get; set; } = Vector2.Zero;

    private Area2D impactPoint;
    private Asteroid asteroid;
    private int health;

    /// <summary>
    /// A counter to send network updates about asteroid position
    /// </summary>
    private float deltaSendNetworkUpdate = 0f;

    /// <summary>
    /// Copy constructor to copy another falling asteroid's attributes
    /// onto this one
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public FallingAsteroid From(FallingAsteroid a)
    {
        Id = a.Id;
        BaseSpeed = a.BaseSpeed;
        ExplosionRadius = a.ExplosionRadius;
        BaseDistance = a.BaseDistance;
        MaxHealth = a.MaxHealth;
        Distance = a.Distance;
        Speed = a.Speed;
        ImpactVector = a.ImpactVector;
        SetupInitialState();
        return this;
    }

    public override void _Ready()
    {
        impactPoint = GetNode<Area2D>("ImpactPoint");
        asteroid = GetNode<Asteroid>("Asteroid");

        Distance = (float)(BaseDistance * GD.RandRange(.75, 3));
        Speed = (float)(BaseSpeed * GD.RandRange(.75, 1.25));
        health = MaxHealth;

        SetupInitialState();

        if (this.IsClient())
        {
            // if we aren't the server, we have to listen to position updates and impact
            // events from the server
            Signals.AsteroidPositionUpdatedEvent += OnAsteroidPositionUpdated;
            Signals.AsteroidImpactEvent += OnAsteroidImpact;
            Signals.AsteroidDestroyedEvent += OnAsteroidDestroyed;
        }

        impactPoint.Connect("area_entered", this, nameof(OnImpactPointAreaEntered));
    }

    public override void _ExitTree()
    {
        if (this.IsClient())
        {
            // if we aren't the server, we have to listen to position updates and impact
            // events from the server
            Signals.AsteroidPositionUpdatedEvent -= OnAsteroidPositionUpdated;
            Signals.AsteroidImpactEvent -= OnAsteroidImpact;
            Signals.AsteroidDestroyedEvent -= OnAsteroidDestroyed;
        }

    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        UpdateAsteroidPosition(asteroid.Position + (ImpactVector * Speed * delta));

        // only the server updates asteroids
        // then it sends the new position to each client
        if (this.IsServerOrSinglePlayer())
        {
            // only send asteroid position updates once every couple seconds
            deltaSendNetworkUpdate += delta;
            if (deltaSendNetworkUpdate > 2)
            {
                RemoteSignals.PublishAsteroidPositionUpdatedEvent(Id, asteroid.Position);
                deltaSendNetworkUpdate = 0;
            }
        }
    }

    #region Event Handlers

    private void OnAsteroidPositionUpdated(int asteroidId, Vector2 position)
    {
        // Server messages cause this signal to be raised
        if (Id == asteroidId)
        {
            UpdateAsteroidPosition(position);
        }
    }

    private void OnAsteroidImpact(int asteroidId, Vector2 impactPoint, int explosionRadius)
    {
        // clients get this event when the server tells them an asteroid impacts the surface
        if (Id == asteroidId)
        {
            QueueFree();
        }
    }

    private void OnAsteroidDestroyed(int asteroidId, Vector2 position, int size)
    {
        // clients get this event when the server tells them an asteroid is destroyed
        if (Id == asteroidId)
        {
            QueueFree();
        }
    }

    private void OnImpactPointAreaEntered(Area2D area)
    {
        // only detect collisions if we are the server (or single player)
        if (this.IsServerOrSinglePlayer())
        {
            if (area == asteroid)
            {
                Impact();
            }
        }
    }

    #endregion

    public void SetupInitialState()
    {
        // When an asteroid is instantiated, we setup it's location way off screen
        // so it hurtles forward
        asteroid.Position += (new Vector2(1, -1) * Distance);
        ImpactVector = (impactPoint.Position - asteroid.Position).Normalized();
    }

    private void UpdateAsteroidPosition(Vector2 position)
    {
        // let any interested parties know how long we have left
        asteroid.Position = position;
        var distanceRemaining = (impactPoint.Position - asteroid.Position).Length();
        Signals.PublishAsteroidTimeEstimateEvent(Id, Size, distanceRemaining / Speed);
    }

    private void Impact()
    {
        var areas = impactPoint.GetOverlappingAreas();
        foreach (var area in areas)
        {
            if (area is ShieldArea shieldArea && shieldArea.Active)
            {
                var damage = health;
                health -= shieldArea.Health.GetValueOrDefault(0);
                shieldArea.Damage(damage);
                if (health <= 0)
                {
                    Destroy();
                }
            }
        }
        if (!Destroyed)
        {
            Destroyed = true;
            Signals.PublishAsteroidImpactEvent(Id, impactPoint.GlobalPosition, ExplosionRadius);
            RemoteSignals.PublishAsteroidImpactEvent(Id, impactPoint.GlobalPosition, ExplosionRadius);
            QueueFree();
        }

    }


    public void Damage(int damage)
    {
        health -= damage;
        if (health < 0)
        {
            Destroy();
        }
    }

    private void Destroy()
    {
        Destroyed = true;
        if (Size == 3)
        {
            Signals.PublishDwarfPlanetDestroyedEvent();
        }
        Signals.PublishAsteroidDestroyedEvent(Id, GlobalPosition, Size);
        RemoteSignals.PublishAsteroidDestroyedEvent(Id, GlobalPosition, Size);
        QueueFree();
    }
}
