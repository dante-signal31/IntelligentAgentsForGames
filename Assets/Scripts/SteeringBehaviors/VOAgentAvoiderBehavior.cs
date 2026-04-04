using System.Collections.Generic;
using PropertyAttribute;
using Tools;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Node to offer an agent avoider steering behavior based on the Velocity Obstacle
/// algorithm.</p>
/// <p>The difference with an obstacle avoidance algorithm is that obstacles don't move
/// while agents do.</p>
/// <remarks> This behavior needs AutoSmooth enabled at the MovingAgent node. It works
/// best if the auto-smoothing method is WeightedMovingAverage with 20 samples and a
/// gaussian curve.</remarks>
/// </summary>
public class VoAgentAvoiderBehavior: SteeringBehavior
{
    [Header("CONFIGURATION:")] 
    [Tooltip("Number of samples for the velocity sampling disc. The higher the " +
             "number, the more accurate the calculated velocity will be, but the " +
             "more expensive it will be to calculate.")]
    [SerializeField] private int samplingDiscResolution = 100;
    [Tooltip("The higher the value, the more aggressive the evasion will be. Keep " +
             "well above than the double of MinimumDistanceBetweenAgents.")]
    [SerializeField] public float evasionStrength = 1.5f;
    [Tooltip("Minimum distance between agents to avoid. 0.5 units of distance may be " +
             "enough to avoid collision with other agents in usual situations. But " +
             "for frontal collisions, you will need a minimum distance of 1 and " +
             "an evasion strength above 2 units.")]
    [SerializeField] public float minimumDistanceBetweenAgents = 0.5f;

    [Header("WIRING:")]
    [Tooltip("Steering behavior to move the agent to the target.")]
    [InterfaceCompliant(typeof(ITargeter))]
    [SerializeField] private SteeringBehavior toTargetSteeringBehavior;
    [Tooltip("Potential collision detector to detect potential collisions with " +
             "other agents.")]
    [SerializeField] private PotentialCollisionDetector collisionDetector;
    
    [Header("DEBUG:")]
    [SerializeField] public bool showGizmos;
    [SerializeField] public Color gizmosColor;
    [SerializeField] public Color noCollisionVelocitiesColor;
    [SerializeField] public Color collisionVelocitiesColor;
    [SerializeField] public float gizmoRadius = 0.2f;
    
    /// <summary>
    /// Number of samples for the velocity sampling disc. The higher the number,
    /// the more accurate the calculated velocity will be, but the more expensive
    /// it will be to calculate.
    /// </summary>
    public uint SamplingDiscResolution
    {
        get => (uint) samplingDiscResolution;
        set
        {
            samplingDiscResolution = (int) value;
            GenerateVelocitySamplingDisc(CurrentMaximumSpeed, value);
        }
    }
    
    private float _currentMaximumSpeed;
    /// <summary>
    /// Current maximum speed for this behavior.
    /// </summary>
    private float CurrentMaximumSpeed
    {
        get => _currentMaximumSpeed;
        set
        {
            _currentMaximumSpeed = value;
            GenerateVelocitySamplingDisc(value, SamplingDiscResolution);
        }
    }
    
    public readonly HashSet<Vector2> noCollisionCandidateVelocities = new();
    public readonly HashSet<Vector2> collisionCandidateVelocities = new();
    [HideInInspector] public Vector2 bestCandidateVelocity;
    
    private const float Phi = 1.618033988749895f;
    private readonly HashSet<Vector2> _velocitySamplingDisc = new();
    private AgentMover _currentAgent;
    

    private void Awake()
    {
        _currentAgent = GetComponentInParent<AgentMover>();
    }

