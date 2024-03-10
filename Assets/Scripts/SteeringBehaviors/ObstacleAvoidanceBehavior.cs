using UnityEngine;

[RequireComponent(typeof(AgentMover))]
[ExecuteAlways]
public class ObstacleAvoidanceBehavior : SteeringBehavior
{
    [Header("WIRING:")] 
    [Tooltip("The agent mover component.")]
    [SerializeField] private AgentMover agentMover;
    [Tooltip("This agent forward volumetric sensor.")]
    [SerializeField] private GameObject sensor;

    [Header("CONFIGURATION:")] 
    [Tooltip("Minimum detection box length.")] 
    [SerializeField] private float minDetectionBoxLength = 2.0f;
    [Tooltip("Layers to avoid.")] 
    [SerializeField] private LayerMask avoidLayers;
    
    private BoxRangeManager _sensorRangeManager;
    private VolumetricSensor _volumetricSensor;

    private Color _agentColor;
    
    private Vector2? _closestContactPoint;
    private float _closestContactPointDistance;
    
    public Vector2 ClosestContactPoint => (Vector2) _closestContactPoint;
    
    private void Awake()
    {
        _sensorRangeManager = sensor.GetComponentInChildren<BoxRangeManager>();
        _volumetricSensor = sensor.GetComponentInChildren<VolumetricSensor>();
        _agentColor = GetComponent<AgentColor>().Color;
    }

    private void UpdateSensorLength()
    {
        // The detection box length is proportional to the current agent speed.
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
        UpdateClosestContactPointIfNeeded(other);
    }

    /// <summary>
    /// Check if this contact point is closer than the current closest contact point and
    /// if that happens update the closest contact point.
    /// </summary>
    private void UpdateClosestContactPointIfNeeded(GameObject other)
    {
        Debug.Log("Collision stayed: " + other.name);
        if (LayerInLayerMask(other.layer, avoidLayers))
        {
            Vector2 contactPoint = _volumetricSensor.SensorCollider.ClosestPoint(other.transform.position);
            float distanceToContactPoint = Vector2.Distance(transform.position, contactPoint);
            if (_closestContactPoint == null || distanceToContactPoint < _closestContactPointDistance)
            {
                _closestContactPoint = contactPoint;
                _closestContactPointDistance = distanceToContactPoint;
            } 
        }
    }

    public void OnObjectStayedSensor(GameObject other)
    {
        UpdateClosestContactPointIfNeeded(other);
    }
    
    public void OnObjectExitedSensor(GameObject other)
    {
        if (LayerInLayerMask(other.layer, avoidLayers))
        {
            Debug.Log("Object exited: " + other.name);
            Vector2 contactPoint = _volumetricSensor.SensorCollider.ClosestPoint(other.transform.position);
            // TODO: This is not working properly, because positions are almost the same but not exactly the same. I must implement an epsilon comparison.
            if (contactPoint == ClosestContactPoint)
            {
                Debug.Log("Object exited was the closest: " + other.name);
                _closestContactPoint = null;
                _closestContactPointDistance = float.MaxValue;
            }
        }
    }

    /// <summary>
    /// Update the distance to corrent closest contact point after performing last movements.
    /// </summary>
    private void UpdateDistanceToClosestContactPoint()
    {
        if (_closestContactPoint == null) return;
        float distanceToContactPoint = Vector2.Distance(transform.position, ClosestContactPoint);
        _closestContactPointDistance = distanceToContactPoint;
    }
    
    private void FixedUpdate()
    {
        UpdateDistanceToClosestContactPoint();
        UpdateSensorLength();
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        throw new System.NotImplementedException();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _agentColor;
        if (_closestContactPoint != null) Gizmos.DrawSphere(ClosestContactPoint, 0.2f);
    }
}
