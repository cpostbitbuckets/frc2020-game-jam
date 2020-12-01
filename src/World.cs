using Godot;
using System;

public class World : Node2D
{
    AsteroidManager asteroidManager;
    Timer endGameDelayTimer;
    CanvasLayer canvasLayer;
    Map map;
    GUI gui;

    Control techTree;
    Control leaderBoard;
    ConfirmationDialog quitPopup;

    int[] numTerritoriesPerPlayer = new int[] { 0, 0, 0, 0, 0 };
    int[] numDestroyedTerritoriesPerPlayer = new int[] { 0, 0, 0, 0, 0 };

    bool gameOver = false;

    public override void _Ready()
    {
        // load in some nodes
        asteroidManager = GetNode<AsteroidManager>("AsteroidManager");
        endGameDelayTimer = GetNode<Timer>("EndGameDelayTimer");
        canvasLayer = GetNode<CanvasLayer>("CanvasLayer");
        map = GetNode<Map>("Map");
        gui = canvasLayer.GetNode<GUI>("GUI");
        techTree = canvasLayer.GetNode<Control>("TechTree");
        leaderBoard = canvasLayer.GetNode<Control>("Leaderboard");
        quitPopup = canvasLayer.GetNode<ConfirmationDialog>("QuitPopup");

        Signals.FinalWaveCompleteEvent += OnFinalWaveComplete;
        Signals.TerritoryDestroyedEvent += OnTerritoryDestroyed;

        gui.TechTreeButtonPressedEvent += OnTechTreeButtonPressed;
        gui.LeaderBoardButtonPressedEvent += OnLeaderBoardButtonPressed;

        quitPopup.Connect("confirmed", this, nameof(OnQuitPopupConfirmed));

        // setup our list of territories per owner
        map.Territories.ForEach(t => numTerritoriesPerPlayer[t.TerritoryOwner - 1]++);
        asteroidManager.Territories = map.Territories;

        AddPlayersToWorld();

        // after the world is setup, tell the server to start the timer and begin the game
        Server.Instance.PostBeginGame();
    }

    public override void _ExitTree()
    {
        Signals.FinalWaveCompleteEvent -= OnFinalWaveComplete;
        Signals.TerritoryDestroyedEvent -= OnTerritoryDestroyed;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsActionPressed("ui_tech_tree"))
        {
            // hide the Leaderboard
            leaderBoard.Visible = false;
            // toggle the TechTree
            techTree.Visible = !techTree.Visible;
            gui.Visible = map.Visible = asteroidManager.Visible = !techTree.Visible;
        }
        else if (@event.IsActionPressed("ui_leaderboard"))
        {
            // hide the TechTree
            techTree.Visible = false;
            // toggle the LeaderBoard
            leaderBoard.Visible = !leaderBoard.Visible;
            gui.Visible = map.Visible = asteroidManager.Visible = !leaderBoard.Visible;
        }
        else if (@event.IsActionPressed("escape"))
        {
            // hide the UIs
            techTree.Visible = false;
            leaderBoard.Visible = false;
            map.Show();
            asteroidManager.Show();
        }
        else if (@event.IsActionPressed("quit"))
        {
            quitPopup.Show();
        }
    }

    /// <summary>
    /// Add all the player scenes to the world
    /// </summary>
    void AddPlayersToWorld()
    {
        // we use this scene to instantiate new players
        PackedScene playerScene = ResourceLoader.Load<PackedScene>("res://src/GameObjects/Player.tscn");

        foreach (var player in PlayersManager.Instance.Players)
        {
            var playerNode = playerScene.Instance() as Player;
            playerNode.Data = player;
            AddChild(playerNode);
        }

    }

    public void WinGame()
    {
        if (gameOver)
        {
            // only do this once
            return;
        }

        gameOver = true;
        // let our stuff get all messed up and damaged before
        // we transition
        endGameDelayTimer.Connect("timeout", this, nameof(OnEndGameWin));
        endGameDelayTimer.Start();

    }

    public void LoseGame()
    {
        if (gameOver)
        {
            // only do this once
            return;
        }

        gameOver = true;
        // let our stuff get all messed up and damaged before
        // we transition
        endGameDelayTimer.Connect("timeout", this, nameof(OnEndGameLose));
        endGameDelayTimer.Start();
    }

    #region Event Handlers

    void OnFinalWaveComplete()
    {
        if (gameOver)
        {
            return;
        }
        gameOver = true;
        endGameDelayTimer.Connect("timeout", this, nameof(OnEndGameWin));
        endGameDelayTimer.Start();
    }

    void OnTerritoryDestroyed(Territory territory)
    {
        if (territory.TerritoryOwner > 0 && territory.TerritoryOwner <= numTerritoriesPerPlayer.Length && territory.TerritoryOwner <= numDestroyedTerritoriesPerPlayer.Length)
        {
            numDestroyedTerritoriesPerPlayer[territory.TerritoryOwner - 1]++;
            if (numDestroyedTerritoriesPerPlayer[territory.TerritoryOwner - 1] == numTerritoriesPerPlayer[territory.TerritoryOwner - 1])
            {
                LoseGame();
            }
        }
    }

    /// <summary>
    /// We lose, switch to lose screen
    /// </summary>
    void OnEndGameLose()
    {
        Signals.PublishGameLostEvent();
        GetTree().ChangeScene("res://src/GUI/LoseScreen.tscn");
    }

    /// <summary>
    /// We win, switch to win or grand winner screen depending on if we won bigly
    /// </summary>
    void OnEndGameWin()
    {
        var playerWithHighestScore = PlayersManager.Instance.Me.Num;
        var highestScore = PlayersManager.Instance.Me.Score;

        // find the highest score
        PlayersManager.Instance.Players.ForEach(p =>
        {
            if (p.Score > highestScore)
            {
                playerWithHighestScore = p.Num;
                highestScore = p.Score;
            }
        });

        if (playerWithHighestScore == PlayersManager.Instance.Me.Num)
        {
            Signals.PublishGameGrandWonEvent();
            GetTree().ChangeScene("res://src/GUI/GrandWinScreen.tscn");
        }
        else
        {
            Signals.PublishGameWonEvent();
            GetTree().ChangeScene("res://src/GUI/WinScreen.tscn");
        }
    }

    void OnLeaderBoardButtonPressed()
    {
        leaderBoard.Show();
    }

    void OnTechTreeButtonPressed()
    {
        techTree.Show();
    }

    void OnQuitPopupConfirmed()
    {
        Client.Instance.CloseConnection();
        Server.Instance.CloseConnection();
        PlayersManager.Instance.Reset();
        GetTree().ChangeScene("res://src/Main.tscn");
    }
    #endregion


}
