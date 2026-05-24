using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Sensors
{
/// <summary>
/// Component to implement a cone of vision sensor.
/// </summary>
public class ConeSensor : MonoBehaviour, ISensor
{
    [Header("CONFIGURATION:")]
    [Tooltip("Range to detect possible collisions.")]
    [SerializeField] private float detectionRange = 10f;
    [Tooltip("Semicone angle for detection (in degrees).")]
    [Range(0f, 90f)]
    [SerializeField] private float detectionSemiconeAngle = 45f;
    [Tooltip("Specifies the physics layers that the sensor will monitor for objects")]
    [SerializeField] private LayerMask layersToDetect;
    [Tooltip("Whether to check line of sight for objects in the detection area. If true," +
             "and an object in range is behind an obstacle, then it will be marked as " +
             "not detected.")]
    [SerializeField] private bool checkLineOfSight = true;
    [Tooltip("Layers for obstacles that should not be considered for line of sight.")]
    [SerializeField] private LayerMask visualObstaclesLayersMask;


    [Header("EVENTS:")]
    [FormerlySerializedAs("objectEnteredCone")]
    [Tooltip("Subscriptions to new detections.")]
    [SerializeField] private UnityEvent<GameObject> objectEnteredSensor;
    
    [FormerlySerializedAs("objectStayCone")]
    [Tooltip("Subscriptions to object staying in volumetric area.")]
    [SerializeField] private UnityEvent<GameObject> objectStayedInSensor;
    
    [FormerlySerializedAs("objectLeftCone")]
    [Tooltip("Subscriptions to object leaving volumetric area.")] 
    [SerializeField] private UnityEvent<GameObject> objectLeftSensor;
    
    [Tooltip("Subscriptions to this cone sensor changing its dimensions. It includes " +
             "<newRange, newDegrees>.")]
    [SerializeField] private UnityEvent<float, float> coneSensorDimensionsChanged;
    
    [Header("WIRING:")]
    [SerializeField] private VolumetricSensor sensor;
    [SerializeField] private BoxRangeManager boxRangeManager;
    [SerializeField] private ConeRange coneRange;
    [SerializeField] private RaySensor lineOfSightSensor;
    
    /// <summary>
    /// Range to detect objects.
    /// </summary>
    public float DetectionRange
    {
        get => detectionRange;
        set
        {
            detectionRange = value;
            UpdateDetectionArea();
            coneSensorDimensionsChanged?.Invoke(
                DetectionRange,
                DetectionSemiconeAngle);
            
            // Guard needed to avoid infinite calls between this component and _coneRange
            // when changing the range.
            _parameterSetFromHere = true;
            coneRange.Range = DetectionRange;
        }
    }
    
    /// <summary>
    /// Semicone angle for detection (in degrees).
    /// </summary>
    public float DetectionSemiconeAngle
    {
        get => detectionSemiconeAngle;
        set
        {
            detectionSemiconeAngle = value;
            UpdateDetectionArea();
            coneSensorDimensionsChanged?.Invoke(
                DetectionRange,
                DetectionSemiconeAngle);

            // Guard needed to avoid infinite calls between this component and _coneRange
            // when changing the angle.
            _parameterSetFromHere = true;
            coneRange.SemiConeDegrees = DetectionSemiconeAngle;
        }
    }
    
    /// <summary>
    /// Specifies the physics layers that the sensor will monitor for potential collisions.
    /// </summary>
    public LayerMask LayersToDetect
    {
        get => layersToDetect;
        set
        {
            layersToDetect = value;
            ConfigureVolumetricSensor(value);
        }
    }

    public bool CheckLineOfSight
    {
        get => checkLineOfSight;
        set
        {
            checkLineOfSight = value;
            lineOfSightSensor.enabled = checkLineOfSight;
        }
    }
        

    public LayerMask VisualObstaclesLayersMask
    {
        get => visualObstaclesLayersMask;
        set
        {
            visualObstaclesLayersMask = value;
            ConfigureLineOfSightSensor(value);
        }
    }
    
    /// <summary>
    /// This sensor forward vector.
    /// </summary>
    public Vector2 Forward
    {
        get => transform.up;
        set => transform.up = value;
    }

    public UnityEvent<GameObject> ObjectEnteredSensor => objectEnteredSensor;

    public UnityEvent<GameObject> ObjectStayedInSensor => objectStayedInSensor;

    public UnityEvent<GameObject> ObjectLeftSensor => objectLeftSensor;
    
    /// <summary>
    /// Whether the sensor is detecting any object.
    /// </summary>
    public bool AnyObjectDetected => DetectedObjects.Count > 0;

    /// <summary>
    /// <p>List of objects currently inside this sensor range.</p>
    /// <p>Only are considered those objects included in the layer mask provided
    /// to ConeSensor.</p> 
    /// </summary>
    public HashSet<GameObject> DetectedObjects
    {
        get
        {
            if (checkLineOfSight) return _objectsInSensorRangeAndVisible;
            return _objectsInSensorRange;
        }
    }
    
    private bool _parameterSetFromHere;
    private readonly HashSet<GameObject> _objectsInSensorRange = new();
    private readonly HashSet<GameObject> _objectsInSensorRangeAndVisible = new();

    private void Start()
    {
        ConfigureVolumetricSensor(LayersToDetect);
        lineOfSightSensor.enabled = checkLineOfSight;
        ConfigureLineOfSightSensor(VisualObstaclesLayersMask);
    }
    
    private void ConfigureVolumetricSensor(LayerMask value)
    {
        if (sensor == null) return;
        sensor.detectionLayers = value;
    }
    
    private void ConfigureLineOfSightSensor(LayerMask value)
    {
        if (lineOfSightSensor == null) return;
        lineOfSightSensor.detectionLayers = LayersToDetect | value;
    }

    /// <summary>
    /// Whether a global position is inside the cone range of the agent.
    /// </summary>
    /// <param name="position">Global position to check.</param>
    /// <returns>True if the position is inside the cone.</returns>
    private bool PositionIsInConeRange(Vector2 position)
    {
        float distance = Vector2.Distance(position, transform.position);
        float heading = Vector2.Angle(
            Forward,
            position - (Vector2) transform.position);
        return distance <= DetectionRange && heading <= DetectionSemiconeAngle;
    }
    
    /// <summary>
    /// <p>Event handler launched when the cone range gizmo is updated.</p>
    /// <p>This way DetectionRange and DetectionSemiconeAngle are updated.</p>
    /// </summary>
    public void OnConeRangeUpdated()
    {
        // Guard needed to avoid infinite calls between this component and _coneRange
        // when changing the range or angle.
        if (_parameterSetFromHere)
        {
            _parameterSetFromHere = false;
            return;
        }
        DetectionRange = coneRange.Range;
        DetectionSemiconeAngle = coneRange.SemiConeDegrees;
    }

    /// <summary>
    /// Event handler to use when another object enters the detection area.
    /// </summary>
    /// <param name="otherObject">The object who enters the detection area.</param>
    public void OnObjectEnteredCone(GameObject otherObject)
    {
        // Remember that the initial detection area is a square, but our final detection
        // area is a cone whose area is a subset of the square area. So, we need to
        // check if the object is inside the cone range.
        if (!PositionIsInConeRange(otherObject.transform.position)) 
            return;
        
        _objectsInSensorRange.Add(otherObject);
        
        if (!CheckLineOfSight) ObjectEnteredSensor?.Invoke(otherObject);

        // Object can be inside the cone range but behind a cover. So, we must check
        // if there is a line-of-sight with the object.
        if (!CheckLineOfSight || !ObjectIsVisible(otherObject)) return; 
            
        _objectsInSensorRangeAndVisible.Add(otherObject);
        
        objectEnteredSensor?.Invoke(otherObject);
    }
    
    /// <summary>
    /// Event handler to use when another object stays in the detection area.
    /// </summary>
    /// <param name="otherObject">The object who stays in the detection area.</param>
    public void OnObjectStayCone(GameObject otherObject)
    {
        // Only keep in DetectedObjects those who are in the detection area and in
        // cone range.
        if (!PositionIsInConeRange(otherObject.transform.position) &&
            _objectsInSensorRange.Contains(otherObject))
        {
            _objectsInSensorRange.Remove(otherObject);
            return;
        }
        
        // Not in the cone range nor in DetectedObjects, so just ignore it.
        if (!PositionIsInConeRange(otherObject.transform.position)) 
            return;
        
        // Can an object appear in stay phase without having being detected
        // in enter phase? Yes, it can. If the game starts with the object already inside
        // the sensor range, then it won't be detected in the enter phase but in the stay
        // phase.
        //
        // If the object is in the cone range, then add it to DetectedObjects.
        _objectsInSensorRange.Add(otherObject);
        
        if (!CheckLineOfSight || !ObjectIsVisible(otherObject))
        {
            if (_objectsInSensorRangeAndVisible.Contains(otherObject)) 
                _objectsInSensorRangeAndVisible.Remove(otherObject);
            return;
        }
        
        // This is a HashSet. The type offers a built-in method to avoid duplicated
        // elements. So, we can simply add the element without checking if it is already
        // in the collection.
        _objectsInSensorRangeAndVisible.Add(otherObject);
        
        objectStayedInSensor?.Invoke(otherObject);
    }

    /// <summary>
    /// Event handler to use when another object exits our detection area.
    /// </summary>
    /// <param name="otherObject">The object who exits our detection area.</param>
    public void OnObjectExitedCone(GameObject otherObject)
    {
        // Only remove from detected object if it was already there.
        if (!_objectsInSensorRange.Contains(otherObject)) return;
        _objectsInSensorRange.Remove(otherObject);
        
        // If we are not checking line-of-sight, then there is nothing more to do.
        if (!CheckLineOfSight)
        {
            ObjectLeftSensor?.Invoke(otherObject);
            return;
        }

        if (!_objectsInSensorRangeAndVisible.Contains(otherObject)) return;
        _objectsInSensorRangeAndVisible.Remove(otherObject);

        ObjectLeftSensor?.Invoke(otherObject);
    }

    /// <summary>
    /// Checks whether the specified object is visible to the sensor, considering
    /// line-of-sight and visual obstacles.
    /// </summary>
    /// <param name="otherObject">The game object to check for visibility.</param>
    /// <returns>True if the object is visible to the sensor; otherwise, false.</returns>
    private bool ObjectIsVisible(GameObject otherObject)
    {
        lineOfSightSensor.EndPosition = otherObject.transform.position;
        lineOfSightSensor.UpdateRay();
        Collider2D detectedCollider = lineOfSightSensor.DetectedCollider;
        if (detectedCollider == null) return false;
        GameObject detectedObject = detectedCollider.gameObject;
        return detectedObject == otherObject;
    }
    
    /// <summary>
    /// Change the detection area to its new dimensions.
    /// </summary>
    private void UpdateDetectionArea()
    {
        if (boxRangeManager == null) return;
        boxRangeManager.Range = DetectionRange;
        boxRangeManager.Width = DetectionRange * 
                                Mathf.Sin(DetectionSemiconeAngle * 
                                          Mathf.Deg2Rad) * 2;
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Force the use of properties to make them update cone range.
        DetectionRange = detectionRange;
        DetectionSemiconeAngle = detectionSemiconeAngle;
        LayersToDetect = layersToDetect;
    }
#endif
}
}
