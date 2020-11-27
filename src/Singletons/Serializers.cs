using Godot;
using System;
using System.Collections.Generic;

public static class Serializers
{
    /// <summary>
    /// Serialize a Player to an array for sending over RPC
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static Godot.Collections.Array ToArray(this PlayerData p)
    {
        return new Godot.Collections.Array {
            p.NetworkId,
            p.Num,
            p.Name,
            p.Ready,
            p.AIControlled,
            p.Color,
            p.Score,
            p.TechBeingResearched,
            p.TechResearchProgress,
            p.Resources.Raw,
            p.Resources.Power,
            p.Resources.Science,
            p.TechLevel[ResearchType.Mine],
            p.TechLevel[ResearchType.Power],
            p.TechLevel[ResearchType.Science],
            p.TechLevel[ResearchType.Missile],
            p.TechLevel[ResearchType.Laser],
            p.TechLevel[ResearchType.Shield],
        };
    }

    /// <summary>
    /// Update a PlayerData from an array of serialized data
    /// </summary>
    /// <param name="p"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static PlayerData FromArray(this PlayerData p, Godot.Collections.Array data)
    {
        int i = 0;
        p.NetworkId = (int)data[i++];
        p.Num = (int)data[i++];
        p.Name = (string)data[i++];
        p.Ready = (Boolean)data[i++];
        p.AIControlled = (Boolean)data[i++];
        p.Color = (Color)data[i++];
        p.Score = (int)data[i++];
        p.TechBeingResearched = (ResearchType)data[i++];
        p.TechResearchProgress = (int)data[i++];
        p.Resources.Raw = (int)data[i++];
        p.Resources.Power = (int)data[i++];
        p.Resources.Science = (int)data[i++];
        p.TechLevel[ResearchType.Mine] = (int)data[i++];
        p.TechLevel[ResearchType.Power] = (int)data[i++];
        p.TechLevel[ResearchType.Science] = (int)data[i++];
        p.TechLevel[ResearchType.Missile] = (int)data[i++];
        p.TechLevel[ResearchType.Laser] = (int)data[i++];
        p.TechLevel[ResearchType.Shield] = (int)data[i++];

        return p;
    }

    /// <summary>
    /// Serialize an Asteroid into a Godot Array
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static Godot.Collections.Array ToArray(this FallingAsteroid a)
    {
        return new Godot.Collections.Array {
            a.Id,
            a.BaseSpeed,
            a.ExplosionRadius,
            a.BaseDistance,
            a.MaxHealth,
            a.Distance,
            a.Speed,
            a.ImpactVector
        };
    }

    /// <summary>
    /// Construct an Asteroid from a Godot Array
    /// </summary>
    /// <param name="a"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static FallingAsteroid FromArray(this FallingAsteroid a, Godot.Collections.Array data)
    {
        int i = 0;
        a.Id = (int)data[i++];
        a.BaseSpeed = (int)data[i++];
        a.ExplosionRadius = (int)data[i++];
        a.BaseDistance = (int)data[i++];
        a.MaxHealth = (int)data[i++];
        a.Distance = (float)data[i++];
        a.Speed = (float)data[i++];
        a.ImpactVector = (Vector2)data[i++];
        a.SetupInitialState();
        return a;
    }
}
