using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(AgentMover))]
[ExecuteAlways]
public class ObstacleAvoidanceBehavior : SteeringBehavior
{
    [Header("WIRING:")] 
    [Tooltip("The agent mover component.")]
    [SerializeField] private AgentMover agentMover;
    [Tooltip("This agent forward volumetric sensor.")]
    [SerializeField] private GameObject volumetricSensor;
    [Tooltip("This agent forward ray sensor.")] 
    [SerializeField] private RaySensor raySensor;

    [Header("CONFIGURATION:")] 
    [Tooltip("Minimum detection box length.")] 
    [SerializeField] private float minDetectionBoxLength = 2.0f;
    [Tooltip("Layers to avoid.")] 
    [SerializeField] private LayerMask avoidLayers;
    [Tooltip("Point separated by this distance or less will be detected as the same points.")]
    [SerializeField] private float detectionResolution = 0.10f;
    
    private BoxRangeManager _sensorRangeManager;
    private VolumetricSensor _volumetricSensor;

    private Color _agentColor;

    private GameObject _closestObstacle;
    private Vector2? _closestContactPoint;
    private Vector2? _closestHitPoint;
    private float _closestHitPointDistance;
    private RaycastHit2D? _closestObstaclePointHit;
    
    public Vector2 ClosestHitPoint => (Vector2) _closestHitPoint;
    
    private void Awake()
    {
        _sensorRangeManager = volumetricSensor.GetComponentInChildren<BoxRangeManager>();
        _volumetricSensor = volumetricSensor.GetComponentInChildren<VolumetricSensor>();
        _agentColor = GetComponent<AgentColor>().Color;
        raySensor.SetLayerMask(avoidLayers);
    }

    private void UpdateSensorLength()
    {
        // The detection box length is proportional to the current agent speed.
        if (agentMover.MaximumSpeed == 0.0f) return;
        float newSensorLength = minDetectionBoxLength + 
                                (agentMover.CurrentSpeed/agentMover.MaximumSpeed) * minDetectionBoxLength;
        _sensorRangeManager.Range = newSensorLength;
    }
    
    /// <summary>
    /// Whether given layer is included in those in layer mask.
    /// </summary>
    /// <param name="layer">Layer to check.</param>
    /// <param name="layerMask">Layer mask to look in.</param>
    /// <returns>True if given layer is included in those in given layer mask. </returns>
    private bool LayerInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask & (1 << layer)) != 0;
    }

    public void OnObjectEnteredSensor(GameObject other)
    { 
        StartCoroutine(UpdateClosestContactPointIfNeeded(other));
    }

    /// <summary>
    /// Check if this contact point is closer than the current closest contact point and
    /// if that happens update the closest contact point.
    /// </summary>
    private IEnumerator UpdateClosestContactPointIfNeeded(GameObject other)
    {
        Debug.Log("Collision stayed: " + other.name);
        if (LayerInLayerMask(other.layer, avoidLayers))
        {
            Vector2 contactPoint = _volumetricSensor.SensorCollider.ClosestPoint(other.transform.position);
            yield return GetObstacleClosestPointHit(contactPoint);
            if (raySensor.IsColliderDetected)
            {
                float distanceToContactPoint = Vector2.Distance(transform.position, 
                    ((RaycastHit2D)_closestObstaclePointHit).point);
                if (_closestHitPoint == null || 
                    distanceToContactPoint < _closestHitPointDistance ||
                    other == _closestObstacle)
                {
                    _closestObstacle = other;
                    _closestHitPoint = ((RaycastHit2D)_closestObstaclePointHit).point;
                    _closestHitPointDistance = distanceToContactPoint;
                    _closestContactPoint = contactPoint;
                } 
            }
            

        }
    }
    
    private IEnumerator GetObstacleClosestPointHit(Vector2 targetPosition)
    {
        // Give ray just a bit of more length to ensure something is hit.
        Vector2 currentPosition = transform.position;
        Vector2 rayToTarget = targetPosition - currentPosition;
        float distanceToTarget = rayToTarget.magnitude;
        rayToTarget = rayToTarget.normalized * (distanceToTarget + 0.3f);
        targetPosition = currentPosition + rayToTarget;
        raySensor.SetRayTarget(targetPosition);
        yield return null;
        _closestObstaclePointHit = raySensor.DetectedHit;
    }
    
    private IEnumerator SkipOneFrame()
    {
        yield return null;
    }

    public void OnObjectStayedSensor(GameObject other)
    {
        StartCoroutine(UpdateClosestContactPointIfNeeded(other));
    }
    
    public void OnObjectExitedSensor(GameObject other)
    {
        if (LayerInLayerMask(other.layer, avoidLayers))
        {
            if (other == _closestObstacle)
            {
                Debug.Log("Object exited was the closest: " + other.name);
                _closestObstacle = null;
                _closestHitPoint = null;
                _closestHitPointDistance = float.MaxValue;
                _closestContactPoint = null;
            }
        }
    }

    /// <summary>
    /// Update the distance to current closest contact point after performing last movements.
    /// </summary>
    private void UpdateDistanceToClosestContactPoint()
    {
        if (_closestHitPoint == null) return;
        float distanceToContactPoint = Vector2.Distance(transform.position, ClosestHitPoint);
        _closestHitPointDistance = distanceToContactPoint;
    }
    
    private void FixedUpdate()
    {
        UpdateSensorLength();
        UpdateDistanceToClosestContactPoint();
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        throw new System.NotImplementedException();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = _agentColor;
        if (_closestContactPoint != null) Gizmos.DrawSphere((Vector2)_closestContactPoint, 0.2f);
        Gizmos.color = Color.blue;
        if (_closestObstaclePointHit != null) 
            Gizmos.DrawSphere(((RaycastHit2D)_closestObstaclePointHit).point, 0.2f);
    }
#endif
}
