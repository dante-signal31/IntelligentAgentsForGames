
using System.Timers;
using PropertyAttribute;
using UnityEngine;

namespace SteeringBehaviors
{
public class ActiveWallAvoiderSteeringBehavior : SteeringBehavior, ITargeter, IGizmos
{
    
    [Header("CONFIGURATION:")]
    [Tooltip("Target to go avoiding other agents.")]
    [SerializeField] private GameObject target;
    [Tooltip("Timeout started, in seconds, after no further collision detected, before " +
             "resuming travel to target.")]
    [SerializeField] private float avoidanceTimeout = 0.5f;
    
    [Header("WIRING:")] 
    [Tooltip("Steering to get to the target. Must comply with ITargeter interface.")]
    [InterfaceCompliant(typeof(ITargeter))]
    [SerializeField] private SteeringBehavior steeringBehavior;
    [Tooltip("Steering to avoid obstacles.")]
    [SerializeField] 
    private PassiveWallAvoiderSteeringBehavior passiveWallAvoiderSteeringBehavior;
    
    [Header("DEBUG:")]
    [Tooltip("Show closest hit marker and evasion velocity vector.")]
    [SerializeField] private bool showGizmos;
    [Tooltip("Color for this object markers.")]
    [SerializeField] private Color gizmosColor = Color.red;
    
    public GameObject Target
    {
        get => target;
        set {
            if (target == value) return;
            target = value;
            if (_targeter == null) return;
            _targeter.Target = value;
        }
    }

    public bool ShowGizmos
    {
        get => showGizmos;
        set => showGizmos = value;
    }

    public Color GizmosColor
    {
        get => gizmosColor;
        set => gizmosColor = value;
    }
    
    private ITargeter _targeter;
    private Vector2 _avoidVector;
    private Vector2 _toTargetVector;
    private Timer _avoidanceTimer;
    private bool _waitingForAvoidanceTimeout;
    private SteeringOutput _currentSteering;
    private SteeringOutput toTargetSteering;
    private SteeringOutput avoidingSteering;
    
    private void Start()
    {
        _targeter = (ITargeter) steeringBehavior;
        _targeter.Target = target;
        SetTimer();
    }
    
    /// <summary>
    /// <p> Set up the timer for running away from an obstacle. </p>
    /// <p> While the timer is on, the object will run away from an obstacle, so will
    /// keep its evasion vector. This is useful to avoid jittering when avoiding small
    /// obstacles.</p>
    /// </summary>
    private void SetTimer()
    {
        _avoidanceTimer = new Timer(avoidanceTimeout * 1000);
        _avoidanceTimer.AutoReset = false;
        _avoidanceTimer.Elapsed += OnTimerTimeout;
    }
    
    private void OnTimerTimeout(object sender, ElapsedEventArgs e)
    {
        _waitingForAvoidanceTimeout = false;
    }
    
    private void StartAvoidanceTimer()
    {
        _waitingForAvoidanceTimeout = true;
        _avoidanceTimer.Stop();
        _avoidanceTimer.Start();
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        
        
        // Get steering to target.
        toTargetSteering = steeringBehavior.GetSteering(args);

        // Get avoid vector.
        avoidingSteering = passiveWallAvoiderSteeringBehavior.GetSteering(args);
        
        if (_waitingForAvoidanceTimeout) return _currentSteering;
        
        // No need to avoid anything? Then just go to the target.
        if (avoidingSteering == SteeringOutput.zero)
        {
            _currentSteering = toTargetSteering;
        }
        // If we need to avoid an obstacle, then return the avoiding steering.
        else
        {
            // Start timer to give time to the agent to avoid the obstacle.
            StartAvoidanceTimer();
            _currentSteering = avoidingSteering;
        }
        
        return _currentSteering;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        if (_currentSteering == null) return;
        
        if (avoidingSteering.IsLinearSet)
        {
            Gizmos.color = gizmosColor;
            Gizmos.DrawLine(
                transform.position, 
                (Vector2) transform.position + avoidingSteering.Linear);
        }

        if (toTargetSteering.IsLinearSet)
        {
            Gizmos.color = Color.beige;
            Gizmos.DrawLine(
                transform.position, 
                (Vector2) transform.position + toTargetSteering.Linear);
        }

        if (_currentSteering.IsLinearSet)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                transform.position, 
                (Vector2) transform.position + _currentSteering.Linear);
        }
    }
#endif
}
}

