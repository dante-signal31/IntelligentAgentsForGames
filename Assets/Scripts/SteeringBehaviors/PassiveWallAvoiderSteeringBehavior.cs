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
    
    private enum RelativeOrientation
    {
        Left, 
        Front, 
        Right
    }
    
    [Header("CONFIGURATION:")]
    [Tooltip("Minimum distance to try to keep from obstacles.")]
    [SerializeField] private float avoidDistance = 0.6f;
    [Tooltip("Maximum longitudinal deviation between avoid vector and heading " +
             "before interpreting we are going perpendicular against obstacle")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float longitudinalTolerance = 0.1f;
    
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
    
    /// <summary>
    /// Determines the relative orientation of a given position with respect to the
    /// current agent.
    /// </summary>
    /// <param name="position">The position to evaluate relative to the agent.</param>
    /// <returns>A value of <see cref="RelativeOrientation"/> indicating whether the
    /// position is to the left, right, or in front of the agent.</returns>
    private RelativeOrientation GetRelativeOrientation(Vector2 position)
    {
        Vector2 relativePosition = position - (Vector2) _agentMover.transform.position;
        float crossProduct = Vector3.Cross(
            _agentMover.Forward, 
            relativePosition.normalized).z;
        if (Mathf.Abs(crossProduct) < longitudinalTolerance)
            return RelativeOrientation.Front;
        if (crossProduct < 0) return RelativeOrientation.Right;
        return RelativeOrientation.Left;
    }
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        // If no obstacle detected, then this behavior has nothing to say.
        if (!_obstacleDetected)
        {
            _currentSteering = new SteeringOutput(linear: Vector2.zero, angular: 0);
            return _currentSteering;
        }
        
        // Millington algorithm: head to a point way from the obstacle. This point is
        // defined by _avoidVector, using aas base the closest point detected by the
        // sensor.
        _avoidVector = GetAvoidVector(args);
        Vector2 recommendedTargetToAvoidObstacle = _closestHit.point + _avoidVector;
        Vector2 vectorToGetRecommendedTarget =
            (recommendedTargetToAvoidObstacle - (Vector2) _agentMover.transform.position)
            .normalized;
        
        _currentSteering = new SteeringOutput(
            linear: vectorToGetRecommendedTarget * args.MaximumSpeed,
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
            
            // Calculated how much the detecting sensor penetrated into the detected
            // obstacle.
            float overShootFactor = GetOverShootFactor(
                closestHitData.detectionSensorIndex, 
                closestHitData.distance);

            // Millington calculates avoidVector this way, but It is 
            // troublesome when the avoid-vector is longitudinal to the
            // current heading, because then avoidVector will stop the agent not make it
            // evade the wall. So, an additional lateral push must be added to the
            // avoidVector. That lateral push is added later in this method.
            avoidVector = _closestHit.normal * 
                          (args.MaximumSpeed * overShootFactor + avoidDistance);
            
            Vector2 normalizedInverseAvoidVector = -avoidVector.normalized;
            Vector2 normalizedHeading = args.CurrentVelocity == Vector2.zero? 
                _agentMover.Forward: 
                args.CurrentVelocity.normalized;
            // How much the avoidVector is longitudinal to the current heading? The
            // greater longitudinalDisplacement, the bigger the difference between
            // avoidVector and currentVelocity.
            float longitudinalDisplacement = 1 -
                                             Vector2.Dot(
                                                 normalizedInverseAvoidVector,
                                                 normalizedHeading);

            // If avoidVector and CurrentVelocity are too aligned (avoidVector is
            // longitudinal to current heading), then we must add
            // a lateral push to avoid the obstacle.
            // avoidVector and currentVelocity are too aligned in two edge cases:
            // - When the agent approaches to a wall perpendicular to its heading.
            // - When one of the lateral sensors touches the end of a wall that is 
            // perpendicular to the agent heading.
            if (longitudinalDisplacement < longitudinalTolerance)
            {
                // Calculate the right vector relative to our current Forward vector.
                //
                // TIP --------------------------
                // I could have done:
                // Vector2 rightVector = Quaternion.Euler(0, 0, 90f) * _agentMover.Forward;
                // 
                // But when you are rotating exactly 90 degrees is more performant just
                // inverting the components.
                // To rotate counterclockwise:
                // Vector2 leftVector = new Vector2(-_currentAgent.Forward.y, _currentAgent.Forward.x);
                // To rotate clockwise:
                // Vector2 rightVector = new Vector2(_currentAgent.Forward.y, -_currentAgent.Forward.x);
                Vector2 rightVector = new Vector2(
                    _agentMover.Forward.y,
                    -_agentMover.Forward.x);
                
                // The detected obstacle is on the left or right side of the agent?
                switch (GetRelativeOrientation(_closestHit.point))
                {
                    // If obstacle on the left, evade to the right.
                    case RelativeOrientation.Left: 
                        avoidVector = avoidVector.magnitude * rightVector;
                        break;
                    // If obstacle on the right, evade to the left.
                    case RelativeOrientation.Right: 
                        avoidVector = rightVector * (avoidVector.magnitude * -1);
                        break;
                    // If obstacle on the front, evade to the right or left.
                    case RelativeOrientation.Front: 
                        float pushDirection = Random.value < 0.5f ? 1 : -1;
                        avoidVector = rightVector * 
                                      (avoidVector.magnitude * pushDirection);
                        break;
                }
            }
        }
        return avoidVector;
    }
    
    /// <summary>
    /// Calculates the overshoot factor based on the sensor index and the
    /// closest distance.
    /// </summary>
    /// <param name="sensorIndex">The index of the sensor used in the calculation.
    /// </param>
    /// <param name="closestDistance">The distance to the closest object detected
    /// by the sensor.</param>
    /// <returns> A normalized value representing the overshoot factor, ranging
    /// from 0 to 1.</returns>
    private float GetOverShootFactor(int sensorIndex, float closestDistance)
    {
        float sensorLength = whiskersSensor.GetSensorLength(sensorIndex);
        float overShoot = sensorLength - closestDistance;
        float overShootFactor = Mathf.InverseLerp(0, sensorLength, overShoot);
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

