using UnityEngine;

/// <summary>
/// <p>Monobehaviour to offer a Seek steering behaviour.</p>
/// 
/// <p>Seek steering behaviour makes the agent move towards a target position.</p>
/// </summary>
[RequireComponent(typeof(AgentMover))]
public class SeekSteeringBehavior : SteeringBehavior, ITargeter
{
    [Header("CONFIGURATION:")]
    [Tooltip("Point this agent is going to.")]
    [SerializeField] private GameObject target;
    [Tooltip("Distance at which we give our goal as reached and we stop our agent.")]
    [SerializeField] private float arrivalDistance = .1f;

    /// <summary>
    /// Point this agent is going to.
    /// </summary>
    public GameObject Target
    {
        get=> target; 
        set=> target = value;
    }
    
    /// <summary>
    /// Distance at which we give our goal as reached and we stop our agent.
    /// </summary>
    public float ArrivalDistance
    {
        get=> arrivalDistance; 
        set=> arrivalDistance = value;
    }
    
    // private GameObject _currentThreath;
    // private Vector2 _threathPosition;
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (target == null) return new SteeringOutput(Vector2.zero, 0);
        
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
