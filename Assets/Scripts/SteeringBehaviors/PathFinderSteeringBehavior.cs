using Pathfinding;
using PropertyAttribute;
using Tools;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Steering behavior to find and follow a path to a given target.</p>
/// <p>The pathfinder algorithm used depends on the IPathFinder instance referenced
/// from the pathFinderBehaviour field.</p> 
/// </summary>
public class PathFinderSteeringBehavior: SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to go to using found path.")]
    public TargetPlacement target;
    [Tooltip("Graph to use for path finding.")]
    public MapGraph graph;
    
    [Header("WIRING:")]
    [Tooltip("Steering Behavior to move using found path.")]
    [SerializeField] private PathFollowingSteeringBehavior pathFollowingSteeringBehavior;
    [Tooltip("Path finder to use. Must comply with IPathFinder interface.")]
    [InterfaceCompliant(typeof(IPathFinder))]
    [SerializeField] private MonoBehaviour pathFinderBehaviour;
    
    private IPathFinder _pathFinder;
    private Path _currentPath;

    private void Awake()
    {
        _pathFinder = (IPathFinder) pathFinderBehaviour;
        _currentPath = new GameObject("CurrentPath").AddComponent<Path>();
    }

    private void Start()
    {
        _pathFinder.Graph = graph;
        target.positionChanged.AddListener(OnPathTargetPositionChanged);
    }

    private void OnPathTargetPositionChanged(Vector2 newTargetPosition)
    {
        PathData newPath = _pathFinder.FindPath(newTargetPosition);
        if (newPath == null) return;
        _currentPath.UpdatePathData(newPath);
        pathFollowingSteeringBehavior.FollowPath = _currentPath;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        return pathFollowingSteeringBehavior.GetSteering(args);
    }
}
}