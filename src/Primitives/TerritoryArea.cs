using Godot;
using System;
using System.Collections.Generic;

public class TerritoryArea : Area2D
{
    public string TypeName { get => nameof(TerritoryArea); }

    public Territory Territory { get => GetChild<Territory>(0); }

    public List<GameBuilding> GetBuildings()
    {
        List<GameBuilding> buildingsInArea = new List<GameBuilding>();

        if (Territory.Type == TerritoryType.Destroyed)
        {
            return new List<GameBuilding>();
        }

        foreach (Area2D area in GetOverlappingAreas())
        {
            if (area is GameBuilding building)
            {
                buildingsInArea.Add(building);
            }
        }
        return buildingsInArea;
    }
}
