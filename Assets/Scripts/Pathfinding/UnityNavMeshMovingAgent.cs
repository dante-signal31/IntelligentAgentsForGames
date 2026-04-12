using Tools;
using UnityEngine;
using UnityEngine.AI;

namespace Pathfinding
{
/// <summary>
/// A Unity MonoBehaviour component that uses Unity's NavMesh system
/// to handle agent movement and pathfinding.
/// </summary>
public class UnityNavMeshMovingAgent : MonoBehaviour
{
    [Header("CONFIGURATION:")] 
    [SerializeField] private Target target;
    
    [Header("WIRING:")]
    [SerializeField] private NavMeshAgent navMeshAgent;

    public Target Target
    {
        get => target;
        set
        {
            // We don't want the event from the previous target any longer.
            if (target != null)
                target.positionChanged?.RemoveListener(OnTargetPositionChanged);
            
            target = value;
            // We want to listen to the new target's position changes.
            target.positionChanged?.AddListener(OnTargetPositionChanged);
            
            // Head to initial target position.
            if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled) return;
            OnTargetPositionChanged(target.transform.position);
        }
    }

    private void Start()
    {
        // Nav mesh agent expects to be used in a 3D environment. In a 2D environment,
        // we need to disable rotation and up axis or the agent will be rotated to be
        // longitudinal to the view axis, making the agent invisible.
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        // Head to initial target position.
        OnTargetPositionChanged(target.transform.position);
    }

    private void OnEnable()
    {
        if (target != null) 
            target.positionChanged.AddListener(OnTargetPositionChanged);
    }

    private void OnDisable()
    {
        if (target != null) 
            target.positionChanged.RemoveListener(OnTargetPositionChanged);
    }

    private void OnTargetPositionChanged(Vector2 arg0)
    {
        navMeshAgent.SetDestination(arg0);
    }
    
    private void Update()
    {
        // As we disabled rotation, we need to rotate the agent manually.
        RotateTowardsMovement();
    }

    private void RotateTowardsMovement()
    {
        if (navMeshAgent.velocity.sqrMagnitude < 0.1f) return;
        transform.up = navMeshAgent.velocity;
    }
}
}

