using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class StartGameScript : MonoBehaviour
{
    public SimpleMultiAgentGroup redAgentGroup;
    public SimpleMultiAgentGroup blueAgentGroup;
    private BlueBaseScript blueBaseScript;
    private RedBaseScript redBaseScript;
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

        redAgentGroup = redBaseScript.m_AgentGroup;
        blueAgentGroup = blueBaseScript.m_AgentGroup;
    }
    public void EndEpisodeForAllAgents()
    {
        Debug.Log("ENDEPISODE!");
        redBaseScript.m_AgentGroup.EndGroupEpisode();
        blueBaseScript.m_AgentGroup.EndGroupEpisode();

        
    }
    public void AddRewardTeam(float reward, string color)
    {
        if (color == "blue")
        {
            blueBaseScript.m_AgentGroup.AddGroupReward(reward);
        }
        else
        {
            redBaseScript.m_AgentGroup.AddGroupReward(reward);
        }
    }
}
