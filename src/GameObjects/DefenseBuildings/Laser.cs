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
        base._Ready();
        // TODO: Add easy mode

        Signals.DayPassedEvent += OnDayPassed;
        Signals.AsteroidDestroyedEvent += OnAsteroidDestroyed;
        Signals.AsteroidImpactEvent += OnAsteroidImpact;

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

        Active = active;
    }

    public override void _ExitTree()
    {
        target = null;
        timer.Stop();

        Signals.DayPassedEvent -= OnDayPassed;
        Signals.AsteroidDestroyedEvent -= OnAsteroidDestroyed;
        Signals.AsteroidImpactEvent -= OnAsteroidImpact;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (target != null && !target.Destroyed)
        {
            // adjust our beam to target the moved object
            beam.SetPointPosition(1, target.GlobalPosition - beam.GlobalPosition);
            if (!audioStreamPlayer.Playing)
            {
                audioStreamPlayer.Play(.5f);
            }
        }
        else
        {
            if (beam.Points[1] != beam.Points[0])
            {
                beam.SetPointPosition(1, beam.Points[0]);
            }
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
            target = null;
        }
        if (target == null || target.Destroyed)
        {
            ReevaluateTargeting();
        }
    }

    /// <summary>
    /// On asteroid impact, remove our target if we are targetting this
    /// </summary>
    /// <param name="asteroidId"></param>
    /// <param name="impactPoint"></param>
    /// <param name="explosionRadius"></param>
    void OnAsteroidImpact(int asteroidId, Vector2 impactPoint, int explosionRadius)
    {
        if (target != null && target.Id == asteroidId)
        {
            target = null;
            ReevaluateTargeting();
        }
    }

    private void OnTimerTimeout()
    {
        if (target != null && !target.Destroyed)
        {
            target.Damage(Damage);
        }
        timer.Start(Cooldown);
    }

    private void OnLaserAreaEntered(Area2D area)
    {
        if (target == null || target.Destroyed)
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

        Damage = Constants.LaserDamage;
        if (laserTechLevel == 2)
        {
            radius = 320;
        }
        else if (laserTechLevel == 3)
        {
            radius = 384;
            Damage = Constants.LaserAdvancedDamage;
        }
        laserArea.Radius = radius;

        if (GameSettings.Instance.Easy)
        {
            Damage *= 2;
        }
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
                // find any asteroids that aren't destroyed
                if (area is Asteroid asteroid && !asteroid.Destroyed)
                {
                    newTarget = asteroid;
                    break;
                }
            }

            // update our target to the new found target, or null if we found no target
            target = newTarget;
        }
        else
        {
            target = null;
        }
    }


}
