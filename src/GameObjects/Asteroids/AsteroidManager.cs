using Godot;
using System;
using System.Collections.Generic;

public class AsteroidManager : Node2D
{
    [Export]
    public int Waves { get; set; } = 15;

    [Export]
    public int AsteroidQuantityModifier { get; set; } = 1;

    [Export]
    public int AsteroidStrengthMultiplier { get; set; } = 3;

    [Export]
    public int MaxStrength { get; set; } = 50;

    [Export]
    public int MaxCount { get; set; } = 50;

    [Export]
    public int BaseWaveTime { get; set; } = 30;

    [Export]
    public int InitialWaveTime { get; set; } = 30;

    private int wave = 0;
    private int asteroidCount = 1;
    private int activeAsteroids = 0;
    private int numAsteroids = 0;
    private Random rng = new Random();
    public List<Territory> Territories { get; set; } = new List<Territory>();

    Timer timer;
    PackedScene asteroidSmall;
    PackedScene asteroidMedium;
    PackedScene asteroidLarge;
    PackedScene dwarfPlanet;

    public override void _Ready()
    {
        timer = GetNode<Timer>("Timer");
        asteroidSmall = ResourceLoader.Load<PackedScene>("res://src/GameObjects/Asteroids/FallingAsteroidSmall.tscn");
        asteroidMedium = ResourceLoader.Load<PackedScene>("res://src/GameObjects/Asteroids/FallingAsteroidMedium.tscn");
        asteroidLarge = ResourceLoader.Load<PackedScene>("res://src/GameObjects/Asteroids/FallingAsteroidLarge.tscn");
        dwarfPlanet = ResourceLoader.Load<PackedScene>("res://src/GameObjects/Asteroids/FallingDwarfPlanet.tscn");

        Signals.AsteroidImpactEvent += OnAsteroidImpact;
        Signals.AsteroidDestroyedEvent += OnAsteroidDestroyed;

        if (this.IsServerOrSinglePlayer())
        {
            // The server manages asteroid waves so it actually starts the timer
            // and publishes wave events
            timer.Connect("timeout", this, nameof(OnTimerTimeout));
            timer.Start(InitialWaveTime);
            Signals.PublishAsteroidWaveTimerUpdatedEvent(timer.TimeLeft);
            Signals.PublishAsteroidWaveStartedEvent(wave, Waves);
        }
        else if (this.IsClient())
        {
            // clients listen for asteroid_incoming events so we can spawn
            // an asteroid on the client.
            ClientSignals.AsteroidIncomingEvent += OnServerAsteroidIncoming;
        }
    }

    public override void _ExitTree()
    {
        Signals.AsteroidImpactEvent -= OnAsteroidImpact;
        Signals.AsteroidDestroyedEvent -= OnAsteroidDestroyed;

        if (this.IsClient())
        {
            ClientSignals.AsteroidIncomingEvent -= OnServerAsteroidIncoming;
        }

    }

    /// <summary>
    /// When the asteroid timer times out, we spawn asteroids and start a new wave timer.
    /// </summary>
    void OnTimerTimeout()
    {
        wave++;
        if (wave < Waves || Waves == -1)
        {
            timer.Start((float)(BaseWaveTime * GD.RandRange(.25, 2)));
            Signals.PublishAsteroidWaveTimerUpdatedEvent(timer.TimeLeft);
        }
        if (wave == Waves)
        {
            FinalWave();
        }

        if (wave > Waves)
        {
            GD.Print("whoops, we called our timer again after our waves are done");
            return;
        }

        Signals.PublishAsteroidWaveStartedEvent(wave, Waves);

        asteroidCount += wave + AsteroidQuantityModifier;

        if (asteroidCount > MaxCount)
        {
            asteroidCount = MaxCount;
        }

        rng.Shuffle(Territories);

        for (var i = 0; i < asteroidCount; i++)
        {
            int asteroidStrength = (int)GD.RandRange(0, wave * AsteroidStrengthMultiplier);
            FallingAsteroid fallingAsteroid = GetAsteroidInstance(asteroidStrength);

            // give each new asteroid an incrementing id
            // so we can uniquely identify them
            fallingAsteroid.Id = numAsteroids++;

            // The FallingAsteroid has two parts, an impact point and an asteroid (that is falling)
            // The GlobalPosition is the impact point, the falling asteroid falls towards it
            fallingAsteroid.GlobalPosition = Territories[i].Center;
            activeAsteroids++;
            AddChild(fallingAsteroid);
            // after this asteroid is setup, send it to the clients
            CallDeferred(nameof(SendAsteroid), fallingAsteroid.GlobalPosition, asteroidStrength, fallingAsteroid);

        }

    }

