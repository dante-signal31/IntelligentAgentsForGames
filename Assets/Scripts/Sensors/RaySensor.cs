using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Sensors
{
/// <summary>
/// <p>Generic component for ray sensors.</p>
///
/// <p>Place it and give it the layer where you want to detect colliders. It will
/// emit a colliderDetected event whenever a ray hits one and a noColliderDetected
/// event when the ray is clear. </p>s
/// </summary>
public class RaySensor : MonoBehaviour, ISensor
{
    [Header("CONFIGURATION:")]
    [Tooltip("Point from ray starts. In local coordinates.")]
    public Vector3 localStartPoint = Vector3.zero;
    [Tooltip("Point ray ends to. In local coordinates.")]
    public Vector3 localEndPoint = Vector3.up;
    [Tooltip("Layers to be detected by this sensor.")] 
    [SerializeField] public LayerMask detectionLayers;
    [Tooltip("Whether to ignore colliders overlapping start point.")]
    [SerializeField] private bool ignoreCollidersOverlappingStartPoint = true;
    [Space]
    [Tooltip("Event to trigger when a collider is detected by this sensor.")]    
    [SerializeField] public UnityEvent<GameObject> objectEnteredSensor = new();
    [Tooltip("Event to trigger when an object keeps being detected by this sensor.")]
    [SerializeField] public UnityEvent<GameObject> objectStayedInSensor = new();
    [Tooltip("Event to trigger when an object no longer is detected by this sensor.")]
    [SerializeField] public UnityEvent<GameObject> objectLeftSensor = new();

    [Header("DEBUG:")] 
    [Tooltip("Whether to show gizmos for this sensor.")]
    [SerializeField] private bool showGizmos = true;
    [Tooltip("Gizmo color for this sensor.")]
    [SerializeField] public Color gizmoColor = Color.red;
    [Tooltip("Color to show when the sensor detects an object.")] 
    [SerializeField] private Color gizmoDetectedColor = Color.green;
    [Tooltip("Radius for the gizmos that mark the ray ends.")]
    [Range(0.01f, 1.0f)]
    [SerializeField] private float gizmoRadius;

    public UnityEvent<GameObject> ObjectEnteredSensor => objectEnteredSensor;

    public UnityEvent<GameObject> ObjectStayedInSensor => objectStayedInSensor;

    public UnityEvent<GameObject> ObjectLeftSensor => objectLeftSensor;

    private readonly List<GameObject> _detectedObjects = new();
    
    public HashSet<GameObject> DetectedObjects => new(_detectedObjects);
    
    /// <summary>
    /// Stores information about all detected objects and their corresponding
    /// raycast hits.
    /// </summary>
    public Dictionary<GameObject, RaycastHit2D> DetectedHits { get; private set; } =
        new();
    
    /// <summary>
    /// Whether this sensor has detected any collider.
    /// </summary>
    public bool AnyObjectDetected => FirstDetectedObject != null;

    /// <summary>
    /// This ray sensor layer mask.
    /// </summary>
    public LayerMask SensorLayerMask
    {
        get => detectionLayers;
        set => detectionLayers = value;
    }

    /// <summary>
    /// Whether to ignore colliders overlapping start point.
    /// </summary>
    public bool IgnoreCollidersOverlappingStartPoint
    {
        get => ignoreCollidersOverlappingStartPoint;
        set => ignoreCollidersOverlappingStartPoint = value;
    }

    /// <summary>
    /// The first detected object, if any, from the detected objects.
    /// Returns null if no objects are detected.
    /// </summary>
    public GameObject FirstDetectedObject => DetectedObjects.Count > 0 ? 
        _detectedObjects[0] : 
        null;

    /// <summary>
    /// The first detected raycast hit, if any, from the detected objects.
    /// Returns null if no objects are detected.
    /// </summary>
    public RaycastHit2D? FirstDetectedHit => DetectedObjects.Count > 0 ? 
        DetectedHits[FirstDetectedObject] : 
        null;

    /// <summary>
    /// Raycast start position. In global coordinates.
    /// </summary>
    public Vector3 GlobalStartPosition
    {
        get => transform.TransformPoint(localStartPoint);
        set
        {
            localStartPoint = transform.InverseTransformPoint(value);
            PerformRaycast();
        }
    }

    /// <summary>
    /// Raycast end position. In global coordinates.
    /// </summary>
    public Vector3 GlobalEndPosition
    {
        get => transform.TransformPoint(localEndPoint);
        set
        {
            localEndPoint = transform.InverseTransformPoint(value);
            PerformRaycast();
        }
    }

    /// <summary>
    /// Whether to show gizmos for this sensor for debugging.
    /// </summary>
    public bool ShowGizmos
    {
        get => showGizmos;
        set => showGizmos = value;
    }

    /// <summary>
    /// Update the raycast without waiting for the next fixed update.
    /// </summary>
    public void UpdateRay()
    {
        PerformRaycast();
    }

    /// <summary>
    /// Calculates and returns the normalized direction vector of the ray
    /// based on the positions of the start and end points.
    /// </summary>
    /// <returns>
    /// A normalized <see cref="Vector3"/> representing the direction of the ray
    /// from the start point to the end point.
    /// </returns>
    private Vector3 GetRayDirection()
    {
        return (GlobalEndPosition - GlobalStartPosition).normalized;
    }

    /// <summary>
    /// Calculates the distance between the start and end points of the ray.
    /// </summary>
    /// <returns>The distance between the start point and end point of the ray.</returns>
    private float GetRayDistance()
    {
        return Vector2.Distance(GlobalEndPosition, GlobalStartPosition);
    }

    private void FixedUpdate()
    {
        PerformRaycast();
    }

    /// <summary>
    /// Performs a raycast using the current origin, direction, distance,
    /// and layer mask, and updates the detected collider and hit information.
    /// </summary>
    private void PerformRaycast()
    {
        // I use RaycastAll to get every collider along the ray, not only the first one.
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            GlobalStartPosition, 
            GetRayDirection(), 
            GetRayDistance(), 
            detectionLayers);
    
        // Nothing detected.
        if (hits.Length == 0)
        {
            DetectedHits.Clear();
            
            // Send an event for every object that is no longer detected.
            foreach (GameObject detectedObject in _detectedObjects)
            {
                ObjectLeftSensor?.Invoke(detectedObject);
            }
            _detectedObjects.Clear();
            
            return;
        }
        
        // Get every game object hit by the ray.
        List<GameObject> currentDetectedObjects = new();
        DetectedHits.Clear();
        foreach (RaycastHit2D hit in hits)
        {
            if (IgnoreCollidersOverlappingStartPoint && 
                Mathf.Approximately(hit.distance, 0f)) continue;
            DetectedHits.Add(hit.collider.gameObject, hit);
            currentDetectedObjects.Add(hit.collider.gameObject);
        }
        
        // Send events for objects that stay or leave the sensor.
        foreach (GameObject oldDetectedObject in _detectedObjects)
        {
            if (currentDetectedObjects.Contains(oldDetectedObject))
            {
                ObjectStayedInSensor?.Invoke(oldDetectedObject);
            }
            else
            {
                ObjectLeftSensor?.Invoke(oldDetectedObject);
            }
        }

        // Send an event for every object that is detected for the first time.
        foreach (GameObject detectedObject in currentDetectedObjects)
        {
            if (!_detectedObjects.Contains(detectedObject))
            {
                ObjectEnteredSensor?.Invoke(detectedObject);
            }
        }
        
        // Update the list of detected objects.
        _detectedObjects.Clear();
        _detectedObjects.AddRange(currentDetectedObjects);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showGizmos) DrawSensor();
    }

    private void DrawSensor()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(GlobalStartPosition, gizmoRadius);
        Gizmos.DrawWireSphere(GlobalEndPosition, gizmoRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(GlobalStartPosition, GlobalEndPosition);
        if (!AnyObjectDetected) return;
        Gizmos.color = gizmoDetectedColor;
        Gizmos.DrawLine(GlobalStartPosition, DetectedHits[_detectedObjects[0]].point);
    }
#endif
}
}