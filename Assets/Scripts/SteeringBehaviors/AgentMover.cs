using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// This component moves its gameobject using the velocity vector calculated
/// by an steering behavior component.
/// </summary>
public class AgentMover : MonoBehaviour
{
    [Header("CONFIGURATION:")]
    [Tooltip("Movement will now surpass this maximum speed.")]
    [SerializeField] private float maximumSpeed;
    [Tooltip("Speed at which we consider agent should stop.")]
    [SerializeField] private float stopSpeed;
    [Tooltip("Movement rotation will not surpass this maximum rotational speed " +
             "(degress).")]
    [SerializeField] private float maximumRotationalSpeed;
    [Tooltip("Rotation will stop when the difference in degrees between the current " +
             "rotation and current forward vector is less than this value.")]
    [SerializeField] private float stopRotationThreshold;
    [Tooltip("Maximum acceleration for this agent.")]
    [SerializeField] private float maximumAcceleration;
    [Tooltip("Maximum deceleration for this agent.")]
    [SerializeField] private float maximumDeceleration;
    
    [Header("WIRING:")]
    [Tooltip("Steering behaviour component that will return movement vectors.")]
    public SteeringBehavior steeringBehavior;
    [Tooltip("This prefab's RigidBody to apply movement vectors over it.")]
    [SerializeField] private Rigidbody2D rigidBody;
    
    /// <summary>
    /// This agent current speed
    /// </summary>
    public float CurrentSpeed => rigidBody.linearVelocity.magnitude;
    
    /// <summary>
    /// This agent current velocity.
    /// </summary>
    public Vector2 Velocity => rigidBody.linearVelocity;
    
    /// <summary>
    /// This agent maximum speed.
    /// </summary>
    public float MaximumSpeed
    {
        get => maximumSpeed;
        set => maximumSpeed = value;
    }

    /// <summary>
    /// Speed at which we consider agent should stop.
    /// </summary>
    public float StopSpeed
    {
        get => stopSpeed;
        set => stopSpeed = value;
    }

    /// <summary>
    /// The agent maximum rotational speed in degrees.
    /// </summary>
    public float MaximumRotationalSpeed
    {
        get => maximumRotationalSpeed;
        set => maximumRotationalSpeed = value;
    }
    
    /// <summary>
    /// Rotation will stop when the difference in degrees between the current
    /// rotation and current forward vector is less than this value.
    /// </summary>
    public float StopRotationThreshold
    {
        get => stopRotationThreshold;
        set => stopRotationThreshold = value;
    }   
    
    /// <summary>
    /// Maximum acceleration for this agent.
    /// </summary>
    public float MaximumAcceleration
    {
        get => maximumAcceleration;
        set => maximumAcceleration = value;
    }

    /// <summary>
    /// Maximum deceleration for this agent.
    /// </summary>
    public float MaximumDeceleration
    {
        get => maximumDeceleration;
        set => maximumDeceleration = value;
    }
    
    /// <summary>
    /// This GameObject rotation is in degrees (using Z as rotation axis because
    /// this is a 2D game).
    /// </summary>
    public float Orientation => transform.rotation.eulerAngles.z;
    
    /// <summary>
    /// This agent forward vector.
    /// </summary>
    public Vector2 Forward
    {
        get => transform.up;
        set => transform.up = value;
    }

    private SteeringBehaviorArgs _behaviorArgs;
    private float _maximumRotationSpeedRadNormalized;
    private float _stopRotationRadThreshold;

    private SteeringBehaviorArgs GetSteeringBehaviorArgs()
    {
        return new SteeringBehaviorArgs(
            gameObject, 
            rigidBody.linearVelocity, 
            maximumSpeed, 
            stopSpeed,
            maximumRotationalSpeed,
            stopRotationThreshold,
            maximumAcceleration,
            maximumDeceleration,
            0);
    }

    private void Awake()
    {
        _behaviorArgs = GetSteeringBehaviorArgs();
        _maximumRotationSpeedRadNormalized = maximumRotationalSpeed * Mathf.Deg2Rad / 
                                             (2 * Mathf.PI);
        _stopRotationRadThreshold = stopRotationThreshold * Mathf.Deg2Rad;
    }

