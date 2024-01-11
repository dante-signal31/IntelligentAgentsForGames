using UnityEngine;

/// <summary>
/// Base class passed as argument to Steering Behaviors Process() methods.
/// </summary>
public class SteeringBehaviorArgs
{
    /// <summary>
    /// Owner of this steering.
    /// </summary>
    public GameObject CurrentAgent { get; private set; }
    
    /// <summary>
    /// Maximum linear speed for this steering.
    /// </summary>
    public float MaximumSpeed { get; private set; }
    
    /// <summary>
    /// Minimum linear speed under which agent is considered stopped.
    /// </summary>
    public float StopSpeed { get; private set; }
    
    /// <summary>
    /// Current owner velocity vector.
    /// </summary>
    public Vector2 CurrentVelocity { get; set; }
    
    /// <summary>
    /// Maximum rotational speed for this steering.
    /// </summary>
    public float MaximumRotationalSpeed { get; private set; }
    
    /// <summary>
    /// Maximum acceleration for this steering.
    /// </summary>
    public float MaximumAcceleration { get; private set; }
    
    /// <summary>
    /// Maximum deceleration for this steering.
    /// </summary>
    public float MaximumDeceleration { get; private set; }
    
    /// <summary>
    /// Delta time since last steering behavior update.
    /// </summary>
    public float DeltaTime { get; set; }

    /// <summary>
    /// This GameObject position.
    /// </summary>
    public Vector2 Position => CurrentAgent.transform.position;

    /// <summary>
    /// This GameObject rotation (Z axis por a 2D game).
    /// </summary>
    public float Orientation => CurrentAgent.transform.rotation.eulerAngles.z;

    public SteeringBehaviorArgs(GameObject currentAgent, Vector2 currentVelocity,
        float maximumSpeed, float stopSpeed, float maximumRotationalSpeed, float maximumAcceleration,
        float maximumDeceleration, float deltaTime)
    {
        CurrentVelocity = currentVelocity;
        MaximumSpeed = maximumSpeed;
        StopSpeed = stopSpeed;
        MaximumRotationalSpeed = maximumRotationalSpeed;
        CurrentAgent = currentAgent;
        MaximumAcceleration = maximumAcceleration;
        MaximumDeceleration = maximumDeceleration;
        DeltaTime = deltaTime;
    }
}
