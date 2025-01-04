using UnityEngine;

/// <summary>
/// Monobehaviour to offer a velocity match steering behaviour.
/// </summary>
public class VelocityMatchingSteeringBehavior : SteeringBehavior, ITargeter
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to match its velocity.")]
    [SerializeField] private GameObject target;
    [Tooltip("Time to match velocity.")] 
    [SerializeField] private float timeToMatch; 

    private GameObject _currentTarget;
    private Rigidbody2D _targetRigidBody;
    private Vector2 _targetVelocity;
    private Vector2 _currentVelocity;
    private Vector2 _currentAcceleration;
    private bool _currentAccelerationUpdateIsNeeded;

    /// <summary>
    /// Target to match its velocity.
    /// </summary>
    public GameObject Target
    {
        get => target;
        set => target = value;
    }
    
    /// <summary>
    /// Time to match velocity.
    /// </summary>
    public float TimeToMatch
    {
        get => timeToMatch;
        set => timeToMatch = value;
    }

    private void Awake()
    {
        // TODO: Don't need a reference to target RigidBody2D. You need target AgentMover offers a Velocity property.
        if (Target != null) 
            _targetRigidBody = Target.GetComponentInChildren<Rigidbody2D>();
    }

    /// <summary>
    /// Load target data.
    /// </summary>
    private void UpdateTargetData()
    {
        if (_currentTarget != Target)
        {
            _targetRigidBody = Target.GetComponentInChildren<Rigidbody2D>();
            _currentTarget = Target;
        }

        // Acceleration should change in two cases:
        // 1. Target velocity has changed.
        // 2. Target velocity and current velocity are the same. So we should
        //    stop accelerating.
        if ((_targetVelocity != _targetRigidBody.linearVelocity) ||
            Mathf.Approximately(_targetVelocity.magnitude, _currentVelocity.magnitude))
        {
            _targetVelocity = _targetRigidBody.linearVelocity;
            _currentAccelerationUpdateIsNeeded = true;
        }
    }
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (Target == null) return new SteeringOutput(Vector2.zero, 0);

        UpdateTargetData();
        _currentVelocity = args.CurrentVelocity;
        float deltaTime = args.DeltaTime;

        if (_currentAccelerationUpdateIsNeeded)
        {
            // Millington executes this code section in every frame, but I
            // think that is an error. Doing that way targets velocity is never
            // reached because current gap between target and current velocity
            // is always divided by timeToMatch. So, my version is executing this
            // code section only when acceleration needs to really update
            // (target has changed its velocity or target velocity has been
            // reached and we no longer need an acceleration).
            float maximumAcceleration = args.MaximumAcceleration;
            Vector2 neededAcceleration = (_targetVelocity - _currentVelocity) / timeToMatch;
            if (neededAcceleration.magnitude > maximumAcceleration)
            {
                neededAcceleration = neededAcceleration.normalized * maximumAcceleration;
            }
            _currentAcceleration = neededAcceleration;
            _currentAccelerationUpdateIsNeeded = false;
        }
        
        Vector2 newVelocity = _currentVelocity + _currentAcceleration * deltaTime;
        
        return new SteeringOutput(newVelocity, 0);
    }
}