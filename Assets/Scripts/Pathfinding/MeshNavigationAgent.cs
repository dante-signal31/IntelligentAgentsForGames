using System;
using UnityEngine;
using UnityEngine.AI;

namespace Pathfinding
{
/// <summary>
/// Navigation agent using Unity's NavMeshAgent as pathfinding algorithm.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class MeshNavigationAgent: NavigationAgent
{
    private Vector2 targetPosition;
    private float radius;
    private bool isReady;
    private bool isTargetReachable;
    private bool isTargetReached;
    private bool isNavigationFinished;
    private Vector2[] pathToTarget;
    private Vector2 pathFinalPosition;

    public override Vector2 TargetPosition
    {
        get => targetPosition;
        set => targetPosition = value;
    }

    public override float Radius
    {
        get => radius;
        set
        {
            radius = value;
        }
    }

    public override bool IsReady => isReady;

    public override bool IsTargetReachable => isTargetReachable;

    public override bool IsTargetReached => isTargetReached;

    public override bool IsNavigationFinished => isNavigationFinished;

    public override Vector2[] PathToTarget => pathToTarget;

    public override Vector2 PathFinalPosition => pathFinalPosition;

    private NavMeshAgent _navMeshAgent;
    
    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public override float DistanceToTarget()
    {
        throw new System.NotImplementedException();
    }

    public override Vector2 GetNextPathPosition()
    {
        throw new System.NotImplementedException();
    }
}
}