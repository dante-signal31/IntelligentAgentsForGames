
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
    private Vector2 _previousAvoidVector;
    private Vector2 _toTargetVector;
    private System.Timers.Timer _avoidanceTimer;
    private bool _waitingForAvoidanceTimeout;
    private SteeringOutput _currentSteering;
    
    private void Start()
    {
        _targeter = (ITargeter) steeringBehavior;
        _targeter.Target = target;
        SetTimer();
    }
    
    /// <summary>
    /// <p> Setup  timer for running away from an obstacle. </p>
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
        _previousAvoidVector = Vector2.zero;
    }
    
    private void StartAvoidanceTimer()
    {
        _waitingForAvoidanceTimeout = true;
        _avoidanceTimer.Stop();
        _avoidanceTimer.Start();
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (_waitingForAvoidanceTimeout) return _currentSteering;

        _avoidVector = Vector2.zero;
        SteeringOutput avoidingSteering =
            passiveWallAvoiderSteeringBehavior.GetSteering(args);
        _avoidVector = avoidingSteering.Linear;
        
        if (_avoidVector != Vector2.zero && _previousAvoidVector == Vector2.zero)
            StartAvoidanceTimer();
        _previousAvoidVector = _avoidVector;
        
        SteeringOutput steeringToTargetVelocity = steeringBehavior.GetSteering(args);
        _toTargetVector = steeringToTargetVelocity.Linear;
        
        _currentSteering = avoidingSteering + steeringToTargetVelocity;
        return _currentSteering;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        if (_currentSteering == null) return;
        Gizmos.color = gizmosColor;
        Gizmos.DrawLine(
            transform.position, 
            (Vector2) transform.position + _avoidVector);
        Gizmos.color = Color.beige;
        Gizmos.DrawLine(
            transform.position, 
            (Vector2) transform.position + _toTargetVector);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(
            transform.position, 
            (Vector2) transform.position + _currentSteering.Linear);
    }
#endif
}
}

