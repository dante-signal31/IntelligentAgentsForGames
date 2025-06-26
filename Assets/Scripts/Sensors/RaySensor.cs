using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// <p>Generic component for ray sensors.</p>
///
/// <p>Just place it and give it the layer were you want to detect colliders. It will
/// emit a colliderDetected event whenever one is hit by ray and a noColliderDetected
/// event when ray is clear. </p>s
/// </summary>
public class RaySensor : MonoBehaviour
{
    [Header("CONFIGURATION:")] 
    [Tooltip("Layers to be detected by this sensor.")] 
    public LayerMask detectionLayers;
    [Tooltip("Whether to ignore colliders overlapping start point.")]
    [SerializeField] private bool ignoreCollidersOverlappingStartPoint = true;
    [Space]
    [Tooltip("Event to trigger when a collider is detected by this sensor.")]    
    public UnityEvent<RaySensor> colliderDetected;
    [Tooltip("Event to trigger when no collider is detected by this sensor.")]
    public UnityEvent noColliderDetected;
    
    [Header("DEBUG:")] 
    [Tooltip("Whether to show gizmos for this sensor.")]
    [SerializeField] private bool showGizmos = true;
    [Tooltip("Gizmo color for this sensor.")]
    [SerializeField] private Color gizmoColor = Color.red;
    [Tooltip("Color to show when the sensor detects an object.")] 
    [SerializeField] private Color gizmoDetectedColor = Color.green;
    [Tooltip("Radius for the gizmos that mark the ray ends.")]
    [Range(0.01f, 1.0f)]
    [SerializeField] private float gizmoRadius;
    
    [Header("WIRING:")]
    [Tooltip("Point from ray starts.")]
    public Transform startPoint;
    [Tooltip("Point ray ends to.")]
    public Transform endPoint;
    
    
    /// <summary>
    /// Whether this sensor has detected any collider.
    /// </summary>
    public bool IsColliderDetected => DetectedCollider != null;
    
    /// <summary>
    /// This ray sensor layer mask.
    /// </summary>
    public LayerMask SensorLayerMask
    {
        get => detectionLayers;
        set
        {
            detectionLayers = value;
            UpdateRayData();
        }
    }

    /// <summary>
    /// Whether to ignore colliders overlapping start point.
    /// </summary>
    public bool IgnoreCollidersOverlappingStartPoint
    {
        get => ignoreCollidersOverlappingStartPoint;
        set
        {
            ignoreCollidersOverlappingStartPoint = value;
            UpdateRayData();
        }
    }

    private Collider2D _detectedCollider;
    
    /// <summary>
    /// Collider currently detected by sensor.
    /// </summary>
    public Collider2D DetectedCollider
    {
        get => _detectedCollider;
        private set
        {
            if (_detectedCollider == value) return;
            _detectedCollider = value;
            if (value == null && noColliderDetected != null)
            {
                noColliderDetected.Invoke();
            } 
            else if (value != null && colliderDetected != null)
            {
                colliderDetected.Invoke(this);
            }
        }
    }
    
    public RaycastHit2D DetectedHit { get; private set; }

    /// <summary>
    /// Raycast start position.
    /// </summary>
    public Vector3 StartPosition
    {
        get => startPoint.position;
        set
        {
            startPoint.position = value;
            UpdateRayData();
        }
    }
    
    /// <summary>
    /// Raycast end position.
    /// </summary>
    public Vector3 EndPosition
    {
        get => endPoint.position;
        set
        {
            endPoint.position = value;
            UpdateRayData();
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

    private Vector3 _rayDirection;
    private float _rayDistance;

    private void Awake()
    {
        UpdateRayData();
    }

    /// <summary>
    /// Start or target point may have changed, so ray direction and distance
    /// need to be updated and a new raycast performed.
    /// </summary>
    private void UpdateRayData()
    {
        _rayDirection = GetRayDirection();
        _rayDistance = GetRayDistance();
    }
    
    private Vector3 GetRayDirection()
    {
        return (endPoint.position - startPoint.position).normalized;
    }

    private float GetRayDistance()
    {
        return Vector2.Distance(endPoint.position, startPoint.position);
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
            startPoint.position, 
            _rayDirection, 
            _rayDistance, 
            detectionLayers);
        
        // Nothing detected.
        if (hits.Length == 0)
        {
            DetectedHit = new RaycastHit2D();
            DetectedCollider = null;
            return;
        }
        
        // If we are not ignoring colliders overlapping start point,
        // then first is good.
        if (!IgnoreCollidersOverlappingStartPoint)
        {
            DetectedHit = hits[0];
            DetectedCollider = DetectedHit.collider;
            return;
        }
        
        // If we are ignoring colliders overlapping start point,
        // then we are searching for the first collider whose distance to start point
        // is greater than zero.
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.distance > 0)
            {
                DetectedHit = hit;
                DetectedCollider = hit.collider;
                return;
            }
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showGizmos) DrawSensor();
    }

    private void DrawSensor()
    {
        Vector3 gizmoSize = new Vector3(gizmoRadius, gizmoRadius, gizmoRadius);
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(startPoint.position, gizmoSize);
        Gizmos.DrawSphere(endPoint.position, gizmoRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(startPoint.position, endPoint.position);
        if (!IsColliderDetected) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(startPoint.position, DetectedHit.point);
    }
#endif
}