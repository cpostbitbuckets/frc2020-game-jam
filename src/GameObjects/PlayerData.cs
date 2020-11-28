
using Godot;
using System;
using System.Collections.Generic;

public class PlayerData : Resource
{
    public int NetworkId { get; set; }
    public int Num { get; set; }
    public string Name { get; set; }
    public Boolean Ready { get; set; }
    public Boolean AIControlled { get; set; }
    public Color Color { get; set; } = Colors.Black;
    public int Score { get; set; }
    public ResearchType TechBeingResearched { get; set; } = ResearchType.None;
    public int TechResearchProgress { get; set; } = 0;
    public Resources Resources { get; } = new Resources();
    public Dictionary<ResearchType, int> TechLevel { get; } = new Dictionary<ResearchType, int>
    {
        { ResearchType.Mine, 1 },
        { ResearchType.Power, 1 },
        { ResearchType.Science, 1 },
        { ResearchType.Missile, 1 },
        { ResearchType.Laser, 1 },
        { ResearchType.Shield, 1 },
    };

    public override string ToString()
    {
        var networkDescription = AIControlled ? "AI Controlled" : $"NetworkId: {NetworkId}";
        return $"Player {Num} {Name} ({networkDescription})";
    }

    /// <summary>
    /// Update our player from another player
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public PlayerData From(PlayerData player)
    {
        NetworkId = player.NetworkId;
        Num = player.Num;
        Name = player.Name;
        Ready = player.Ready;
        AIControlled = player.AIControlled;
        Color = player.Color;
        Score = player.Score;
        TechBeingResearched = player.TechBeingResearched;
        TechResearchProgress = player.TechResearchProgress;
        Resources.Raw = player.Resources.Raw;
        Resources.Power = player.Resources.Power;
        Resources.Science = player.Resources.Science;
        TechLevel[ResearchType.Mine] = player.TechLevel[ResearchType.Mine];
        TechLevel[ResearchType.Power] = player.TechLevel[ResearchType.Power];
        TechLevel[ResearchType.Science] = player.TechLevel[ResearchType.Science];
        TechLevel[ResearchType.Missile] = player.TechLevel[ResearchType.Missile];
        TechLevel[ResearchType.Laser] = player.TechLevel[ResearchType.Laser];
        TechLevel[ResearchType.Shield] = player.TechLevel[ResearchType.Shield];

        return this;
    }

    public void AddScore(ScoreType type, int value = 0)
    {
        if (value != 0)
        {
            Score += value;
        }
        else
        {
            Score += Constants.ScoreGranted[type];
        }
        Score += Constants.ScoreGranted[type];
        Signals.PublishPlayerScoreChangedEvent(this);
    }

    /// <summary>
    /// Check if we are done researching our selected tech
    /// </summary>
    public void CheckResearchComplete()
    {
        if (TechBeingResearched != ResearchType.None)
        {
            int currentLevel = TechLevel[TechBeingResearched];
            int cost = Constants.ResearchCosts[TechBeingResearched][currentLevel];

            if (cost <= TechResearchProgress)
            {
                TechResearchComplete(TechBeingResearched);

                // reclaim any leftover science points
                Resources.Science = TechResearchProgress - cost;

                // reset our research stuff to 0
                TechBeingResearched = ResearchType.None;
                TechResearchProgress = 0;
            }
        }
    }

    /// <summary>
    /// Complete tech research, increase our level and signal listeners
    /// </summary>
    /// <param name="type">The type of research completed</param>
    private void TechResearchComplete(ResearchType type)
    {
        // rev our tech level
        TechLevel[type]++;
        Signals.PublishPlayerResearchCompletedEvent(this, type);
        AddScore(ScoreType.ResearchComplete);
    }

    /// <summary>
    /// Check if this player can afford a building
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool CanAffordBuilding(GameBuildingType type)
    {
        var buildingCost = Constants.BuildingCosts[type];

        // if we can't afford building type one, return false
        if (buildingCost.Cost < Resources[buildingCost.Type1])
        {
            return false;
        }

        // if we have a type2, check if we have enough resources
        if (buildingCost.Type2 != ResourceType.EXCEPTIION &&
        buildingCost.Cost < Resources[buildingCost.Type2])
        {
            return false;
        }

        // we have enough of both types, return true because we can afford it
        return true;
    }

    /// <summary>
    /// Check if we can afford to donate resources
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool CanDonateAmount(ResourceType type, int amount)
    {
        return Resources[type] >= amount;
    }
}
