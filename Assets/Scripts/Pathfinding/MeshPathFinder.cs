using System;
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
    public UnityEvent pathChanged = new();
    
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
    
    public PathData FindPath(Vector2 targetPosition)
    {
        _targetPosition = targetPosition;
        RecalculatePath();
        return _pathData;
    }
    
    private void RecalculatePath()
    {
        NavMesh.CalculatePath(transform.position, 
            _targetPosition, 
            NavMesh.AllAreas, 
            _navMeshPath);
        UpdatePathData();
        pathChanged?.Invoke();
    }

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