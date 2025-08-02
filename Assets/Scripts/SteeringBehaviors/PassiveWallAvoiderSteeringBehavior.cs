
using Sensors;
using UnityEngine;

namespace SteeringBehaviors
{
public class PassiveWallAvoiderSteeringBehavior : SteeringBehavior, IGizmos
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
    
    [Header("WIRING:")] 
    [Tooltip("Sensor to detect walls and obstacles.")]
    [SerializeField] private WhiskersSensor whiskersSensor;
    
    [Header("DEBUG:")]
    [Tooltip("Show closest hit marker and evasion velocity vector.")]
    [SerializeField] private bool showGizmos;
    [Tooltip("Color for this object markers.")]
    [SerializeField] private Color gizmosColor = Color.red;
    
    public bool ShowGizmos
    {
        get => showGizmos;
        set {
            showGizmos = value;
            if (whiskersSensor == null) return;
            whiskersSensor.ShowGizmos = value;
        }
    }

    public Color GizmosColor
    {
        get => gizmosColor;
        set => gizmosColor = value;
    }
    
    private bool _obstacleDetected;
    private RaycastHit2D _closestHit;
    private Vector2 _avoidVector;
    private AgentMover _agentMover;
    private SteeringOutput _currentSteering;
    
    private void Awake()
    {
        // AgentMover can be in the same gameobject or in one of its parents, so be must
        // search there.
        _agentMover = GetComponentInParent<AgentMover>();
    }
    
    private void Start()
    {
        whiskersSensor.SubscribeToColliderDetected(OnColliderDetected);
        whiskersSensor.SubscribeToNoColliderDetected(OnNoColliderDetected);
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
        _avoidVector = GetAvoidVector(args);
        
        _currentSteering = new SteeringOutput(
            linear: _avoidVector,
            angular: 0);
        
        return _currentSteering;
    }

    private Vector2 GetAvoidVector(SteeringBehaviorArgs args)
    {
        Vector2 avoidVector = Vector2.zero;
        
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
            avoidVector = _closestHit.normal * (args.MaximumSpeed * overShootFactor);
            
            // Calculate relative side of detecting sensor.
            // 0 is right side, 1 is left side, 0.5 is center.
            float indexSide = Mathf.InverseLerp(
                0, 
                whiskersSensor.SensorAmount-1,
                closestHitData.DetectionSensorIndex);
            
            // Positive means the detecting sensor is in the right side of the
            // agent. Negative means the detecting sensor is in the left side of the
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
                // Push to the right when sensor detects obstacle in the left side.
                pushDirection = 1;
            } 
            else
            {
                // Push to the left when sensor detects obstacle in the right side.
                pushDirection = -1;
            }
            
            // Calculate right vector relative to our current Forward vector.
            //
            // TIP --------------------------
            // I could have done:
            // Vector2 rightVector = _currentAgent.Forward.Rotated(Mathf.Pi / 2).Normalized();
            // 
            // But when you are rotating exactly 90 degrees is more performant just
            // inverting the components.
            // To rotate clockwise (In Godot):
            // Vector2 rightVector = new Vector2(-_currentAgent.Forward.y, _currentAgent.Forward.x);
            // To rotate counterclockwise:
            // Vector2 rightVector = new Vector2(_currentAgent.Forward.y, -_currentAgent.Forward.x);
            Vector2 rightVector = new Vector2(
                -_agentMover.Forward.y, 
                _agentMover.Forward.x);
            
            // Calculate the push vector.
            Vector2 pushVector = rightVector * 
                                 (pushDirection * pushFactor * args.MaximumSpeed);
            
            // Add the push vector to the avoidVector.
            avoidVector += pushVector;
        }
        
        return avoidVector;
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

