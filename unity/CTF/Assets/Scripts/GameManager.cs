using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameManager
{
    public static List<GameObject> RedAgents = new List<GameObject> { };
    public static List<GameObject> BlueAgents = new List<GameObject> { };

    public static void AddRedAgent(GameObject agent)
    {
        RedAgents.Add(agent);
    }

    public static void AddBlueAgent(GameObject agent)
    {
        BlueAgents.Add(agent);
    }

    public static void RemoveRedAgent(GameObject agent)
    {
        RedAgents.Remove(agent);
    }

    public static void RemoveBlueAgent(GameObject agent)
    {
        BlueAgents.Remove(agent);
    }

    public static bool IsAnyRed()
    {
        return RedAgents.Count > 0;
    }

    public static bool IsAnyBlue()
    {
        return BlueAgents.Count > 0;
    }

    public static bool isAny()
    {
        return IsAnyRed() || IsAnyBlue();
    }
}
