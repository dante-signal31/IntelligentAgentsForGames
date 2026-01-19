using Pathfinding;
using PropertyAttribute;
using Tools;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Steering behavior to find and follow a path to a given target.</p>
/// <p>The pathfinder algorithm used depends on the IGraphPathFinder instance referenced
/// from the pathFinderBehaviour field.</p> 
/// </summary>
public class PathFinderSteeringBehavior: SteeringBehavior, IGizmos
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to go to using found path.")]
    public Target target;
    [Tooltip("Graph to use for path finding.")]
    public MapGraph graph;
    
    [Header("WIRING:")]
    [Tooltip("Steering Behavior to move using found path.")]
    [SerializeField] private PathFollowingSteeringBehavior pathFollowingSteeringBehavior;
    [Tooltip("Path finder to use. Must comply with IGraphPathFinder interface.")]
    [InterfaceCompliant(typeof(IGraphPathFinder))]
    [SerializeField] private MonoBehaviour pathFinderBehaviour;
    
    [Header("DEBUG:")] 
    [SerializeField] private bool showGizmos;
    [SerializeField] private Color gizmosColor = Color.green;
    
    public bool ShowGizmos
    {
        get => showGizmos;
        set
        {
            showGizmos = value;
            if (_currentPath == null) return;
            _currentPath.ShowGizmos = value;
        }
    }
    public Color GizmosColor
    {
        get => gizmosColor;
        set
        {
            gizmosColor = value;
            if (_currentPath == null) return;
            _currentPath.GizmosColor = value;
        }
    }
    
    private IGraphPathFinder graphPathFinder;
    private Path _currentPath;
    
    private void Awake()
    {
        graphPathFinder = (IGraphPathFinder) pathFinderBehaviour;
        graphPathFinder.Graph = graph;
        // Create a GameObject at the scene root to include the new path instance in
        // Unity life cycle.
        _currentPath = new GameObject($"{name} - CurrentPath").AddComponent<Path>();
        _currentPath.ShowGizmos = showGizmos;
        _currentPath.GizmosColor = gizmosColor;
    }

    private void OnEnable()
    {
        if (target == null) return;
        target.positionChanged.AddListener(OnPathTargetPositionChanged);
    }

    private void OnDisable()
    {
        if (target == null) return;
        // We don't want to calculate paths while the game object is not active.
        target.positionChanged.RemoveListener(OnPathTargetPositionChanged);
    }

    private void OnPathTargetPositionChanged(Vector2 newTargetPosition)
    {
        PathData newPath = graphPathFinder.FindPath(newTargetPosition);
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