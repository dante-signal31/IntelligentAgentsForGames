using Pathfinding;
using PropertyAttribute;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// Represents a steering behavior that allows an agent to follow a predefined path
/// while steering towards each successive target along the path.
/// </summary>
public class PathFollowingSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [SerializeField] private Path followPath;
    [SerializeField] public float arrivalDistance;

    [Header("WIRING:")]
    [Tooltip("Steering to actually move the agent.")]
    [InterfaceCompliant(typeof(ITargeter))]
    [SerializeField] private SteeringBehavior steeringBehavior;
    
    private bool _pathStarted;
    private ITargeter _targeter;
    private GameObject _target;
    
    public Path FollowPath
    {
        get => followPath;
        set
        {
            followPath = value;
            _pathStarted = false;
        }
    }

    private void OnPathUpdated()
    {
        // If a new path has been generated, we must enter that path in a natural way. We
        // could start the path from the beginning. However, we have no guarantee that we
        // have not actually advanced the new path, and starting from the beginning would
        // actually mean going backwards to return back later to our former position. No,
        // it's better to query the new path where is the best point to start following
        // it. One naive approach would be asking for the path position nearest to our
        // current position. However, that could make us going backwards (for instance)
        // if we have just abandoned a path position and the path change is only some
        // new positions at the path end. I think it's better to enter the new path by
        // the path position nearest to our current target (the target position of our
        // current path). This way you can avoid going backwards.
        (Vector2 newTargetPosition, uint pathIndex) = 
            FollowPath.GetNearestPosition(FollowPath.CurrentTargetPosition);
        _target.transform.position = newTargetPosition;
        FollowPath.CurrentTargetPositionIndex = (int) pathIndex;
    }
    
    private void Awake()
    {
        _targeter = (ITargeter) steeringBehavior;
        _target = new GameObject($"TargetMarker_{name}");
        // We will move the target to make the targeter behavior calculate the velocity
        // to get there.
        _targeter.Target = _target;
    }

    private void Start()
    {
        if (FollowPath == null) return;
        _target.transform.position = FollowPath.CurrentTargetPosition;
        FollowPath.pathUpdated.AddListener(OnPathUpdated);
    }

    private void OnDestroy()
    {
        Destroy(_target);
    }
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (FollowPath == null || FollowPath.PathLength == 0) 
            return SteeringOutput.Zero;
        
        if (!_pathStarted)
        {
            _target.transform.position = FollowPath.CurrentTargetPosition;
            _pathStarted = true;
        }
        
        float distanceToTarget = 
            Vector2.Distance(transform.position, FollowPath.CurrentTargetPosition);
        if (distanceToTarget < arrivalDistance)
        {
            _target.transform.position = FollowPath.GetNextPositionTarget();
        }
        
        return steeringBehavior.GetSteering(args);
    }
}
}

