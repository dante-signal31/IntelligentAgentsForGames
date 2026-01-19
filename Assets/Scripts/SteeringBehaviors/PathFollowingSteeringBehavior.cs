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
    // TODO: I'm repeating this param in the underlying steering behavior. I must refactor this into ITargeter interface.
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
    
    // /// <summary>
    // /// Whether we have got the path end.
    // /// </summary>
    // public bool IsPathEnded => 
    //     Vector2.Distance(
    //         transform.position, 
    //         FollowPath.Data.positions[^1]) < arrivalDistance;

    private void Awake()
    {
        _targeter = (ITargeter) steeringBehavior;
        _target = new GameObject($"TargetMarker_{name}");
        _targeter.Target = _target;
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

