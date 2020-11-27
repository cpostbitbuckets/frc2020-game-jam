using Godot;
using System;
using System.Collections.Generic;

public class DefenseBuilding : GameBuilding
{
    protected override HashSet<GameBuildingType> AllowedTypes
    {
        get => allowedTypes;
    }
    HashSet<GameBuildingType> allowedTypes = new HashSet<GameBuildingType>()
    {
        GameBuildingType.Radar,
        GameBuildingType.Missile,
        GameBuildingType.Laser,
        GameBuildingType.Shield
    };

}
