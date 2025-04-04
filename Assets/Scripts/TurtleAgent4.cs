using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;
using UnityEngine.Timeline;

public class TurtleAgent4 : Agent 
{
    [SerializeField] private Transform _goal;
    [SerializeField] private Transform _wall;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180.0f;
    
    [SerializeField] private MeshRenderer floorMeshRenderer;
    
    private int _currentEpisode = 0;
    private float _cumulativeReward = 0f;

    private bool _hasPassedWall;
    private bool _hasTouchedGoal;
    
    private Color _previousColor;
    
    private Vector3 _lastPosition;
    private float _turtleSpeed;
    private Vector3 _lastWallPosition;
    private float _wallSpeed;
    
    public override void Initialize()
    {
        Debug.Log("Initialize()");
        
        _currentEpisode = 0;
        _cumulativeReward = 0f;
        _hasTouchedGoal = false;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin()");

        _lastPosition = transform.localPosition;
        _turtleSpeed = 0f;
        _lastWallPosition = _wall.localPosition;
        _wallSpeed = 0f;
        
        _hasPassedWall = false;
        
        // Check the cumulative reward from the previous episode
        if (_hasTouchedGoal)
        {
            // Change the floor color to green if the agent succeeded
            if (floorMeshRenderer != null)
            {
                floorMeshRenderer.material.color = Color.green;
            }
        }
        else
        {
            // Change the floor color to red if the agent failed
            if (floorMeshRenderer != null)
            {
                floorMeshRenderer.material.color = Color.red;
            }
        }
        
        _hasTouchedGoal = false;
        
        _currentEpisode++;
        _cumulativeReward = 0f;
        
        SpawnObjects();
    }

    private void SpawnObjects()
    {
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(Random.Range(-4.5f, -0.7f), 0.15f, Random.Range(-4.5f, 4.5f));
        
        _goal.localPosition = new Vector3(Random.Range(0.7f, 4.5f), 0.3f, Random.Range(-4.5f, 4.5f));
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
        
        // The Turtle's speed
        float turtleSpeed_normalized = Mathf.Clamp01(_turtleSpeed / 5f);
        // The Wall's speed
        float wallSpeed_normalized = Mathf.Clamp01(_wallSpeed / 5f);
        
        sensor.AddObservation(goalPosX_normalized);
        sensor.AddObservation(goalPosZ_normalized);
        sensor.AddObservation(turtlePosX_normalized);
        sensor.AddObservation(turtlePosZ_normalized);
        sensor.AddObservation(turtleRotation_normalized);
        sensor.AddObservation(wallPosX_normalized);
        sensor.AddObservation(wallPosZ_normalized);
        sensor.AddObservation(turtleSpeed_normalized);
        sensor.AddObservation(wallSpeed_normalized);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        _turtleSpeed = Vector3.Distance(transform.localPosition, _lastPosition) / Time.deltaTime;
        _lastPosition = transform.localPosition;
        _wallSpeed = Vector3.Distance(_wall.localPosition, _lastWallPosition) / Time.deltaTime;
        _lastWallPosition = _wall.localPosition;
        
        // Get the continuous actions
        float moveForward = actions.ContinuousActions[0];
        float rotate = actions.ContinuousActions[1];
        
        //Move the agent using the action
        MoveAgent(moveForward, rotate);
        
        //Penalty given each step to encourage the agent to reach the goal faster
        AddReward(-4f / MaxStep);
        
        // Reward for passing the middle wall
        if (!_hasPassedWall && transform.localPosition.x > 1f)
        {
            AddReward(0.5f);  // Tune this value
            _hasPassedWall = true;
        }
        
        if (_turtleSpeed < 0.2f)  // Threshold: tune based on your movement scale
        {
            AddReward(-0.05f);  // Light penalty for being too slow
        }
        
        //Update the cumulative reward after adding the step penalty
        _cumulativeReward = GetCumulativeReward();
    }
    
    public override void Heuristic(in ActionBuffers actionsOut) 
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Vertical");
        continuousActions[1] = Input.GetAxisRaw("Horizontal");
    }

    public void MoveAgent(float moveForward, float rotate)
    {
        // Move forward
        transform.localPosition += transform.forward * moveForward * _moveSpeed * Time.deltaTime;

        // Rotate
        transform.Rotate(0f, rotate * _rotationSpeed * Time.deltaTime, 0f);
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
        AddReward(10f); //Reward the agent for reaching the goal
        _cumulativeReward = GetCumulativeReward();
        _hasTouchedGoal = true;
        
        EndEpisode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            //Apply a small penalty for colliding with a wall
            AddReward(-5f);
            // //Change the color of Turtle to red
            // if (floorMeshRenderer != null)
            // {
            //     _previousColor = floorMeshRenderer.material.color;
            //     floorMeshRenderer.material.color = Color.cyan;
            // }
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Middle Wall"))
        {
            AddReward(-5f);
            EndEpisode();
        }
    }
    //
    // private void OnCollisionStay(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag("Wall"))
    //     {
    //         //Continuously apply a small penalty while in contact with a wall
    //         AddReward(-0.2f * Time.fixedDeltaTime);
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
    //             floorMeshRenderer.material.color = _previousColor;
    //         }
    //     }
    // }
}
