using System;
using UnityEngine;
using UnityEngine.AI;

namespace Pathfinding
{
/// <summary>
/// Navigation agent using Unity's NavMesh as pathfinding algorithm.
/// </summary>
public class MeshNavigationAgent: NavigationAgent
{
    [Header("CONFIGURATION:")]
    [SerializeField] private LayerMask navigationLayers;
    
    private Vector2 _targetPosition;
    private float _radius;
    private Vector2[] _pathToTarget;
    private Vector2 _pathFinalPosition;

    public override Vector2 TargetPosition
    {
        get => _targetPosition;
        set
        {
            if (_targetPosition == value) return;
            _targetPosition = value;
            RecalculatePath();
        }
    }

    public override float Radius
    {
        get => _radius;
        set
        {
            if (Mathf.Approximately(_radius, value)) return;
            _radius = value;
            RecalculatePath();
        }
    }

    public override bool IsReady => _pathToTarget.Length > 0;

    public override bool IsTargetReachable => 
        TargetPosition == PathFinalPosition;

    public override bool IsTargetReached => 
        (Vector2)transform.position == TargetPosition;

    public override bool IsNavigationFinished => 
        (Vector2)transform.position == PathFinalPosition;

    public override Vector2[] PathToTarget => _pathToTarget;

    public override Vector2 PathFinalPosition => _pathToTarget[^1];
    
    private NavMeshPath _navMeshPath;
    private int _currentPathIndex = 0;

    private void Awake()
    {
        _navMeshPath = new NavMeshPath();
    }

    public override float DistanceToTarget()
    {
        float distance = 0;
        // Path distance.
        for (int i = _currentPathIndex; i < _pathToTarget.Length; i++)
        {
            if (i == _pathToTarget.Length - 1) break;
            distance += Vector2.Distance(_pathToTarget[i], _pathToTarget[i + 1]);
        }
        // Distance to path start.
        distance += Vector2.Distance(transform.position, _pathToTarget[_currentPathIndex]);
        return distance;
    }

    public override Vector2 GetNextPathPosition()
    {
        return _pathToTarget[_currentPathIndex];
    }
    
    private void RecalculatePath()
    {
        NavMesh.CalculatePath(transform.position, TargetPosition, NavMesh.AllAreas, _navMeshPath);
        UpdatePathToTarget();
        _currentPathIndex = 0;
    }

    private void UpdatePathToTarget()
    {
        Vector3[] path = _navMeshPath.corners;
        Vector2[] path2D = new Vector2[path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            path2D[i] = path[i];
        }
        _pathToTarget = path2D;
    }

    private void FixedUpdate()
    {
        if (PathToTarget.Length == 0 || IsNavigationFinished) return;
        if ((Vector2)transform.position == _pathToTarget[_currentPathIndex])
        {
            _currentPathIndex++;
        }
        
    }
}
}