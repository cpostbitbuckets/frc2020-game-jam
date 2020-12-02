using Godot;
using System;

public class TechTree : Control
{
    Texture textureDisabled;
    Texture textureResearching;
    Texture textureTier2;
    Texture textureTier3;

    ResearchPopup researchPopup;

    public override void _Ready()
    {
        researchPopup = GetNode<ResearchPopup>("ResearchPopup");
        GetNode<Label>("TechTree/Row 1/TextureMine3/Mine3/LabelMine3").Text = "Chromium\nMines";
        GetNode<Label>("TechTree/Row 1/TextureMissile3/Missile3/LabelMissile3").Text = "Ultimate\nMissiles";
        GetNode<Label>("TechTree/Row 2/TextureMine2/Mine2/LabelMine2").Text = "Titanium\nMines";
        GetNode<Label>("TechTree/Row 2/TextureMissile2/Missile2/LabelMissile2").Text = "Advanced\nMissiles";
        GetNode<Label>("TechTree/Row 3/TextureMine1/Mine1/LabelMine1").Text = "Iron\nMines";
        GetNode<Label>("TechTree/Row 3/TextureMissile1/Missile1/LabelMine2").Text = "Basic\nMissiles";
        GetNode<Label>("TechTree/Middle Row/TexturePower3/Power3/LabelPower3").Text = "Nuclear\nPower";
        GetNode<Label>("TechTree/Middle Row/TexturePower2/Power2/LabelPower2").Text = "Solar\nPower";
        GetNode<Label>("TechTree/Middle Row/TexturePower1/Power1/LabelPower1").Text = "Coal\nPower";
        GetNode<Label>("TechTree/Middle Row/TextureShield1/Shield1/LabelShield1").Text = "Basic\nShields";
        GetNode<Label>("TechTree/Middle Row/TextureShield2/Shield2/LabelShield2").Text = "Advanced\nShields";
        GetNode<Label>("TechTree/Middle Row/TextureShield3/Shield3/LabelShield3").Text = "Ultimate\nShields";
        GetNode<Label>("TechTree/Row 4/TextureLaser1/Laser1/LabelLaser1").Text = "Basic\nLasers";
        GetNode<Label>("TechTree/Row 4/TextureScience1/Science1/LabelScience1").Text = "Basic\nLabs";
        GetNode<Label>("TechTree/Row 5/TextureLaser2/Laser2/LabelLaser2").Text = "Advanced\nLasers";
        GetNode<Label>("TechTree/Row 5/TextureScience2/Science2/LabelScience2").Text = "Advanced\nLabs";
        GetNode<Label>("TechTree/Row 6/TextureLaser3/Laser3/LabelLaser3").Text = "Ultimate\nLasers";
        GetNode<Label>("TechTree/Row 6/TextureScience3/Science3/LabelScience3").Text = "Supreme\nLabs";

        textureDisabled = ResourceLoader.Load<Texture>("res://assets/icons/techtree/disabled.png");
        textureResearching = ResourceLoader.Load<Texture>("res://assets/icons/techtree/researching.png");
        textureTier2 = ResourceLoader.Load<Texture>("res://assets/icons/techtree/tier2.png");
        textureTier3 = ResourceLoader.Load<Texture>("res://assets/icons/techtree/tier3.png");

        Signals.PlayerUpdatedEvent += OnPlayerUpdated;
        researchPopup.ConfirmedEvent += OnResearchPopupConfirmed;

        SetMissileDisabled();
        UpdateNodes();

        // wire up all the tech press buttons
        foreach (var type in Enum.GetValues(typeof(ResearchType)))
        {
            for (int i = 2; i <= 3; i++)
            {
                if (FindNode($"{type}{i}") is TextureButton button)
                {
                    // pass the type and level to the pressed event
                    button.Connect("pressed", this, nameof(OnTechPressed), new Godot.Collections.Array(new object[] { type, i }));
                }
            }

        }
    }

