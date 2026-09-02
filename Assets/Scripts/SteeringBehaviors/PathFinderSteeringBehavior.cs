using System;
using Pathfinding;
using PropertyAttribute;
using UnityEngine;
using UnityEngine.Serialization;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Steering behavior to find, and follow, a path to a given target.</p>
/// <p>The pathfinder algorithm used depends on the IGraphPathFinder instance referenced
/// from the pathFinder field.</p> 
/// </summary>
public class PathFinderSteeringBehavior: SteeringBehavior, IGizmos
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to go to using found path.")]
    [SerializeField] public GameObject target;
    [Tooltip("Minimum changed distance to consider the target position updated.")]
    [SerializeField] public float minDistanceToUpdate = 0.1f;
    
    [Header("WIRING:")]
    [Tooltip("Steering Behavior to move using found path.")]
    [SerializeField] private PathFollowingSteeringBehavior pathFollowingSteeringBehavior;
    [FormerlySerializedAs("pathFinderBehaviour")]
    [Tooltip("Path finder to use. Must comply with IGraphPathFinder interface.")]
    [InterfaceCompliant(typeof(IGraphPathFinder))]
    [SerializeField] private MonoBehaviour pathFinder;
    [Tooltip("Steering behavior to execute when path is finished until reaching the " +
             "target.")]
    [InterfaceCompliant(typeof(ITargeter))]
    [SerializeField] private SteeringBehavior finalSteeringBehaviour;
    
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
    
    private IGraphPathFinder _graphPathFinder;
    private ITargeter _finalSteeringTargeter;
    private Path _currentPath;
    private GameObject _targeterMarker;
    private bool _calculatingPath;
    
    private void Awake()
    {
        _finalSteeringTargeter = (ITargeter) finalSteeringBehaviour;
        _graphPathFinder = (IGraphPathFinder) pathFinder;
        
        // Create a GameObject at the scene root to include the new path instance in
        // Unity life cycle.
        _currentPath = new GameObject($"{name} - CurrentPath").AddComponent<Path>();
        _currentPath.ShowGizmos = showGizmos;
        _currentPath.GizmosColor = gizmosColor;
        
        // Create a GameObject as the final steering targeter target.
        _targeterMarker = new GameObject($"{name} - TargetMarker");
        _finalSteeringTargeter.Target = _targeterMarker;
        _targeterMarker.transform.position = target.transform.position;
    }

    private void Start()
    {
        OnPathTargetPositionChanged(target.transform.position);
    }

    private void FixedUpdate()
    {
        if (Vector2.Distance(
                target.transform.position,
                _targeterMarker.transform.position) > minDistanceToUpdate)
        {
            OnPathTargetPositionChanged(target.transform.position);
        }
    }

    /// <summary>
    /// Handles the event triggered when the target's position changes. Updates the path
    /// to the new target position and configures the path-following behavior to follow
    /// the updated path.
    /// </summary>
    /// <param name="newTargetPosition">The new position of the target that the
    /// pathfinding will calculate a path to reach.</param>
    private void OnPathTargetPositionChanged(Vector2 newTargetPosition)
    {
        // Lock to avoid multiple pathfinding calculations at the same time.
        if (_calculatingPath) return;
        _calculatingPath = true;
        
        // Update pathfinding.
        PathData newPath = _graphPathFinder.FindPath(newTargetPosition);
        if (newPath == null) return;
        _currentPath.UpdatePathData(newPath);
        pathFollowingSteeringBehavior.FollowPath = _currentPath;
        
        // Update final steering behavior.
        _targeterMarker.transform.position = target.transform.position;
        
        // Release lock.
        _calculatingPath = false;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        SteeringOutput pathFollowingOutput = 
            pathFollowingSteeringBehavior.GetSteering(args);
        
        // If path is not finished, return the steering output from the path-following
        // behavior.
        if (pathFollowingOutput != SteeringOutput.zero) 
            return pathFollowingOutput;
        
        // If path is finished, execute final steering behavior to get the target.
        return finalSteeringBehaviour.GetSteering(args);
    }
}
}