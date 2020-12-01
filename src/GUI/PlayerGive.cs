using Godot;
using System;

[Tool]
public class PlayerGive : HBoxContainer
{
    [Export]
    public int PlayerNum
    {
        get => playerNum;
        set
        {
            playerNum = value;
        }
    }
    int playerNum = 2;

    [Export]
    public string PlayerName
    {
        get => playerName;
        set
        {
            playerName = value;
            if (playerNameLabel != null)
            {
                playerNameLabel.Text = value;
            }

        }
    }
    private string playerName = "";

    int resourceGiveAmount = 1;
    int sourcePlayerNum = 1;
    Label playerNameLabel;

    public override void _Ready()
    {
        if (!Engine.EditorHint)
        {
            sourcePlayerNum = PlayersManager.Instance.Me.Num;
        }

        playerNameLabel = GetNode<Label>("PlayerName");

        Signals.PlayerUpdatedEvent += OnPlayerUpdated;
        GetNode<TextureButton>("Resources/Raw/GiveRawButton").Connect("pressed", this, nameof(OnGiveRawButtonPressed));
        GetNode<TextureButton>("Resources/Power/GivePowerButton").Connect("pressed", this, nameof(OnGivePowerButtonPressed));
        GetNode<TextureButton>("Resources/Science/GiveScienceButton").Connect("pressed", this, nameof(OnGiveScienceButtonPressed));
    }

    public override void _ExitTree()
    {
        Signals.PlayerUpdatedEvent -= OnPlayerUpdated;
    }



    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsActionPressed("ui_give_100"))
        {
            resourceGiveAmount = 100;
        }
        if (@event.IsActionPressed("ui_give_10"))
        {
            resourceGiveAmount = 10;
        }
        if (@event.IsActionReleased("ui_give_100") || @event.IsActionReleased("ui_give_10"))
        {
            resourceGiveAmount = 1;
        }
    }

    private void OnPlayerUpdated(PlayerData player)
    {
        if (player.Num == PlayerNum)
        {
            GetNode<Label>("Resources/Raw/Value").Text = player.Resources.Raw.ToString();
            GetNode<Label>("Resources/Power/Value").Text = player.Resources.Power.ToString();
            GetNode<Label>("Resources/Science/Value").Text = player.Resources.Science.ToString();
        }
    }

    void OnGiveRawButtonPressed()
    {
        Signals.PublishPlayerResourcesGivenEvent(sourcePlayerNum, PlayerNum, ResourceType.Raw, resourceGiveAmount);
        // TODO: Figure out RPC
    }

    void OnGivePowerButtonPressed()
    {
        Signals.PublishPlayerResourcesGivenEvent(sourcePlayerNum, PlayerNum, ResourceType.Power, resourceGiveAmount);
    }

    void OnGiveScienceButtonPressed()
    {
        Signals.PublishPlayerResourcesGivenEvent(sourcePlayerNum, PlayerNum, ResourceType.Science, resourceGiveAmount);
    }

}
