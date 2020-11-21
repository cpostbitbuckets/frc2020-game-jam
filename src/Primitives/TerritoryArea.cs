using Godot;
using System;
using System.Collections.Generic;

public class TerritoryArea : Area2D
{
    public string TypeName { get => nameof(TerritoryArea); }

    List<Node2D> buildingsInArea = new List<Node2D>();

    public Territory Territory { get => GetChild<Territory>(0); }

    public List<Node2D> GetBuildings()
    {
        if (Territory.Type == TerritoryType.Destroyed)
        {
            return new List<Node2D>();
        }

        foreach (Area2D area in GetOverlappingAreas())
        {
            if (area.Get("external_class_name") as string == "GameBuilding")
            {
                buildingsInArea.Add(area);
            }
        }
        return buildingsInArea;
    }
}
