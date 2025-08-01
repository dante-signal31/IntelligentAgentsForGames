using System.Timers;
using UnityEngine;

namespace SteeringBehaviors
{
public class ActiveAgentAvoiderSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("Timeout started after no further collision detected, before resuming " +
             "travel to target.")]
    [SerializeField] private float avoidanceTimeout = 1.0f;

    [Header("WIRING:")]
    [Tooltip("Steering to go to target.")]
    [SerializeField] private SteeringBehavior steeringToTarget;
    [Tooltip("Steering to avoid other agents.")]
    [SerializeField] 
    private PassiveAgentAvoiderSteeringBehavior passiveAgentAvoiderSteering;
    
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
    
    private System.Timers.Timer _avoidanceTimer;
    private bool _waitingForAvoidanceTimeout;
    private SteeringOutput _currentSteeringOutput;

    private void Start()
    {
        _avoidanceTimer = new Timer();
        SetUpAvoidanceTimer();
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
        _avoidanceTimer.Stop();
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
        SteeringOutput steeringToTargetVelocity = steeringToTarget.GetSteering(args);
        SteeringOutput avoidingSteeringVelocity = 
            passiveAgentAvoiderSteering.GetSteering(args);
        
        // Nothing to avoid, but we are waiting for avoidance timeout, so let's
        // continue our current velocity.
        if (avoidingSteeringVelocity.Equals(SteeringOutput.Zero) &&
            _waitingForAvoidanceTimeout)
            return _currentSteeringOutput;
        
        // Nothing to avoid and waiting nothing, so let's just go to our target.
        if (avoidingSteeringVelocity.Equals(SteeringOutput.Zero)) 
            return steeringToTargetVelocity;
        
        // If we get here, then there's an agent to avoid. Add avoiding vector to our
        // velocity to avoid collision. 
        Vector2 newVelocity = steeringToTargetVelocity.Linear + 
                              avoidingSteeringVelocity.Linear;
        _currentSteeringOutput = new SteeringOutput(
            newVelocity, 
            steeringToTargetVelocity.Angular);
        
        // We need a cooldown or we can get stuck in a cycle where our agent changes 
        // its heading to avoid collision but, in the next frame, as its heading is
        // different, then algorithm can conclude there is no longer a collision risk
        // so avoiding vector is discarded... and, in the next frame, agent is looking
        // in the same direction as originally and collision risk returns restarting
        // the cycle.
        StartAvoidanceTimer();
        
        return _currentSteeringOutput;
    }
}
}

