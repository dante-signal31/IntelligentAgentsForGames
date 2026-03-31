using System.Collections.Generic;
using Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Node to offer an agent avoider steering behavior based on the Avoid Nearest
/// Neighbor algorithm.</p>
/// <p>ANN algorithm selects the nearest agent to the current agent and calculates
/// a vector to avoid it. This vector is opposed to the relative position between agents,
/// and its magnitude is inversely proportional to the current distance between agents,
/// getting its maximum value when distance reduces to minimumDistanceBetweenAgents.</p>
/// <p>The difference with an obstacle avoidance algorithm is that obstacles don't move
/// while agents do.</p>
/// </summary>
public class AnnPassiveAgentAvoiderBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")] 
    [Tooltip("Maximum approach allowable between agents.")]
    [SerializeField] public float minimumDistanceBetweenAgents = 2.0f;
    /// <summary>
    /// Threshold factor for determining when to use normal vector avoidance.
    /// When the dot product between avoidance and collision vectors exceeds this value 
    /// (positive or negative), the avoidance vector is replaced with a vector normal 
    /// to the collision agent's velocity to prevent chase or collision scenarios.
    /// </summary>
    [SerializeField] public float tooAlignedFactor = 0.95f;
    
    [Header("WIRING:")]
    [SerializeField] private VolumetricSensor sensor;
    
    [Header("DEBUG:")]
    [SerializeField] public bool showGizmos = true;
    [SerializeField] public Color gizmosColor = Color.red;
    
    private Vector2 _currentAgentAvoidingVelocity = Vector2.zero;
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        // No agents near? No need to avoid.
        if (!sensor.AnyObjectDetected) return SteeringOutput.Zero;

        // Find the nearest agent.
        HashSet<GameObject> detectedObjects = sensor.ObjectsDetected;
        AgentMover nearestAgent = null;
        float distance = float.MaxValue;
        Vector2 evasionVector = Vector2.zero;
        foreach (GameObject detectedObject in detectedObjects)
        {
            AgentMover detectedAgent = detectedObject.GetComponent<AgentMover>();
            if (detectedAgent == null) continue;
            float currentDistance = Vector2.Distance(
                transform.position, 
                detectedAgent.transform.position);
            if (currentDistance >= distance) continue;
            evasionVector = 
                (transform.position - detectedAgent.transform.position).normalized;
            distance = currentDistance;
            nearestAgent = detectedAgent;
        }
        
        // If the sensor detected anything, then we must have found the nearest agent.
        // But just in case...
        if (nearestAgent == null) return SteeringOutput.Zero;
        
        // Calculate the evasion vector and speed. The evasion vector is the vector
        // opposed to the relative vector from the agent to the nearest agent. The evasion
        // speed is higher the closer the agent is to the nearest agent, getting infinite
        // when they are at MinimumDistanceBetweenAgents.
        AgentMover currentAgent = args.CurrentAgent.GetComponent<AgentMover>();
        float addedRadius = nearestAgent.Radius + currentAgent.Radius;
        float evasionMagnitude = 1 / 
                                 Mathf.Max(
                                     Mathf.Epsilon, 
                                     distance - addedRadius - 
                                     minimumDistanceBetweenAgents);
        float evasionSpeed = Mathf.Min(
            args.MaximumSpeed, 
            evasionMagnitude * args.MaximumSpeed);
        
        // Finally, we can calculate the final evasion vector.
        _currentAgentAvoidingVelocity = evasionVector * evasionSpeed;
        
        // THE EDGE CASE:
        // This algorithm suffers the same edge problem as Millington's. The problem
        // with the original algorithm is that it does not seem to take in count the edge
        // case where the two agents are going one against the other directly, in opposite 
        // directions. The rest of the method fixes that.
        
        // One way to find out if the two agents are going one against the other in 
        // opposite directions is to check the dot product between the evasion vector
        // and the current velocity. If the absolute value of a dot product is near 1,
        // that means the two agents are going away or approaching, in both cases in
        // the same "line". In the first case, it wouldn't be a collision, but we want
        // an avoidance movement, not a chase.In the second case, that means that the
        // two agents are approaching in opposite directions.
        float alignmentFactor = Mathf.Abs(
            Vector2.Dot(evasionVector,currentAgent.Velocity.normalized)); 
        if (Mathf.Abs(alignmentFactor) >= tooAlignedFactor)
        {
            // If relative velocity is too aligned with evasionVector, then it means
            // we can end in a direct hit, so we try an evasion vector that is
            // perpendicular to the current agent's velocity.
            evasionVector =
                (Quaternion.Euler(0, 0, 90) * currentAgent.Velocity).normalized *
                // Turn to one side or another randomly.
                (Random.Range(0, 1) * 2 - 1);
            
            _currentAgentAvoidingVelocity = evasionVector * args.MaximumSpeed;
        }
        
        return new SteeringOutput(_currentAgentAvoidingVelocity);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        if (sensor == null) return;

        if (!sensor.AnyObjectDetected) return;
        
        // Draw current agent avoider velocity.
        Gizmos.color = gizmosColor;
        Gizmos.DrawLine(
            transform.position, 
            transform.position + (Vector3) _currentAgentAvoidingVelocity);
    }
#endif
}
}

