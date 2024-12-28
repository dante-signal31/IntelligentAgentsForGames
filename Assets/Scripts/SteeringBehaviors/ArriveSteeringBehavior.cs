using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// <p>Monobehaviour to offer an Arrive steering behaviour.</p>
/// 
/// <p> Arrive behavior is a Seek-like steering behaviour in which agent accelerates at
/// the startup and brakes gradually when approachs the end.</p>
/// </summary>
public class ArriveSteeringBehavior : SteeringBehavior, ITargeter
{
    [Header("CONFIGURATION:")]
    [Tooltip("Point to arrive to.")]
    [SerializeField] private GameObject target;
    [Tooltip("Radius to start slowing down using deceleration curve.")]
    [SerializeField] private float brakingRadius;
    [Tooltip("At this distance from target, agent will full stop.")]
    [SerializeField] private float arrivalDistance;
    [Tooltip("Deceleration curve.")] 
    [SerializeField] private AnimationCurve decelerationCurve;
    [Tooltip("At this distance from start, agent will be at full speed, finishing its " +
             "acceleration curve.")]
    [SerializeField] private float accelerationRadius;
    [Tooltip("Acceleration curve.")] 
    [SerializeField] private AnimationCurve accelerationCurve;

    /// <summary>
    /// Point this agent is going to.
    /// </summary>
    public GameObject Target
    {
        get => target; 
        set => target = value;
    }

    /// <summary>
    /// Radius to start slowing down using deceleration curve.
    /// </summary>
    public float BrakingRadius
    {
        get => brakingRadius;
        set => brakingRadius = value;
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
    /// At this distance from start, agent will be at full speed, finishing its
    /// acceleration curve.
    /// </summary>
    public float AccelerationRadius
    {
        get => accelerationRadius;
        set => accelerationRadius = value;
    }
    
    private Vector2 _startPosition;
    private float _distanceFromStart;
    private bool _idle = true;

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        Vector2 targetPosition = target.transform.position;
        Vector2 currentPosition = args.Position;
        Vector2 currentVelocity = args.CurrentVelocity;
        float stopSpeed = args.StopSpeed;
        float maximumSpeed = args.MaximumSpeed;
        
        
        Vector2 toTarget = targetPosition - currentPosition;
        float distanceToTarget = toTarget.magnitude;

        float newSpeed = 0.0f;
        
        if (_idle && _distanceFromStart > 0) _distanceFromStart = 0;
        
        if (distanceToTarget >= arrivalDistance && 
            _distanceFromStart < accelerationRadius)
        { // Acceleration phase.
            if (_idle)
            {
                _startPosition = currentPosition;
                _idle = false;
            }
            _distanceFromStart = (currentPosition - _startPosition).magnitude;
            // Acceleration curve should start at more than 0 or agent will not
            // start to move.
            newSpeed = maximumSpeed * accelerationCurve.Evaluate(
                Mathf.InverseLerp(0, accelerationRadius, _distanceFromStart));
        }
        else if (distanceToTarget < brakingRadius && distanceToTarget >= arrivalDistance)
        { // Deceleration phase.
            newSpeed = currentVelocity.magnitude > stopSpeed?
                maximumSpeed * decelerationCurve.Evaluate(
                    Mathf.InverseLerp(brakingRadius, 0, distanceToTarget)):
                0;
        }
        else if (distanceToTarget < arrivalDistance)
        { // Stop phase.
            newSpeed = 0;
            _idle = true;
        }
        else
        { // Cruise speed phase.
            newSpeed = maximumSpeed;
        }
        
        Vector2 newVelocity = toTarget.normalized * newSpeed;
        
        return new SteeringOutput(newVelocity, 0);
    }
}