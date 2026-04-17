using PropertyAttribute;
using Tools;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace SteeringBehaviors
{
/// <summary>
/// Defines a steering behavior for avoiding collisions with other agents. This behavior
/// uses Unity's builtin NavMeshAgent component to calculate avoidance vector.
/// </summary>
/// <remarks>
/// By itself, the algorithm works, but it may show some oscillation. To avoid that
/// oscillation, you can enable autoSmooth in the AgentMover component.  
/// </remarks>
public class MeshAgentAvoiderSteeringBehavior: SteeringBehavior
{
    [Header("CONFIGURATION:")] 
    [Tooltip("Maximum approach allowable between agents.")]
    [SerializeField] public float minimumDistanceBetweenAgents = 2.0f;
    [Tooltip("Change in degrees for direction to consider it a new direction.")]
    [SerializeField] private float directionChangeThreshold = 3f;
    [FormerlySerializedAs("FarDistance")]
    [Tooltip("Distance from the agent to calculate to project current direction.")]
    [SerializeField] private float farDistance = 100f;
    
    [Header("WIRING:")]
    [InterfaceCompliant(typeof(ITargeter))]
    [SerializeField] private SteeringBehavior steeringToTarget;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private PotentialCollisionDetector potentialCollisionDetector;
    
    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos;
    [SerializeField] private Color gizmosColorSteering = Color.green;
    [SerializeField] private Color gizmosColorAvoidance = Color.red;
    [SerializeField] private Color gizmosColorResultingVelocity = Color.blue;

    
    private Vector2 _currentVelocity = Vector2.zero;
    private Vector2 _velocityToTarget = Vector2.zero;
    private Vector2 _lastRequestedDirection = Vector2.zero;
    private Vector2 _avoidVector = Vector2.zero;
    private Vector2 _newVelocity = Vector2.zero;
    private AgentMover _currentAgent;

    public float MinimumDistanceBetweenAgents
    {
        get => minimumDistanceBetweenAgents;
        set
        {
            minimumDistanceBetweenAgents = value;
            float avoidanceRadius = minimumDistanceBetweenAgents/2 + _currentAgent.Radius;
            potentialCollisionDetector.AgentRadius = avoidanceRadius;
            navMeshAgent.radius = avoidanceRadius;
        }
    }

    private void Awake()
    {
        _currentAgent = GetComponentInParent<AgentMover>();
    }

    private void Start()
    {
        MinimumDistanceBetweenAgents = minimumDistanceBetweenAgents;
        navMeshAgent.speed = _currentAgent.MaximumSpeed;
        navMeshAgent.angularSpeed = _currentAgent.MaximumRotationalSpeed;
        navMeshAgent.acceleration = _currentAgent.MaximumAcceleration;
        // We only want to query the navMeshAgent. We don't want it to move the agent by
        // itself.
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
    }

    private void Update()
    {
        UpdateAgent();
    }

    /// <summary>
    /// Feed the navMeshAgent with the data it needs to calculate the nearest vector
    /// to the one that leads to the destination that also avoids collisions with other
    /// agents.
    /// </summary>
    private void UpdateAgent()
    {
        // Update the navMeshAgent simulation with the real velocity and position.
        // Otherwise, navMeshAgent internal simulation could diverge from the real
        // situation of the agent.
        navMeshAgent.nextPosition = transform.position;
        navMeshAgent.velocity = _currentVelocity;
        
        if (_currentVelocity.sqrMagnitude < 0.0001f) return;
        
        // Only request for a new navMeshAgent calculation if it is the first
        // initialization for the _lastRequestedDirection or the direction has changed
        // significantly.
        if (_lastRequestedDirection == Vector2.zero || // First initialization.
            Vector2.Angle(_velocityToTarget, _lastRequestedDirection) > 
            directionChangeThreshold) // Direction has changed significantly.
        {
            // We can only enter destinations to the navMeshAgent, but we only have
            // a desired velocity to target. So, we need to project the desired velocity
            // to a far away point, and we use as destination the
            // point where that projection intersects the navMesh boundary.
            Vector3 farPoint = transform.position + 
                               (Vector3) _velocityToTarget.normalized * farDistance;
            // Here I use NavMesh.Raycast to get one projection point in the navMesh
            // boundary. Another option could be to use navMesh.SamplePosition to get the
            // point in the navMesh nearest to the far away point. I tried it, but I
            // wasn't to make it work. I don't know why, but the projection point always
            // ended in a corner of the navMesh. The Raycast approach worked through.
            if (NavMesh.Raycast(
                    transform.position, 
                    farPoint, 
                    out var hit, 
                    NavMesh.AllAreas))
                // Once navMeshAgent has the destination, it will start to calculate
                // the necessary velocity vector to get there avoiding collisions with
                // other agents. That calculation won't be ready in the same frame, it
                // will need at least one frame. That's why we need to set the
                // destination at Update() but request the calculated velocity in the
                // call to GetSteering() from FixedUpdate(). 
                navMeshAgent.SetDestination(hit.position);
            _lastRequestedDirection = _velocityToTarget;
        }
    }

    /// <summary>
    /// Calculates the avoidance vector for a NavMeshAgent. This vector is used to adjust
    /// the agent's movement to avoid potential collisions with other agents or obstacles
    /// detected in the environment. The avoidance vector is computed only when a
    /// potential collision is detected.
    /// </summary>
    /// <remarks>
    /// The calculation is based on subtracting the last requested direction of the
    /// agent's movement from the desired velocity of the NavMeshAgent, which includes
    /// both the agent's movement to the target destination and adjustments for
    /// collision avoidance. If no potential collision is detected, the avoidance
    /// vector is set to zero.
    /// </remarks>
    private void CalculateAvoidanceVector()
    {
        if (navMeshAgent.pathPending) return;
        // Only compute the avoidance vector if there is any risk of colliding. That
        // way we minimize the chance of computing as an avoidance vector an actual
        // path vector update to get destination.
        _avoidVector = potentialCollisionDetector.PotentialCollisionDetected? 
            // NavMeshAgent desiredVelocity comprises the velocity of the agent to get its
            // destination and the avoidance vector we need. So, to get the avoidance
            // vector, we need to subtract the current velocity. Actually,
            // _lastRequestedDirection is one frame behind the desiredVelocity, but
            // that difference is negligible.
            (Vector2)navMeshAgent.desiredVelocity - _lastRequestedDirection:
            Vector2.zero;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        SteeringOutput steeringOutputToTarget = steeringToTarget.GetSteering(args);
        // Ideal velocity to get the target. It would be the one to use if there was no
        // risk of collision.
        _velocityToTarget = steeringOutputToTarget.Linear;
        // Current velocity of the agent. Previous to decide the next velocity.
        _currentVelocity = args.CurrentVelocity;

        CalculateAvoidanceVector();

        _newVelocity = steeringOutputToTarget.Linear + _avoidVector;
        
        return new SteeringOutput(_newVelocity, steeringOutputToTarget.Angular);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmosColorAvoidance;
        Gizmos.DrawLine(
            transform.position, 
            transform.position + (Vector3) _avoidVector);
        Gizmos.color = gizmosColorSteering;
        Gizmos.DrawLine(
            transform.position, 
            transform.position + (Vector3) _velocityToTarget);
        Gizmos.color = gizmosColorResultingVelocity;
        Gizmos.DrawLine(
            transform.position, 
            transform.position + (Vector3) _newVelocity);
        Gizmos.DrawWireSphere(navMeshAgent.destination, 0.1f);
    }
#endif
}
}