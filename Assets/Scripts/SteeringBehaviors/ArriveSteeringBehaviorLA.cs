using UnityEngine;

/// <summary>
/// <p>Monobehaviour to offer an Arrive steering behaviour.</p>
/// 
/// <p>Arrive behavior is a Seek-like steering behaviour in which agent accelerates at
/// the startup and brakes gradually when approachs the end.</p>
/// <p> LA behavior implements a Linear-Acceleration approach. So, 
/// acceleration is given by a fixed acceleration values. In this case, that values are
/// maximum acceleration and maximum deceleration values from agent.</p>
/// </summary>
public class ArriveSteeringBehaviorLA : SteeringBehavior, ITargeter
{
    [Header("CONFIGURATION:")]
    [Tooltip("Point to arrive to.")]
    [SerializeField] private GameObject target;
    [Tooltip("At this distance from target, agent will full stop.")]
    [SerializeField] private float arrivalDistance;

    /// <summary>
    /// Point this agent is going to.
    /// </summary>
    public GameObject Target
    {
        get => target; 
        set => target = value;
    }
    
    /// <summary>
    /// At this distance from target, agent will full stop.
    /// </summary>
    public float ArrivalDistance
    {
        get => arrivalDistance;
        set => arrivalDistance = value;
    }
    
    /// <summary>
    /// Radius to start slowing down using deceleration curve.
    /// </summary>
    public float BrakingRadius=>
        GetBrakingRadius(_currentSpeed, _currentMaximumDeceleration);
    
    // private Vector2 _startPosition;
    // private float _distanceFromStart;
    // private bool _idle = true;
    private float _currentSpeed;
    private float _currentMaximumDeceleration;

    private float GetBrakingRadius(float speed, float deceleration)
    {
        return Mathf.Pow(speed, 2) / (2 * deceleration);
    }
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        Vector2 targetPosition = target.transform.position;
        Vector2 currentPosition = args.Position;
        Vector2 currentVelocity = args.CurrentVelocity;
        _currentSpeed = currentVelocity.magnitude;
        float maximumSpeed = args.MaximumSpeed;
        float currentMaximumAcceleration = args.MaximumAcceleration;
        _currentMaximumDeceleration = args.MaximumDeceleration;
        float deltaTime = args.DeltaTime;
        
        Vector2 toTarget = targetPosition - currentPosition;
        float distanceToTarget = toTarget.magnitude;

        float newSpeed = 0.0f;
        
        if (distanceToTarget >= ArrivalDistance &&
            distanceToTarget > BrakingRadius &&
            _currentSpeed < maximumSpeed)
        { // Acceleration phase.
            newSpeed = Mathf.Min(
                maximumSpeed,
                _currentSpeed + currentMaximumAcceleration * deltaTime);
        } 
        else if (distanceToTarget >= ArrivalDistance &&
                 distanceToTarget > BrakingRadius &&
                 _currentSpeed >= maximumSpeed)
        { // Full speed phase.
            newSpeed = maximumSpeed;
        }
        else if (distanceToTarget <= BrakingRadius &&
                 distanceToTarget >= ArrivalDistance)
        { // Braking phase.
            newSpeed = Mathf.Max(
                0, 
                _currentSpeed - _currentMaximumDeceleration * deltaTime);
        }
        else if (distanceToTarget < ArrivalDistance)
        { // Full stop phase.
            newSpeed = 0;
        }
        
        Vector2 newVelocity = toTarget.normalized * newSpeed;
        
        return new SteeringOutput(newVelocity, 0);
    }
}

