using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneScript : MonoBehaviour
{
    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }

    void Awake()
    {
       if(GetArg("--trainer")!=null)
        {
  
            GameManager.IsSpectatorMode = false;
            SceneManager.LoadScene("SceneMain");
        }
    }

    public void SpectatorModeSelected()
    {
        GameManager.IsSpectatorMode = true;
        SceneManager.LoadScene("SceneUIMapGeneration");
    }
    public void LearningModeSelected()
    {
        GameManager.IsSpectatorMode = false;
        SceneManager.LoadScene("SceneUILearningOptions");
    }
    public void GoToMainMenu()
    {
        GameManager.RedAgents = new List<GameObject> { };
        GameManager.BlueAgents = new List<GameObject> { };
        SceneManager.LoadScene("SceneUIStart");
    }
    public void GoToMainScene()
    {
        SceneManager.LoadScene("SceneMain");
    }
}