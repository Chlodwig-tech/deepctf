using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class AiTrainer
{
    public enum FIELD_TYPE { DEFAULT, GRASS, GRASS_ACCELERATE_DESERT, GRASS_WATER, RANDOM_FIELD};

    public static bool AITrainerMode = GameObject.Find("Agents").GetComponent<AgentsComponents>().AiTrainerMode;
    public static int test_id; // test level form 0 to 5
    public static FIELD_TYPE variant_id; // test level form 0 to 3
    private static List<TestLevel> level = new List<TestLevel>() {
                                           new TestLevel0(), new TestLevel1(),
                                           new TestLevel2(), new TestLevel3(),
                                           new TestLevel4(), new TestLevel5()};

    public static void getLearningConfig()
    {
        // get file from streaming assets and load values;

        List<string> lines = new List<string>();
        string path = Application.streamingAssetsPath + "/LearningConfig.txt";
        StreamReader reader = new StreamReader(path);

        string[] config = reader.ReadLine().Split(';');
        test_id = int.Parse(config[0]);
        switch(int.Parse(config[1]))
        {
            case 1:
                variant_id = !AITrainerMode ? FIELD_TYPE.DEFAULT : FIELD_TYPE.GRASS;
                break;
            case 2:
                variant_id = !AITrainerMode ? FIELD_TYPE.DEFAULT : FIELD_TYPE.GRASS_ACCELERATE_DESERT;
                break;
            case 3:
                variant_id = !AITrainerMode ? FIELD_TYPE.DEFAULT : FIELD_TYPE.GRASS_WATER;
                break;
            case 4:
                variant_id = !AITrainerMode ? FIELD_TYPE.DEFAULT : FIELD_TYPE.RANDOM_FIELD;
                break;
            default:
                variant_id = !AITrainerMode ? FIELD_TYPE.DEFAULT : FIELD_TYPE.DEFAULT;
                break;
        }

        reader.Close();
    }


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
                lakesPercentage = Random.Range(15, 30);
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
        var levelOrFreeMode = PlayerPrefs.GetInt("level");
        return level[test_id].Run();
        //return level[levelOrFreeMode].Run();
    }

    public static void Spawn()
    {
        /*
        var levelOrFreeMode = PlayerPrefs.GetInt("level");
        if (levelOrFreeMode != 4)
        {
            AITrainerMode = true;
        }
        else
        {
            AITrainerMode = false;
        }
        if (!AITrainerMode)
           return;
        */
        level[test_id].Spawn();
        //level[levelOrFreeMode].Spawn();
    }

    public static bool GetAITrainerMode()
    {
        /*
        var levelOrFreeMode = PlayerPrefs.GetInt("level");
        if (levelOrFreeMode != 4)
        {
            AITrainerMode = true;
        }
        else
        {
            AITrainerMode = false;
        }
        */
        return AITrainerMode;
    }
}