    /// <summary>
    /// Generate a cloud of relative positions. Every position represents a potential
    /// velocity vector for the agent. So, the cloud is supposed to be centered on the
    /// agent position, and it cannot extend further than the maximum speed.
    /// </summary>
    /// <param name="maximumSpeed">Maximum radius for the cloud of points.</param>
    /// <param name="resolution">Number of points for the cloud.</param>
    private void GenerateVelocitySamplingDisc(float maximumSpeed, uint resolution)
    {
        // The first and easiest way to generate the points would be to use two for loops
        // that iterate over angles and radius. But here’s a better solution, using the
        // most irrational number (the golden ratio) to uniformly generate points
        // on a disk.
        // Got from: https://jasonfantl.com/posts/Collision-Avoidance/#sampling-velocities
        _velocitySamplingDisc.Clear();
        for (int i = 1; i <= resolution; i++)
        {
            float radius = Mathf.Sqrt((float)i / resolution) * maximumSpeed;
            float angle = i * 2 * Mathf.PI * Phi;
            Vector2 newPoint = new Vector2(
                radius * Mathf.Cos(angle), 
                radius * Mathf.Sin(angle));
            _velocitySamplingDisc.Add(newPoint);
        }
    }
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        // If maximum speed has changed, then we need to regenerate the velocity
        // sampling disc. Just updating the property makes that.
        if (!Mathf.Approximately(args.MaximumSpeed, CurrentMaximumSpeed)) 
            CurrentMaximumSpeed = args.MaximumSpeed;
        
        SteeringOutput steeringToTargetVelocity = 
            toTargetSteeringBehavior.GetSteering(args);
     
        noCollisionCandidateVelocities.Clear();
        collisionCandidateVelocities.Clear();
        
        // If there is no potential collision, then we don't need to do anything. Just go
        // straight to the target.
        if (!collisionDetector.EvaluatingPotentialCollision)
        {
            bestCandidateVelocity = steeringToTargetVelocity.Linear;
            return steeringToTargetVelocity;
        }

        // If a collision is going to happen in the future, then calculate the avoiding
        // vector nearest to the ideal velocity to target.
        float lowestPenalty = float.MaxValue;
        bestCandidateVelocity = Vector2.zero;
        foreach (Vector2 candidateVelocity in _velocitySamplingDisc)
        {
            // Add candidate velocity to one of the lists used for debugging.
            if (collisionDetector.IsCollidingVelocity(
                    candidateVelocity,
                    _currentAgent.Radius + minimumDistanceBetweenAgents,
                    out var collisionTime))
            {
                collisionCandidateVelocities.Add(candidateVelocity);
            }
            else
            {
                noCollisionCandidateVelocities.Add(candidateVelocity);
            }
            float vectorDivergence =
                (steeringToTargetVelocity.Linear - candidateVelocity).magnitude;
            float penalty = vectorDivergence + (evasionStrength / collisionTime);
            if (penalty < lowestPenalty)
            {
                lowestPenalty = penalty;
                bestCandidateVelocity = candidateVelocity;
            }
        }
        
        // Return the best candidate velocity as part of the steering output.
        return new SteeringOutput(
            bestCandidateVelocity, 
            steeringToTargetVelocity.Angular);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmosColor;
        // Draw current selected velocity.
        Gizmos.DrawLine(
            transform.position, 
            transform.position + (Vector3) bestCandidateVelocity);
        
        // Draw velocity obstacle cone towards nearest collision agent.
        AgentMover nearestCollisionAgent = collisionDetector.PotentialCollisionAgent;
        if (nearestCollisionAgent == null) return;
        Vector2 toNearestCollisionAgent =
            nearestCollisionAgent.transform.position - transform.position;
        float nearestCollisionAgentRadius = nearestCollisionAgent.Radius;
        float semiAngle = Mathf.Atan(
            (nearestCollisionAgentRadius + _currentAgent.Radius + minimumDistanceBetweenAgents) / 
            toNearestCollisionAgent.magnitude);
        Vector2 coneSide1 = Quaternion.Euler(0, 0, semiAngle * Mathf.Rad2Deg) * 
                            toNearestCollisionAgent.normalized;
        Vector2 coneSide2 = Quaternion.Euler(0, 0, -semiAngle * Mathf.Rad2Deg) * 
                            toNearestCollisionAgent.normalized;
        Gizmos.color = collisionVelocitiesColor;
        Gizmos.DrawLine(
            transform.position, 
            transform.position + (Vector3) coneSide1 * _currentMaximumSpeed);
        Gizmos.DrawLine(
            transform.position, 
            transform.position + (Vector3) coneSide2 * _currentMaximumSpeed);
    }
}
}