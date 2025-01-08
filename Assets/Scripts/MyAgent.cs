using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class MyAgent : Agent
{
    // Agent Variables
    [SerializeField] public float speed = 2f; // Geschwindigkeit
    private Rigidbody rb;

    // Food Variables
    //public Transform target; // Zielposition
    [SerializeField] public int foodCount = 3;
    [SerializeField] public GameObject food;
    [SerializeField] private List<GameObject> spawnedFoodList = new List<GameObject>();
    [SerializeField] public float minDistanceFood = 1f;

    // Enviroment Variables
    [SerializeField] private Transform enviromentLocation;

    // Time Variables
    [SerializeField] private int timePerEpisode;
    private float timeLeft;

    // Enemy Agent
    public CarnivoreAgent classObject;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Wird zu Beginn jeder Episode aufgerufen
    public override void OnEpisodeBegin()
    {
        //Agent spawn
        transform.localPosition = new Vector3(Random.Range(-7f, 7f), 0, Random.Range(-7f, 7f));
        transform.Rotate(0, Random.Range(-180f, 180f), 0);

        //Target spawn
        //target.localPosition = new Vector3(Random.Range(-7f, 7f), 0, Random.Range(-7f, 7f));
        CreateFood();

        //Timer
        EpisodeTimerNew();
    }

    private void Update()
    {
        CheckRemainingTime();
    }

    private void CreateFood()
    {
        if(spawnedFoodList.Count != 0)
        {
            RemoveFood(spawnedFoodList);
        }

        for(int i = 0; i < foodCount; i++)
        {
            GameObject newFood = Instantiate(food);

            newFood.transform.parent = enviromentLocation;

            Vector3 foodLocation = new Vector3(Random.Range(-7f, 7f), 0, Random.Range(-7f, 7f));

            while(CheckOverlap(foodLocation, spawnedFoodList, minDistanceFood))
            {
               foodLocation = new Vector3(Random.Range(-7f, 7f), 0, Random.Range(-7f, 7f)); 
            }

            newFood.transform.localPosition = foodLocation;

            Debug.Log(newFood.transform.localPosition);

            spawnedFoodList.Add(newFood);
        }
    }

    private void RemoveFood(List<GameObject> foodToDelete)
    {
        foreach(GameObject i in foodToDelete)
        {
            Destroy(i.gameObject);
        }
        foodToDelete.Clear();
    }

    public bool CheckOverlap(Vector3 posToCheck, List<GameObject> existingObjects, float minDistance)
    {
        if(existingObjects.Count == 0)
        {
            return false;
        }

        foreach(GameObject i in existingObjects)
        {
            Vector3 objectPos = i.transform.localPosition;

            float Distance = Vector3.Distance(posToCheck, objectPos);

            if(Distance < minDistance)
            {
                return true;
            }

        }

        return false;
    }

    // Wird verwendet, um Beobachtungen zu sammeln
    public override void CollectObservations(VectorSensor sensor)
    {
        // Füge Beobachtungen hinzu (z. B. die relative Position zum Ziel)
        sensor.AddObservation(transform.localPosition); // Position des Agenten
        //sensor.AddObservation(target.localPosition);    // Position des Ziels
    }

    // Wird aufgerufen, wenn der Agent eine Aktion ausführt
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Natrual Locomotion
        float rotateMove = actionBuffers.ContinuousActions[0];
        float forwardMove = actionBuffers.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * forwardMove * speed * Time.deltaTime);
        transform.Rotate(0f, rotateMove * speed, 0f, Space.Self);


        // xy-Movement:
        /* // Aktionen aus den ActionBuffers extrahieren
        float moveX = actionBuffers.ContinuousActions[0]; // X-Bewegung
        float moveZ = actionBuffers.ContinuousActions[1]; // Z-Bewegung

        // Bewegung des Agenten basierend auf den Aktionen
        Vector3 move = new Vector3(moveX, 0, moveZ);
        // normalized nötig, damit diagonale Bewegungen genauso "schnell" sind wie gerade
        transform.localPosition += move.normalized * speed * Time.deltaTime; */
    } 

    void OnCollisionEnter(Collision colobjct)
    {
        if (colobjct.gameObject.CompareTag("food"))
        {
            spawnedFoodList.Remove(colobjct.gameObject);
            Destroy(colobjct.gameObject);
            AddReward(10f);
            if(spawnedFoodList.Count == 0)
            {
                AddReward(5f);
                classObject.AddReward(-5f);
                EndEpisode();
                classObject.EndEpisode();
            }
            
        }
        else if(colobjct.gameObject.CompareTag("wall"))
        {
            AddReward(-15f);
            EndEpisode();
            classObject.EndEpisode();
        }
    }


    // Optional: Heuristik für manuelle Steuerung (z. B. für Tests)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal"); // Bewegung auf der X-Achse
        continuousActions[1] = Input.GetAxisRaw("Vertical");   // Bewegung auf der Z-Achse
    }

    private void EpisodeTimerNew()
    {
        timeLeft = Time.time + timePerEpisode;
    }

    private void CheckRemainingTime()
    {
        if(Time.time >= timeLeft)
        {
            AddReward(-15f);
            classObject.AddReward(-15f);

            EndEpisode();
            classObject.EndEpisode();
        }
    }

}
