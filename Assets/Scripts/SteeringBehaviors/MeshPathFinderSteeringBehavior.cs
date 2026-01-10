using System.Collections.Generic;
using Pathfinding;
using Tools;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// A steering behavior responsible for pathfinding on a navigation mesh.
/// This class allows an agent to follow a dynamically updated path
/// to a specified target while considering obstacles and navigation-specific constraints.
/// </summary>
public class MeshPathFinderSteeringBehavior : SteeringBehavior, IGizmos
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to go to.")]
    [SerializeField] private Target pathTarget;
    [Tooltip("Radius of the agent. Needed to find a path wide enough.")]
    public float agentRadius = 0.5f;
    
    [Header("WIRING:")]
    [Tooltip("Component to move the agent following the generated path.")]
    [SerializeField] private PathFollowingSteeringBehavior pathFollowingSteeringBehavior;
    [Tooltip("Component to generate a valid path using mesh navigation")]
    [SerializeField] private MeshNavigationAgent meshNavigationPathFinder;

    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos;
    [SerializeField] private Color gizmosColor;

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
    
    public Target PathTarget
    {
        get => pathTarget;
        set
        {
            if (pathTarget == value) return;
            SetupNewTarget(value);
        }
    }
    
    private Path _currentPath;
    
    /// <summary>
    /// Assign a new target game object to follow.
    /// </summary>
    /// <param name="value">New target game object.</param>
    private void SetupNewTarget(Target value)
    {
        // In case we are changing the followed target, then we are no longer interested
        // in the old target's position changes.
        if (pathTarget != null) 
            pathTarget.positionChanged.RemoveListener(OnPathTargetPositionChanged);
        
        // Set the new target and subscribe to its position changes.
        pathTarget = value;
        pathTarget.positionChanged.AddListener(OnPathTargetPositionChanged);
        
        // Update the mesh navigation pathfinder's target position.'
        if (meshNavigationPathFinder == null) return;
        meshNavigationPathFinder.TargetPosition = value.TargetPosition;
    }

    /// <summary>
    /// <p>Callback for when the target position changes.</p>
    /// <p>This method updates the mesh navigation server's target position and generates
    /// a synthetic call to the server to force it to recalculate the path to the new
    /// target.</p>'
    /// </summary>
    /// <param name="newTargetPosition">New target position.</param>
    private void OnPathTargetPositionChanged(Vector2 newTargetPosition)
    {
        meshNavigationPathFinder.TargetPosition = newTargetPosition;
    }
    
    /// <summary>
    /// Callback for when the mesh navigation updates its path to the new target
    /// position.
    /// </summary>
    private void OnPathUpdated()
    {
        _currentPath.Data.LoadPathData(
            new List<Vector2>(meshNavigationPathFinder.PathToTarget));
    }

    private void Awake()
    {
        // Create a GameObject at the scene root to include the new path instance in
        // Unity life cycle.
        _currentPath = 
            new GameObject("MeshPathFinderSteeringBehavior - CurrentPath")
                .AddComponent<Path>();
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
        meshNavigationPathFinder.pathChanged.AddListener(OnPathUpdated);
        meshNavigationPathFinder.Radius = agentRadius;
        
        // Make the agent head to the target if we already have one.
        if (pathTarget == null) return;
        SetupNewTarget(pathTarget);
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        return pathFollowingSteeringBehavior.GetSteering(args);
    }
}
}

