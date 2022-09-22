using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnFlagScript : MonoBehaviour
{
    [SerializeField] GameObject FlagInOtherBase;
    [SerializeField] GameObject OwnFlagInOtherBase;

    private void passTheFlag(GameObject object1, GameObject object2)
    {
        bool isActive = object1.activeSelf;
        object1.SetActive(!isActive);
        isActive = object2.activeSelf;
        object2.SetActive(!isActive);
    }
    private void win(GameObject agent, GameObject object1, GameObject object2)
    {
        passTheFlag(object1, object2);
        // team wins
        string color = agent.GetComponent<AgentComponentsScript>().color;
        Debug.Log("Team " + color + " wins!");
        Reward(agent, color);
    }
    private void win(GameObject object1, GameObject agent)
    {
        string agentColor = agent.GetComponent<AgentComponentsScript>().color;
        string color = agentColor == "blue" ? "red" : "blue";
        bool isActive = object1.activeSelf;
        object1.SetActive(!isActive);
        // team wins
        Debug.Log("Team " + color + " wins!");
        Reward(agent, color);
    }
    private void Reward(GameObject agent, string color)
    {
        if (color == "blue")
        {
            GameManager.AddRewardTeam(RewardValuesScript.rewards["gameLost_team"], "red");
            GameManager.AddRewardTeam(RewardValuesScript.rewards["gameWon_team"], "blue");
            GameManager.EnvContr.textBlueWin.SetActive(true);
            GameManager.EnvContr.EndEpisode();
        }
        else
        {
            GameManager.AddRewardTeam(RewardValuesScript.rewards["gameLost_team"], "blue");
            GameManager.AddRewardTeam(RewardValuesScript.rewards["gameWon_team"], "red");
            GameManager.EnvContr.textRedWin.SetActive(true);
            GameManager.EnvContr.EndEpisode();
        }
    }
    public void returnFlagFromBase(GameObject collidingAgent, GameObject EnemyFlagInBase)
    {
        if (FlagInOtherBase.activeSelf) // if the agent touches enemy's base, own flag is in enemy's base, and enemy's flag is in own base
        {
            win(collidingAgent, EnemyFlagInBase, FlagInOtherBase);
        }
        else                            // if the agent touches enemy's base, own flag is in enemy's base, and enemy's flag is not in own base
        {
            passTheFlag(EnemyFlagInBase, OwnFlagInOtherBase);
        }
    }

    public void returnFlagFromAgent(GameObject agentFlag, GameObject agentWithFlag)
    {
        if (FlagInOtherBase.activeSelf)
        {
            RewardValuesScript.getRewardValues();
            win(FlagInOtherBase, agentWithFlag);
        }
        else
        {
            passTheFlag(agentFlag, OwnFlagInOtherBase);
        }
    }
}
