﻿using System.Collections.Generic;
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
    [Header("CONFIGURATION:")] 
    [Tooltip("Layers to be detected by this sensor.")] 
    [SerializeField] private LayerMask layerMask;
    [Tooltip("Whether to ignore colliders overlapping start point.")]
    [SerializeField] private bool ignoreCollidersOverlappingStartPoint = true;
    [Space]
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
    
    [Header("WIRING:")]
    [Tooltip("Point from ray starts.")]
    [SerializeField] private Transform startPoint;
    [Tooltip("Point ray ends to.")]
    [SerializeField] private Transform endPoint;

    /// <summary>
    /// Whether this sensor has detected any collider.
    /// </summary>
    public bool IsColliderDetected => DetectedCollider != null;

    private Collider2D _detectedCollider;
    
    /// <summary>
    /// This ray sensor layer mask.
    /// </summary>
    public LayerMask SensorLayerMask
    {
        get => layerMask;
        set
        {
            layerMask = value;
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
                _detectedCollider = value;
                if (value == null && noColliderDetected != null)
                {
                    noColliderDetected.Invoke();
                } 
                else if (value != null && colliderDetected != null)
                {
                    colliderDetected.Invoke(value);
                }
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
    public Vector3 TargetPosition
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
        PerformRaycast();
    }

    /// <summary>
    /// Bind a listener to the colliderDetected event.
    /// </summary>
    /// <param name="action">Method to bind.</param>
    public void SubscribeToColliderDetected(UnityAction<Collider2D> action)
    {
        colliderDetected.AddListener(action);
    }
    
    /// <summary>
    /// Unbind a listener from the colliderDetected event.
    /// </summary>
    /// <param name="action">Method to unbind.</param>
    public void UnsubscribeFromColliderDetected(UnityAction<Collider2D> action)
    {
        colliderDetected.RemoveListener(action);
    }

    /// <summary>
    /// Bind a listener to the noColliderDetected event.
    /// </summary>
    /// <param name="action">Method to bind.</param>
    public void SubscribeToNoColliderDetected(UnityAction action)
    {
        noColliderDetected.AddListener(action);
    }
    
    /// <summary>
    /// Unbind a listener from the noColliderDetected event.
    /// </summary>
    /// <param name="action">Method to unbind.</param>
    public void UnsubscribeFromNoColliderDetected(UnityAction action)
    {
        noColliderDetected.RemoveListener(action);
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
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            startPoint.position, 
            _rayDirection, 
            _rayDistance, 
            layerMask);
        
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
            DetectedCollider = DetectedHit.collider;;
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

    /// <summary>
    /// Sets the target position for the ray, using the provided target position in 3D space.
    /// </summary>
    /// <param name="target">Position for the ray target.</param>
    public void SetRayTarget(Vector3 target)
    {
        endPoint.position = target;
        UpdateRayVectorData();
    }

    /// <summary>
    /// Set the ray origin, using the provided position in 3D space.
    /// </summary>
    /// <param name="origin">Position for the ray origin.</param>
    public void SetRayOrigin(Vector3 origin)
    {
        startPoint.position = origin;
        UpdateRayVectorData();
    }
    
    /// <summary>
    /// Update the ray distance and direction data based on the current ray origin and target.
    /// </summary>
    private void UpdateRayVectorData()
    {
        _rayDistance = GetRayDistance();
        _rayDirection = GetRayDirection();
    }
    
    // /// <summary>
    // /// Set this ray sensor layer mask.
    // /// </summary>
    // /// <param name="layerMask">Layermask for this sensor.</param>
    // public void SetLayerMask(LayerMask layerMask)
    // {
    //     this.layerMask = layerMask;
    // }
    
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