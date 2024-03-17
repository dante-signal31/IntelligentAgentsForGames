using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;


/// <summary>
/// <p>Generic component for ray sensors.</p>
///
/// <p>Just place it and give it the layer were you want to detect colliders. It will emit
/// a colliderDetected event whenever one is hit by ray and a noColliderDetected event when
/// ray is clear. </p>s
/// </summary>
public class RaySensor : MonoBehaviour
{
    [Header("WIRING:")]
    [Tooltip("Point from ray starts.")]
    [SerializeField] private Transform startPoint;
    [Tooltip("Point ray ends to.")]
    [SerializeField] private Transform endPoint;

    [Header("CONFIGURATION:")] 
    [Tooltip("Layers to be detected by this sensor.")] 
    [SerializeField] private LayerMask layerMask;
    [Tooltip("Event to trigger when a collider is detected by this sensor.")]    
    [SerializeField] private UnityEvent<Collider2D> colliderDetected;
    [Tooltip("Event to trigger when no collider is detected by this sensor.")]
    [SerializeField] private UnityEvent noColliderDetected;
    
    [Header("DEBUG:")] 
    [Tooltip("Whether to show gizmos for this sensor.")]
    [SerializeField] private bool showGizmos = true;
    [Tooltip("Gizmo color for this sensor.")]
    [SerializeField] private Color gizmoColor = Color.green;
    [Tooltip("Radius for the gizmos that mark the ray ends.")]
    [Range(0.01f, 1.0f)]
    [SerializeField] private float gizmoRadius;

    /// <summary>
    /// Whether this sensor has detected any collider.
    /// </summary>
    public bool IsColliderDetected => DetectedCollider != null;

    private Collider2D _detectedCollider;

    /// <summary>
    /// Collider currently detected by sensor.
    /// </summary>
    public Collider2D DetectedCollider
    {
        get => _detectedCollider;
        private set
        {
            if (_detectedCollider != value)
            {
                if (value == null && noColliderDetected != null)
                {
                    noColliderDetected.Invoke();
                } 
                else if (value != null && colliderDetected != null)
                {
                    colliderDetected.Invoke(value);
                }
                _detectedCollider = value;
            }
        }
    }
    
    public RaycastHit2D DetectedHit { get; private set; }

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
        RaycastHit2D hit = Physics2D.Raycast(startPoint.position, 
            GetRayDirection(), 
            _rayDistance, 
            layerMask);
        DetectedCollider = hit.collider;
        DetectedHit = hit;
    }
    
    /// <summary>
    /// Sets the target position for the ray, using the provided target position in 3D space.
    /// </summary>
    /// <param name="target">Position for the ray target.</param>
    public void SetRayTarget(Vector3 target)
    {
        endPoint.position = target;
    }
    
    /// <summary>
    /// Set the ray origin, using the provided position in 3D space.
    /// </summary>
    /// <param name="origin">Position for the ray origin.</param>
    public void SetRayOrigin(Vector3 origin)
    {
        startPoint.position = origin;
    }
    
    /// <summary>
    /// Set this ray sensor layer mask.
    /// </summary>
    /// <param name="layerMask">Layermask for this sensor.</param>
    public void SetLayerMask(LayerMask layerMask)
    {
        this.layerMask = layerMask;
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
        Gizmos.color = IsColliderDetected ? Color.green : Color.red;
        Gizmos.DrawLine(startPoint.position, endPoint.position);
    }
#endif
}