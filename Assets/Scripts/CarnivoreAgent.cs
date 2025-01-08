using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class CarnivoreAgent : Agent
{
    // Agent Variables
    [SerializeField] public float speed = 2f; // Geschwindigkeit
    private Rigidbody rb;

    [SerializeField] public float minDistanceHerbCarn = 1f;

    public GameObject env;

    public GameObject herbivore;
    public MyAgent classObject;
    public List<GameObject> herbivoreList = new List<GameObject>();
    private float previousDistance;
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        //Debug.Log("CarnivoreAgent Initialized");
    }

    // Wird zu Beginn jeder Episode aufgerufen
    public override void OnEpisodeBegin()
    {
        //Debug.Log("OnEpisodeBegin called for CarnivoreAgent");
        
        //Agent spawn
        Vector3 spawnLocation = new Vector3(Random.Range(-7f, 7f), 0, Random.Range(-7f, 7f));

        herbivoreList.Clear();
        herbivoreList.Add(herbivore);

        while(classObject.CheckOverlap(spawnLocation, herbivoreList, minDistanceHerbCarn))
        {
            spawnLocation = new Vector3(Random.Range(-7f, 7f), 0, Random.Range(-7f, 7f));
        }

        transform.localPosition = spawnLocation;
        transform.Rotate(0, Random.Range(-180f, 180f), 0);

        // Initialize previousDistance
        previousDistance = Vector3.Distance(transform.position, herbivore.transform.position);
        Debug.Log($"Initial distance to herbivore: {previousDistance}");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Füge Beobachtungen hinzu (z. B. die relative Position zum Ziel)
        sensor.AddObservation(transform.localPosition); // Position des Agenten
        //Debug.Log("CollectObservations called");
    }

    // Wird aufgerufen, wenn der Agent eine Aktion ausführt
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Natrual Locomotion
        float rotateMove = actionBuffers.ContinuousActions[0];
        float forwardMove = actionBuffers.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * forwardMove * speed * Time.deltaTime);
        transform.Rotate(0f, rotateMove * speed, 0f, Space.Self);

        // Calculate distance to prey
        float distanceToPrey = Vector3.Distance(transform.position, herbivore.transform.position);
        
        // Reward for reducing distance
        if (distanceToPrey < previousDistance)
        {
            AddReward(0.01f);
            Debug.Log("Reward for reducing distance: +0.01");
        }
        // Penalty for increasing distance
        else if (distanceToPrey > previousDistance)
        {
            AddReward(-0.01f);
            Debug.Log("Penalty for increasing distance: -0.01");
        }

        // Update previous distance
        previousDistance = distanceToPrey;
        Debug.Log($"Distance to herbivore: {distanceToPrey}");
    }

    void OnCollisionEnter(Collision colobjct)
    {
        if (colobjct.gameObject.CompareTag("herbivore"))
        {
            AddReward(10f);
            classObject.AddReward(-15f);
            //Debug.Log("Collision with herbivore: +10 reward, -15 reward for herbivore");

            EndEpisode();
            classObject.EndEpisode();
            
        }
        else if(colobjct.gameObject.CompareTag("wall"))
        {
            AddReward(-15f);
            //Debug.Log("Collision with wall: -15 reward, End episode");
            EndEpisode();
        }
    }

}
