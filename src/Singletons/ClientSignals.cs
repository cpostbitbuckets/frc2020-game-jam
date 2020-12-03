using Godot;
using System;

/// <summary>
/// ClientSignals are used for signals that are only listened to by clients that
/// join multiplayer games, like Asteroid position update events.
/// They don't really make sense in the context of a single player game
/// </summary>
public class ClientSignals : Node
{
    public delegate void AsteroidIncoming(Vector2 position, int strength, FallingAsteroid asteroid);
    public static event AsteroidIncoming AsteroidIncomingEvent;

    public delegate void AsteroidPositionUpdated(int asteroidId, Vector2 position);
    public static event AsteroidPositionUpdated AsteroidPositionUpdatedEvent;

    public delegate void ShieldUpdated(String buildingId, bool active);
    public static event ShieldUpdated ShieldUpdatedEvent;

    public delegate void ShieldDamaged(String buildingId, int damage);
    public static event ShieldDamaged ShieldDamagedEvent;

    public static ClientSignals Instance { get; private set; }

    ClientSignals()
    {
        Instance = this;
    }

    public static void PublishShieldUpdatedEvent(String buildingId, bool active)
    {
        ShieldUpdatedEvent?.Invoke(buildingId, active);
    }

    public static void PublishShieldDamagedEvent(String buildingId, int damage)
    {
        ShieldDamagedEvent?.Invoke(buildingId, damage);
    }

    public static void PublishAsteroidIncomingEvent(Vector2 position, int strength, FallingAsteroid asteroid)
    {
        AsteroidIncomingEvent?.Invoke(position, strength, asteroid);
    }

    public static void PublishAsteroidPositionUpdatedEvent(int asteroidId, Vector2 position)
    {
        AsteroidPositionUpdatedEvent?.Invoke(asteroidId, position);
    }

}
