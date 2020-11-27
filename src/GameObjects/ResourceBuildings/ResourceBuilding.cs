using Godot;
using System;
using System.Collections.Generic;

public class ResourceBuilding : GameBuilding
{
    protected override HashSet<GameBuildingType> AllowedTypes
    {
        get => allowedTypes;
    }
    HashSet<GameBuildingType> allowedTypes = new HashSet<GameBuildingType>()
    {
        GameBuildingType.Mine,
        GameBuildingType.PowerPlant,
        GameBuildingType.ScienceLab
    };

    /// <summary>
    /// Override this so we export it
    /// </summary>
    /// <value></value>
    [Export]
    public override GameBuildingType Type { get => base.Type; set => base.Type = value; }

    [Export(PropertyHint.Enum)]
    public ResourceType ResourceType { get; set; }

    [Export]
    public int ResourceAmount { get; set; } = 1;

    public override void _Ready()
    {
        Signals.DayPassedEvent += OnDayPassed;
    }

    private void OnDayPassed(int day)
    {
        if (Active && IsResourceBuilding && ResourceType != ResourceType.EXCEPTIION && ResourceAmount > 0)
        {
            Signals.PublishResourceGeneratedEvent(PlayerNum, ResourceType, ResourceAmount);
        }
    }
}