    /// <summary>
    /// Send a message to any clients about a new asteroid incoming
    /// </summary>
    /// <param name="globalPosition"></param>
    /// <param name="asteroidStrength"></param>
    /// <param name="asteroid"></param>
    void SendAsteroid(Vector2 globalPosition, int asteroidStrength, FallingAsteroid asteroid)
    {
        ClientSignals.PublishAsteroidIncomingEvent(globalPosition, asteroidStrength, asteroid);
    }

    private FallingAsteroid GetAsteroidInstance(int asteroidStrength)
    {
        if (asteroidStrength > MaxStrength)
        {
            asteroidStrength = MaxStrength;
        }
        if (asteroidStrength < 15)
        {
            return (FallingAsteroid)asteroidSmall.Instance();
        }
        else if (asteroidStrength < 30)
        {
            return (FallingAsteroid)asteroidMedium.Instance();
        }
        else
        {
            return (FallingAsteroid)asteroidLarge.Instance();
        }
    }

    /// <summary>
    /// This is the final wave!
    /// </summary>
    void FinalWave()
    {
        FallingAsteroid boss = (FallingAsteroid)dwarfPlanet.Instance();
        boss.Id = numAsteroids++;
        activeAsteroids++;
        foreach (var territory in Territories)
        {
            if (!PlayersManager.Instance.GetPlayer(territory.TerritoryOwner).AIControlled)
            {
                boss.GlobalPosition = territory.Center;
                AddChild(boss);
                CallDeferred(nameof(SendAsteroid), boss.GlobalPosition, 40000, boss);
                break;
            }
        }
    }

    void RemoveActiveAsteroid()
    {
        activeAsteroids--;
    }

    void OnAsteroidImpact(int asteroidId, Vector2 impactPoint, int explosionRadius)
    {
        RemoveActiveAsteroid();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="asteroidId"></param>
    /// <param name="position"></param>
    /// <param name="size"></param>
    void OnAsteroidDestroyed(int asteroidId, Vector2 position, int size)
    {
        RemoveActiveAsteroid();
    }

    #region Client Events

    /// <summary>
    /// Event fired when a server tells us (the client) that anew asteroid is incoming.
    /// We spawn it on the client to keep them in sync
    /// </summary>
    /// <param name="position"></param>
    /// <param name="strength"></param>
    /// <param name="asteroid"></param>
    void OnServerAsteroidIncoming(Vector2 position, int strength, FallingAsteroid asteroid)
    {
        // only clients care about this method
        // they spawn identical asteroids on their side
        if (this.IsClient())
        {
            FallingAsteroid asteroidInstance;
            if (strength == 40000)
            {
                asteroidInstance = (FallingAsteroid)dwarfPlanet.Instance();
            }
            else
            {
                asteroidInstance = GetAsteroidInstance(strength);
            }
            asteroidInstance.GlobalPosition = position;
            activeAsteroids++;
            AddChild(asteroidInstance);
            CallDeferred(nameof(UpdateAsteroidInstanceAfterSpawn), asteroidInstance, asteroid);
        }
    }

    void UpdateAsteroidInstanceAfterSpawn(FallingAsteroid asteroidInstance, FallingAsteroid asteroid)
    {
        // copy attributes from the network asteroid to this instance
        asteroidInstance.From(asteroid);
    }

    #endregion

}
