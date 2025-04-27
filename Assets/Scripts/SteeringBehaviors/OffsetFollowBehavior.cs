using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Component to offer an offset Follow steering behaviour.</p>
/// <p>Represents a steering behavior where an agent follows a target with an offset.
/// The agent anticipates the target's position based on its current position, velocity,
/// and a look-ahead time.</p>
/// </summary>
[RequireComponent(typeof(ArriveSteeringBehaviorNLA), typeof(AgentMover))]
public class OffsetFollowBehavior: SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to follow")]
    [SerializeField] public AgentMover target;
    // TODO: Create a handle to set this field visually.
    [SerializeField] public Vector2 offsetFromTarget;

    [Header("DEBUG:")] 
    [SerializeField] private bool showGizmos;
    
    private ArriveSteeringBehaviorNLA _followSteeringBehavior;
    private GameObject _offsetFromTargetMarker;
    private AgentMover _currentAgent;
    
    private Color AgentColor => _currentAgent.GetComponent<AgentColor>().Color;

    private void Awake()
    {
        _currentAgent = GetComponent<AgentMover>();
        _followSteeringBehavior = GetComponent<ArriveSteeringBehaviorNLA>();
        _offsetFromTargetMarker = new GameObject("OffsetFromTargetMarker");
        _offsetFromTargetMarker.transform.parent = transform;
    }

    private void Start()
    {
        _followSteeringBehavior.Target = _offsetFromTargetMarker;
        UpdateOffsetFromTarget();
    }

    /// <summary>
    /// <p>Updates the offset position from the target in the local coordinate space.</p>
    /// </summary>
    private void UpdateOffsetFromTarget()
    {
        _offsetFromTargetMarker.transform.position = target.transform.TransformPoint(
            offsetFromTarget);
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        // The look-ahead time is proportional to the distance between the target and
        // the followed; and is inversely proportional to the sum of the agent's
        // velocities.
        float lookAheadTime = offsetFromTarget.magnitude / 
                              (args.MaximumSpeed + target.CurrentSpeed);
        
        // Place the marker where we think the target will be at the look-ahead
        // time.
        _offsetFromTargetMarker.transform.position = 
            target.transform.TransformPoint(offsetFromTarget) + 
            (Vector3)target.Velocity * lookAheadTime;
        
        // Let the child steering behavior get to the new marker position.
        return _followSteeringBehavior.GetSteering(args);
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (target == null || _offsetFromTargetMarker == null) return;
    
        Gizmos.color = AgentColor;
        Gizmos.DrawLine(transform.position, _offsetFromTargetMarker.transform.position);
        Gizmos.DrawWireSphere(_offsetFromTargetMarker.transform.position, 0.3f);
        Gizmos.DrawLine(
            target.transform.position, 
            _offsetFromTargetMarker.transform.position);
    }
#endif
}
}