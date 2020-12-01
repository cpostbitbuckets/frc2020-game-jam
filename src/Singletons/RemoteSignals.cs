using Godot;

using System;

/// <summary>
/// RemoteSignals are used to communicate with the game when a remote action takes place, like a player
/// update or a new asteroid launch.
/// </summary>
public class RemoteSignals
{

    public delegate void AsteroidPositionUpdated(int id, Vector2 position);
    public static event AsteroidPositionUpdated AsteroidPositionUpdatedEvent;

    public delegate void AsteroidImpact(int asteroidId, Vector2 impactPoint, int explosionRadius);
    public static event AsteroidImpact AsteroidImpactEvent;

    public delegate void AsteroidDestroyed(int asteroidId, Vector2 position, int size);
    public static event AsteroidDestroyed AsteroidDestroyedEvent;

    public delegate void GameBuildingPlaced(string buildingId, int playerNum, GameBuildingType type, Vector2 position);
    public static event GameBuildingPlaced GameBuildingPlacedEvent;

    #region Event Publishers

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

    public static void PublishGameBuildingPlacedEvent(string buildingId, int playerNum, GameBuildingType type, Vector2 position)
    {
        GameBuildingPlacedEvent?.Invoke(buildingId, playerNum, type, position);
    }

    #endregion
}
