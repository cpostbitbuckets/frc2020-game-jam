using Godot;
using System;

/// <summary>
/// Each building has a cost that is constant
/// </summary>
public readonly struct BuildingCost
{
    public ResourceType Type1 { get; }
    public ResourceType Type2 { get; }

    public int Cost { get; }

    /// <summary>
    /// Construct a new BuildingCost struct.
    /// </summary>
    /// <param name="type1"></param>
    /// <param name="cost"></param>
    /// <param name="type2"></param>
    public BuildingCost(ResourceType type1, int cost, ResourceType type2 = ResourceType.EXCEPTIION)
    {
        Type1 = type1;
        Cost = cost;
        Type2 = type2;
    }

}
