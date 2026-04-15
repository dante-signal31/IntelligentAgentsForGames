using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace SteeringBehaviors
{
/// <summary>
/// A steering behavior responsible for pathfinding on a navigation mesh.
/// This class allows an agent to follow a dynamically updated path
/// to a specified target while considering obstacles and navigation-specific constraints.
/// </summary>
public class MeshPathFinderSteeringBehavior : SteeringBehavior, IGizmos, ITargeter
{
    [Header("CONFIGURATION:")]
    [FormerlySerializedAs("pathTarget")]
    [Tooltip("Target to go to.")]
    [SerializeField] private GameObject target;
    [Tooltip("Radius of the agent. Needed to find a path wide enough.")]
    public float agentRadius = 0.5f;
    
    [Header("WIRING:")]
    [Tooltip("Component to move the agent following the generated path.")]
    [SerializeField] private PathFollowingSteeringBehavior pathFollowingSteeringBehavior;
    [Tooltip("Component to generate a valid path using mesh navigation")]
    [SerializeField] private MeshPathFinder meshPathFinder;

    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos;
    [SerializeField] private Color gizmosColor;

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
    
    public GameObject Target
    {
        get => target;
        set
        {
            if (target == value) return;
            TargetPosition = value.transform.position;
        }
    }

    private Vector2 _targetPosition;
    /// <summary>
    /// Target position to go to.
    /// </summary>
    private Vector2 TargetPosition
    {
        get => _targetPosition;
        set
        {
            _targetPosition = value;
            UpdatePath(value);
        }
    }
    
    /// <summary>
    /// Current pathfinder used to generate the path.
    /// </summary>
    public MeshPathFinder CurrentPathFinder => meshPathFinder;
    
    private Path _currentPath;
    
    private void UpdatePath(Vector2 newTargetPosition)
    {
        PathData newPath = meshPathFinder.FindPath(newTargetPosition);
        if (newPath == null) return;
        _currentPath.UpdatePathData(newPath);
    }

    private void Awake()
    {
        // Create a GameObject at the scene root to include the new path instance in
        // Unity life cycle.
        _currentPath = 
            new GameObject("MeshPathFinderSteeringBehavior - CurrentPath")
                .AddComponent<Path>();
        _currentPath.ShowGizmos = ShowGizmos;
        _currentPath.GizmosColor = GizmosColor;
    }

    private void Start()
    {
        // Show path gizmo if we are debugging.
        _currentPath.name = $"{name} - Path";
        _currentPath.ShowGizmos = ShowGizmos;
        _currentPath.GizmosColor = GizmosColor;
        
        // Configure the path following steering behavior to follow the path generated
        // here.
        pathFollowingSteeringBehavior.FollowPath = _currentPath;
        
        // Configure the mesh pathfinder.
        meshPathFinder.Radius = agentRadius;
    }

    private void FixedUpdate()
    {
        if (Target == null) return;
        if ((Vector2)Target.transform.position != TargetPosition)
        {
            // When you change the target position, the pathfinder will be updated.
            TargetPosition = Target.transform.position;
        }
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        return pathFollowingSteeringBehavior.GetSteering(args);
    }


}
}

