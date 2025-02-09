using UnityEngine;

namespace SteeringBehaviors
{
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
    private bool _isBraking;

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
    
        _currentVelocity = args.CurrentVelocity;
        float stopSpeed = args.StopSpeed;
        float deltaTime = args.DeltaTime;
        float maximumAcceleration = args.MaximumAcceleration;
        float maximumDeceleration = args.MaximumDeceleration;

        if (_targetVelocity != Target.Velocity)
        {
            _targetVelocity = Target.Velocity;
        
            // Millington recalculates neededAcceleration in every frame, but I
            // think that is an error. Doing that way apparently works but, actually,
            // target velocity is never entirely reached because current gap between
            // target and current velocity is always divided by timeToMatch.
            // So, my version calculates neededAcceleration only when target velocity
            // has changed. This way target velocity is accurately reached, although I
            // have to check for border cases where a minimum amount of acceleration can
            // make us overpass target velocity.
            Vector2 neededAcceleration = (_targetVelocity - _currentVelocity) / 
                                         TimeToMatch;
        
            // If braking, then target velocity is zero or the acceleration vector is
            // opposed to current velocity direction.
            _isBraking = _targetVelocity == Vector2.zero || 
                         Mathf.Approximately(
                             Vector2.Dot(
                                 neededAcceleration.normalized, 
                                 _currentVelocity.normalized), -1);

            // Make sure velocity change is not greater than its maximum values.
            if (!_isBraking && neededAcceleration.magnitude > maximumAcceleration)
            {
                neededAcceleration = neededAcceleration.normalized * maximumAcceleration;
            }
            else if (_isBraking && _currentVelocity.magnitude <= stopSpeed)
            {
                return new SteeringOutput(Vector2.zero, 0);
            }
            else if (_isBraking && neededAcceleration.magnitude > maximumDeceleration)
            {
                neededAcceleration = neededAcceleration.normalized * maximumDeceleration;
            }

            _currentAcceleration = neededAcceleration;
        }
    
        Vector2 frameAcceleration = _currentAcceleration * deltaTime;

        // Check for border cases, where just a minimum amount of acceleration can make 
        // us overpass target velocity.
        Vector2 newVelocity = new();
        // While brake, don't brake too much and invert direction.
        if (_isBraking && frameAcceleration.magnitude > _currentVelocity.magnitude)
        { 
            newVelocity = Vector2.zero;
        } 
        // While not accelerating, don't overpass target velocity
        else if (!_isBraking && 
                 frameAcceleration.magnitude > 
                 (_targetVelocity - _currentVelocity).magnitude)
        {
            newVelocity = _targetVelocity;
        }
        // Normal case.
        else
        {
            newVelocity = _currentVelocity + frameAcceleration;
        }
    
        return new SteeringOutput(newVelocity, 0);
    }
}
}