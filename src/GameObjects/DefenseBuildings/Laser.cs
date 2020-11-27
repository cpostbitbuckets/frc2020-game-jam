using Godot;
using System;

public class Laser : DefenseBuilding
{
    /// <summary>
    /// Override to always return a laser
    /// </summary>
    /// <value></value>
    public override GameBuildingType Type { get => GameBuildingType.Laser; }

    [Export]
    public override Boolean Active
    {
        get => active; set
        {
            active = value;
            if (laserArea != null)
            {
                laserArea.Visible = !active;
            }
        }
    }
    private bool active;

    [Export]
    public int Damage { get; set; } = 20;

    [Export]
    public float Cooldown { get; set; } = .5f;

    private Asteroid target;
    private Timer timer;
    private Line2D beam;
    private AudioStreamPlayer audioStreamPlayer;
    private LaserArea laserArea;

    public override void _Ready()
    {
        // TODO: Add easy mode

        Signals.DayPassedEvent += OnDayPassed;
        Signals.AsteroidDestroyedEvent += OnAsteroidDestroyed;

        // get some nodes we interact with
        timer = GetNode<Timer>("Timer");
        beam = GetNode<Line2D>("Beam");
        audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        laserArea = GetNode<LaserArea>("LaserArea");

        // connect up laser area events
        laserArea.Connect("area_entered", this, nameof(OnLaserAreaEntered));
        laserArea.Connect("area_exited", this, nameof(OnLaserAreaExited));

        // connect up mouse events
        Connect("mouse_entered", this, nameof(OnMouseEntered));
        Connect("mouse_exited", this, nameof(OnMouseExited));

        // the laser damages its target every .5 seconds (cooldown)
        timer.Connect("timeout", this, nameof(OnTimerTimeout));
        timer.Start(Cooldown);

        // setup based on our current tech level
        TechCheck();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (target != null)
        {
            // adjust our beam to target the moved object
            beam.Points[1] = target.GlobalPosition - beam.GlobalPosition;
            if (!audioStreamPlayer.Playing)
            {
                audioStreamPlayer.Play(.5f);
            }
        }
        else
        {
            beam.Points[1] = beam.Points[0];
            audioStreamPlayer.Stop();
        }
    }

    #region Event Handlers
    private void OnDayPassed(int day)
    {
        TechCheck();
        ReevaluateTargeting();
    }

    private void OnAsteroidDestroyed(int asteroidId, Vector2 position, int size)
    {
        if (target != null && target.Id == asteroidId)
        {
            var buildingOwner = PlayersManager.Instance.GetPlayer(PlayerNum);
            buildingOwner.AddScore(ScoreType.AsteroidDestroyed);
        }
        if (target == null || (target.Destroyed is bool destroyed && destroyed))
        {
            ReevaluateTargeting();
        }
    }

    private void OnTimerTimeout()
    {
        if (target != null)
        {
            target.Damage(Damage);
        }
    }

    private void OnLaserAreaEntered(Area2D area)
    {
        if (target == null)
        {
            ReevaluateTargeting();
        }
    }

    private void OnLaserAreaExited(Area2D area)
    {
        if (target == area)
        {
            ReevaluateTargeting();
        }
    }

    private void OnMouseEntered()
    {
        if (Active)
        {
            laserArea.Visible = true;
        }
    }

    private void OnMouseExited()
    {
        if (Active)
        {
            laserArea.Visible = false;
        }
    }

    #endregion

    /// <summary>
    /// Check the owner's Laser TechLevel and update the radius
    /// 
    /// </summary>
    private void TechCheck()
    {
        var buildingOwner = PlayersManager.Instance.GetPlayer(PlayerNum);
        var radius = 256.0f;
        var laserTechLevel = buildingOwner.TechLevel[ResearchType.Laser];

        if (laserTechLevel == 2)
        {
            radius = 320;
        }
        else if (laserTechLevel == 3)
        {
            radius = 384;
            Damage = 25;
        }
        laserArea.Radius = radius;
    }

    /// <summary>
    /// Find a new target or remove a target if there are no more in our area
    /// </summary>
    private void ReevaluateTargeting()
    {
        if (Active)
        {
            var areas = laserArea.GetOverlappingAreas();
            Asteroid newTarget = null;
            foreach (var area in areas)
            {
                if (area is Asteroid)
                {
                    newTarget = area as Asteroid;
                    break;
                }
            }

            // update our target to the new found target, or null if we found no target
            target = newTarget;
        }
    }


}
