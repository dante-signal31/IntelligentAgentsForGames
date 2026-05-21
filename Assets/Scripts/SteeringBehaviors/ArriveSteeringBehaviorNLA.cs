using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Script to offer an Arrive steering behavior.</p>
/// 
/// <p>Arrive behavior is a Seek-like steering behavior in which the agent speeds up at
/// the startup and brakes gradually when it approaches the end.</p>
/// <p> NLA behavior implements a Non-Linear-Acceleration approach. So, in this case
/// curves give acceleration instead of a fixed acceleration value.</p>
/// </summary>
public class ArriveSteeringBehaviorNla : SteeringBehavior, ITargeter
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
    /// Radius to start slowing down using the deceleration curve.
    /// </summary>
    public float BrakingRadius
    {
        get => brakingRadius;
        set => brakingRadius = value;
    }

    /// <summary>
    /// At this distance from the target, the agent will fully stop.
    /// </summary>
    public float ArrivalDistance
    {
        get => arrivalDistance;
        set => arrivalDistance = value;
    }

    /// <summary>
    /// At this distance from the start, the agent will be at full speed, finishing its
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
        if (Target == null) return SteeringOutput.zero;
    
        Vector2 targetPosition = Target.transform.position;
        Vector2 currentPosition = args.Position;
        Vector2 currentVelocity = args.CurrentVelocity;
        float stopSpeed = args.StopSpeed;
        float maximumSpeed = args.MaximumSpeed;
    
    
        Vector2 toTarget = targetPosition - currentPosition;
        float distanceToTarget = toTarget.magnitude;

        float newSpeed;
    
        if (_idle && _distanceFromStart > 0) _distanceFromStart = 0;
    
        if (distanceToTarget >= ArrivalDistance && 
            _distanceFromStart < AccelerationRadius)
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
        else if (distanceToTarget < BrakingRadius && distanceToTarget >= arrivalDistance)
        { // Deceleration phase.
            newSpeed = currentVelocity.magnitude > stopSpeed?
                maximumSpeed * decelerationCurve.Evaluate(
                    Mathf.InverseLerp(BrakingRadius, 0, distanceToTarget)):
                0;
        }
        else if (distanceToTarget < ArrivalDistance)
        { // Stop phase.
            newSpeed = 0;
            _idle = true;
        }
        else
        { // Cruise speed phase.
            newSpeed = maximumSpeed;
        }
    
        Vector2 newVelocity = toTarget.normalized * newSpeed;
    
        return new SteeringOutput(newVelocity);
    }
}
}