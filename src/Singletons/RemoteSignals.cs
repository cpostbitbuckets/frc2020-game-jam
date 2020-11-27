using Godot;

using System;

/// <summary>
/// RemoteSignals are used to communicate with the game when a remote action takes place, like a player
/// update or a new asteroid launch.
/// </summary>
public class RemoteSignals
{

    public delegate void PlayerUpdated(PlayerData player);
    public static event PlayerUpdated PlayerUpdatedEvent;

    public delegate void AsteroidSpawn(Vector2 globalPosition, int asteroidStrength, FallingAsteroid asteroid);
    public static event AsteroidSpawn AsteroidSpawnEvent;

    public delegate void AsteroidPositionUpdated(int id, Vector2 position);
    public static event AsteroidPositionUpdated AsteroidPositionUpdatedEvent;

    public delegate void AsteroidImpact(int asteroidId, Vector2 impactPoint, int explosionRadius);
    public static event AsteroidImpact AsteroidImpactEvent;

    public delegate void AsteroidDestroyed(int asteroidId, Vector2 position, int size);
    public static event AsteroidDestroyed AsteroidDestroyedEvent;

    #region Event Publishers

    /// <summary>
    /// Publish this event when you want a player update to be sent to peers
    /// </summary>
    /// <param name="player"></param>
    public static void PublishPlayerUpdatedEvent(PlayerData player)
    {
        PlayerUpdatedEvent?.Invoke(player);
    }

    public static void PublishAsteroidPositionUpdatedEvent(int id, Vector2 position)
    {
        AsteroidPositionUpdatedEvent?.Invoke(id, position);
    }

    public static void PublishAsteroidImpactEvent(int id, Vector2 impactPoint, int explosionRadius)
    {
        AsteroidImpactEvent?.Invoke(id, impactPoint, explosionRadius);
    }

    public static void PublishAsteroidDestroyedEvent(int asteroidId, Vector2 position, int size)
    {
        AsteroidDestroyedEvent?.Invoke(asteroidId, position, size);
    }

    internal static void PublishAsteroidSpawnEvent(Vector2 globalPosition, int asteroidStrength, FallingAsteroid asteroid)
    {
        AsteroidSpawnEvent?.Invoke(globalPosition, asteroidStrength, asteroid);
    }

    #endregion
}
