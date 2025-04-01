using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.Timeline;

public class TurtleAgent3 : Agent 
{
    [SerializeField] private Transform _goal;
    [SerializeField] private Transform _wall;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180.0f;
    
    [SerializeField] private MeshRenderer floorMeshRenderer;
    
    private int _currentEpisode = 0;
    private float _cumulativeReward = 0f;
    
    public override void Initialize()
    {
        Debug.Log("Initialize()");
        
        _currentEpisode = 0;
        _cumulativeReward = 0f;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin()");
        
        _currentEpisode++;
        _cumulativeReward = 0f;
        
        SpawnObjects();
    }

    private void SpawnObjects()
    {
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(Random.Range(-4.5f, -0.7f), 0.15f, Random.Range(-4.5f, 4.5f));
        
        _goal.localPosition = new Vector3(Random.Range(0.7f, 4.5f), 0.3f, Random.Range(-4.5f, 4.5f));
        
        _wall.localPosition = new Vector3(0f, 0.25f, Random.Range(-2.5f, 2.5f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //The Goal's position
        float goalPosX_normalized = _goal.localPosition.x / 5f;
        float goalPosZ_normalized = _goal.localPosition.z / 5f;
        
        //The Turtle's position
        float turtlePosX_normalized = transform.localPosition.x / 5f;
        float turtlePosZ_normalized = transform.localPosition.z / 5f;
        
        //The Turtle's direction (on the Y-axis)
        float turtleRotation_normalized = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;
        
        float wallPosX_normalized = _wall.localPosition.x / 5f;
        float wallPosZ_normalized = _wall.localPosition.z / 5f;
        
        sensor.AddObservation(goalPosX_normalized);
        sensor.AddObservation(goalPosZ_normalized);
        sensor.AddObservation(turtlePosX_normalized);
        sensor.AddObservation(turtlePosZ_normalized);
        sensor.AddObservation(turtleRotation_normalized);
        sensor.AddObservation(wallPosX_normalized);
        sensor.AddObservation(wallPosZ_normalized);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //Move the agent using the action
        MoveAgent(actions.DiscreteActions);
        
        //Penalty given each step to encourage the agent to reach the goal faster
        AddReward(-2f / MaxStep);
        
        //Update the cumulative reward after adding the step penalty
        _cumulativeReward = GetCumulativeReward();
    }
    
    public override void Heuristic(in ActionBuffers actionsOut) 
    {
        var discreteActions = actionsOut.DiscreteActions;
    
        if (Input.GetKey(KeyCode.UpArrow)) // Move forward
        {
            discreteActions[0] = 1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow)) // Rotate left
        {
            discreteActions[0] = 2;
        }
        else if (Input.GetKey(KeyCode.RightArrow)) // Rotate right
        {
            discreteActions[0] = 3;
        }
        else
        {
            discreteActions[0] = 0; // No movement
        }
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];

        switch (action)
        {
            case 1: //Move forward
                transform.localPosition += transform.forward * _moveSpeed * Time.deltaTime;
                break;
            case 2: //Rotate left
                transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f);
                break;
            case 3: //Rotate right
                transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            GoalReached();
        }
    }

    private void GoalReached()
    {
        AddReward(1f); //Reward the agent for reaching the goal
        _cumulativeReward = GetCumulativeReward();

        //Change the color of Turtle to green
        if (floorMeshRenderer != null)
        {
            floorMeshRenderer.material.color = Color.green;
        }
        
        EndEpisode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            //Apply a small penalty for colliding with a wall
            AddReward(-1f);
            EndEpisode();
            
            //Change the color of Turtle to red
            if (floorMeshRenderer != null)
            {
                floorMeshRenderer.material.color = Color.red;
            }
        }
    }

    // private void OnCollisionStay(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag("Wall"))
    //     {
    //         //Continuously apply a small penalty while in contact with a wall
    //         AddReward(-0.01f * Time.fixedDeltaTime);
    //     }
    // }
    //
    // private void OnCollisionExit(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag("Wall"))
    //     {
    //         //Change the color of the Turtle back to blue
    //         if (floorMeshRenderer != null)
    //         {
    //             floorMeshRenderer.material.color = Color.gray;
    //         }
    //     }
    // }
}
