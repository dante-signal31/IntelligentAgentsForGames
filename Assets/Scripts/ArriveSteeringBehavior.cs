using UnityEngine;

/// <summary>
/// Monobehaviour to offer a Seek steering behaviour.
/// </summary>
public class ArriveSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("Point to arrive to.")]
    public GameObject target;
    [Tooltip("Radius to start to slow down.")]
    [SerializeField] private float brakingRadius;
    [Tooltip("At this distance from target agent will full stop.")]
    [SerializeField] private float arrivingRadius;
    [Tooltip("Deceleration curve.")] 
    [SerializeField] private AnimationCurve decelerationCurve = new AnimationCurve();
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        Vector2 targetPosition = target.transform.position;
        Vector2 currentPosition = args.Position;
        Vector2 currentVelocity = args.CurrentVelocity;
        float stopSpeed = args.StopSpeed;
        float maximumSpeed = args.MaximumSpeed;
        float maximumAcceleration = args.MaximumAcceleration;
        float maximumDeceleration = args.MaximumDeceleration;
        
        
        Vector2 toTarget = targetPosition - currentPosition;
        float distanceToTarget = toTarget.magnitude;

        float newSpeed = 0.0f;
        if (distanceToTarget < brakingRadius && distanceToTarget >= arrivingRadius)
        {
            newSpeed = currentVelocity.magnitude > stopSpeed?
                maximumSpeed * decelerationCurve.Evaluate(distanceToTarget / brakingRadius):
                0;
        }
        else if (distanceToTarget < arrivingRadius)
        {
            newSpeed = 0;
        }
        else
        {
            newSpeed = maximumSpeed;
        }
        
        Vector2 newVelocity = toTarget.normalized * newSpeed;
        
        return new SteeringOutput(newVelocity, 0);
    }
}