using System;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Node to offer an agent avoider steering behaviour over a given velocity.</p>
/// <p>Represents a steering behavior where an agent avoids another agents it may
/// collision with in its path.</p>
/// <p>The difference with an obstacle avoidance algorithm is that obstacles don't move
/// while agents do.</p>
/// </summary>
public class PassiveAgentAvoiderSteeringBehavior : SteeringBehavior, IGizmos
{
    [Header("CONFIGURATION:")]
    [Tooltip("Threshold factor for determining when to use normal vector avoidance.")]
    [SerializeField] private float tooAlignedFactor = 0.95f;
    
    [Header("WIRING:")]
    [SerializeField] private PotentialCollisionDetector potentialCollisionDetector;
    
    [Header("DEBUG:")]
    [Tooltip("Show gizmos.")]
    [SerializeField] private bool showGizmos;
    [Tooltip("Color for this component's gizmos.")]
    [SerializeField] private Color gizmosColor;
    
    /// <summary>
    /// Threshold factor for determining when to use normal vector avoidance.
    /// When the dot product between normalized avoidance and collision vectors
    /// exceeds this value (positive or negative), the avoidance vector is replaced
    /// with a vector normal to the collision agent's velocity to prevent chase or
    /// collision scenarios.
    /// </summary>
    public float TooAlignedFactor
    {
        get => tooAlignedFactor;
        set => tooAlignedFactor = value;
    }

    /// <summary>
    /// Show gizmos.
    /// </summary>
    public bool ShowGizmos
    {
        get => showGizmos;
        set => showGizmos = value;
    }

    /// <summary>
    /// Color for this component's gizmos.
    /// </summary>
    public Color GizmosColor
    {
        get => gizmosColor;
        set => gizmosColor = value;
    }
    
    private AgentMover _currentAgent;
    private SteeringOutput _currentSteeringOutput;
    
    private void Awake()
    {
        _currentAgent = GetComponentInParent<AgentMover>();
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (!potentialCollisionDetector.PotentialCollisionDetected) 
            return SteeringOutput.Zero;
        
        Vector2 minimumDistanceRelativePosition;
        // If we're going to collide, or are already colliding, then we do the steering
        // based on the current position.
        if (potentialCollisionDetector.SeparationAtPotentialCollision <= 0
            ||
            potentialCollisionDetector.CurrentDistanceToPotentialCollisionAgent <
            potentialCollisionDetector.CollisionDistance)
        {
            minimumDistanceRelativePosition = potentialCollisionDetector
                .CurrentRelativePositionToPotentialCollisionAgent;
        }
        else
        {
            // If a collision is going to happen in the future, then calculate the 
            // relative position at that moment.
            minimumDistanceRelativePosition = 
                potentialCollisionDetector.RelativePositionAtPotentialCollision;
        }

        // One issue I have with the Millington algorithm is that it multiplies
        // relativePosition with MaximumAcceleration. But I think the right thing to
        // do is to multiply the opposite of relativePosition vector, because that
        // vector goes from agent to its target, so as it is that vector would approach
        // those two agents. To make them farther away, you should take the opposite
        // vector as I'm doing here with -minimumDistanceRelativePosition.
        Vector2 avoidanceVelocity = -minimumDistanceRelativePosition.normalized *
                                    args.MaximumSpeed;
        
        // Here comes another change from the Millington algorithm. The problem with
        // the original algorithm is that it does not seem to take in count the edge case
        // where the two agents are going one against the other directly, in opposite 
        // directions. The rest of the method fixes that.
        
        // One way to find out if the two agents are going one against the other in 
        // opposite directions is to check the dot product between the relative position
        // and the relative velocity. If the absolute value of a dot product is near 1,
        // that means the two agents are going away or approaching, in both cases in
        // opposite directions. In the first case, it wouldn't be collision, so we
        // wouldn't be here because of the guard at the beginning of the method (the one
        // that returns a Zero if no potential collision is detected). So, if the absolute
        // value of dot product is near 1, that means that the two agents are approaching
        // in opposite directions.
        float relativeStartingPosition = 
            Vector2.Dot(
                potentialCollisionDetector.CurrentRelativePositionToPotentialCollisionAgent
                .normalized, 
                potentialCollisionDetector.CurrentRelativeVelocityToPotentialCollisionAgent
                    .normalized);
        if (Mathf.Abs(relativeStartingPosition) >= TooAlignedFactor)
        {
            // If relative velocity is too aligned with relative position, then it means
            // we can end in a direct hit, so we try an avoidance vector that is
            // perpendicular to the collision agent's velocity.
            Vector2 neededVelocity = 
                Vector2.Perpendicular(potentialCollisionDetector.PotentialCollisionAgent.Velocity.normalized) * 
                (args.MaximumSpeed * (Random.Range(0,2) * 2 - 1)); 
            avoidanceVelocity = neededVelocity - _currentAgent.Velocity;
        }
        
        _currentSteeringOutput = new SteeringOutput(
            linear: avoidanceVelocity,
            angular: 0);
        
        return _currentSteeringOutput;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (potentialCollisionDetector.PotentialCollisionDetected)
        {
            Vector2 currentAgentCollisionPosition =
                potentialCollisionDetector.TimeToPotentialCollision *
                _currentAgent.Velocity + 
                (Vector2) _currentAgent.transform.position;
            Vector2 otherAgentCollisionPosition =
                potentialCollisionDetector.TimeToPotentialCollision *
                potentialCollisionDetector.PotentialCollisionAgent.Velocity +
                (Vector2) potentialCollisionDetector.PotentialCollisionAgent.transform.position;
            
            // Draw positions for potential collision.
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentAgentCollisionPosition);
            Gizmos.DrawWireSphere(currentAgentCollisionPosition, 0.1f);
            Gizmos.DrawLine(
                potentialCollisionDetector.PotentialCollisionAgent.transform.position,
                otherAgentCollisionPosition);
            Gizmos.DrawWireSphere(otherAgentCollisionPosition, 0.1f);
            
            // Draw current collision agent velocity.
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                potentialCollisionDetector.PotentialCollisionAgent.transform.position, 
                potentialCollisionDetector.PotentialCollisionAgent.transform.position + 
                (Vector3) potentialCollisionDetector.PotentialCollisionAgent.Velocity);
        }

        if (_currentAgent == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, 
            (Vector2) transform.position + _currentAgent.Velocity);
    }
#endif
}
}

