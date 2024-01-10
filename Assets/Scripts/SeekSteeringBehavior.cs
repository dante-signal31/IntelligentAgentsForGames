using UnityEngine;

/// <summary>
/// Monobehaviour to offer a Seek steering behaviour.
/// </summary>
public class SeekSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")]
    public GameObject target;
    [Tooltip("Distance at which we give our goal as reached and we stop our agent.")]
    [SerializeField] private float arrivalDistance;
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        Vector2 targetPosition = target.transform.position;
        Vector2 currentPosition = args.CurrentAgent.transform.position;
        float maximumSpeed = args.MaximumSpeed;

        Vector2 toTarget = targetPosition - currentPosition;
        
        Vector2 newVelocity = toTarget.magnitude > arrivalDistance? 
            toTarget.normalized * maximumSpeed:
            Vector2.zero;
        
        return new SteeringOutput(newVelocity, 0);
    }
}
