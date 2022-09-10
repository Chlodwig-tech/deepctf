using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class StartGameScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        blueBaseScript = GameObject.Find("Blue Base(Clone)").GetComponent<BlueBaseScript>();
        redBaseScript = GameObject.Find("Red Base(Clone)").GetComponent<RedBaseScript>();

        GameObject.Find("ButtonStart").GetComponent<UnityEngine.UI.Button>().interactable = false;
        GameObject.Find("ButtonPlaceBlue").GetComponent<UnityEngine.UI.Button>().interactable = false;
        GameObject.Find("ButtonPlaceRed").GetComponent<UnityEngine.UI.Button>().interactable = false;

        blueBaseScript.OnGameStart();
        redBaseScript.OnGameStart();

        GameManager.redAgentGroup = redBaseScript.m_AgentGroup;
        GameManager.blueAgentGroup = blueBaseScript.m_AgentGroup; 

        AiTrainer.Spawn();
    }
}
