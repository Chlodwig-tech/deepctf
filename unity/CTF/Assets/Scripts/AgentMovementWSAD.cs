using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;

public class AgentMovementWSAD : Agent
{
    public float rotateSpeed = 180f;
    public float forwardSpeed = 600f;
    public float backSpeed = 450f;
    private float speedModifier = 1f;
    public GameObject[] agents;
    private List<GameObject> teamBlue = new List<GameObject>{};
    private List<GameObject> teamRed = new List<GameObject>{};

    private int numberOfRays = 10;

    void Start()
    {
        //GetTeams();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var forwardAxis = actionBuffers.DiscreteActions[0];
        var rotateAxis = actionBuffers.DiscreteActions[1];

        Rigidbody rb = GetComponent<Rigidbody>();
        switch (forwardAxis)
        {
            case 1: //do przodu
                rb.AddForce(transform.rotation * Vector3.forward * forwardSpeed * Time.deltaTime * speedModifier, ForceMode.VelocityChange);
                break;
            case 2: //do ty�u
                rb.AddForce(transform.rotation * Vector3.back * backSpeed * Time.deltaTime * speedModifier, ForceMode.VelocityChange);
                break;
        }

        switch (rotateAxis)
        {
            case 1: //w lewo
                transform.Rotate(0, -rotateSpeed * Time.deltaTime * speedModifier, 0, Space.World);
                break;
            case 2: // w prawo
                transform.Rotate(0, rotateSpeed * Time.deltaTime * speedModifier, 0, Space.World);
                break;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float[,] arrRays = raysPerception(); //40 floatow
        for (int i = 0; i < numberOfRays; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                sensor.AddObservation(arrRays[i,j]);
            }

         }
        BiomEyesScript bes = (BiomEyesScript)GetComponentInChildren(typeof(BiomEyesScript));
        int[] arint = bes.GetBiomSensors();
        for (int i =0; i<arint.Length;i++){
             sensor.AddObservation((float)arint[i]);
        }
    }


    private float[,] raysPerception()
    {
        int layerMask = 1 << 6;

        float RayDistance = 200.0f;

        float startDegree = -90.0f;//zawsze musi byc ujemne!
        float stepDegree = -2 * startDegree / (float)numberOfRays;
        

        float[,] outputArray = new float[10,4]; //10 promieni, po 4 zmienne, i,0-distance, i,1 - rodzaj, i,2 kolor, i,3 czy z flaga?
        RaycastHit hit;
        Ray ray;
        for (int i =0;i<numberOfRays;i++)
        {
           ray  = new Ray(transform.position, transform.TransformDirection(Quaternion.Euler(0, startDegree+stepDegree*i, 0) * Vector3.forward));

            if (Physics.Raycast(ray, out hit, RayDistance, layerMask))
            {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);
                rayResponseComponent rayrespond = hit.collider.gameObject.GetComponent<rayResponseComponent>();
                Debug.Log(this.name + "Ray" + i + "Did Hit: " + hit.collider.gameObject + " in distance: " + hit.distance);
                Debug.Log(this.name + "Ray" + i + "Did Hit: " + hit.collider.gameObject + " in distance: " + hit.distance + "|" + rayrespond.type + rayrespond.color + rayrespond.isFlag);
                outputArray[i, 0] = hit.distance;

                outputArray[i, 1] = rayrespond.type;
                float col = rayrespond.color;
                outputArray[i, 2] = col;
                outputArray[i, 3] = rayrespond.isFlag;

            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * RayDistance, Color.green);
                //Debug.Log(this.name + "Ray"+i+ "Did not Hit");
            }

        }
        return outputArray;
    }

    //public override void Heuristic(in ActionBuffers actionsOut)
    public void FixedUpdate()
    {
        bool W = Input.GetKey(KeyCode.W);
        bool S = Input.GetKey(KeyCode.S);
        bool A = Input.GetKey(KeyCode.A);
        bool D = Input.GetKey(KeyCode.D);

        /*
        bool W = true ? Random.Range(0, 10) > 1 : false;
        bool S = true ? Random.Range(0, 2) == 1 : false;
        bool A = true ? Random.Range(0, 10) > 2 : false;
        bool D = true ? Random.Range(0, 20) > 1 : false;*/

        // Faster forward than back
        Rigidbody rb = GetComponent<Rigidbody>();
        if (W)
            rb.AddForce(transform.rotation * Vector3.forward * forwardSpeed * Time.deltaTime * speedModifier, ForceMode.VelocityChange);
        else if (S)
            rb.AddForce(transform.rotation * Vector3.back * backSpeed * Time.deltaTime * speedModifier, ForceMode.VelocityChange);
        else
            rb.velocity = Vector3.zero;


        if (A)
            transform.Rotate(0, -rotateSpeed * Time.deltaTime * speedModifier, 0, Space.World);
        if (D)
            transform.Rotate(0, rotateSpeed * Time.deltaTime * speedModifier, 0, Space.World);


        speedModifier = 1f;

        //dla widzenia promieni
       // raysPerception();
        //BiomEyesScript bes = (BiomEyesScript)GetComponentInChildren(typeof(BiomEyesScript));
        //bes.GetBiomSensors();
    }


    public void ChangeSpeedModifier(float newModified)
    {
        speedModifier = newModified;
    }

    public void AddRewardAgent(float reward)
    {
        AddReward(reward);
    }


    public void Kill()
    {
        var rewardValues = gameObject.GetComponent<RewardValuesScript>();
        rewardValues.getRewardValues();
        AddRewardAgent(rewardValues.rewards["agentDead"]);
        gameObject.SetActive(false);
        
        CheckIfLost();
    }

    private void CheckIfLost()
    {
        bool aliveAgents = false;
        Transform parent = gameObject.transform.parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject agent = parent.GetChild(i).gameObject;
            if (agent.activeSelf)
            {
                aliveAgents = true;
            }
        }

        if (!aliveAgents) // if all agents from team died
        {
            var rewardValues = gameObject.GetComponent<RewardValuesScript>();
            rewardValues.getRewardValues();
            if (gameObject.GetComponent<AgentComponentsScript>().color == "blue")
            {
                Debug.Log("Team red wins!");
                getSGS().AddRewardTeam(rewardValues.rewards["gameLost"], "blue");
                getSGS().AddRewardTeam(rewardValues.rewards["gameWon"], "red");
                getSGS().EndEpisodeForAllAgents();
            }
            else
            {
                Debug.Log("Team blue wins!");
                getSGS().AddRewardTeam(rewardValues.rewards["gameLost"], "red");
                getSGS().AddRewardTeam(rewardValues.rewards["gameWon"], "blue");
                getSGS().EndEpisodeForAllAgents();
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("EPISODE BEGIN");
        //getSGS().StartGame();
       
    }


    //private void GetTeams()
    //{
    //    Transform agents = gameObject.transform.parent.transform.parent;
    //    Transform redAgents = agents.GetChild(0);
    //    Transform blueAgents = agents.GetChild(1);
    //    for (int i = 0; i < gameObject.transform.parent.childCount; i++)
    //    {
    //        teamRed.Add(redAgents.GetChild(i).gameObject);
    //        teamBlue.Add(blueAgents.GetChild(i).gameObject);
    //    }
    //}
    public StartGameScript getSGS()
    {
        Scene sceneMain = SceneManager.GetActiveScene();
        GameObject interfaceCamera = sceneMain.GetRootGameObjects()[7].gameObject;
        StartGameScript startGameScript = interfaceCamera.GetComponentInChildren<StartGameScript>();
        return startGameScript;
    }
}
