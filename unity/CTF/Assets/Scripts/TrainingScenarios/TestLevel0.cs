using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLevel0 : TestLevel
{
    public override (bool, bool, bool, bool) Run()
    {
        bool W = false;
        bool S = false;
        bool A = false;
        bool D = false;
        return (W, S, A, D);
    }

    
    public override void Spawn()
    {
        foreach (var agent in GameManager.BlueAgents)
        {
            agent.transform.position = new Vector3(agent.transform.position.x, 20, agent.transform.position.z);
        }
    }

}