using Godot;
using System;

public class Shield : DefenseBuilding
{
    /// <summary>
    /// Override to always return a laser
    /// </summary>
    /// <value></value>
    public override GameBuildingType Type { get => GameBuildingType.Shield; }

    [Export]
    public override Boolean Active
    {
        get => active; set
        {
            active = value;
            if (shieldArea != null)
            {
                // set the shield to mostly transparent
                shieldArea.Sprite.Modulate = new Color(1, 1, 1, .25f);
                shieldArea.Active = value;
            }
        }
    }
    private bool active;

    public int MaxHealth { get; set; } = 100;
    public int Regen { get; set; } = 10;
    public int Cooldown { get; set; } = 10;
    public int Health { get; set; }

    private ShieldArea shieldArea;
    private Timer timer;
    private AudioStreamPlayer rechargeAudio;
    private AudioStreamPlayer asteroidStrikeAudio;

    public override void _Ready()
    {
        base._Ready();

        Health = MaxHealth;

        Signals.DayPassedEvent += OnDayPassed;

        if (this.IsClient())
        {
            ClientSignals.ShieldUpdatedEvent += OnShieldUpdated;
            ClientSignals.ShieldDamagedEvent += OnShieldDamaged;
        }

        shieldArea = GetNode<ShieldArea>("ShieldArea");
        timer = GetNode<Timer>("Timer");
        rechargeAudio = GetNode<AudioStreamPlayer>("RechargeAudio");
        asteroidStrikeAudio = GetNode<AudioStreamPlayer>("AsteroidStrikeAudio");

        timer.Connect("timeout", this, nameof(OnTimerTimeout));

        // connect up mouse events
        Connect("mouse_entered", this, nameof(OnMouseEntered));
        Connect("mouse_exited", this, nameof(OnMouseExited));


        // setup based on our current tech level
        TechCheck();

        // we don't have a "ShieldArea" variable until we are ready, but we aren't ready until after
        // active is set
        Active = active;
    }

    public override void _ExitTree()
    {
        Signals.DayPassedEvent -= OnDayPassed;
        if (this.IsClient())
        {
            ClientSignals.ShieldUpdatedEvent -= OnShieldUpdated;
            ClientSignals.ShieldDamagedEvent -= OnShieldDamaged;
        }
    }

    #region Event Handlers
    void OnDayPassed(int day)
    {
        TechCheck();
        if (Active)
        {
            Regenerate();
        }
    }

    void OnTimerTimeout()
    {
        Enable();
    }

    void OnMouseEntered()
    {
        if (Active)
        {
            shieldArea.Sprite.Modulate = new Color(1, 1, 1, 1);
        }
    }

    void OnMouseExited()
    {
        if (Active)
        {
            shieldArea.Sprite.Modulate = new Color(1, 1, 1, .25f);
        }
    }

    #region Client Only Events

    /// <summary>
    /// This is only called on clients. This event happens when a server tells us a shield recharged or was taken down
    /// (so we can update our view)
    /// </summary>
    /// <param name="buildingId"></param>
    /// <param name="active"></param>
    void OnShieldUpdated(String buildingId, bool active)
    {
        if (BuildingId == buildingId)
        {
            if (Active && !active)
            {
                Disable();
            }
            else if (!Active && active)
            {
                Enable();
            }

        }
    }

    /// <summary>
    /// This is only called on clients. This event happens when a server tells us a shield has been damaged 
    /// (so we can play a sound)
    /// </summary>
    /// <param name="buildingId"></param>
    /// <param name="damage"></param>
    void OnShieldDamaged(String buildingId, int damage)
    {
        if (BuildingId == buildingId)
        {
            Damage(damage);
        }

    }

    #endregion

    #endregion

    /// <summary>
    /// Damage the shield
    /// </summary>
    /// <param name="damage"></param>
    public void Damage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Disable();
        }
        asteroidStrikeAudio?.Play();

        PlayersManager.Instance.GetPlayer(PlayerNum).AddScore(ScoreType.AsteroidDeflected);
    }


    /// <summary>
    /// Check the owner's Laser TechLevel and update the radius
    /// 
    /// </summary>
    void TechCheck()
    {
        var buildingOwner = PlayersManager.Instance.GetPlayer(PlayerNum);
        var radius = 256.0f;
        var health = 100;
        var regen = 20;

        var shieldTechLevel = buildingOwner?.TechLevel[ResearchType.Shield];

        if (shieldTechLevel == 2)
        {
            radius = 288.0f;
            health = 200;
            regen = 20;
        }
        else if (shieldTechLevel == 3)
        {
            radius = 320.0f;
            health = 400;
            regen = 40;
        }

        shieldArea.Radius = radius;
        MaxHealth = health;
        Regen = regen;
    }

    void Regenerate()
    {
        Health += Regen;
        if (Health > MaxHealth)
        {
            Health = MaxHealth;
        }

    }

    void Enable()
    {
        timer.Stop();
        Health = MaxHealth / 4;
        shieldArea.Visible = true;
        Active = true;
        rechargeAudio?.Play();
        ClientSignals.PublishShieldUpdatedEvent(BuildingId, Active);
    }

    void Disable()
    {
        Health = 0;
        shieldArea.Visible = false;
        Active = false;
        timer.Start(Cooldown);
        ClientSignals.PublishShieldUpdatedEvent(BuildingId, Active);
    }
}
