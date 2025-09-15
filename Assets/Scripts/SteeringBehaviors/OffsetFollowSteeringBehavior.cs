using PropertyAttribute;
using UnityEngine;
using UnityEngine.Serialization;

namespace SteeringBehaviors
{
    /// <summary>
    /// <p>Component to offer an offset Follow steering behaviour.</p>
    /// <p>Represents a steering behavior where an agent follows a target with an offset.
    /// The agent anticipates the target's position based on its current position,
    /// velocity, and a look-ahead time.</p>
    /// </summary>
    public class OffsetFollowSteeringBehavior : SteeringBehavior, ITargeter, IGizmos

    {
    [Header("CONFIGURATION:")] 
    [Tooltip("Target to follow")] 
    [SerializeField] public GameObject target;
    [Tooltip("Location, relative to target, to follow.")]
    [SerializeField] public Vector2 offsetFromTarget;

    [Header("WIRING:")]
    [Tooltip("Steering behavior to actually move this agent. Must comply with " +
             "ITargeter interface.")]
    //[InterfaceCompliant(typeof(ITargeter))]
    [SerializeField] private SteeringBehavior followSteeringBehavior;

    [Header("DEBUG:")] 
    [SerializeField] private bool showGizmos;
    [SerializeField] private Color gizmosColor = Color.red;

    /// <summary>
    /// Target to follow.
    /// </summary>
    public GameObject Target
    {
        get => target;
        set
        {
            target = value;
            UpdateOffsetFromTarget();
        }
    }
    
    public bool ShowGizmos
    {
        get => showGizmos;
        set => showGizmos = value;
    }
    
    public Color GizmosColor
    {
        get => gizmosColor;
        set => gizmosColor = value;
    }

    private GameObject _offsetFromTargetMarker;
    private AgentMover _currentAgent;
    private ITargeter _followTargeter;

    private Color AgentColor => _currentAgent.GetComponent<AgentColor>().Color;

    private void Awake()
    {
        _currentAgent = GetComponentInParent<AgentMover>();
        _offsetFromTargetMarker = new GameObject("OffsetFromTargetMarker");
        _offsetFromTargetMarker.transform.parent = transform;
        _followTargeter = (ITargeter) followSteeringBehavior;
    }

    private void Start()
    {
        _followTargeter.Target = _offsetFromTargetMarker;
        UpdateOffsetFromTarget();
    }

    /// <summary>
    /// <p>Updates the offset position from the target in the local coordinate space.</p>
    /// </summary>
    public void UpdateOffsetFromTarget()
    {
        if (target == null) return;
        _offsetFromTargetMarker.transform.position = target.transform.TransformPoint(
            offsetFromTarget);
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        // Buckland uses a look-ahead algorithm to place marker. In my tests I didn't
        // like it because in movement the follower approached nearer than the offset,
        // and when the target stopped, the follower retreated in an odd way. So I
        // discarded the look-ahead algorithm. Nevertheless, I let it commented if you
        // want to assess it.
        //
        // // The look-ahead time is proportional to the distance between the target and
        // // the followed; and is inversely proportional to the sum of the agent's
        // // velocities.
        // float lookAheadTime = offsetFromTarget.magnitude / 
        //                       (args.MaximumSpeed + target.CurrentSpeed);
        // // Place the marker where we think the target will be at the look-ahead
        // // time.
        // _offsetFromTargetMarker.transform.position = 
        //     target.transform.TransformPoint(offsetFromTarget) + 
        //     (Vector3)target.Velocity * lookAheadTime;

        UpdateOffsetFromTarget();

        // Let the child steering behavior get to the new marker position.
        return followSteeringBehavior.GetSteering(args);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (target == null || _offsetFromTargetMarker == null) return;

        Gizmos.color = GizmosColor;
        Gizmos.DrawLine(transform.position, _offsetFromTargetMarker.transform.position);
        Gizmos.DrawWireSphere(_offsetFromTargetMarker.transform.position, 0.3f);
        Gizmos.color = new Color(GizmosColor.r, GizmosColor.g, GizmosColor.b, 0.5f);
        Gizmos.DrawLine(
            target.transform.position,
            _offsetFromTargetMarker.transform.position);
    }
#endif
    }
}