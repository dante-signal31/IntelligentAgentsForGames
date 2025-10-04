using Tools;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace SteeringBehaviors
{
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
    [SerializeField] protected float maximumRotationalSpeed;
    [Tooltip("Rotation will stop when the difference in degrees between the current " +
             "rotation and current forward vector is less than this value.")]
    [SerializeField] protected float stopRotationThreshold;
    [Tooltip("Maximum acceleration for this agent.")]
    [SerializeField] private float maximumAcceleration;
    [Tooltip("Maximum deceleration for this agent.")]
    [SerializeField] private float maximumDeceleration;
    [Tooltip("Smooth heading averaging velocity vector.")] 
    [SerializeField] private bool autoSmooth;
    [Tooltip("How many samples to use to smooth heading.")]
    [SerializeField] private int autoSmoothSamples = 10;
    
    [Header("WIRING:")]
    [Tooltip("Steering behaviour component that will return movement vectors.")]
    [SerializeField] protected SteeringBehavior steeringBehavior;
    [Tooltip("This prefab's RigidBody to apply movement vectors over it.")]
    [SerializeField] protected Rigidbody2D rigidBody;

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
    /// rotation and the current forward vector is less than this value.
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
    /// Smooth heading averaging velocity vector.
    /// </summary>
    public bool AutoSmooth
    {
        get => autoSmooth;
        set => autoSmooth = value;   
    }

    /// <summary>
    /// How many samples to use to smooth heading.
    /// </summary>
    public int AutoSmoothSamples
    {
        get => autoSmoothSamples;
        set
        {
            if (value == autoSmoothSamples) return;
            autoSmoothSamples = value;  
            lastRotations = new MovingWindow(value);
        } 
    }

    /// <summary>
    /// Steering behaviour component that will return movement vectors.
    /// </summary>
    public SteeringBehavior SteeringBehavior
    {
        get => steeringBehavior;
        set => steeringBehavior = value;
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

    public LayerMask CollisionLayer => gameObject.layer;

    /// <summary>
    /// Disable this agent's autonomous movement.
    /// <remarks>Only useful for formations.</remarks>   
    /// </summary>
    public bool AutonomousMovementDisabled { get; set; }

    protected SteeringBehaviorArgs behaviorArgs;
    private float maximumRotationSpeedRadNormalized;
    private MovingWindow lastRotations;

    protected virtual SteeringBehaviorArgs GetSteeringBehaviorArgs()
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

    protected virtual void Start()
    {
        if (rigidBody == null) return;
        
        behaviorArgs = GetSteeringBehaviorArgs();
        maximumRotationSpeedRadNormalized = maximumRotationalSpeed * Mathf.Deg2Rad / 
                                             (2 * Mathf.PI);
        lastRotations = new MovingWindow(autoSmoothSamples);
    }

    protected virtual void FixedUpdate()
    {
        if (AutonomousMovementDisabled || behaviorArgs == null) return;
        
        UpdateSteeringBehaviorArgs(Time.fixedDeltaTime);

        // Get steering output.
        SteeringOutput steeringOutput = SteeringBehavior.GetSteering(behaviorArgs);
        
        // Apply the necessary rotation.
        if (steeringOutput.Angular == 0 && steeringOutput.Linear == Vector2.zero)
        {
            // Sometimes, when an agent touches a surface, a residual angular velocity
            // is left behind. This is a problem because when the agent stops, it starts
            // to spin. The solution I found is to set angular velocity to zero.
            rigidBody.angularVelocity = 0;
        }
        else if (steeringOutput.Angular == 0 && steeringOutput.Linear != Vector2.zero)
        {
            if (autoSmooth)
            {
                // If no explicit angular steering, but autoSmoothing is desired, we will
                // smooth the heading by averaging the last few rotations.
                float rotationNeeded = Vector2.SignedAngle(
                    Forward, 
                    steeringOutput.Linear);
                lastRotations.Add(rotationNeeded);
                float averageRotation = lastRotations.Average;
                Vector2 averageHeading = Quaternion.Euler(0, 0, averageRotation) * 
                                         Forward;
                SetRotation(averageHeading);
            }
            else
            {
                // If no explicit angular steering and no autoSmoothing desired, we will
                // just look at the direction we are moving, but clamping our rotation by
                // our rotational speed.
                SetRotation(steeringOutput.Linear);
            }
        }
        else if (steeringOutput.Angular != 0)
        {
            // In this case, our steering wants us to face and move in different
            // directions. Steering checks that no threshold is surpassed.
            rigidBody.angularVelocity = steeringOutput.Angular;
        }
        
        // Apply the new velocity vector to our GameObject. I don't enforce the StopSpeed
        // because I've found it more flexible to do it at steering behavior level.
        rigidBody.linearVelocity = steeringOutput.Linear;
    }

    protected virtual void UpdateSteeringBehaviorArgs(float deltaTime = 0)
    {
        behaviorArgs.MaximumSpeed = MaximumSpeed;
        behaviorArgs.StopSpeed = StopSpeed;
        behaviorArgs.CurrentVelocity = rigidBody.linearVelocity;
        behaviorArgs.MaximumRotationalSpeed = MaximumRotationalSpeed;
        behaviorArgs.StopRotationThreshold = StopRotationThreshold;
        behaviorArgs.MaximumAcceleration = MaximumAcceleration;
        behaviorArgs.MaximumDeceleration = MaximumDeceleration;
        behaviorArgs.DeltaTime = deltaTime;
    }

    /// <summary>
    /// Rotates the agent towards the given heading vector within the constraints of the
    /// agent's maximum rotational speed and stop rotation threshold.
    /// </summary>
    /// <param name="heading">
    /// The direction the agent should face, represented as a 2D vector.
    /// </param>
    private void SetRotation(Vector2 heading)
    {
        float totalRotationNeeded = Vector2.Angle(Forward, heading);
        if (totalRotationNeeded > stopRotationThreshold)
        {
            Vector3 newHeading = Vector3.Slerp(
                Forward, 
                heading, 
                maximumRotationSpeedRadNormalized * Time.fixedDeltaTime);
            rigidBody.MoveRotation(
                rigidBody.rotation + 
                Vector2.SignedAngle(Forward, newHeading));
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-assign to properties to run initialization code.
        MaximumSpeed = maximumSpeed;
        StopSpeed = stopSpeed;
        MaximumRotationalSpeed = maximumRotationalSpeed;
        StopRotationThreshold = stopRotationThreshold;
        MaximumAcceleration = maximumAcceleration;
        MaximumDeceleration = maximumDeceleration;
        AutoSmooth = autoSmooth;
        AutoSmoothSamples = autoSmoothSamples;
    }
#endif

}
}