    public override void _ExitTree()
    {
        Signals.PlayerUpdatedEvent -= OnPlayerUpdated;
    }

    void OnTechPressed(ResearchType type, int level)
    {
        var player = PlayersManager.Instance.Me;
        if (player.TechBeingResearched == ResearchType.None && player.CanResearch(type, level))
        {
            researchPopup.SetInfo(type, level);
            researchPopup.PopupCentered();
        }
    }

    void OnResearchPopupConfirmed(ResearchType type, int level)
    {
        PlayersManager.Instance.Me.StartResearch(type);
        Signals.PublishPlayerStartResearchEvent(PlayersManager.Instance.Me.Num, type);
        researchPopup.Hide();
    }


    void SetMissileDisabled()
    {
        GetNode<TextureRect>("TechTree/Row 3/TextureMissile1").Texture = textureDisabled;
        GetNode<TextureRect>("TechTree/Row 2/TextureMissile2").Texture = textureDisabled;
        GetNode<TextureRect>("TechTree/Row 1/TextureMissile3").Texture = textureDisabled;

    }

    void UpdateNodes()
    {
        var player = PlayersManager.Instance.Me;
        UpdateNodeTexture(player, "TechTree/Row 2/TextureMine2", ResearchType.Mine, 2, true);
        UpdateNodeTexture(player, "TechTree/Row 1/TextureMine3", ResearchType.Mine, 3, false);

        // UpdateNodeTexture(player, "TechTree/Row 2/TextureMissile2", ResearchType.Missile, 2, true);
        // UpdateNodeTexture(player, "TechTree/Row 1/TextureMissile3", ResearchType.Missile, 3, false);

        UpdateNodeTexture(player, "TechTree/Middle Row/TexturePower2", ResearchType.Power, 2, true);
        UpdateNodeTexture(player, "TechTree/Middle Row/TexturePower3", ResearchType.Power, 3, false);

        UpdateNodeTexture(player, "TechTree/Middle Row/TextureShield2", ResearchType.Shield, 2, true);
        UpdateNodeTexture(player, "TechTree/Middle Row/TextureShield3", ResearchType.Shield, 3, false);

        UpdateNodeTexture(player, "TechTree/Row 5/TextureScience2", ResearchType.Science, 2, true);
        UpdateNodeTexture(player, "TechTree/Row 6/TextureScience3", ResearchType.Science, 3, false);

        UpdateNodeTexture(player, "TechTree/Row 5/TextureLaser2", ResearchType.Laser, 2, true);
        UpdateNodeTexture(player, "TechTree/Row 6/TextureLaser3", ResearchType.Laser, 3, false);
    }

    /// <summary>
    /// For a single tech node, update the texture based on whether we can research it, are researching it, or what level it is
    /// </summary>
    /// <param name="player"></param>
    /// <param name="nodePath"></param>
    /// <param name="type"></param>
    /// <param name="level"></param>
    /// <param name="tier"></param>
    void UpdateNodeTexture(PlayerData player, string nodePath, ResearchType type, int level, bool tier)
    {
        var node = GetNode<TextureRect>(nodePath);
        // if the player is currently researching this ResearchType but doesn't have this level yet, set it to Researched
        if (player.TechBeingResearched == type && player.TechLevel[type] < level)
        {
            node.Texture = textureResearching;
        }
        else
        {
            bool techResearched = player.HasTech(type, level);
            bool techAvailableToResearch = player.CanResearch(type, level);

            // if we aren't researching any techs, set this texture based
            // on if we can research it orr not
            if (player.TechBeingResearched == ResearchType.None && !techResearched && techAvailableToResearch)
            {
                if (tier)
                {
                    node.Texture = textureTier2;
                }
                else
                {
                    node.Texture = textureTier3;
                }
            }
            else
            {
                node.Texture = textureDisabled;
            }
        }

    }

    void OnPlayerUpdated(PlayerData player)
    {
        if (player.Num == PlayersManager.Instance.Me.Num)
        {
            UpdateNodes();
        }
    }
}
