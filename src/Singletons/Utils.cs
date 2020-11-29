using Godot;
using System;
using System.Collections.Generic;

public static class Utils
{

    public static void Shuffle<T>(this Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    public static void Shuffle<T>(this Random rng, List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = list[n];
            list[n] = list[k];
            list[k] = temp;
        }
    }

    public static bool IsDefenseBuilding(this GameBuildingType type)
    {
        return type == GameBuildingType.Laser || type == GameBuildingType.Radar || type == GameBuildingType.Shield || type == GameBuildingType.Missile;
    }

    public static bool IsResourceBuilding(this GameBuildingType type)
    {
        return type == GameBuildingType.Mine || type == GameBuildingType.PowerPlant || type == GameBuildingType.ScienceLab;
    }

    public static String GetName(this GameBuildingType type)
    {
        switch (type)
        {
            case GameBuildingType.PowerPlant:
                return "Power Plant";
            case GameBuildingType.ScienceLab:
                return "Science Lab";
            default:
                return type.ToString();
        }
    }

    /// <summary>
    /// Return the path to a GameBuilding scene based on its type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetScenePath(this GameBuildingType type)
    {
        var folder = type.IsDefenseBuilding() ? "DefenseBuildings" : "ResourceBuildings";
        return $"res://src/GameObjects/{folder}/{type.ToString()}.tscn";
    }

    public static List<T> GetAllNodesOfType<T>(this Node node) where T : Node
    {
        List<T> nodes = new List<T>();
        foreach (Node child in node.GetChildren())
        {
            if (child is T)
            {
                nodes.Add(child as T);
            }
            else if (child.GetChildCount() > 0)
            {
                nodes.AddRange(child.GetAllNodesOfType<T>());
            }
        }

        return nodes;
    }
}

