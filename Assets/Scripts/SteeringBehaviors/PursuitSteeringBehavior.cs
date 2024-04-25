using UnityEngine;

/// <summary>
/// Monobehaviour to offer a Pursuit steering behaviour.
/// </summary>
[RequireComponent(typeof(SeekSteeringBehavior))]
public class PursuitSteeringBehavior : SteeringBehavior
{
    [Header("WIRING:")] 
    [SerializeField] private SeekSteeringBehavior seekSteeringBehaviour; 
    
    [Header("CONFIGURATION:")]
    [Tooltip("Agent to pursue to.")]
    [SerializeField] private GameObject targetAgent;
    [Tooltip("Distance at which we give our goal as reached and we stop our agent.")]
    [Min(0)]
    [SerializeField] private float arrivalDistance;
    [Tooltip("Degrees from forward vector inside which we consider an object is ahead.")]
    [Range(0, 90)]
    [SerializeField] private float aheadSemiConeDegrees;
    [Tooltip("Degrees from forward vector inside which we consider an object is going toward us.")]
    [Range(0, 90)]
    [SerializeField] private float comingToUsSemiConeDegrees;
    
    [Header("DEBUG:")]
    [Tooltip("Make visible position marker.")] 
    [SerializeField] private bool predictedPositionMarkerVisible = true;

    /// <summary>
    /// Agent pursued.
    /// </summary>
    public GameObject TargetAgent=> targetAgent;

    /// <summary>
    /// Distance at which we give our goal as reached and we stop our agent.
    /// </summary>
    public float ArrivalDistance
    {
        get => arrivalDistance;
        set
        {
            arrivalDistance = Mathf.Max(0, value);
            seekSteeringBehaviour.arrivalDistance = arrivalDistance;
        }
    }

    /// <summary>
    /// Radians from forward vector inside which we consider an object is ahead.
    /// </summary>
    public float AheadSemiConeDegrees
    {
        get => aheadSemiConeDegrees;
        set => aheadSemiConeDegrees = Mathf.Clamp(value, 0, 90);
    }

    /// <summary>
    /// Radians from forward vector inside which we consider an object is going toward us.
    /// </summary>
    public float ComingToUsSemiConeDegrees
    {
        get => comingToUsSemiConeDegrees;
        set => comingToUsSemiConeDegrees = Mathf.Clamp(value, 0, 90);
    }

    private Rigidbody2D _targetRigidBody;
    private Vector2 _targetPosition;
    private GameObject _currentTarget;

    private float _cosAheadSemiConeRadians;
    private float _cosComingToUsSemiConeRadians;
    private GameObject _predictedPositionMarker;
    private Color _agentColor;
    private Color _targetColor;

    private void Start()
    {
        _cosAheadSemiConeRadians = Mathf.Cos(aheadSemiConeDegrees * Mathf.Deg2Rad);
        _cosComingToUsSemiConeRadians = Mathf.Cos(comingToUsSemiConeDegrees * Mathf.Deg2Rad);
        seekSteeringBehaviour.arrivalDistance = arrivalDistance;
        _predictedPositionMarker = new GameObject();
        seekSteeringBehaviour.target = targetAgent;
        _agentColor = GetComponent<AgentColor>().Color;
        _targetColor = targetAgent.GetComponent<AgentColor>().Color;
    }

    private void OnDestroy()
    {
        Destroy(_predictedPositionMarker);
    }

    /// <summary>
    /// Load target data.
    /// </summary>
    private void UpdateTargetData()
    {
        if (targetAgent != _currentTarget)
        {
            _targetRigidBody = targetAgent.GetComponentInChildren<Rigidbody2D>();
            _currentTarget = targetAgent;
        }
        _targetPosition = targetAgent.transform.position;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        UpdateTargetData();
        Vector2 currentPosition = args.Position;
        float maximumSpeed = args.MaximumSpeed;

        Vector2 toTarget = _targetPosition - currentPosition;
        
        if (TargetIsComingToUs(args))
        { // Target ahead so just go straight to it.
            _predictedPositionMarker.transform.position = targetAgent.transform.position;
            seekSteeringBehaviour.target = _predictedPositionMarker;
            return seekSteeringBehaviour.GetSteering(args);
        }
        else
        {   // Target is not ahead so we must predict where it will be.
            // The look-ahead time is proportional to the distance between the evader
            // and the pursuer; and is inversely proportional to the sum of the
            // agents' velocities
            float lookAheadTime = toTarget.magnitude / (maximumSpeed + _targetRigidBody.velocity.magnitude);
            _predictedPositionMarker.transform.position = _targetPosition + _targetRigidBody.velocity * lookAheadTime;
            seekSteeringBehaviour.target = _predictedPositionMarker;
            return seekSteeringBehaviour.GetSteering(args);
        }
    }

    private bool TargetIsComingToUs(SteeringBehaviorArgs args)
    {
        Vector2 currentPosition = args.Position;
        Vector2 currentVelocity = args.CurrentVelocity;
        
        Vector2 toTarget = _targetPosition - currentPosition;
        bool targetInFrontOfUs = Vector2.Dot(currentVelocity.normalized, toTarget.normalized) > _cosAheadSemiConeRadians;
        bool targetComingToUs = Vector2.Dot(currentVelocity.normalized, _targetRigidBody.velocity.normalized) < (-1 * _cosComingToUsSemiConeRadians);
        
        return targetInFrontOfUs && targetComingToUs;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (predictedPositionMarkerVisible && _predictedPositionMarker != null)
        {
            Gizmos.color = _agentColor;
            Gizmos.DrawLine(transform.position, _predictedPositionMarker.transform.position);
            Gizmos.DrawWireSphere(_predictedPositionMarker.transform.position, 0.3f);
            Gizmos.color = _targetColor;
            Gizmos.DrawLine(targetAgent.transform.position, _predictedPositionMarker.transform.position);
        }
    }
#endif
}