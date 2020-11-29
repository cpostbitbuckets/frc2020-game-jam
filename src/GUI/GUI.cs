using Godot;
using System;


public class GUI : Control
{
	public event Action TechTreeButtonPressedEvent;
	public event Action LeaderBoardButtonPressedEvent;

	int DaysUntilNextAsteroid
	{
		get => daysUntilNextAsteroid;
		set
		{
			if (value > 0)
			{
				if (daysUntilNextAsteroid != value)
				{
					daysUntilNextAsteroid = value;
					UpdateAsteroidWaveMessage();
				}
			}
			else
			{
				daysUntilNextAsteroid = 0;
			}
		}
	}

	float TimeToBossImpact
	{
		get => timeToBossImpact;
		set
		{
			if (value > 0)
			{
				timeToBossImpact = value;
			}
			else
			{
				timeToBossImpact = 0;
			}
		}
	}
	float timeToBossImpact;

	int daysUntilNextAsteroid = 0;

	int wave = 1;
	int waves = 15;

	CircleLabel scoreLabel;
	CircleLabel dayLabel;

	public override void _Ready()
	{
		scoreLabel = GetNode<CircleLabel>("TopMenu/Left/HBoxContainer/Score");
		dayLabel = GetNode<CircleLabel>("TopMenu/Right/HBoxContainer/Days");

		Signals.GameBuildingPlacedEvent += OnGameBuildingPlaced;

		// The server sends us a day update and our client emits a day_passed signal
		Signals.DayPassedEvent += OnDayPassed;

		// update our asteroid incoming message
		Signals.AsteroidWaveTimerUpdatedEvent += OnAsteroidWaveTimerUpdated;
		Signals.AsteroidWaveStartedEvent += OnAsteroidWaveStarted;
		Signals.AsteroidTimeEstimateEvent += OnAsteroidTimeEstimate;

		// update score gui node
		Signals.PlayerScoreChangedEvent += OnPlayerScoreChanged;

		FindNode("LeaderBoardButton").Connect("pressed", this, nameof(OnLeaderBoardButtonPressed));
		FindNode("TechTreeButton").Connect("pressed", this, nameof(OnTechTreeButtonPressed));

		var player = PlayersManager.Instance.Me;
		scoreLabel.Label = $"{player.Name} Score";
		scoreLabel.Modulate = player.Color;

		var otherPlayerNum = 1;
		foreach (var otherPlayer in PlayersManager.Instance.Players)
		{
			if (otherPlayer.Num != player.Num)
			{
				PlayerGive playerGiveNode = (PlayerGive)FindNode($"PlayerGive{otherPlayerNum++}");
				playerGiveNode.PlayerName = otherPlayer.Name;
				playerGiveNode.PlayerNum = otherPlayer.Num;
			}
		}

		foreach (GameBuildingButton gameBuildingButton in this.GetAllNodesOfType<GameBuildingButton>())
		{
			gameBuildingButton.PressedEvent += OnGameBuildingButtonPressed;
		}

	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (@event.IsActionPressed("escape"))
		{
			RestoreCursor();
			Signals.PublishGameBuildingCancelledEvent();
		}
	}

	void UpdateAsteroidWaveMessage()
	{
		var header = GetNode<Label>("TopMenu/Center/VBoxContainer/HeaderLabel");
		if ((wave + 1) >= waves)
		{
			header.Text = $"FINAL WAVE! SHORE UP YOUR DEFENSES! {TimeToBossImpact:0.0}";
		}
		else
		{
			header.Text = $"Asteroid Wave {wave} of {waves} incoming in {DaysUntilNextAsteroid} days!";
		}
	}

	void RestoreCursor()
	{
		Input.SetMouseMode(Input.MouseMode.Visible);
		Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
	}

	#region Event Handlers

	void OnDayPassed(int day)
	{
		dayLabel.Value = day.ToString();
		DaysUntilNextAsteroid--;
	}

	void OnPlayerScoreChanged(PlayerData player)
	{
		scoreLabel.Value = PlayersManager.Instance.Me.Score.ToString();
	}

	void OnAsteroidWaveTimerUpdated(float timeLeft)
	{
		DaysUntilNextAsteroid = ((int)(timeLeft / Constants.SecondsPerDay));
	}

	void OnAsteroidTimeEstimate(int asteroidId, int size, float timeToImpact)
	{
		if (size == 3)
		{
			timeToBossImpact = timeToImpact;
			UpdateAsteroidWaveMessage();
		}
	}

	void OnAsteroidWaveStarted(int wave, int waves)
	{
		if (this.wave != wave || this.waves != waves)
		{
			this.wave = wave;
			this.waves = waves;
			UpdateAsteroidWaveMessage();
		}
	}

	void OnGameBuildingPlaced(string buildingId, int playerNum, GameBuildingType type, Vector2 position)
	{
		RestoreCursor();
	}

	void OnGameBuildingButtonPressed(GameBuildingButton button)
	{
		// cancel previous selections
		Signals.PublishGameBuildingCancelledEvent();

		// Tell the map we are ready to place a building
		Signals.PublishGameBuildingSelectedEvent(button.Type);
	}

	void OnLeaderBoardButtonPressed()
	{
		LeaderBoardButtonPressedEvent?.Invoke();
	}

	void OnTechTreeButtonPressed()
	{
		TechTreeButtonPressedEvent?.Invoke();
	}


	#endregion

}
