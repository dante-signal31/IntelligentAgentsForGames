using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Pathfinding
{
/// <summary>
/// Pathfinder using Unity's NavMesh.
/// </summary>
public class MeshPathFinder: MonoBehaviour, IPathFinder
{
    [Header("CONFIGURATION:")]
    [Tooltip("Event invoked when the path changes.")]
    [SerializeField] public UnityEvent pathChanged = new();
    
    /// <summary>
    /// Radius of the agent that uses this pathfinder.
    /// </summary>
    public float Radius
    {
        get => _radius;
        set
        {
            if (Mathf.Approximately(_radius, value)) return;
            _radius = value;
            RecalculatePath();
        }
    }
    
    private Vector2 _targetPosition;
    private float _radius;
    private NavMeshPath _navMeshPath;
    private readonly PathData _pathData = new();

    private void Awake()
    {
        _navMeshPath = new NavMeshPath();
    }
    
    public PathData FindPath(Vector2 targetPosition, Vector2 fromPosition=default)
    {
        _targetPosition = targetPosition;
        RecalculatePath(fromPosition);
        return _pathData;
    }

    /// <summary>
    /// Recalculates the navigation path from the specified starting position
    /// to the current target position. If no starting position is provided,
    /// the recalculation starts from the object's current position.
    /// </summary>
    /// <param name="fromPosition">
    /// The starting position for recalculation. If not specified,
    /// the object's current position is used as the starting point.
    /// </param>
    private void RecalculatePath(Vector2 fromPosition=default)
    {
        Vector2 sourcePosition = fromPosition == default ? 
            transform.position : 
            fromPosition;
        NavMesh.CalculatePath(sourcePosition, 
            _targetPosition, 
            NavMesh.AllAreas, 
            _navMeshPath);
        UpdatePathData();
        pathChanged?.Invoke();
    }

    /// <summary>
    /// Updates the internal path data structure with the current navigation path.
    /// Converts the navigation path corners from 3D to 2D coordinates and loads
    /// them into the path data object.
    /// </summary>
    private void UpdatePathData()
    {
        Vector3[] path = _navMeshPath.corners;
        List<Vector2> path2D = new();
        foreach (var position in path)
        {
            path2D.Add(position);
        }
        _pathData.LoadPathData(new List<Vector2>(path2D));
    }
}
}