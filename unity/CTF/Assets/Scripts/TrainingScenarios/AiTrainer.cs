using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AiTrainer
{
    public enum FIELD_TYPE { DEFAULT, GRASS, GRASS_ACCELERATE_DESERT, GRASS_WATER, RANDOM_FIELD};

    public static bool AITrainerMode = GameObject.Find("Agents").GetComponent<AgentsComponents>().AiTrainerMode;
    public static int test_id = 2; // test level form 0 to 5
    public static FIELD_TYPE variant_id = !AITrainerMode ? FIELD_TYPE.DEFAULT : FIELD_TYPE.GRASS; // test level form 0 to 3
    private static List<TestLevel> level = new List<TestLevel>() {
                                           new TestLevel0(), new TestLevel1(),
                                           new TestLevel2(), new TestLevel3(),
                                           new TestLevel4(), new TestLevel5()};

    public static void SetPercentages(ref int lakesPercentage, ref int accelerateSurfacePercentage, ref int desertsPercentage)
    {
        switch (variant_id)
        {
            case FIELD_TYPE.GRASS:
                lakesPercentage = 0;
                accelerateSurfacePercentage = 0;
                desertsPercentage = 0;
                return;
            case FIELD_TYPE.GRASS_ACCELERATE_DESERT:
                lakesPercentage = 0;
                accelerateSurfacePercentage = Random.Range(0, 70);
                desertsPercentage = 70 - accelerateSurfacePercentage;
                return;
            case FIELD_TYPE.GRASS_WATER:
                lakesPercentage = Random.Range(30, 70);
                accelerateSurfacePercentage = 0;
                desertsPercentage = 0;
                return;
            case FIELD_TYPE.RANDOM_FIELD:
                lakesPercentage = 0;
                accelerateSurfacePercentage = 0;
                desertsPercentage = 0;
                for (int i = 0; i < 90; i++)
                {
                    switch (Random.Range(0, 4))
                    {
                        case 1:
                            lakesPercentage++;
                            break;
                        case 2:
                            accelerateSurfacePercentage++;
                            break;
                        case 3:
                            desertsPercentage++;
                            break;
                    }
                }
                return;
            default:
                return;
        }
    }

    public static (bool, bool, bool, bool) Run()
    {
        return level[test_id].Run();
    }

    public static void Spawn()
    {
        if (!AITrainerMode)
           return;

        level[test_id].Spawn();
    }

    public static bool GetAITrainerMode()
    {
        return AITrainerMode;
    }
}
