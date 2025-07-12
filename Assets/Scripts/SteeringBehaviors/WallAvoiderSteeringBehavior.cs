using System.Timers;
using Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SteeringBehaviors
{
/// <summary>
/// Steering behavior to avoid walls and obstacles.
/// </summary>
[RequireComponent(typeof(SeekSteeringBehavior), typeof(AgentMover))]
public class WallAvoiderSteeringBehavior : SteeringBehavior, IGizmos, ITargeter
{
    /// <summary>
    /// Data about the closest hit.
    /// </summary>
    private struct ClosestHitData
    {
        public float Distance;
        public RaycastHit2D Hit;
        public int DetectionSensorIndex;
    }
    
    [Header("CONFIGURATION:")]
    [Tooltip("Target to go avoiding other agents.")]
    [SerializeField] private GameObject target;
    [Tooltip("Layers to avoid.")] 
    [SerializeField] private LayerMask layerMask;
    [Tooltip("Timeout started, in seconds, after no further collision detected, before " +
             "resuming travel to target.")]
    [SerializeField] private float avoidanceTimeout = 0.5f;

    [Header("DEBUG:")]
    [Tooltip("Show closest hit marker and evasion velocity vector.")]
    [SerializeField] private bool showGizmos;
    [Tooltip("Color for this object markers.")]
    [SerializeField] private Color gizmosColor = Color.red;

    [Header("WIRING:")] 
    [Tooltip("Sensor to detect walls and obstacles.")]
    [SerializeField] private WhiskersSensor whiskersSensor;
    
    /// <summary>
    /// Target to go avoiding other agents.
    /// </summary>
    public GameObject Target
    {
        get => target;
        set
        {
            target = value;
            if (_seekSteeringBehavior == null) return;
            _seekSteeringBehavior.Target = target;
        }
    }
    
    /// <summary>
    /// Layers to avoid.
    /// </summary>
    public LayerMask AvoidLayerMask
    {
        get => layerMask;
        set
        {
            layerMask = value;
            whiskersSensor.SensorsLayerMask = value;
        }
    }
    
    /// <summary>
    /// Show closest hit marker and evasion velocity vector.
    /// </summary>
    public bool ShowGizmos
    {
        get => showGizmos;
        set => showGizmos = value;
    }

    /// <summary>
    /// Color for this object markers.
    /// </summary>
    public Color GizmosColor
    {
        get => gizmosColor;
        set => gizmosColor = value;
    }
    
    private AgentMover _agentMover;
    private SeekSteeringBehavior _seekSteeringBehavior;
    private RaycastHit2D _closestHit;
    private Vector2 _avoidVector;
    private bool _obstacleDetected;
    private float _runningAwayElapsedTime;
    private Timer _avoidanceTimer;
    private bool _waitingForAvoidanceTimeout;
    private SteeringOutput _currentSteering;
    private bool showGizmos1;
    private Color gizmosColor1;

    // // I need this method because of an odd error with automated tests, that
    // // make me call this method at the beggining of the test to make it work.
    // public void RefreshSensors()
    // {
    //     whiskersSensor.UpdateSensors();
    // }
    
    private void Awake()
    {
        _agentMover = GetComponent<AgentMover>();
        _seekSteeringBehavior = GetComponent<SeekSteeringBehavior>();
    }

    private void Start()
    {
        Target = target;
        AvoidLayerMask = layerMask;
        whiskersSensor.SubscribeToColliderDetected(OnColliderDetected);
        whiskersSensor.SubscribeToNoColliderDetected(OnNoColliderDetected);
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
        StopAvoidanceTimer();
        _waitingForAvoidanceTimeout = false;
    }
    
    private void StartAvoidanceTimer()
    {
        // TODO: Reimplement Timer to work the same way as Godot's version.
        _avoidanceTimer.Interval = avoidanceTimeout * 1000;
        _avoidanceTimer.Enabled = true;
    }

    private void StopAvoidanceTimer()
    {
        _avoidanceTimer.Enabled = false;
    }
    
    /// <summary>
    /// Method to bind to whisker's ColliderDetected event.
    /// </summary>
    /// <param name="_"></param>
    private void OnColliderDetected(Collider2D _)
    {
        _obstacleDetected = true;
    }

    /// <summary>
    /// Method to bind to whisker's NoColliderDetected event.
    /// </summary>
    private void OnNoColliderDetected()
    {
        _obstacleDetected = false;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
         if (_waitingForAvoidanceTimeout) return _currentSteering;
        
        SteeringOutput steeringToTargetVelocity = _seekSteeringBehavior.GetSteering(args);
        _avoidVector = Vector2.zero;
        if (_obstacleDetected)
        {
            ClosestHitData closestHitData = GetClosestHit(args);
            _closestHit = closestHitData.Hit;
            
            float overShootFactor = GetOverShootFactor(
                closestHitData.DetectionSensorIndex, 
                closestHitData.Distance);

            // Buckland and Millington calculate avoidVector this way but I is 
            // troublesome when center sensor detect a wall perpendicular to current
            // heading, because then avoidVector will stop the agent not make it
            // evade the wall. So, an additional lateral push must be added to the
            // avoidVector. That lateral push should be at its maximum when detecting
            // sensor is in the center and minimum when detecting sensor is in the
            // right or left side.
            _avoidVector = _closestHit.normal * (args.MaximumSpeed * overShootFactor);
            
            // Calculate relative side of detecting sensor.
            // 0 is left side, 1 is right side, 0.5 is center.
            float indexSide = Mathf.InverseLerp(
                0, 
                whiskersSensor.SensorAmount-1,
                closestHitData.DetectionSensorIndex);
            
            // Positive means the detecting sensor is in the left side of the
            // agent. Negative means the detecting sensor is in the right side of the
            // agent.
            float distanceFromCenterFactor = 0.5f - indexSide;
            
            // Minimum when near 1 (so, when detection is near left or right side) and
            // maximum when near 0 (so, when detection is near center).
            float pushFactor = Mathf.InverseLerp(
                1, 
                0, 
                Mathf.Abs(distanceFromCenterFactor));

            float pushDirection;
            if (Mathf.Approximately(indexSide, 0.5f))
            {
                // Random direction when sensor detects in the center
                pushDirection = Random.value < 0.5f ? 1 : -1;
            } 
            else if (indexSide > 0.5)
            {
                // Push to the left when sensor detects obstacle in the right side.
                pushDirection = -1;
            } 
            else
            {
                // Push to the right when sensor detects obstacle in the left side.
                pushDirection = 1;
            }
            
            // Calculate right vector relative to our current Forward vector.
            //
            // TIP --------------------------
            // I could have done:
            // Vector2 rightVector = (Vector2)(Quaternion.Euler(0, 0, -90) * _agentMover.Forward);
            // 
            // But when you are rotating exactly 90 degrees is more performant just
            // inverting the components.
            // To rotate clockwise (in Unity):
            // Vector2 rightVector = new Vector2(_agentMover.Forward.y, -_agentMover.Forward.x);
            // To rotate counterclockwise:
            // Vector2 rightVector = new Vector2(-_agentMover.Forward.y, _agentMover.Forward.x);
            Vector2 rightVector = new Vector2(
                _agentMover.Forward.y, 
                -_agentMover.Forward.x);

            
            // Calculate the push vector.
            Vector2 pushVector = rightVector * 
                                 (pushDirection * pushFactor * args.MaximumSpeed);
            
            // Add the push vector to the avoidVector.
            _avoidVector += pushVector;
            
            // Start avoid timer to avoid jittering.
            StartAvoidanceTimer();
            _waitingForAvoidanceTimeout = true;
        }

        _currentSteering = new SteeringOutput(
            linear: steeringToTargetVelocity.Linear + _avoidVector,
            angular: steeringToTargetVelocity.Angular);
        return _currentSteering;
    }

    /// <summary>
    /// Calculates the overshoot factor based on the sensor index and the
    /// closest distance.
    /// </summary>
    /// <param name="sensorIndex">The index of the sensor used in the calculation.</param>
    /// <param name="closestDistance">The distance to the closest object detected
    /// by the sensor.</param>
    /// <returns> A normalized value representing the overshoot factor, ranging
    /// from 0 to 1.</returns>
    private float GetOverShootFactor(int sensorIndex, float closestDistance)
    {
        float sensorLength = whiskersSensor.GetSensorLength(sensorIndex);
        float normalizedShootPoint = Mathf.InverseLerp(0, sensorLength, closestDistance);
        float overShootFactor = 1 - normalizedShootPoint;
        return overShootFactor;
    }

    /// <summary>
    /// Get the closest hit data.
    /// </summary>
    /// <param name="args">SteeringBehaviorArgs arguments passed to this class
    /// <see cref="GetSteering"/> method.</param>
    /// <returns>Hit data.</returns>
    private ClosestHitData GetClosestHit(SteeringBehaviorArgs args)
    {
        ClosestHitData closestHit = new();
        closestHit.Distance = float.MaxValue;
        closestHit.DetectionSensorIndex = -1;

        foreach ((RaycastHit2D hit, int index) in whiskersSensor.DetectedHits)
        {
            float hitDistance = Vector2.Distance(hit.point, args.Position);
            if (hitDistance < closestHit.Distance)
            {
                closestHit.Distance = hitDistance;
                closestHit.Hit = hit;
                closestHit.DetectionSensorIndex= index;
            }
        }

        return closestHit;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos || !whiskersSensor.IsAnyColliderDetected) return;
        Gizmos.color = gizmosColor;
        Gizmos.DrawWireSphere(_closestHit.point, 0.2f);
        Gizmos.DrawLine(_closestHit.point, _closestHit.point + _avoidVector);
    }
#endif

}
}
