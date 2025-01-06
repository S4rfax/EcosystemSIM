using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MyAgent : Agent
{
    // Felder für Bewegung und Beobachtung
    public float speed = 2f; // Geschwindigkeit
    public Transform target; // Zielposition

    private Rigidbody rb;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Wird zu Beginn jeder Episode aufgerufen
    public override void OnEpisodeBegin()
    {
        //Agent spawn
        transform.localPosition = new Vector3(Random.Range(-7f, 7f), 0, Random.Range(-7f, 7f));

        //Target spawn
        target.localPosition = new Vector3(Random.Range(-7f, 7f), 0, Random.Range(-7f, 7f));
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
            //collision.gameObject.GetComponent<FoodLogic>().OnEaten();
            AddReward(5f);
            EndEpisode();
        }
        else if(colobjct.gameObject.CompareTag("wall"))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }


    // Optional: Heuristik für manuelle Steuerung (z. B. für Tests)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal"); // Bewegung auf der X-Achse
        continuousActions[1] = Input.GetAxisRaw("Vertical");   // Bewegung auf der Z-Achse
    }
}
