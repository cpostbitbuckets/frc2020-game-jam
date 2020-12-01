using Godot;
using System;

[Tool]
public class GameBuildingButton : TextureButton
{
    [Export]
    public GameBuildingType Type
    {
        get => type;
        set
        {
            type = value;
            TextureNormal = GD.Load<Texture>($"res://assets/icons/{type}.png");
            GetNode<Label>("Name").Text = type.GetName();
            GetNode<Label>("Cost").Text = GetCostLabel();
        }
    }
    GameBuildingType type = GameBuildingType.Mine;

    public event Action<GameBuildingButton> PressedEvent;

    public override void _Ready()
    {
        if (!Engine.EditorHint)
        {
            Connect("pressed", this, nameof(OnPressed));
            Signals.PlayerUpdatedEvent += OnPlayerUpdated;
        }
    }

    public override void _ExitTree()
    {
        if (!Engine.EditorHint)
        {
            Signals.PlayerUpdatedEvent -= OnPlayerUpdated;
        }
    }

    private void OnPlayerUpdated(PlayerData player)
    {
        if (player.Num == PlayersManager.Instance.Me.Num)
        {
            if (player.CanAffordBuilding(Type))
            {
                Modulate = Colors.White;
            }
            else
            {
                Modulate = Colors.DarkGray;
            }
        }
    }

    void OnPressed()
    {
        PressedEvent?.Invoke(this);
        GetNode<AudioStreamPlayer>("ClickSound").Play(.2f);
    }

    String GetCostLabel()
    {
        BuildingCost cost = Constants.BuildingCosts[Type];
        if (cost.Type2 == ResourceType.EXCEPTIION)
        {
            return $"{cost.Type1}: {cost.Cost}";
        }
        else
        {
            return $"{cost.Type1}, {cost.Type2}: {cost.Cost}";
        }
    }
}
