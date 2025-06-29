using Sensors;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// Steering behavior to avoid walls and obstacles.
/// </summary>
public class WallAvoidanceSteeringBehavior : SteeringBehavior
{
    [Header("WIRING:")]
    [Tooltip("Sensor to detect walls and obstacles.")]
    [SerializeField] private WhiskersSensor whiskersSensor;
    [Tooltip("Layers to avoid.")] 
    [SerializeField] private LayerMask layerMask;

    [Header("DEBUG:")]
    [Tooltip("Show closest hit marker and evasion velocity vector.")]
    [SerializeField] private bool markersVisible = false;
    [Tooltip("Color for this object markers.")]
    [SerializeField] private Color markerColor = Color.red;

    private RaycastHit2D _closestHit;
    private Vector2 _avoidVector;
    private bool _obstacleDetected;
    private bool _runningAwayFromObstacle = false;
    private float _runningAwayElapsedTime;

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

    private void Start()
    {
        AvoidLayerMask = layerMask;
    }

    /// <summary>
    /// Start a timer for running away from an obstacle.
    ///
    /// While the timer is on, the object will run away from an obstacle, so will keep its
    /// evasion vector. This is useful to avoid jittering when avoiding small obstacles. 
    /// </summary>
    private void StartRunningAwayTimer()
    { // TODO: Reimplement this timer. Actually, is doing nothing now.
        _runningAwayFromObstacle = true;
        _runningAwayElapsedTime = 0.0f;
    }

    /// <summary>
    /// End running away timer.
    ///
    /// At this time an object will stop running away from obstacle, so will stop its
    /// evasion vector.
    /// </summary>
    private void EndRunningAway()
    {
        _runningAwayFromObstacle = false;
    }

    private void OnEnable()
    {
        // OnEnable runs before Start, so the first time OnEnable is called, sensors are
        // not initialized. That's why I call SubscribeToSensorsEvents in Start.
        // Nevertheless, I call it here too just in case the current object is disabled
        // and then enabled again.
        whiskersSensor.SubscribeToColliderDetected(OnColliderDetected);
        whiskersSensor.SubscribeToNoColliderDetected(OnNoColliderDetected);
    }

    private void OnDisable()
    {
        whiskersSensor.UnsubscribeFromColliderDetected(OnColliderDetected);
        whiskersSensor.UnsubscribeFromNoColliderDetected(OnNoColliderDetected);
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
        if (_obstacleDetected)
        {
            var (closestDistance, 
                closestHit, 
                sensorIndexClosestDetectedHit) = GetClosestHitData(args);
            _closestHit = closestHit;
        
            float overShootFactor = GetOverShootFactor(sensorIndexClosestDetectedHit, 
                closestDistance);

            if (whiskersSensor.IsCenterSensor(sensorIndexClosestDetectedHit))
            {
                // Buckland uses this approach for every sensor, but I only use it for
                // the center sensor to only brake when an obstacle is just in front of
                // us.
                _avoidVector = closestHit.normal * (args.MaximumSpeed * overShootFactor);
            }
            else
            {
                // For every other sensor than front one I only project a normal vector
                // as a lateral push. This way I make the agent rotate without braking
                // when an obstacle is not in front of it.
                float lateralPush = Vector3.Dot(
                    closestHit.normal, 
                    transform.right);
                _avoidVector = transform.right * lateralPush * 
                               args.MaximumSpeed * overShootFactor;
            }
        
            StartRunningAwayTimer();
            return new SteeringOutput(_avoidVector, 0);
        }
        else
        {
            return new SteeringOutput(Vector2.zero, 0);
        }
    }

    /// <summary>
    /// Get how far this ray has penetrated the object surface.
    /// </summary>
    /// <param name="sensorIndexClosestDetectedHit"></param>
    /// <param name="closestDistance"></param>
    /// <returns></returns>
    private float GetOverShootFactor(
        int sensorIndexClosestDetectedHit, 
        float closestDistance)
    {
        float sensorLength = whiskersSensor.GetSensorLength(sensorIndexClosestDetectedHit);
        float closestDistanceFromObjectSurface = closestDistance - whiskersSensor.MinimumRange;
        float overShoot = sensorLength - closestDistanceFromObjectSurface;
        float overShootFactor = Mathf.InverseLerp(0, sensorLength, overShoot);
        return overShootFactor;
    }

    /// <summary>
    /// Get the closest hit data.
    /// </summary>
    /// <param name="args">SteeringBehaviorArgs arguments passed to this class
    /// <see cref="GetSteering"/> method.</param>
    /// <returns>A tuple with the closest distance, the closest hit and
    /// the sensor index which detected the closest hit.</returns>
    private (
        float closestDistance, 
        RaycastHit2D closestHit, 
        int sensorIndexClosestDetectedHit)
        GetClosestHitData(SteeringBehaviorArgs args)
    {
    
        float closestDistance = float.MaxValue;
        RaycastHit2D closestHit = new RaycastHit2D();
        int sensorIndexClosestDetectedHit = 0;

        Vector2 currentAgentPosition = args.CurrentAgent.transform.position;

        foreach ((RaycastHit2D hit, int sensorIndex) in whiskersSensor.DetectedHits)
        {
            float hitDistance = Vector2.Distance(hit.point, currentAgentPosition);

            if (hitDistance < closestDistance)
            {
                closestDistance = hitDistance;
                closestHit = hit;
                sensorIndexClosestDetectedHit = sensorIndex;
            }
        }

        return (closestDistance, closestHit, sensorIndexClosestDetectedHit);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (markersVisible && _obstacleDetected)
        {
            Gizmos.color = markerColor;
            Gizmos.DrawWireSphere(_closestHit.point, 0.1f);
            Gizmos.DrawLine(_closestHit.point, _closestHit.point + _avoidVector);
        }
    }
#endif
}
}
