using Pathfinding;
using PropertyAttribute;
using UnityEngine;

namespace SteeringBehaviors
{
public class PathFollowingSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [SerializeField] private Path followPath;
    // TODO: I'm repeating this param in the underlying steering behavior. I must refactor this into ITargeter interface.
    [SerializeField] private float arrivalDistance;

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

