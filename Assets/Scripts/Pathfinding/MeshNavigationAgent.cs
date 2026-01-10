using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Pathfinding
{
/// <summary>
/// Navigation agent using Unity's NavMesh as a pathfinding algorithm.
/// </summary>
public class MeshNavigationAgent: NavigationAgent
{
    [Header("CONFIGURATION:")]
    [Tooltip("Distance at which we give our goal as reached.")]
    [SerializeField] private float arrivalDistance = 0.1f;
    [Tooltip("Event invoked when the path changes.")]
    public UnityEvent pathChanged = new();
    
    private Vector2 _targetPosition;
    private float _radius;
    private Vector2[] _pathToTarget;
    private Vector2 _pathFinalPosition;

    public override float ArrivalDistance
    {
        get => arrivalDistance;
        set
        { 
            if (Mathf.Approximately(arrivalDistance, value)) return;
            arrivalDistance = value;
            RecalculatePath();
        }
    }

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

    private bool _isNavigationFinished;
    public override bool IsNavigationFinished => _isNavigationFinished;

    public override Vector2[] PathToTarget => _pathToTarget;

    public override Vector2 PathFinalPosition => _pathToTarget[^1];
    
    private NavMeshPath _navMeshPath;
    private int _currentPathIndex;
    private float _arrivalDistance;
    
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
        _navMeshPath ??= new NavMeshPath();
        NavMesh.CalculatePath(transform.position, 
            TargetPosition, 
            NavMesh.AllAreas, 
            _navMeshPath);
        UpdatePathToTarget();
        _currentPathIndex = 0;
        pathChanged?.Invoke();
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
        if (PathToTarget.Length == 0) return;
        if (Vector2.Distance(transform.position, 
                _pathToTarget[_currentPathIndex]) < ArrivalDistance)
        {
            if (_currentPathIndex == _pathToTarget.Length - 1)
            {
                _isNavigationFinished = true;
                return;
            }
            _isNavigationFinished = false;
            _currentPathIndex++;
        }
        
    }
}
}