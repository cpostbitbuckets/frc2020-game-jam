using Godot;
using System;

public class ResearchProgress : ProgressBar
{
    public override void _Ready()
    {
        var font = new DynamicFont();
        font.Size = 16;
        AddFontOverride("font", font);

        SetPosition(new Vector2(460, 15));
        SetSize(new Vector2(1000, 30));
        Value = 0;

        Signals.PlayerUpdatedEvent += OnPlayerUpdated;
    }

    public override void _ExitTree()
    {
        Signals.PlayerUpdatedEvent -= OnPlayerUpdated;
    }

    private void OnPlayerUpdated(PlayerData player)
    {
        if (player.Num == PlayersManager.Instance.Me.Num)
        {
            var type = player.TechBeingResearched;
            if (type != ResearchType.None)
            {
                var cost = Constants.ResearchCosts[type][player.TechLevel[type] - 1];
                Value = (float)player.TechResearchProgress / (float)cost;

                if (Value > MaxValue)
                {
                    Value = MaxValue;
                }
            }
            else
            {
                Value = 0;
            }
        }
    }
}
