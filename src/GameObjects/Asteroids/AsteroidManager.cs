using Godot;
using System;

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
    public Territory[] Territories { get; set; } = Array.Empty<Territory>();

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

        // clients listen for asteroid_incoming messages
        Signals.AsteroidIncomingEvent += OnAsteroidIncoming;
        Signals.DwarfPlanetDestroyedEvent += OnDwarfPlanetDestroyed;

        if (!GetTree().HasNetworkPeer() || GetTree().IsNetworkServer())
        {
            timer.Connect("timeout", this, nameof(OnTimerTimeout));
            timer.Start(InitialWaveTime);
            Signals.PublishAsteroidWaveTimerUpdatedEvent(timer.TimeLeft);
            Signals.PublishAsteroidWaveStartedEvent(wave, Waves);
        }
    }

    private void OnTimerTimeout()
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
            var asteroid = GetAsteroidInstance(asteroidStrength);

            // give each new asteroid an incrementing id
            // so we can uniquely identify them
            asteroid.Id = numAsteroids++;

            asteroid.GlobalPosition = Territories[i].Center;
            activeAsteroids++;
            AddChild(asteroid);
            // after this asteroid is setup, send it to the clients
            CallDeferred(nameof(SendAsteroid), asteroid.GlobalPosition, asteroidStrength, asteroid);

        }

    }

    private void SendAsteroid(Vector2 globalPosition, int asteroidStrength, FallingAsteroid asteroid)
    {
        RemoteSignals.PublishAsteroidSpawnEvent(globalPosition, asteroidStrength, asteroid);
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

    private void FinalWave()
    {
        FallingAsteroid boss = (FallingAsteroid)dwarfPlanet.Instance();
        boss.Id = numAsteroids;
        activeAsteroids++;
        numAsteroids++;
        foreach (var territory in Territories)
        {
            if (!PlayersManager.Instance.GetPlayer(territory.TerritoryOwner).AIControlled)
            {
                boss.GlobalPosition = territory.Center;
                break;
            }
            AddChild(boss);
            CallDeferred(nameof(SendAsteroid), boss.GlobalPosition, 40000, boss);
        }
    }

    private void RemoveActiveAsteroid()
    {
        activeAsteroids--;
    }

    private void OnAsteroidImpact(int asteroidId, Vector2 impactPoint, int explosionRadius)
    {
        RemoveActiveAsteroid();
    }


    private void OnAsteroidDestroyed(int asteroidId, Vector2 position, int size)
    {
        RemoveActiveAsteroid();
    }

    private void OnAsteroidIncoming(Vector2 position, int strength, Asteroid asteroid)
    {
        if (GetTree().HasNetworkPeer() && !GetTree().IsNetworkServer())
        {
            // only clients care about this method
            // they spawn identical asteroids on their side
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

    private void UpdateAsteroidInstanceAfterSpawn(FallingAsteroid asteroidInstance, FallingAsteroid asteroid)
    {
        // copy attributes from the network asteroid to this instance
        asteroidInstance.From(asteroid);
    }

    private void OnDwarfPlanetDestroyed()
    {
        throw new NotImplementedException();
    }
}
