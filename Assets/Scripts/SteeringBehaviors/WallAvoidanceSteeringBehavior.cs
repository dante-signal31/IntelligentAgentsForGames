using System;
using UnityEngine;

/// <summary>
/// Steering behavior to avoid walls ans obstacles.
/// </summary>
public class WallAvoidanceSteeringBehavior : SteeringBehavior
{
    [Header("WIRING:")]
    [Tooltip("Sensor to detect walls and obstacles.")]
    [SerializeField] private Whiskers whiskers;
    [Tooltip("Layers to avoid.")] 
    [SerializeField] private LayerMask layerMask;
    
    [Header("DEBUG:")]
    [Tooltip("Show closest hit marker and evasion velocity vector.")]
    [SerializeField] private bool markersVisible = false;
    [Tooltip("Color fot this object markers.")]
    [SerializeField] private Color markerColor = Color.red;

    private RaycastHit2D _closestHit;
    private Vector2 _avoidVector;
    private bool _obstacleDetected;

    /// <summary>
    /// Layers to avoid.
    /// </summary>
    public LayerMask AvoidLayerMask
    {
        get => layerMask;
        set
        {
            layerMask = value;
            whiskers.SensorsLayerMask = value;
        }
    }

    private void Start()
    {
        AvoidLayerMask = layerMask;
    }

    private void OnEnable()
    {
        // OnEnable runs before Start, so first time OnEnable is called, sensors are not
        // initialized. That's why I call to SubscribeToSensorsEvents in Start. Nevertheless
        // I call it here too just in case object is disabled and then enabled again.
        whiskers.SubscribeToColliderDetected(OnColliderDetected);
        whiskers.SubscribeToNoColliderDetected(OnNoColliderDetected);
    }

    private void OnDisable()
    {
        whiskers.UnsubscribeFromColliderDetected(OnColliderDetected);
        whiskers.UnsubscribeFromNoColliderDetected(OnNoColliderDetected);
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

            _avoidVector = closestHit.normal * (args.MaximumSpeed * overShootFactor);
            return new SteeringOutput(_avoidVector, 0);
        }
        else
        {
            return new SteeringOutput(Vector2.zero, 0);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sensorIndexClosestDetectedHit"></param>
    /// <param name="closestDistance"></param>
    /// <returns></returns>
    private float GetOverShootFactor(int sensorIndexClosestDetectedHit, float closestDistance)
    {
        float sensorLength = whiskers.GetSensorLength(sensorIndexClosestDetectedHit);
        float overShoot = sensorLength - closestDistance;
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
    private (float closestDistance, RaycastHit2D closestHit, int sensorIndexClosestDetectedHit)
        GetClosestHitData(SteeringBehaviorArgs args)
    {
        
        float closestDistance = float.MaxValue;
        RaycastHit2D closestHit = new RaycastHit2D();
        int sensorIndexClosestDetectedHit = 0;

        Vector2 currentAgentPosition = args.CurrentAgent.transform.position;

        foreach ((RaycastHit2D hit, int sensorIndex) in whiskers.DetectedHits)
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
