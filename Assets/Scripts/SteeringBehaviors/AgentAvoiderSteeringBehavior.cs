
using System;
using System.Timers;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Component to offer an agent avoider steering behavior.</p>
/// <p>Represents a steering behavior where an agent avoids another agent it may
/// collide with in its path.</p>
/// <p>The difference with an obstacle avoidance algorithm is that obstacles don't move
/// while agents do.</p>
/// </summary>
[RequireComponent(typeof(AgentMover)), 
 RequireComponent(typeof(SeekSteeringBehavior)), 
 RequireComponent(typeof(AgentColor))]
public class AgentAvoiderSteeringBehavior : SteeringBehavior, ITargeter
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to go avoiding other agents.")]
    [SerializeField] private GameObject target;
    [Tooltip("Timeout started after no further collision detected, before resuming " +
             "travel to target.")]
    [SerializeField] private float avoidanceTimeout = 1.0f;

    [Tooltip("Threshold factor for determining when to use normal vector avoidance.")]
    [SerializeField]
    private float tooAlignedFactor = 0.95f;
    
    [Header("WIRING:")]
    [SerializeField] private PotentialCollisionDetector potentialCollisionDetector;
    
    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos = true;

    /// <summary>
    /// Target to go avoiding other agents.
    /// </summary>
    public GameObject Target
    {
        get => target;
        set
        {
            if (target == value) return;
            target = value;
            
            if (_seekSteeringBehavior == null) return;
            _seekSteeringBehavior.Target = value;
        }
    }

    /// <summary>
    /// Timeout duration, in seconds, that starts after no further collisions are
    /// detected before resuming travel toward the target.
    /// </summary>
    public float AvoidanceTimeout
    {
        get => avoidanceTimeout;
        set
        {
            avoidanceTimeout = value;
            if (_avoidanceTimer == null) return;
            _avoidanceTimer.Interval = AvoidanceTimeout * 1000;
        }
    }
    
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
    
    private SeekSteeringBehavior _seekSteeringBehavior;
    private bool _waitingForAvoidanceTimeout;
    private AgentMover _currentAgent;
    private SteeringOutput _currentSteeringOutput;
    private AgentColor _agentColor;
    
    private Timer _avoidanceTimer;

    private Color AgentColor => _agentColor.Color;

    private void Awake()
    {
        _currentAgent = GetComponent<AgentMover>();
        _agentColor = GetComponent<AgentColor>();
        _seekSteeringBehavior = GetComponent<SeekSteeringBehavior>();
    }

    private void Start()
    {
        _avoidanceTimer = new Timer();
        SetUpAvoidanceTimer();
        _seekSteeringBehavior.Target = Target;
    }

    /// <summary>
    /// If we head to the main target as soon a collision forecast dissapears, we can end
    /// with jittering. That's because when we head again to the main target we can put
    /// ourselves in the same collision path we were just an instant ago. To avoid that
    /// we let our avoidance maneuver act for a moment before heading again towards our
    /// main target. This avoidance timer defines how long that moment takes.
    /// </summary>
    private void SetUpAvoidanceTimer()
    {
        if (_avoidanceTimer == null) return;
        _avoidanceTimer.Interval = AvoidanceTimeout * 1000;
        _avoidanceTimer.AutoReset = false;
        _avoidanceTimer.Elapsed += OnAvoidanceTimeout;
    }

    /// <summary>
    /// Start avoidance timer from zero.
    /// </summary>
    private void StartAvoidanceTimer()
    {
        _avoidanceTimer.Start();
        _waitingForAvoidanceTimeout = true;
    }

    /// <summary>
    /// Event handler when the avoidance timer emits its timeout event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnAvoidanceTimeout(object sender, ElapsedEventArgs e)
    {
        _waitingForAvoidanceTimeout = false;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        // If no potential collision detected, and we are not yet avoiding another
        // agent, then head to the target.
        SteeringOutput steeringToTargetVelocity = _seekSteeringBehavior.GetSteering(args);
        if (!potentialCollisionDetector.PotentialCollisionDetected &&
            !_waitingForAvoidanceTimeout)
            return steeringToTargetVelocity;
        
        // If we are already avoiding another agent, we need to keep avoidance maneuver
        // until timer times out.
        if (!potentialCollisionDetector.PotentialCollisionDetected)
            return _currentSteeringOutput;
        
        // If we get here, it means we have detected an agent we can collide with if
        // the current heading is not changed.
        // So, if we're going to collide, or are already colliding, then we do the
        // steering based on the current position.
        Vector2 minimumDistanceRelativePosition;
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
        
        // Another issue I have with the Millington algorithm is that it multiplies
        // relativePosition with MaximumAcceleration. But I think the right thing to
        // do is to multiply the opposite of relativePosition vector, because that
        // vector goes from agent to its target, so as it is that vector would approach
        // those two agents. To make them farther away, you should take the opposite
        // vector as I'm doing here with -minimumDistanceRelativePosition.
        Vector2 avoidanceVelocity = -minimumDistanceRelativePosition.normalized *
                                    args.MaximumSpeed;
        
        // Compose our avoidance maneuver mixin our current velocity with the one
        // needed to avoid collision.
        Vector2 newVelocity = steeringToTargetVelocity.Linear + avoidanceVelocity;
        
        // This is another change from Millington algorithm.
        float relativeAvoidance = Vector2.Dot(potentialCollisionDetector
                .CurrentRelativePositionToPotentialCollisionAgent
                .normalized,
            potentialCollisionDetector.CurrentRelativeVelocityToPotentialCollisionAgent
                .normalized);

        if (Mathf.Abs(relativeAvoidance) >= TooAlignedFactor)
        {
            // If relative velocity is too aligned with relative position, then our
            // avoidance vector can end in a direct hit or a chase, so we try an
            // avoidance vector that is perpendicular to the collision agent's velocity.
            newVelocity = 
                Vector2.Perpendicular(
                        potentialCollisionDetector.PotentialCollisionAgent.Velocity)
                    .normalized * (newVelocity.magnitude * (Random.Range(0,2) * 2 - 1)); 
        } 
        else
        {
            // It's harder to evade collision agent if we end going along the same
            // direction. So, we want to use a resulting vector pointing in the opposite
            // direction than the velocity of the collision agent. This way we will
            // avoid it passing it across its tail.
            int sign = 
                Vector2.Dot(
                    potentialCollisionDetector.PotentialCollisionAgent.Velocity, 
                    newVelocity) > 0
                    ? -1
                    : 1;
            newVelocity *= sign;
        }
        
        _currentSteeringOutput = new SteeringOutput(
            newVelocity, 
            steeringToTargetVelocity.Angular);
        StartAvoidanceTimer();
        return _currentSteeringOutput;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos || Target == null) return;

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

