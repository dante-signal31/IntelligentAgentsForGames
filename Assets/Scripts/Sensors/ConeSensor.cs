using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.Events;

namespace Sensors
{
/// <summary>
/// Component to implement a cone of vision sensor.
/// </summary>
public class ConeSensor : MonoBehaviour
{
    [Header("CONFIGURATION:")]
    [Tooltip("Range to detect possible collisions.")]
    [SerializeField] private float detectionRange = 10f;
    [Tooltip("Semicone angle for detection (in degrees).")]
    [Range(0f, 90f)]
    [SerializeField] private float detectionSemiconeAngle = 45f;
    [Tooltip("Specifies the physics layers that the sensor will monitor for objects")]
    [SerializeField] private LayerMask layersToDetect;
    
    [Header("EVENTS:")]
    [Tooltip("Subscriptions")] 
    [SerializeField] private UnityEvent<GameObject> objectEnteredCone;
    [Tooltip("Subscriptions to object staying in volumetric area.")]
    [SerializeField] private UnityEvent<GameObject> objectStayCone;
    [Tooltip("Subscriptions to object leaving volumetric area.")] 
    [SerializeField] private UnityEvent<GameObject> objectLeftCone;
    [Tooltip("Subscriptions to this cone sensor changing its dimensions. It includes " +
             "<newRange, newDegrees>.")]
    [SerializeField] private UnityEvent<float, float> coneSensorDimensionsChanged;
    
    [Header("WIRING:")]
    [SerializeField] private VolumetricSensor sensor;
    [SerializeField] private BoxRangeManager boxRangeManager;
    [SerializeField] private ConeRange2D coneRange;

    private bool _parameterSetFromHere = false;
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
            if (coneSensorDimensionsChanged != null) 
                coneSensorDimensionsChanged.Invoke(
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
            if (coneSensorDimensionsChanged != null) 
                coneSensorDimensionsChanged.Invoke(
                    DetectionRange, 
                    DetectionSemiconeAngle);

            // Guard needed to avoid infinite calls between this component and _coneRange
            // when changing the angle.
            _parameterSetFromHere = true;
            coneRange.SemiConeDegrees = DetectionSemiconeAngle;
        }
    }
    
    /// <summary>
    /// Specifies the physics layers that the will monitor for potential collisions.
    /// </summary>
    public LayerMask LayersToDetect
    {
        get => layersToDetect;
        set => layersToDetect = value;
    }
    
    /// <summary>
    /// This sensor forward vector.
    /// </summary>
    public Vector2 Forward
    {
        get => transform.up;
        set => transform.up = value;
    }

    /// <summary>
    /// <p>List of objects currently inside this sensor range.</p>
    /// <p>Only are considered those objects included in the layermask provided
    /// to ConeSensor.</p> 
    /// </summary>
    // TODO: Try to change this type for an HashSet.
    public HashSet<GameObject> DetectedObjects { get; private set; } = new();
    
    /// <summary>
    /// Whether the provided object layer is included in the provided LayerMask.
    /// </summary>
    /// <param name="obj">Object to check.</param>
    /// <param name="layerMask">List of layers.</param>
    /// <returns>True if the object's layer is in the layermask.</returns>
    private bool ObjectIsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) != 0;
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
    /// <param name="range">How far we will detect other agents.</param>
    /// <param name="semiConeDegrees">How many degrees from forward we will admit
    /// detecting an agent.</param>
    public void OnConeRangeUpdated(float range, float semiConeDegrees)
    {
        // Guard needed to avoid infinite calls between this component and _coneRange
        // when changing the range or angle.
        if (_parameterSetFromHere)
        {
            _parameterSetFromHere = false;
            return;
        }
        DetectionRange = range;
        DetectionSemiconeAngle = semiConeDegrees;
    }

    /// <summary>
    /// Event handler to use when another object enters the detection area.
    /// </summary>
    /// <param name="otherObject">The object who enters the detection area.</param>
    public void OnObjectEnteredCone(GameObject otherObject)
    {
        if (ObjectIsInLayerMask(otherObject, LayersToDetect))
        {
            if (!PositionIsInConeRange(otherObject.transform.position)) 
                return;
            
            DetectedObjects.Add(otherObject);
            
            if (objectEnteredCone != null) 
                objectEnteredCone.Invoke(otherObject);
        }
    }
    
    /// <summary>
    /// Event handler to use when another object stays in the detection area.
    /// </summary>
    /// <param name="otherObject">The object who stays in the detection area.</param>
    public void OnObjectStayCone(GameObject otherObject)
    {
        if (ObjectIsInLayerMask(otherObject, LayersToDetect))
        {
            // Only keep in DetectedObjects those who are in the detection area and in
            // cone range.
            if (!PositionIsInConeRange(otherObject.transform.position) &&
                DetectedObjects.Contains(otherObject))
            {
                DetectedObjects.Remove(otherObject);
                return;
            }
            
            // Not in the cone range nor in DetectedObjects, so just ignore it.
            if (!PositionIsInConeRange(otherObject.transform.position)) 
                return;
            
            // If the object is in cone range but not in DetectedObjects, then add it.
            if (!DetectedObjects.Contains(otherObject)) DetectedObjects.Add(otherObject);
            
            if (objectStayCone != null)
                objectStayCone.Invoke(otherObject);
        }
    }

    /// <summary>
    /// Event handler to use when another object exits our detection area.
    /// </summary>
    /// <param name="otherObject">The object who exits our detection area.</param>
    public void OnObjectExitedCone(GameObject otherObject)
    {
        if (ObjectIsInLayerMask(otherObject, LayersToDetect))
        {
            if (!DetectedObjects.Contains(otherObject)) return;
            
            DetectedObjects.Remove(otherObject);
            
            if (objectLeftCone != null) 
                objectLeftCone.Invoke(otherObject);
        }
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
    }
#endif
}
}
