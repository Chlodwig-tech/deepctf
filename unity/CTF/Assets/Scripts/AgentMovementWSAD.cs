using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public class AgentMovementWSAD : Agent
{
    public float rotateSpeed = 180f;
    public float forwardSpeed = 600f;
    public float backSpeed = 450f;
    private float speedModifier = 1f;
    public GameObject[] agents;

    private int numberOfRays = 24;
    private float RayDistance = 500.0f;

    private GameObject ownBase;
    private GameObject enemyBase;

    [HideInInspector]
    public bool isAgentSet;

    private float latestDistanceOwn;
    private float latestDistanceEnemy;
    private bool latestPurposeWasEnemyFlag;
    private int decisionPeriod;

    public bool weGotEnemyFlag = false;
    public bool weGotOurFlag = true;
    public bool isTrainer = false;


    private float Reward__Delete_it = 0;

    void Awake()
    {
        /*
        var levelOrFreeMode = PlayerPrefs.GetInt("level");
        if (levelOrFreeMode != 4 && gameObject.GetComponent<AgentComponentsScript>().color == "blue")
        {
            isTrainer = true;
            gameObject.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.HeuristicOnly;
        }
        else
        {
            isTrainer = false;
            gameObject.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.Default;
        }
        */
        isAgentSet = false;
        if (isTrainer)
        {
            isTrainerAgent = true;
        }
    }

    void Start()
    {

        if (gameObject.GetComponent<AgentComponentsScript>().color == "blue")
        {
            enemyBase = GameObject.Find("Red Base(Clone)");
            ownBase = GameObject.Find("Blue Base(Clone)");
        }
        else
        {
            enemyBase = GameObject.Find("Blue Base(Clone)");
            ownBase = GameObject.Find("Red Base(Clone)");
        }

        latestDistanceOwn = Vector3.Distance(gameObject.transform.position, ownBase.transform.position);
        latestDistanceEnemy = Vector3.Distance(gameObject.transform.position, enemyBase.transform.position);
        latestPurposeWasEnemyFlag = true;
        decisionPeriod = gameObject.GetComponent<DecisionRequester>().DecisionPeriod;

        isAgentSet = true;
    }

    private void FixedUpdate()
    {
        RewardValuesScript.getRewardValues();
        AddRewardAgent(RewardValuesScript.rewards["agentTimeRewardForNothing"]);
        // FixUpdDistanceStandard();
        if (!weGotEnemyFlag || gameObject.GetComponent<AgentComponentsScript>().AgentFlag.activeSelf)    // jeśli moja drużyna nie ma flagi przeciwnika lub ja mam flagę przeciwnika
            DistanceRewardKacpraPoKonsultacji();

    }

    private void FixUpdDistanceStandard()
    {
        RewardValuesScript.getRewardValues();
        float distance;
        if (GameManager.EnvContr.GetSteps() % 200 == 0 && GameManager.EnvContr.GetSteps() != 0)
        {
            if (gameObject.GetComponent<AgentComponentsScript>().AgentFlag.activeSelf)
            {
                distance = Vector3.Distance(gameObject.transform.position, ownBase.transform.position);
            }
            else
            {
                distance = Vector3.Distance(gameObject.transform.position, enemyBase.transform.position);
            }
            var reward = FlagDistanceReward(distance);
            AddRewardAgent(reward[0]);

            GameManager.AddRewardTeam(reward[1], gameObject.GetComponent<AgentComponentsScript>().color);
            UnityEngine.Debug.Log(gameObject.ToString() + " ::: NAGRODA " + reward);
        }
        
    }

    private void FixUpdDistanceRewardsCloserBetter()
    {
        RewardValuesScript.getRewardValues();
        if (GameManager.EnvContr.GetSteps() % 200 == 0 && GameManager.EnvContr.GetSteps() != 0)
        {
            float distance;
            if (gameObject.GetComponent<AgentComponentsScript>().AgentFlag.activeSelf)
            {
                distance = Vector3.Distance(gameObject.transform.position, ownBase.transform.position);
                if (distance < latestDistanceOwn)
                {
                    var reward = FlagDistanceReward(distance);
                    AddRewardAgent(reward[0]);
                    GameManager.AddRewardTeam(reward[1], gameObject.GetComponent<AgentComponentsScript>().color);
                    UnityEngine.Debug.Log(gameObject.ToString() + " ::: NAGRODA " + reward);
                }
                else
                {
                    var reward = FlagDistanceReward(distance);
                    AddRewardAgent(-0.5f * reward[0]);
                    GameManager.AddRewardTeam(-0.5f * reward[1], gameObject.GetComponent<AgentComponentsScript>().color);
                    UnityEngine.Debug.Log(gameObject.ToString() + " ::: NAGRODA " + reward);
                }
            }
            else
            {
                distance = Vector3.Distance(gameObject.transform.position, enemyBase.transform.position);
                if (distance < latestDistanceEnemy)
                {
                    var reward = FlagDistanceReward(distance);
                    AddRewardAgent(reward[0]);
                    GameManager.AddRewardTeam(reward[1], gameObject.GetComponent<AgentComponentsScript>().color);
                    UnityEngine.Debug.Log(gameObject.ToString() + " ::: NAGRODA " + reward);
                }
                else
                {
                    var reward = FlagDistanceReward(distance);
                    AddRewardAgent(-0.5f * reward[0]);
                    GameManager.AddRewardTeam(-0.5f * reward[1], gameObject.GetComponent<AgentComponentsScript>().color);
                    UnityEngine.Debug.Log(gameObject.ToString() + " ::: NAGRODA " + reward);
                }
            }


            latestDistanceOwn = Vector3.Distance(gameObject.transform.position, ownBase.transform.position);
            latestDistanceEnemy = Vector3.Distance(gameObject.transform.position, ownBase.transform.position);

        }
    }

    private void DistanceRewardKacpraPoKonsultacji()
    {
        RewardValuesScript.getRewardValues();
        float distanceOwnNow = Vector3.Distance(gameObject.transform.position, ownBase.transform.position);
        float distanceEnemyNow = Vector3.Distance(gameObject.transform.position, enemyBase.transform.position);

        // MOŻLIWE ŻE LEPIEJ WYJDZIE JAK CO 5 STEPÓW BĘDZIE (CZY RACZEJ CO TYLE ILE MAMY decision interval (u nas chyba 5))
        if (GameManager.EnvContr.GetSteps() % decisionPeriod == 0 && GameManager.EnvContr.GetSteps() != 0)
        {
            if (gameObject.GetComponent<AgentComponentsScript>().AgentFlag.activeSelf)
            {
                if (!weGotEnemyFlag) // jeśli jeszcze nie wiemy że mamy flage przeciwnika, musze powiedzieć o tym kompanom, a wrogom o tym że stracili swoją
                    GameManager.EnvContr.TeamGotEnemyFlag(gameObject.GetComponent<AgentComponentsScript>().color);

                if (latestPurposeWasEnemyFlag)  // ten if jest żeby po zdobyciu flagi nie miał od razu kary z rozpędu za oddalanie się od swojej flagi i miał szansę się wycofać
                {
                    latestDistanceOwn = distanceOwnNow;
                    latestDistanceEnemy = distanceEnemyNow;
                    latestPurposeWasEnemyFlag = false;
                    return;
                }
                AddFlagDistanceRewardBasedOnDistanceDifference(distanceOwnNow, latestDistanceOwn);
            }
            else
            {
                if (weGotEnemyFlag) // jeśli jeszcze nie wiemy że straciliśmy flage przeciwnika, musze powiedzieć o tym kompanom, a wrogom o tym że już odzyskali swoją
                    GameManager.EnvContr.TeamLostEnemyFlag(gameObject.GetComponent<AgentComponentsScript>().color);

                if (!latestPurposeWasEnemyFlag)  // ten if jest żeby po straceniu flagi nie miał od razu kary z rozpędu za oddalanie się od flagi wroga i miał szansę się wycofać
                {
                    latestDistanceOwn = distanceOwnNow;
                    latestDistanceEnemy = distanceEnemyNow;
                    latestPurposeWasEnemyFlag = true;
                    return;
                }
                AddFlagDistanceRewardBasedOnDistanceDifference(distanceEnemyNow, latestDistanceEnemy);
            }
        }
        latestDistanceOwn = distanceOwnNow;
        latestDistanceEnemy = distanceEnemyNow;
    }

    private float[] FlagDistanceReward(float distance)
    {
        float max = RewardValuesScript.rewards["agentCloseToFlag"];
        float maxTeam = RewardValuesScript.rewards["agentCloseToFlag_team"];

        float y = (-distance * max / 400) + max;
        float yTeam = (-distance * maxTeam / 400) + maxTeam;

        y = y > 0 ? y : 0;
        yTeam = yTeam > 0 ? yTeam : 0;
        return new float[] { y, yTeam };
    }

    private void AddFlagDistanceRewardBasedOnDistanceDifference(float distanceNow, float lastestDistance)
    {
        float maxReward = RewardValuesScript.rewards["agentCloseToFlag"];
        float distanceDifference = lastestDistance - distanceNow;
        float reward = maxReward * distanceDifference;
        // Debug.Log("Distance Difference = " + distanceDifference + "  Reward = " + reward);
        Reward__Delete_it += reward;
        // Debug.Log("TOTAL DISTANCE REW. = " + Reward__Delete_it);
        AddRewardAgent(reward);

        float maxRewardTeam = RewardValuesScript.rewards["agentCloseToFlag_team"];
        float rewardTeam = maxRewardTeam * distanceDifference;
        GameManager.AddRewardTeam(rewardTeam, gameObject.GetComponent<AgentComponentsScript>().color);
        //Debug.Log("Team reward for distance: " + rewardTeam);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var forwardAxis = actionBuffers.DiscreteActions[0];
        var rotateAxis = actionBuffers.DiscreteActions[1];

        Rigidbody rb = GetComponent<Rigidbody>();
        switch (forwardAxis)
        {
            case 0: // do tylu
                rb.AddForce(transform.rotation * Vector3.back * backSpeed * Time.deltaTime * speedModifier, ForceMode.VelocityChange);
                break;
            case 1: // brak akcji
                break;
            case 2: // do przodu
                rb.AddForce(transform.rotation * Vector3.forward * forwardSpeed * Time.deltaTime * speedModifier, ForceMode.VelocityChange);
                break;
        }

        switch (rotateAxis)
        {
            case 0: // w lewo
                transform.Rotate(0, -rotateSpeed * Time.deltaTime * speedModifier, 0, Space.World);
                break;
            case 1: // brak rotacji
                break;
            case 2: // w prawo
                transform.Rotate(0, rotateSpeed * Time.deltaTime * speedModifier, 0, Space.World);
                break;
        }
        speedModifier = 1f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        /*if(isTrainer)
        {
            return;
        }*/

        bool agentHoldsFlag = gameObject.GetComponent<AgentComponentsScript>().AgentFlag.activeSelf;    // IS HOLDING FLAG? 1 float
        sensor.AddObservation(agentHoldsFlag ? 1.0f : 0.0f );
        // walls
        float[] arrRaysWalls = raysPerceptionWalls(6); // 24 rays * 1 variables = 24
        for (int i = 0; i < numberOfRays; i++)
        {
            sensor.AddObservation(arrRaysWalls[i]);
         }
        // agents
        float[,] arrRaysAgents = raysPerceptionAgents(7); // 24 rays * 4 variables = 96
        for (int i = 0; i < numberOfRays; i++)
        {
            for (int j = 0; j < arrRaysAgents.GetLength(1); j++)
            {
                sensor.AddObservation(arrRaysAgents[i, j]);
            }
        }
        // bases
        float[,] arrRaysBases = raysPerceptionBases(8); // 24 rays * 5 variables = 120
        for (int i = 0; i < numberOfRays; i++)
        {
            for (int j = 0; j < arrRaysBases.GetLength(1); j++)
            {
                sensor.AddObservation(arrRaysBases[i, j]);
            }
        }
        BiomEyesScript biomEyes = (BiomEyesScript)GetComponentInChildren(typeof(BiomEyesScript));
        //Debug.Log("BIOMEYES:" + biomEyes);
        int[] arrint = biomEyes.GetBiomSensors();
       // Debug.Log("BIOMEYES array:" + arrint+ arrint.Length);
        for (int i = 0; i < arrint.Length; i++){ //44 biomEyes * 4 inputy
            float[] biomEye = { 0, 0, 0, 0 }; //0-nic/inne, 1-accelerate, 2-desert, 3-lake
            biomEye[arrint[i]] = 1;
            for(int j=0; j < biomEye.Length; j++)
                sensor.AddObservation(biomEye[j]);
        }

        sensor.AddObservation(weGotOurFlag ? 1.0f : 0.0f);
        sensor.AddObservation(weGotEnemyFlag ? 1.0f : 0.0f);
    }

    private float[,] raysPerceptionBases(int layerShift)
    {
        int layerMask = 1 << layerShift;
        float startDegree = -90.0f; //zawsze musi byc ujemne!
        float stepDegree = -2 * startDegree / (float)numberOfRays;

        float[,] outputArray = new float[numberOfRays, 5]; //25 promieni, 5 zmiennych, 0-odwrotność dystansu 1-agent/baza 2-kolor(wróg(-1)/swój(1)) 3-czy ma swoją flagę 4-czy ma wrogą flagę
        RaycastHit hit;
        Ray ray;
        for (int i = 0; i < numberOfRays; i++)
        {
            ray = new Ray(transform.position, transform.TransformDirection(Quaternion.Euler(0, startDegree + stepDegree * i, 0) * Vector3.forward));

            if (Physics.Raycast(ray, out hit, RayDistance, layerMask))
            {
                string hittedColor = hit.collider.gameObject.transform.parent.name == "Blue Base(Clone)" ? "blue" : "red";  // CZY TO DZIAŁA?? NIE SPRAWDZANE !!!!!!!!!!!
                outputArray[i, 0] = Normalize(Inverse(hit.distance));
                outputArray[i, 1] = 1; // 1 bo znalazł bazę
                outputArray[i, 2] = hittedColor == gameObject.GetComponent<AgentComponentsScript>().color ? 1.0f : -1.0f; // wroga baza -1 / swoja baza 1
                if (gameObject.GetComponent<AgentComponentsScript>().color == "blue")
                {
                    outputArray[i, 3] = hit.collider.gameObject.transform.parent.gameObject.transform.Find("BlueFlag").gameObject.activeSelf ? 1.0f : 0.0f; // 1 jeśli ma swoją flagę, 0 jak nie ma
                    outputArray[i, 4] = hit.collider.gameObject.transform.parent.gameObject.transform.Find("RedFlag").gameObject.activeSelf ? 1.0f : 0.0f; // 1 jeśli ma wrogą flagę, 0 jak nie ma
                }
                else
                {
                    outputArray[i, 3] = hit.collider.gameObject.transform.parent.gameObject.transform.Find("RedFlag").gameObject.activeSelf ? 1.0f : 0.0f; // 1 jeśli ma swoją flagę, 0 jak nie ma
                    outputArray[i, 4] = hit.collider.gameObject.transform.parent.gameObject.transform.Find("BlueFlag").gameObject.activeSelf ? 1.0f : 0.0f; // 1 jeśli ma wrogą flagę, 0 jak nie ma
                }
                
                //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);
            }
            else
            {
                // tablice od inicjalizacji są wypełnione 0
                ;//Debug.DrawRay(ray.origin, ray.direction * RayDistance, Color.green);
            }

        }
        return outputArray;
    }

    private float[,] raysPerceptionAgents(int layerShift)
    {
        int layerMask = 1 << layerShift;
        float startDegree = -90.0f; //zawsze musi byc ujemne!
        float stepDegree = -2 * startDegree / (float)numberOfRays;

        float[,] outputArray = new float[numberOfRays, 4]; //25 promieni, 4 zmienne, 0-odwrotność dystansu 1-agent/baza 2-kolor(wróg(-1)/swój(1)) 3-czy ma flage
        RaycastHit hit;
        Ray ray;
        for (int i = 0; i < numberOfRays; i++)
        {
            ray = new Ray(transform.position, transform.TransformDirection(Quaternion.Euler(0, startDegree + stepDegree * i, 0) * Vector3.forward));

            if (Physics.Raycast(ray, out hit, RayDistance, layerMask))
            {
                string hittedColor = hit.collider.gameObject.GetComponent<AgentComponentsScript>().color;
                outputArray[i, 0] = Normalize(Inverse(hit.distance));
                outputArray[i, 1] = 1; // 1 bo znalazł agenta
                outputArray[i, 2] = hittedColor == gameObject.GetComponent<AgentComponentsScript>().color ? 1.0f : -1.0f; // wróg -1 / swój 1
                outputArray[i, 3] = hit.collider.gameObject.GetComponent<AgentComponentsScript>().AgentFlag.activeSelf ? 1.0f : 0.0f; // 1 jeśli ma flagę, 0 jak nie ma
                // Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);
            }
            else
            {
                // tablice od inicjalizacji są wypełnione 0
                ;// Debug.DrawRay(ray.origin, ray.direction * RayDistance, Color.green);
            }

        }
        return outputArray;
    }

    private float[] raysPerceptionWalls(int layerShift)
    {
        int layerMask = 1 << layerShift;
        float startDegree = -90.0f; //zawsze musi byc ujemne!
        float stepDegree = -2 * startDegree / (float)numberOfRays;

        float[] outputArray = new float[numberOfRays]; //25 promieni, 1 zmienna, 0-odwrotność dystansu
        RaycastHit hit;
        Ray ray;
        for (int i = 0; i < numberOfRays; i++)
        {
            ray = new Ray(transform.position, transform.TransformDirection(Quaternion.Euler(0, startDegree + stepDegree * i, 0) * Vector3.forward));

            if (Physics.Raycast(ray, out hit, RayDistance, layerMask))
            {
                outputArray[i] = Normalize(Inverse(hit.distance));
                //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);
            }
            else
            {
                // tablice od inicjalizacji są wypełnione 0
                ;// Debug.DrawRay(ray.origin, ray.direction * RayDistance, Color.green);
            }

        }
        return outputArray;
    }

    private float Inverse(float distance)
    {
        return RayDistance - distance;
    }

    private float Normalize(float inverseDistance)
    {
        return inverseDistance/RayDistance;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        bool W, S, A, D;
        if (isTrainer && AiTrainer.GetAITrainerMode())
        {
            (W, S, A, D) = AiTrainer.Run();
        }
        else
        {
            W = Input.GetKey(KeyCode.W);
            S = Input.GetKey(KeyCode.S);
            A = Input.GetKey(KeyCode.A);
            D = Input.GetKey(KeyCode.D);
        }
        var forwardaction = 1;//staniewmiejscu
        if (W)
            forwardaction = 2;//do przodu
        else if (S)
            forwardaction = 0;//dotylu

        actionsOut.DiscreteActions.Array[0] = forwardaction;
        var turnaction = 1;//staniewmiejscu
        if (A)
            turnaction = 0;
        else if (D)
            turnaction = 2;

        actionsOut.DiscreteActions.Array[1] = turnaction;

    }

    public void ChangeSpeedModifier(float newModified)
    {
        speedModifier = newModified;
    }

    public void AddRewardAgent(float reward)
    {
        AddReward(reward);
    }

    public override void OnEpisodeBegin()
    {
        ; // Debug.Log("EPISODE BEGIN");
    }
}
