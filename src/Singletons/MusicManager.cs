using Godot;
using System;

public class MusicManager : Node
{
    /// <summary>
    /// MusicManager is a singleton
    /// </summary>
    private static MusicManager instance;
    public static MusicManager Instance
    {
        get
        {
            return instance;
        }
    }

    MusicManager()
    {
        instance = this;
    }

    AudioStreamPlayer gameMusic;

    public override void _Ready()
    {
        gameMusic = GetNode<AudioStreamPlayer>("GameMusic");
        GameSettings.Instance.SettingsChangedEvent += OnSettingsChanged;
        if (GameSettings.Instance.PlayMusic)
        {
            gameMusic.Play();
        }
    }

    public override void _ExitTree()
    {
        GameSettings.Instance.SettingsChangedEvent -= OnSettingsChanged;
    }

    void OnSettingsChanged()
    {
        gameMusic.Playing = GameSettings.Instance.PlayMusic;
    }
}
