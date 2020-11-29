using Godot;
using System;
using System.Collections.Generic;

public class Constants
{
    public static int SecondsPerDay { get; } = 3;

    public static int NumPlayers { get; } = 5;

    public static Resources StartingResources { get; } = new Resources
    {
        Raw = 500,
        Power = 500,
        Science = 0
    };

    /// <summary>
    /// Costs to build various buildings
    /// </summary>
    /// <value></value>
    public static Dictionary<GameBuildingType, BuildingCost> BuildingCosts { get; } = new Dictionary<GameBuildingType, BuildingCost>
    {
        { GameBuildingType.Mine, new BuildingCost(ResourceType.Raw, 15) },
        { GameBuildingType.PowerPlant, new BuildingCost(ResourceType.Raw, 15) },
        { GameBuildingType.ScienceLab, new BuildingCost(ResourceType.Raw, 15) },
        { GameBuildingType.Radar, new BuildingCost(ResourceType.Raw, 25, ResourceType.Power) },
        { GameBuildingType.Missile, new BuildingCost(ResourceType.Raw, 100) },
        { GameBuildingType.Laser, new BuildingCost(ResourceType.Raw, 50, ResourceType.Power) },
        { GameBuildingType.Shield, new BuildingCost(ResourceType.Raw, 50, ResourceType.Power) },
    };

    /// <summary>
    /// Research costs by type, per level (2, and 3)
    /// </summary>
    /// <value></value>
    public static Dictionary<ResearchType, int[]> ResearchCosts { get; } = new Dictionary<ResearchType, int[]>
    {
        { ResearchType.Mine, new int[] { 100, 300} },
        { ResearchType.Power, new int[] { 100, 200} },
        { ResearchType.Science, new int[] { 200, 400} },
        { ResearchType.Missile, new int[] { 300, 600} },
        { ResearchType.Laser, new int[] { 250, 500} },
        { ResearchType.Shield, new int[] { 100, 300} },
    };

    /// <summary>
    /// Scores for various activities
    /// </summary>
    /// <value></value>
    public static Dictionary<ScoreType, int> ScoreGranted { get; } = new Dictionary<ScoreType, int>
    {
        { ScoreType.BuildingBuilt, 200 },
        { ScoreType.ResearchComplete, 500 },
        { ScoreType.Donated, 750 },
        { ScoreType.AsteroidShot, 250 },
        { ScoreType.AsteroidDeflected, 0 },
        { ScoreType.AsteroidDestroyed, 0 },
    };

    /// <summary>
    /// A list of names for assigning players
    /// </summary>
    /// <value></value>
    public static string[] Names { get; } = new string[] {
        "Bit Buckets",
        "Murphy's Outlaws",
        "Javawockies",
        "Blue Alliange",
        "Alumiboti",
        "Blarglefish"
    };

    public static Color[] PlayerColors { get; } = new Color[] {
        Colors.Black,
        new Color("c33232"),
        new Color("1f8ba7"),
        new Color("43a43e"),
        new Color("8d29cb"),
        new Color("b88628")
    };

}
