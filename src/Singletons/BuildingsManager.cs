using Godot;
using System;
using System.Collections.Generic;

public class BuildingsManager : Node
{
    private int buildingCount = 0;
    private Dictionary<string, GameBuilding> buildingsById = new Dictionary<string, GameBuilding>();

    /// <summary>
    /// BuildingsManager is a singleton
    /// </summary>
    private static BuildingsManager instance;
    public static BuildingsManager Instance
    {
        get
        {
            return instance;
        }
    }

    BuildingsManager()
    {
        instance = this;
    }

    /// <summary>
    /// Get a new id for a placed building, this is our network id combined with the count so we don't conflict with other clients
    /// </summary>
    /// <returns></returns>
    public string GetNextId()
    {
        return $"{GetTree().GetNetworkUniqueId()}-{buildingCount++}";
    }

    /// <summary>
    /// init the building manager with our starting buildings
    /// if this is a new game, all buildings are id'd by the server, so we need to keep them
    /// in sync. Ideally, we'd pass these building datatypes over the wire before a game starts, but
    /// in the interests of time, we'll just make them match up
    /// </summary>
    /// <param name="buildings"></param>
    /// <param name="initIds"></param>
    /// <param name="idPrefix"></param>
    public void InitBuildings(List<GameBuilding> buildings, bool initIds = true, string idPrefix = "1-")
    {
        foreach (var building in buildings)
        {
            var buildingId = building.BuildingId;

            if (initIds)
            {
                buildingId = $"{idPrefix}{buildingCount++}";
            }

            buildingsById[buildingId] = building;
        }
    }

    /// <summary>
    /// Add a building to our manager
    /// </summary>
    /// <param name="building"></param>
    public void AddBuilding(GameBuilding building)
    {
        buildingsById[building.BuildingId] = building;
    }
}
