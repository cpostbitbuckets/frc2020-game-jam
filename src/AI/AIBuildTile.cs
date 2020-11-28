using Godot;
using System;

public class AIBuildTile : Position2D
{
    [Export]
    public int PlayerNum { get; set; }

    [Export]
    public int BuildOrder { get; set; } = 1;

    [Export]
    public GameBuildingType BuildingToBuild { get; set; } = GameBuildingType.Shield;

    /// <summary>
    /// Helper to get the current player
    /// </summary>
    /// <returns></returns>
    PlayerData Player { get => PlayersManager.Instance.GetPlayer(PlayerNum); }

    private bool built = false;

    /// <summary>
    /// Build a building at this location if we can afford it
    /// </summary>
    /// <returns></returns>
    public bool Build()
    {
        if (built)
        {
            return false;
        }

        if (Player.CanAffordBuilding(BuildingToBuild))
        {
            var buildingId = BuildingsManager.Instance.GetNextId();
            Signals.PublishGameBuildingPlacedEvent(buildingId, PlayerNum, BuildingToBuild, Position);
            RemoteSignals.PublishGameBuildingPlacedEvent(buildingId, PlayersManager.Instance.Me.Num, BuildingToBuild, Position);

            // once we build, we don't want this tile around anymore
            QueueFree();
            return true;
        }

        return false;
    }

}
