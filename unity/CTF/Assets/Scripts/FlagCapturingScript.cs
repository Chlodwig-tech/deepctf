using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlagCapturingScript : MonoBehaviour
{
    [SerializeField] string baseColor;
    [SerializeField] GameObject FlagInBase;
    [SerializeField] GameObject EnemyFlagInBase;
    private GameObject OtherBase;
    private RewardValuesScript rewardValues;

    private void passTheFlag(GameObject object1, GameObject object2) {
        bool isActive = object1.activeSelf;
        object1.SetActive(!isActive);
        isActive = object2.activeSelf;
        object2.SetActive(!isActive);
    }
    private void win (string color, GameObject object1, GameObject object2, GameObject collidingAgent)
    {
        bool isActive = object1.activeSelf;
        object1.SetActive(!isActive);
        isActive = object2.activeSelf;
        object2.SetActive(!isActive);
        // team wins
        Debug.Log("Team " + color + " wins!");
        if (color == "blue")
        {
            getSGS().AddRewardTeam(rewardValues.rewards["gameLost"], "red");
            getSGS().AddRewardTeam(rewardValues.rewards["gameWon"], "blue");
            getSGS().EndEpisodeForAllAgents();
        }
        else
        {
            getSGS().AddRewardTeam(rewardValues.rewards["gameLost"], "blue");
            getSGS().AddRewardTeam(rewardValues.rewards["gameWon"], "red");
            getSGS().EndEpisodeForAllAgents();
        }
    }

    private void OnTriggerEnter(Collider collidingObject)
    {
        if (collidingObject.tag == "Agent")
        {
            if (baseColor == "blue")
                OtherBase = GameObject.Find("Red Base(Clone)");
            else
                OtherBase = GameObject.Find("Blue Base(Clone)");

            var collidingAgent = collidingObject.gameObject.transform.parent.gameObject;

            GameObject AgentFlag = collidingAgent.GetComponent<AgentComponentsScript>().AgentFlag;
            bool agentColorEqualsBaseColor = collidingAgent.GetComponent<AgentComponentsScript>().color == baseColor;
            bool agentHoldsEnemyFlag = AgentFlag.activeSelf;

            rewardValues = collidingAgent.GetComponent<RewardValuesScript>();
            rewardValues.getRewardValues();

            if (agentColorEqualsBaseColor)
            {
                if (!agentHoldsEnemyFlag) 
                    return; 

                if (FlagInBase.activeSelf) // if the agent touches own base, holds enemy's flag, and own flag is in own base
                {
                    collidingAgent.GetComponent<AgentMovementWSAD>().AddRewardAgent(rewardValues.rewards["flagDelivered"]);
                    win(collidingAgent.GetComponent<AgentComponentsScript>().color, AgentFlag, FlagInBase, collidingAgent);
                }
                else                       // if the agent touches own base, holds enemy's flag, and own flag is not in own base
                {
                    collidingAgent.GetComponent<AgentMovementWSAD>().AddRewardAgent(rewardValues.rewards["flagDelivered"]);
                    passTheFlag(EnemyFlagInBase, AgentFlag);
                }
            }
            else
            {
                if (!AgentFlag.activeSelf && FlagInBase.activeSelf) // if the agent touches enemy's base, does not hold enemy's flag, and enemy's flag is in enemy's base
                {
                    collidingAgent.GetComponent<AgentMovementWSAD>().AddRewardAgent(rewardValues.rewards["flagCaptured"]);
                    passTheFlag(FlagInBase, AgentFlag);
                }
                else if (EnemyFlagInBase.activeSelf)                // if the agent touches enemy's base, and own flag is in enemy's base
                {
                    collidingAgent.GetComponent<AgentMovementWSAD>().AddRewardAgent(rewardValues.rewards["flagCaptured"]);
                    OtherBase.GetComponent<ReturnFlagScript>().returnFlagFromBase(collidingAgent, EnemyFlagInBase);
                }
            }
        }
    }
    public StartGameScript getSGS()
    {
        Scene sceneMain = SceneManager.GetActiveScene();
        GameObject interfaceCamera = sceneMain.GetRootGameObjects()[7].gameObject;
        StartGameScript startGameScript = interfaceCamera.GetComponentInChildren<StartGameScript>();
        return startGameScript;
    }
}