    private void FixedUpdate()
    {
        // Update steering behavior args.
        _behaviorArgs.MaximumSpeed = MaximumSpeed;
        _behaviorArgs.StopSpeed = StopSpeed;
        _behaviorArgs.CurrentVelocity = rigidBody.linearVelocity;
        _behaviorArgs.MaximumRotationalSpeed = MaximumRotationalSpeed;
        _behaviorArgs.StopRotationThreshold = StopRotationThreshold;
        _behaviorArgs.MaximumAcceleration = MaximumAcceleration;
        _behaviorArgs.MaximumDeceleration = MaximumDeceleration;
        _behaviorArgs.DeltaTime = Time.fixedDeltaTime;
        
        // Get steering output.
        SteeringOutput steeringOutput = steeringBehavior.GetSteering(_behaviorArgs);
        
        // Apply new steering output to our GameObject. I don't enforce the StopSpeed
        // because I've found more flexible to do it at steering behavior level.
        rigidBody.linearVelocity = steeringOutput.Linear;
        
        if (steeringOutput.Angular == 0 && rigidBody.linearVelocity != Vector2.zero)
        {
            // If no explicit angular steering, we will just look at the direction we
            // are moving, but clamping our rotation by our rotational speed.
            float totalRotationNeeded = Vector2.Angle(Forward, rigidBody.linearVelocity);
            if (Mathf.Abs(totalRotationNeeded) > stopRotationThreshold)
            {
                Vector3 newHeading = Vector3.Slerp(
                    Forward, 
                    rigidBody.linearVelocity, 
                    _maximumRotationSpeedRadNormalized * Time.fixedDeltaTime);
                Forward = newHeading.normalized * CurrentSpeed;
            }
        }
        else if (steeringOutput.Angular != 0)
        {
            // In this case, our steering wants us to face and move in different
            // directions. Steering checks that no threshold is surpassed.
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 
                transform.eulerAngles.y, 
                transform.eulerAngles.z + steeringOutput.Angular * Time.fixedDeltaTime);
        }
        
    }

    // /// <summary>
    // /// Get velocity vector updated with current steering and clamped by maximum speed.
    // /// </summary>
    // /// <returns>New velocity vector (older plus steering).</returns>
    // private Vector2 GetNewVelocity(SteeringOutput newSteeringOutput, float delta)
    // {
    //     Vector2 newVelocity = rigidBody.linearVelocity + newSteeringOutput.Linear * delta;
    //     newVelocity = ClampVector2(
    //         newVelocity, 
    //         0, 
    //         maximumSpeed);
    //     return newVelocity;
    // }

    /// <summary>
    /// <p>Clamp linear acceleration of an steering output.</p>
    /// <p><b>Angular acceleration</b> is not clamped yet.</p>
    /// </summary>
    /// <param name="steeringOutput">Steering output to clamp.</param>
    /// <returns>Steering output clamped.</returns>
    // private SteeringOutput ClampSteeringOutput(SteeringOutput steeringOutput)
    // {
    //     if (isAccelerationSteering(steeringOutput))
    //     {
    //         return new SteeringOutput(
    //             ClampVector2(
    //                 steeringOutput.Linear, 
    //                 0, 
    //                 maximumAcceleration),
    //             steeringOutput.Angular
    //         );
    //     }
    //     else
    //     {
    //         return new SteeringOutput(
    //             ClampVector2(
    //                 steeringOutput.Linear, 
    //                 0, 
    //                 maximumDeceleration),
    //             steeringOutput.Angular
    //         );
    //     }
    // }

    // /// <summary>
    // /// Whether this steering output is going to increase speed or not.
    // /// </summary>
    // /// <param name="steeringOutput">Current steering.</param>
    // /// <returns>True if this steering is going to accelerate the agent. False
    // /// otherwise.</returns>
    // private bool isAccelerationSteering(SteeringOutput steeringOutput)
    // {
    //     return Vector2.Dot(rigidBody.linearVelocity, steeringOutput.Linear) >= 0;
    // }
    
    /// <summary>
    /// Clamp vector magnitude between a minimum and a maximum length.
    /// </summary>
    /// <param name="vectorToClamp">Vector to check.</param>
    /// <param name="minimumMagnitude">Minimum length for the clamped vector.</param>
    /// <param name="maximumMagnitude">Maximin length for the clamped vector.</param>
    /// <returns>Vector clamped.</returns>
    // private Vector2 ClampVector2(
    //     Vector2 vectorToClamp, 
    //     float minimumMagnitude, 
    //     float maximumMagnitude)
    // {
    //     if (vectorToClamp.magnitude > maximumMagnitude)
    //     {
    //         vectorToClamp = vectorToClamp.normalized * maximumMagnitude;
    //     } 
    //     else if (vectorToClamp.magnitude < minimumMagnitude)
    //     {   
    //         vectorToClamp = Mathf.Approximately(minimumMagnitude, 0.0f)? 
    //             Vector2.zero:  
    //             vectorToClamp.normalized * minimumMagnitude;
    //     }
    //     return vectorToClamp;
    // }
    
    //
    // /// <summary>
    // /// Get rotation scalar updated with current steering and clamped by maximum rotational speed.
    // /// </summary>
    // /// <returns>New rotational scalar (older plus steering).</returns>
    // private float GetNewRotation(float delta)
    // {
    //     float newRotation = Mathf.Clamp(rigidBody.rotation + steeringBehavior.Angular * delta, 
    //         0, 
    //         maximumRotationalSpeed);
    //     return newRotation;
    // }
}
