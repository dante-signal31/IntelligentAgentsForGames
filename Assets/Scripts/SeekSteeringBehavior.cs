using UnityEngine;

/// <summary>
/// Monobehaviour to offer a Seek steering behaviour.
/// </summary>
public class SeekSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")]
    public GameObject target;
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        Vector2 targetPosition = target.transform.position;
        Vector2 currentPosition = args.CurrentAgent.transform.position;
        Vector2 currentVelocity = args.CurrentVelocity;
        float maximumSpeed = args.MaximumSpeed;

        Vector2 newVelocity = (targetPosition - currentPosition).normalized * maximumSpeed;
        
        return new SteeringOutput(newVelocity, 0);
    }
}
