using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLevel2 : TestLevel
{
    public int variant = 0;

    public float rotateSpeed = 180f;
    public float forwardSpeed = 600f;
    public float backSpeed = 450f;

    public override (bool, bool, bool, bool) Run()
    {
        bool W = Random.Range(0, 10) > 5;
        bool S = !W;
        bool A = Random.Range(0, 10) > 5;
        bool D = !A;

        return (W, S, A, D);
    }

    public override void Spawn()
    {
        var blueBaseScript = GameObject.Find("Blue Base(Clone)").GetComponent<BlueBaseScript>();

        for (int i = 0; i < Random.Range(10, 30); i++)
        {
            int index;
            do
            {
                index = Random.Range(0, 8899);
                if (blueBaseScript.CheckIfCanSpawnAt(index))
                    break;
            } while (true);

            blueBaseScript.SpawnAgentAt(index);
        }
    }
}
