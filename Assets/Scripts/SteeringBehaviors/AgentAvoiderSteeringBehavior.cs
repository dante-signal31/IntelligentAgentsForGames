
using System;
using System.Timers;
using Tools;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Component to offer an agent avoider steering behaviour.</p>
/// <p>Represents a steering behavior where an agent avoids another agents it may
/// collision with in its path.</p>
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
    [SerializeField] private float tooAlignedFactor = 0.95f;
    
    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos = true;

    /// <summary>
    /// Target to go avoiding other agents.
    /// </summary>
    public GameObject Target
    {
        get => target;
        set => target = value;
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

    private ITargeter _targeter;
    private SteeringBehavior _steeringBehavior;
    private PotentialCollisionDetector _potentialCollisionDetector;
    private Timer _avoidanceTimer;
    private bool _waitingForAvoidanceTimeout;
    private AgentMover _currentAgent;
    private SteeringOutput _currentSteeringOutput;
    private AgentColor _agentColor;

    private Color AgentColor => _agentColor.Color;

    private void Awake()
    {
        _currentAgent = GetComponent<AgentMover>();
        _agentColor = GetComponent<AgentColor>();
        _targeter = GetComponent<ITargeter>();
        _steeringBehavior = (SteeringBehavior)_targeter;
        _potentialCollisionDetector = GetComponent<PotentialCollisionDetector>();
    }

    private void Start()
    {
        _avoidanceTimer = new Timer();
        SetUpAvoidanceTimer();
        _targeter.Target = Target;
    }

    private void SetUpAvoidanceTimer()
    {
        if (_avoidanceTimer == null) return;
        _avoidanceTimer.Interval = AvoidanceTimeout * 1000;
        _avoidanceTimer.AutoReset = false;
        _avoidanceTimer.Elapsed += OnAvoidanceTimeout;
    }

    private void StartAvoidanceTimer()
    {
        _avoidanceTimer.Start();
        _waitingForAvoidanceTimeout = true;
    }

    private void OnAvoidanceTimeout(object sender, ElapsedEventArgs e)
    {
        _waitingForAvoidanceTimeout = false;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        throw new System.NotImplementedException();
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos || Target == null) return;

        if (_potentialCollisionDetector.PotentialCollisionDetected)
        {
            Vector2 currentAgentCollisionPosition =
                _potentialCollisionDetector.TimeToPotentialCollision *
                _currentAgent.Velocity + 
                (Vector2) _currentAgent.transform.position;
            Vector2 otherAgentCollisionPosition =
                _potentialCollisionDetector.TimeToPotentialCollision *
                _potentialCollisionDetector.PotentialCollisionAgent.Velocity +
                (Vector2) _potentialCollisionDetector.PotentialCollisionAgent.transform.position;
            
            // Draw positions for potential collision.
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentAgentCollisionPosition);
            Gizmos.DrawWireSphere(currentAgentCollisionPosition, 0.1f);
            Gizmos.DrawLine(
                _potentialCollisionDetector.PotentialCollisionAgent.transform.position,
                otherAgentCollisionPosition);
            Gizmos.DrawWireSphere(otherAgentCollisionPosition, 0.1f);
            
            // Draw current collision agent velocity.
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                _potentialCollisionDetector.PotentialCollisionAgent.transform.position, 
                _potentialCollisionDetector.PotentialCollisionAgent.transform.position + 
                (Vector3) _potentialCollisionDetector.PotentialCollisionAgent.Velocity);
        }

    }
#endif
    
}
}

