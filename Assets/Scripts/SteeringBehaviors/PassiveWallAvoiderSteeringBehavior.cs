using Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SteeringBehaviors
{
public class PassiveWallAvoiderSteeringBehavior : SteeringBehavior, IGizmos
{
    /// <summary>
    /// Data about the closest hit.
    /// </summary>
    private struct ClosestHitData
    {
        public float distance;
        public RaycastHit2D hit;
        public int detectionSensorIndex;
    }
    
    [Header("CONFIGURATION:")]
    [Tooltip("Minimum distance to try to keep from obstacles.")]
    [SerializeField] private float avoidDistance = 0.6f;
    [Tooltip("Maximum longitudinal deviation between avoid vector and heading " +
             "before interpreting we are going perpendicular against obstacle")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float longitudinalTolerance = 0.1f;
    [Tooltip("Minimum push when avoiding an obstacle.")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float minimumPushFactor = 0.5f;
    
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
    private bool _calculationCooldownActive;
    
    private void Awake()
    {
        // AgentMover can be in the same game object or in one of its parents, so be must
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
            _closestHit = closestHitData.hit;
            
            float overShootFactor = GetOverShootFactor(
                closestHitData.detectionSensorIndex, 
                closestHitData.distance);

            // Buckland and Millington calculate avoidVector this way, but It is 
            // troublesome when the avoid-vector is longitudinal to the
            // current heading, because then avoidVector will stop the agent not make it
            // evade the wall. So, an additional lateral push must be added to the
            // avoidVector. That lateral push should be at its maximum when the detecting
            // sensor is in the center and minimum when the detecting sensor is on the
            // right or left side.
            avoidVector = _closestHit.normal * 
                          (args.MaximumSpeed * overShootFactor + avoidDistance);
            
            Vector2 normalizedInverseAvoidVector = -avoidVector.normalized;
            
            Vector2 normalizedHeading = args.CurrentVelocity == Vector2.zero? 
                _agentMover.Forward: 
                args.CurrentVelocity.normalized;
            
            float longitudinalDisplacement = 1 -
                                             Vector2.Dot(
                                                 normalizedInverseAvoidVector,
                                                 normalizedHeading);

            // If avoidVector and CurrentVelocity are too aligned, then we must add
            // a lateral push to avoid the obstacle.
            if (longitudinalDisplacement < longitudinalTolerance)
            {
                // Calculate relative side of detecting sensor.
                // 0 is the right side, 1 is the left side, 0.5 is center.
                float indexSide = Mathf.InverseLerp(
                    0,
                    whiskersSensor.SensorAmount - 1,
                    closestHitData.detectionSensorIndex);

                // Positive means the detecting sensor is on the right side of the
                // agent. Negative means the detecting sensor is on the left side of the
                // agent.
                float distanceFromCenterFactor = 0.5f - indexSide;

                // Minimum when near 1 (so, when detection is near the left or right side) and
                // maximum when near 0 (so, when detection is near the center).
                float pushFactor = Mathf.Lerp(
                    1,
                    minimumPushFactor,
                    Mathf.Abs(distanceFromCenterFactor));

                Vector2 pushVector;
                if (Mathf.Approximately(indexSide, 0.5f))
                {
                    // The usual way to decide which way to go when the obstacle is detected
                    // at the center sensor should be this commented code. I've commented it
                    // because I'm using a tilemap for my scene level and each tilemap has
                    // a global collider with a single center at the level center. So I cannot
                    // use the usual way and I have to select a random direction.

                    // Vector3 toColliderCenter = 
                    //     _closestHit.collider.bounds.center - transform.position;
                    // Vector3 toCollisionHit = (Vector3) _closestHit.point - transform.position;
                    // Vector3 cross = Vector3.Cross(toCollisionHit, toColliderCenter);
                    // // In Unity, the cross-product Z value is positive when you go
                    // // counterclockwise from lhs to rhs. So, that value will be positive
                    // // if the collider center is on the left side of the hit point. If the
                    // // collider center is on the left side of the hit point, then push to
                    // // the right. 
                    // pushDirection = cross.z > 0 ? 1 : -1;

                    // Random direction when the sensor detects in the center
                    float pushDirection = Random.value < 0.5f ? 1 : -1;
                    
                    // Calculate the right vector relative to our current Forward vector.
                    //
                    // TIP --------------------------
                    // I could have done:
                    // Vector2 rightVector = Quaternion.Euler(0, 0, 90f) * _agentMover.Forward;
                    // 
                    // But when you are rotating exactly 90 degrees is more performant just
                    // inverting the components.
                    // To rotate counterclockwise:
                    // Vector2 rightVector = new Vector2(-_currentAgent.Forward.y, _currentAgent.Forward.x);
                    // To rotate clockwise:
                    // Vector2 rightVector = new Vector2(_currentAgent.Forward.y, -_currentAgent.Forward.x);
                    Vector2 rightVector = new Vector2(
                        _agentMover.Forward.y,
                        -_agentMover.Forward.x);

                    // Calculate the push vector.
                    pushVector = rightVector *
                                 (pushDirection * pushFactor * args.MaximumSpeed);
                }
                else {
                    // If the wall is laterally detected, then push forward to get
                    // the wall end.
                    pushVector = _agentMover.Forward * (pushFactor * args.MaximumSpeed);
                }
                // Add the push vector to the avoidVector.
                avoidVector += pushVector;
            }
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
        ClosestHitData closestHit = new()
        {
            distance = float.MaxValue,
            detectionSensorIndex = -1
        };

        foreach ((RaycastHit2D hit, int index) in whiskersSensor.DetectedHits)
        {
            float hitDistance = Vector2.Distance(hit.point, args.Position);
            if (hitDistance < closestHit.distance)
            {
                closestHit.distance = hitDistance;
                closestHit.hit = hit;
                closestHit.detectionSensorIndex= index;
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

