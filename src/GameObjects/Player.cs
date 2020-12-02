using Godot;
using System;

public class Player : Node2D
{
    public PlayerData Data { get; set; }

    public override void _Ready()
    {
        Signals.ResourceGeneratedEvent += OnResourceGenerated;
        Signals.GameBuildingPlacedEvent += OnGameBuildingPlaced;
        Signals.PlayerResourcesGivenEvent += OnPlayerResourcesGiven;
    }

    public override void _ExitTree()
    {
        Signals.ResourceGeneratedEvent -= OnResourceGenerated;
        Signals.GameBuildingPlacedEvent -= OnGameBuildingPlaced;
        Signals.PlayerResourcesGivenEvent -= OnPlayerResourcesGiven;
    }

    private void OnResourceGenerated(int playerNum, ResourceType type, int amount)
    {
        if (Data.Num == playerNum)
        {
            switch (type)
            {
                case ResourceType.Raw:
                    Data.Resources.Raw += amount * Data.TechLevel[ResearchType.Mine];
                    break;
                case ResourceType.Power:
                    Data.Resources.Power += amount * Data.TechLevel[ResearchType.Power];
                    break;
                case ResourceType.Science:
                    var science = amount * Data.TechLevel[ResearchType.Science];
                    if (Data.TechBeingResearched != ResearchType.None)
                    {
                        Data.TechResearchProgress += science;
                    }
                    else
                    {
                        Data.Resources.Science += amount * Data.TechLevel[ResearchType.Science];
                    }
                    Data.CheckResearchComplete();
                    break;
            }

            // resource generation randomly generates score
            if (GD.RandRange(0, 10) > 7)
            {
                Data.AddScore(ScoreType.ResourceGenerated, amount);
            }

            Signals.PublishPlayerUpdatedEvent(Data);
        }
    }

    private void OnGameBuildingPlaced(string buildingId, int playerNum, GameBuildingType type, Vector2 position)
    {
        if (Data.Num == playerNum)
        {
            var buildingCost = Constants.BuildingCosts[type];
            Data.Resources[buildingCost.Type1] -= buildingCost.Cost;
            if (buildingCost.Type2 != ResourceType.EXCEPTIION)
            {
                Data.Resources[buildingCost.Type2] -= buildingCost.Cost;
            }
            Signals.PublishPlayerUpdatedEvent(Data);
        }
    }

    private void OnPlayerResourcesGiven(int sourcePlayerNum, int destPlayerNum, ResourceType type, int amount)
    {
        if (!PlayersManager.Instance.GetPlayer(sourcePlayerNum).CanDonateAmount(type, amount))
        {
            return;
        }

        if (sourcePlayerNum == Data.Num)
        {
            Data.Resources[type] -= amount;
            Signals.PublishPlayerUpdatedEvent(Data, true);
        }
        else if (destPlayerNum == Data.Num)
        {
            Data.Resources[type] += amount;
            Signals.PublishPlayerUpdatedEvent(Data, true);
        }
    }

}
