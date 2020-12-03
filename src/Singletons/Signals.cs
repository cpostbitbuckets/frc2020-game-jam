using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// The Signals csharp class services as a way to bridge csharp and gdscript until 
/// everything is rewritten in .net
/// </summary>
public class Signals : Node
{
    public delegate void DayPassed(int day);
    public static event DayPassed DayPassedEvent;

    #region Network Events

    public static event Action ServerDisconnectedEvent;

    public delegate void PreStartGame(List<PlayerData> players);
    public static event PreStartGame PreStartGameEvent;

    public static event Action PostStartGameEvent;

    #endregion

    #region Player Connection Events

    public delegate void PlayerUpdated(PlayerData player);
    public static event PlayerUpdated PlayerUpdatedEvent;

    public delegate void PlayerJoined(int networkId);
    public static event PlayerJoined PlayerJoinedEvent;

    public delegate void PlayerLeft(int networkId);
    public static event PlayerLeft PlayerLeftEvent;

    public delegate void PlayerReadyToStart(int networkId, bool ready);
    public static event PlayerReadyToStart PlayerReadyToStartEvent;

    public static event Action<PlayerMessage> PlayerMessageEvent;

    #endregion 

    #region Player Update Events

    public delegate void PlayerScoreChanged(PlayerData player);
    public static event PlayerScoreChanged PlayerScoreChangedEvent;

    public delegate void ResourceGenerated(int playerNum, ResourceType type, int amount);
    public static event ResourceGenerated ResourceGeneratedEvent;

    public delegate void PlayerStartResearch(int num, ResearchType type);
    public static event PlayerStartResearch PlayerStartResearchEvent;

    public delegate void PlayerResearchCompleted(PlayerData player, ResearchType type);
    public static event PlayerResearchCompleted PlayerResearchCompletedEvent;

    public delegate void PlayerResourcesGiven(int sourcePlayerNum, int destPlayerNum, ResourceType type, int amount);
    public static event PlayerResourcesGiven PlayerResourcesGivenEvent;

    #endregion

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

    public delegate void AsteroidTimeEstimate(int asteroidId, int size, float timeToImpact);
    public static event AsteroidTimeEstimate AsteroidTimeEstimateEvent;

    [Signal]
    public delegate void AsteroidImpact(int asteroidId, Vector2 impactPoint, int explosionRadius);
    public static event AsteroidImpact AsteroidImpactEvent;

    public delegate void AsteroidDestroyed(int asteroidId, Vector2 position, int size);
    public static event AsteroidDestroyed AsteroidDestroyedEvent;

    public delegate void FinalWaveComplete();
    public static event FinalWaveComplete FinalWaveCompleteEvent;

    public delegate void TerritoryDestroyed(Territory territory);
    public static event TerritoryDestroyed TerritoryDestroyedEvent;

    #endregion

    #region Game Events

    public static event Action GameLostEvent;
    public static event Action GameWonEvent;
    public static event Action GameGrandWonEvent;

    #endregion


    // The GDScript signals object
    public static Signals Instance { get; private set; }

    Signals()
    {
        Instance = this;
    }

    #region Event Publishers

    public static void PublishPreStartGameEvent(List<PlayerData> players)
    {
        PreStartGameEvent?.Invoke(players);
    }

    public static void PublishPostStartGameEvent()
    {
        PostStartGameEvent?.Invoke();
    }

    /// <summary>
    /// Publish a player updated event for any listeners
    /// </summary>
    /// <param name="player"></param>
    /// <param name="notifyPeers">True if we should notify peers of this player data</param>
    public static void PublishPlayerUpdatedEvent(PlayerData player, bool notifyPeers = false)
    {
        PlayerUpdatedEvent?.Invoke(player);
        if (notifyPeers)
        {
            RPC.Instance.SendPlayerUpdated(player);
        }
    }

    public static void PublishPlayerJoinedEvent(int networkId)
    {
        PlayerJoinedEvent?.Invoke(networkId);
    }

    public static void PublishPlayerLeftEvent(int networkId)
    {
        PlayerLeftEvent?.Invoke(networkId);
    }

    public static void PublishPlayerReadyToStart(int networkId, bool ready)
    {
        PlayerReadyToStartEvent?.Invoke(networkId, ready);
    }

    public static void PublishPlayerMessageEvent(PlayerMessage message)
    {
        PlayerMessageEvent?.Invoke(message);
    }

    public static void PublishPlayerScoreChangedEvent(PlayerData player)
    {
        PlayerScoreChangedEvent?.Invoke(player);
    }

    public static void PublishPlayerStartResearchEvent(int num, ResearchType type)
    {
        PlayerStartResearchEvent?.Invoke(num, type);
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

    public static void PublishGameBuildingSelectedEvent(GameBuildingType type)
    {
        GameBuildingSelectedEvent?.Invoke(type);
    }

    public static void PublishGameBuildingCancelledEvent()
    {
        GameBuildingCancelledEvent?.Invoke();
    }

    public static void PublishDayPassedEvent(int day)
    {
        DayPassedEvent?.Invoke(day);
    }

    public static void PublishResourceGeneratedEvent(int playerNum, ResourceType resourceType, int resourceAmount)
    {
        ResourceGeneratedEvent?.Invoke(playerNum, resourceType, resourceAmount);
    }

    public static void PublishAsteroidWaveTimerUpdatedEvent(float timeLeft)
    {
        AsteroidWaveTimerUpdatedEvent?.Invoke(timeLeft);
    }

    public static void PublishAsteroidWaveStartedEvent(int wave, int waves)
    {
        AsteroidWaveStartedEvent?.Invoke(wave, waves);
    }

    public static void PublishAsteroidTimeEstimateEvent(int asteroidId, int size, float timeToImpact)
    {
        AsteroidTimeEstimateEvent?.Invoke(asteroidId, size, timeToImpact);
    }

    public static void PublishAsteroidImpactEvent(int id, Vector2 impactPoint, int explosionRadius)
    {
        AsteroidImpactEvent?.Invoke(id, impactPoint, explosionRadius);
        // The screenshake listens for this signal, so send it as a normal signal as well as
        // an event
        Instance.EmitSignal("AsteroidImpact", id, impactPoint, explosionRadius);
    }

    public static void PublishAsteroidDestroyedEvent(int asteroidId, Vector2 position, int size)
    {
        AsteroidDestroyedEvent?.Invoke(asteroidId, position, size);
    }

    public static void PublishFinalWaveCompleteEvent()
    {
        FinalWaveCompleteEvent?.Invoke();
    }

    public static void PublishTerritoryDestroyedEvent(Territory territory)
    {
        TerritoryDestroyedEvent?.Invoke(territory);
    }

    public static void PublishGameLostEvent()
    {
        GameLostEvent?.Invoke();
    }

    public static void PublishGameWonEvent()
    {
        GameWonEvent?.Invoke();
    }

    public static void PublishGameGrandWonEvent()
    {
        GameGrandWonEvent?.Invoke();
    }

    public static void PublishServerDisconnectedEvent()
    {
        ServerDisconnectedEvent?.Invoke();
    }

    #endregion
}
