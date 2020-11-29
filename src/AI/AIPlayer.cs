using Godot;
using System;
using System.Collections.Generic;

/*
From Elias: So I was told to tell everyone my strategy, so here it is:
-Spend all starting materials on mines and maybe a power plant or 2
-Build a shield
-Improve economy when low on resources from now on
-Build a laser
-Build another shield
-Keep building lasers (and the occasional shield) whenever possible
-Pray that the dwarf planet targets a well defended area
*/

/// <summary>
/// AI Player to research and build tiles
/// TODO: The player will build lasers without checking if it's possible
/// </summary>
public class AIPlayer : Node2D
{
    [Export]
    public int PlayerNum { get; set; }

    private List<KeyValuePair<ResearchType, int>> researchList = new List<KeyValuePair<ResearchType, int>>() {
        new KeyValuePair<ResearchType, int>(ResearchType.Mine, 2),
        new KeyValuePair<ResearchType, int>(ResearchType.Power, 2),
        new KeyValuePair<ResearchType, int>(ResearchType.Science, 2),
        new KeyValuePair<ResearchType, int>(ResearchType.Shield, 2),
        new KeyValuePair<ResearchType, int>(ResearchType.Laser, 2),
        new KeyValuePair<ResearchType, int>(ResearchType.Mine, 3),
        new KeyValuePair<ResearchType, int>(ResearchType.Power, 3),
        new KeyValuePair<ResearchType, int>(ResearchType.Shield, 3),
        new KeyValuePair<ResearchType, int>(ResearchType.Laser, 3),
        new KeyValuePair<ResearchType, int>(ResearchType.Science, 3),
    };

    /// <summary>
    /// A list of territories this AI will analyze to place buildings on
    /// </summary>
    /// <typeparam name="Territory"></typeparam>
    /// <returns></returns>
    public List<Territory> Territories { get; set; }

    /// <summary>
    /// Helper to get the current player
    /// </summary>
    /// <returns></returns>
    PlayerData Player { get => PlayersManager.Instance.GetPlayer(PlayerNum); }

    public override void _Ready()
    {
        Signals.DayPassedEvent += OnDayPassed;
    }

    private void OnDayPassed(int day)
    {
        // if this player is AIControlled and we are single player or the server
        if (Player.AIControlled && (!GetTree().HasNetworkPeer() || GetTree().IsNetworkServer()))
        {
            // queue up research
            ResearchNext();

            var allBuildTiles = GetTree().GetNodesInGroup("ai_tiles");
            var ourBuildTiles = new List<AIBuildTile>();

            // find our build tiles
            foreach (AIBuildTile buildTile in allBuildTiles)
            {
                if (buildTile != null && buildTile.PlayerNum == PlayerNum)
                {
                    ourBuildTiles.Add(buildTile);
                }
            }
            ourBuildTiles.Sort((tile1, tile2) => tile1.BuildOrder.CompareTo(tile2.BuildOrder));

            if (ourBuildTiles.Count > 0)
            {
                // we have a pre-determined build tile, build it
                ourBuildTiles[0].Build();
            }
            else
            {
                if (Player.CanAffordBuilding(GameBuildingType.Laser))
                {
                    // get a list of available territories
                    var ownedTerritories = Territories.FindAll(t => t.TerritoryOwner == PlayerNum && t.Type != TerritoryType.Destroyed);

                    GD.Randomize();
                    var randomTerritory = ownedTerritories[(int)GD.Randi() % ownedTerritories.Count];
                    var position = randomTerritory.Center;

                    bool addPosition = GD.Randi() % 2 == 0;
                    bool changeX = GD.Randi() % 2 == 0;
                    position = ChangePosition(position, addPosition, changeX);

                    var buildingId = BuildingsManager.Instance.GetNextId();
                    Signals.PublishGameBuildingPlacedEvent(buildingId, PlayerNum, GameBuildingType.Laser, position);
                    RemoteSignals.PublishGameBuildingPlacedEvent(buildingId, PlayersManager.Instance.Me.Num, GameBuildingType.Laser, position);
                }

            }
        }
    }

    /// <summary>
    /// Randomly change the position to a new place on the build tile
    /// </summary>
    /// <param name="position"></param>
    /// <param name="addPosition"></param>
    /// <param name="changeX"></param>
    /// <returns></returns>
    private Vector2 ChangePosition(Vector2 position, bool addPosition, bool changeX)
    {
        if (addPosition)
        {
            if (changeX)
            {
                position.x += 15;
            }
            else
            {
                position.y += 15;
            }
        }
        else
        {
            if (changeX)
            {
                position.x -= 15;
            }
            else
            {
                position.y -= 15;
            }
        }
        return position;
    }

    /// <summary>
    /// If we aren't currently researching something, pick a new research item from our list
    /// </summary>
    private void ResearchNext()
    {
        if (Player.TechBeingResearched == ResearchType.None)
        {
            foreach (var researchItem in researchList)
            {
                // if the Player's research level for this tech is less than the target in our list
                // research it
                if (Player.TechLevel[researchItem.Key] < researchItem.Value)
                {
                    Player.TechBeingResearched = researchItem.Key;
                }
            }
        }
    }

}
