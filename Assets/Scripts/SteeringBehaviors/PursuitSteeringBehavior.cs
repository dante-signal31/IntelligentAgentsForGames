using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// <p>Monobehaviour to offer a Pursuit steering behaviour.</p>
/// <p>To pursue another agent if won't be enough to go to its current position. If that
/// agent displaces then we will only follow its trail. Instead, pursuer must predict
/// whare chased agent will be and aim to that position.</p>
/// </summary>
[RequireComponent(typeof(SeekSteeringBehavior))]
public class PursuitSteeringBehavior : SteeringBehavior
{
    [FormerlySerializedAs("targetAgent")]
    [Header("CONFIGURATION:")]
    [Tooltip("Agent to pursue to.")]
    [SerializeField] private AgentMover target;
    [Tooltip("Distance at which we give our goal as reached and we stop our agent.")]
    [Min(0)]
    [SerializeField] private float arrivalDistance;
    [Tooltip("Degrees from forward vector inside which we consider an object is ahead.")]
    [Range(0, 90)]
    [SerializeField] private float aheadSemiConeDegrees;
    [Tooltip("Degrees from forward vector inside which we consider an object is going " +
             "toward us.")]
    [Range(0, 90)]
    [SerializeField] private float comingToUsSemiConeDegrees;
    
    [Header("DEBUG:")]
    [Tooltip("Make visible position marker.")] 
    [SerializeField] private bool predictedPositionMarkerVisible = true;
    
    /// <summary>
    /// Agent pursued.
    /// </summary>
    public AgentMover Target
    {
        get => target;
        set => target = value;
    }

    /// <summary>
    /// Distance at which we give our goal as reached and we stop our agent.
    /// </summary>
    public float ArrivalDistance
    {
        get => arrivalDistance;
        set
        {
            arrivalDistance = Mathf.Max(0, value);
            _seekSteeringBehaviour.ArrivalDistance = arrivalDistance;
        }
    }

    /// <summary>
    /// Radians from forward vector inside which we consider an object is ahead.
    /// </summary>
    public float AheadSemiConeDegrees
    {
        get => aheadSemiConeDegrees;
        set
        {
            aheadSemiConeDegrees = Mathf.Clamp(value, 0, 90);
            _cosAheadSemiConeRadians = Mathf.Cos(aheadSemiConeDegrees * Mathf.Deg2Rad);
        }
    }

    /// <summary>
    /// Radians from forward vector inside which we consider an object is going toward us.
    /// </summary>
    public float ComingToUsSemiConeDegrees
    {
        get => comingToUsSemiConeDegrees;
        set
        {
            comingToUsSemiConeDegrees = Mathf.Clamp(value, 0, 90);
            _cosComingToUsSemiConeRadians = Mathf.Cos(
                comingToUsSemiConeDegrees * 
                Mathf.Deg2Rad);
        }
    }
    
    private SeekSteeringBehavior _seekSteeringBehaviour; 
    private float _cosAheadSemiConeRadians;
    private float _cosComingToUsSemiConeRadians;
    private GameObject _predictedPositionMarker;
    private Color _agentColor;
    private Color _targetColor;

    private void Awake()
    {
        // Most methods use radians as input, but most humans understand better degrees. 
        // So, we accept degrees to configure scripts but convert them to radians to work.
        _cosAheadSemiConeRadians = Mathf.Cos(aheadSemiConeDegrees * Mathf.Deg2Rad);
        _cosComingToUsSemiConeRadians = Mathf.Cos(
            comingToUsSemiConeDegrees * 
            Mathf.Deg2Rad);
        // Create an invisible object as marker to place it at target predicted future
        // position. That marker will be used by seek steering behaviour as target.
        _predictedPositionMarker = new GameObject();
        if (Target != null)
            _predictedPositionMarker.transform.position = Target.transform.position;
        _seekSteeringBehaviour = GetComponent<SeekSteeringBehavior>();
        _seekSteeringBehaviour.ArrivalDistance = arrivalDistance;
        _seekSteeringBehaviour.Target = _predictedPositionMarker;
        // Configure our gizmos.
        _agentColor = GetComponent<AgentColor>().Color;
        _targetColor = target.GetComponent<AgentColor>().Color;
    }

    private void OnDestroy()
    {
        Destroy(_predictedPositionMarker);
    }
    
    /// <summary>
    /// Whether target is coming to us.
    /// </summary>
    /// <param name="args">Our current data.</param>
    /// <returns>True if target has a velocity vector that is going toward us.</returns>
    private bool TargetIsComingToUs(SteeringBehaviorArgs args)
    {
        Vector2 currentPosition = args.Position;
        Vector2 currentDirection = args.CurrentVelocity.normalized;
        Vector2 targetPosition = Target.transform.position;
        Vector2 targetDirection = Target.Velocity.normalized;
        
        Vector2 toTarget = targetPosition - currentPosition;
        bool targetInFrontOfUs = Vector2.Dot(
            currentDirection, 
            toTarget.normalized) > _cosAheadSemiConeRadians;
        bool targetComingToUs = Vector2.Dot(
            currentDirection, 
            targetDirection) < (-1 * _cosComingToUsSemiConeRadians);
        
        return targetInFrontOfUs && targetComingToUs;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (Target == null) return new SteeringOutput(Vector2.zero, 0);
        
        Vector2 targetPosition = Target.transform.position;
        
        if (TargetIsComingToUs(args))
        {   // Target is coming to us so just go straight to it.
            _predictedPositionMarker.transform.position = targetPosition;
            return _seekSteeringBehaviour.GetSteering(args);
        }
        else
        {   // Target is not coming to us so we must predict where it will be.
            // The look-ahead time is proportional to the distance between the chased
            // and the pursuer and is inversely proportional to the sum of the
            // agents velocities.
            Vector2 currentPosition = args.Position;
            float currentSpeed = args.CurrentVelocity.magnitude;
            float targetSpeed = Target.Velocity.magnitude;
            Vector3 targetVelocity = Target.Velocity;
            float distanceToTarget = (targetPosition - currentPosition).magnitude;
            float lookAheadTime = distanceToTarget / (currentSpeed + targetSpeed);

            // Avoid divide-by-zero error when both agents are stationary.
            if (float.IsInfinity(lookAheadTime))
                return new SteeringOutput(Vector2.zero, 0);
            
            // Place the marker where we think the target will be at the look-ahead
            // time.
            _predictedPositionMarker.transform.position = (Vector3) targetPosition + 
                (targetVelocity * lookAheadTime);
            
            // Let the seek steering behavior get to the new marker position.
            _seekSteeringBehaviour.Target = _predictedPositionMarker;
            
            return _seekSteeringBehaviour.GetSteering(args);
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_predictedPositionMarker == null || !predictedPositionMarkerVisible) return;
        
        Gizmos.color = _agentColor;
        Gizmos.DrawLine(transform.position, _predictedPositionMarker.transform.position);
        Gizmos.DrawWireSphere(_predictedPositionMarker.transform.position, 0.3f);
        Gizmos.color = _targetColor;
        Gizmos.DrawLine(Target.transform.position, _predictedPositionMarker.transform.position);
    }
#endif
}