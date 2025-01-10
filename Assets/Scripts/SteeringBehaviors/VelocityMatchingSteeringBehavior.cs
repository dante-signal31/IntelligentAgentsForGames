using System.Diagnostics;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Debug = UnityEngine.Debug;

/// <summary>
/// <p>Monobehaviour to offer a velocity match steering behaviour.</p>
/// <p> Velocity matching steering behaviour makes the agent get the same velocity than
/// a target AgentMover. </p>
/// </summary>
public class VelocityMatchingSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to match its velocity.")]
    [SerializeField] private AgentMover target;
    [Tooltip("Time to match velocity.")] 
    [SerializeField] private float timeToMatch; 
    
    private Vector2 _targetVelocity;
    private Vector2 _currentVelocity;
    private Vector2 _currentAcceleration;

    /// <summary>
    /// Target to match its velocity.
    /// </summary>
    public AgentMover Target
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
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (Target == null) return new SteeringOutput(Vector2.zero, 0);
        

        _targetVelocity = Target.Velocity;
        _currentVelocity = args.CurrentVelocity;
        float stopSpeed = args.StopSpeed;
        float deltaTime = args.DeltaTime;
        float maximumAcceleration = args.MaximumAcceleration;
        float maximumDeceleration = args.MaximumDeceleration;
        
        Vector2 neededAcceleration = (_targetVelocity - _currentVelocity) / TimeToMatch;
    
        // if braking, then target velocity is zero or the acceleration vector is
        // opposed to current velocity direction.
        bool braking = _targetVelocity == Vector2.zero || 
                       Vector2.Dot(neededAcceleration, _currentVelocity) < 0;
    
        // Make sure velocity change is not greater than its maximum values.
        if (!braking && neededAcceleration.magnitude > maximumAcceleration)
        {
            neededAcceleration = neededAcceleration.normalized * maximumAcceleration;
        }
        else if (braking && _currentVelocity.magnitude <= stopSpeed)
        {
            return new SteeringOutput(Vector2.zero, 0);
        }
        else if (braking && neededAcceleration.magnitude > maximumDeceleration)
        {
            neededAcceleration = neededAcceleration.normalized * maximumDeceleration;
        }
    
        _currentAcceleration = neededAcceleration;
        
        
        Vector2 newVelocity = _currentVelocity + _currentAcceleration * deltaTime;

        return new SteeringOutput(newVelocity, 0);
    }
}