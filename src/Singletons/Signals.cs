using Godot;

using System;

/// <summary>
/// The Signals csharp class services as a way to bridge csharp and gdscript until 
/// everything is rewritten in .net
/// </summary>
public class Signals : Node
{
    public delegate void PlayerUpdated(PlayerData player);
    public static event PlayerUpdated PlayerUpdatedEvent;

    public delegate void PlayerJoined(int networkId);
    public static event PlayerJoined PlayerJoinedEvent;

    public delegate void PlayerLeft(int networkId);
    public static event PlayerLeft PlayerLeftEvent;

    public delegate void PlayerScoreChanged(PlayerData player);
    public static event PlayerScoreChanged PlayerScoreChangedEvent;

    public delegate void ResourceGenerated(int playerNum, ResourceType type, int amount);
    public static event ResourceGenerated ResourceGeneratedEvent;

    public delegate void PlayerResearchCompleted(PlayerData player, ResearchType type);
    public static event PlayerResearchCompleted PlayerResearchCompletedEvent;

    public delegate void PlayerResourcesGiven(int sourcePlayerNum, int destPlayerNum, ResourceType type, int amount);
    public static event PlayerResourcesGiven PlayerResourcesGivenEvent;

    public delegate void DayPassed(int day);
    public static event DayPassed DayPassedEvent;

    #region GameBuildings

    public delegate void GameBuildingPlaced(string buildingId, int playerNum, GameBuildingType type, Vector2 position);
    public static event GameBuildingPlaced GameBuildingPlacedEvent;

    public delegate void GameBuildingSelected(GameBuildingType type);
    public static event GameBuildingSelected GameBuildingSelectedEvent;

    public delegate void GameBuildingCancelled();
    public static event GameBuildingCancelled GameBuildingCancelledEvent;

    #endregion


    #region Asteroids

    public delegate void AsteroidWaveTimerUpdated(float timeLeft);
    public static event AsteroidWaveTimerUpdated AsteroidWaveTimerUpdatedEvent;

    public delegate void AsteroidWaveStarted(int wave, int waves);
    public static event AsteroidWaveStarted AsteroidWaveStartedEvent;

    [Signal]
    public delegate void AsteroidImpact(int asteroidId, Vector2 impactPoint, int explosionRadius);
    public static event AsteroidImpact AsteroidImpactEvent;

    public delegate void AsteroidDestroyed(int asteroidId, Vector2 position, int size);
    public static event AsteroidDestroyed AsteroidDestroyedEvent;

    public delegate void DwarfPlanetDestroyed();
    public static event DwarfPlanetDestroyed DwarfPlanetDestroyedEvent;

    public delegate void AsteroidIncoming(Vector2 position, int strength, Asteroid asteroid);
    public static event AsteroidIncoming AsteroidIncomingEvent;

    public delegate void AsteroidPositionUpdated(int asteroidId, Vector2 position);
    public static event AsteroidPositionUpdated AsteroidPositionUpdatedEvent;

    public delegate void AsteroidTimeEstimate(int asteroidId, int size, float timeToImpact);
    public static event AsteroidTimeEstimate AsteroidTimeEstimateEvent;

    public delegate void FinalWaveComplete();
    public static event FinalWaveComplete FinalWaveCompleteEvent;

    public delegate void TerritoryDestroyed(Territory territory);
    public static event TerritoryDestroyed TerritoryDestroyedEvent;

    #endregion

    #region Shields

    public delegate void ShieldUpdated(String buildingId, bool active);
    public static event ShieldUpdated ShieldUpdatedEvent;

    public delegate void ShieldDamaged(String buildingId, int damage);
    public static event ShieldDamaged ShieldDamagedEvent;


    #endregion


    // The GDScript signals object
    public static Node Instance { get; private set; }

    public override void _Ready()
    {
        Instance = GetTree().Root.GetNode("Signals");
    }

    #region Event Publishers

    public static void PublishPlayerUpdatedEvent(PlayerData player)
    {
        PlayerUpdatedEvent?.Invoke(player);
    }

    public static void PublishPlayerJoinedEvent(int networkId)
    {
        PlayerJoinedEvent?.Invoke(networkId);
    }


    public static void PublishPlayerLeftEvent(int networkId)
    {
        PlayerLeftEvent?.Invoke(networkId);
    }

    public static void PublishPlayerScoreChangedEvent(PlayerData player)
    {
        PlayerScoreChangedEvent?.Invoke(player);
    }

    public static void PublishPlayerResearchCompletedEvent(PlayerData player, ResearchType type)
    {
        PlayerResearchCompletedEvent?.Invoke(player, type);
    }

    public static void PublishPlayerResourcesGivenEvent(int sourcePlayerNum, int destPlayerNum, ResourceType type, int amount)
    {
        PlayerResourcesGivenEvent?.Invoke(sourcePlayerNum, destPlayerNum, type, amount);
    }

    public static void PublishGameBuildingPlacedEvent(string buildingId, int playerNum, GameBuildingType type, Vector2 position)
    {
        GameBuildingPlacedEvent?.Invoke(buildingId, playerNum, type, position);
    }


    public static void PublishDayPassedEvent(int day)
    {
        DayPassedEvent?.Invoke(day);
    }

    public static void PublishResourceGeneratedEvent(int playerNum, ResourceType resourceType, int resourceAmount)
    {
        ResourceGeneratedEvent?.Invoke(playerNum, resourceType, resourceAmount);
    }

    public static void PublishShieldUpdatedEvent(String buildingId, bool active)
    {
        ShieldUpdatedEvent?.Invoke(buildingId, active);
    }

    public static void PublishShieldDamagedEvent(String buildingId, int damage)
    {
        ShieldDamagedEvent?.Invoke(buildingId, damage);
    }

    public static void PublishAsteroidDestroyedEvent(int asteroidId, Vector2 position, int size)
    {
        AsteroidDestroyedEvent?.Invoke(asteroidId, position, size);
    }

    public static void PublishAsteroidWaveTimerUpdatedEvent(float timeLeft)
    {
        AsteroidWaveTimerUpdatedEvent?.Invoke(timeLeft);
    }

    public static void PublishAsteroidWaveStartedEvent(int wave, int waves)
    {
        AsteroidWaveStartedEvent?.Invoke(wave, waves);
    }

    public static void PublishAsteroidImpactEvent(int id, Vector2 impactPoint, int explosionRadius)
    {
        AsteroidImpactEvent?.Invoke(id, impactPoint, explosionRadius);
        Instance.EmitSignal("AsteroidImpact", id, impactPoint, explosionRadius);
    }

    public static void PublishDwarfPlanetDestroyedEvent()
    {
        DwarfPlanetDestroyedEvent?.Invoke();
    }

    public static void PublishAsteroidIncomingEvent(Vector2 position, int strength, Asteroid asteroid)
    {
        AsteroidIncomingEvent?.Invoke(position, strength, asteroid);
    }

    public static void PublishAsteroidPositionUpdatedEvent(int asteroidId, Vector2 position)
    {
        AsteroidPositionUpdatedEvent?.Invoke(asteroidId, position);
    }

    public static void PublishAsteroidTimeEstimateEvent(int asteroidId, int size, float timeToImpact)
    {
        AsteroidTimeEstimateEvent?.Invoke(asteroidId, size, timeToImpact);
    }

    public static void PublishFinalWaveCompleteEvent()
    {
        FinalWaveCompleteEvent?.Invoke();
    }

    public static void PublishTerritoryDestroyedEvent(Territory territory)
    {
        TerritoryDestroyedEvent?.Invoke(territory);
    }

    #endregion
}
