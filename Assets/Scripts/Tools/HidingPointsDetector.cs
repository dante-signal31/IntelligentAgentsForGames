
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
[RequireComponent(typeof(RaySensor))]    
public class HidingPointsDetector : MonoBehaviour
{
    [Header("CONFIGURATION:")]
    [Tooltip("Agent to hide from.")]
    [SerializeField] private GameObject threat;
    [Tooltip("At which physics layers the obstacles belong to?")]
    [SerializeField] private LayerMask obstaclesLayer;
    [Tooltip("How much separation our hiding point must show from obstacles?")]
    [SerializeField] private float separationFromObstacles;
    [Tooltip("How wide is the agent we want to hide?")] 
    [SerializeField] private float agentRadius;
    [Tooltip("A position with any of this physic layers objects is not empty ground " +
             "to be a valid hiding point.")]
    [SerializeField] private LayerMask notEmptyGroundLayers;
    [Tooltip("Maximum scene obstacles inner distance.")] 
    [SerializeField] private float maximumAdvanceAfterCollision;

    [Tooltip("Step length to advance the inner ray. The smaller value gives more " +
             "accuracy to calculate the exit point but it's slower to calculate.")]
    [SerializeField] private float innerRayStep;
    
    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmosColor = Color.green;
    [SerializeField] private Color cleanRadiusColor = Color.red;
    [SerializeField] private float collisionPointRadius;
    [SerializeField] private float hidingPointRadius;
    
    /// <summary>
    /// Agent to hide from.
    /// </summary>
    public GameObject Threat
    {
        get => threat;
        set => threat = value;
    }
    
    /// <summary>
    /// Obstacles positions in the level.
    /// </summary>
    public List<Vector2> ObstaclesPositions { get; set; }
    
    /// <summary>
    /// At which physics layers the obstacles belong to?
    /// </summary>
    public LayerMask ObstaclesLayer
    {
        get => obstaclesLayer;
        set => obstaclesLayer = value;
    }
    
    /// <summary>
    /// How much separation our hiding point must show from obstacles?
    /// </summary>
    public float SeparationFromObstacles
    {
        get => separationFromObstacles;
        set => separationFromObstacles = value;
    }
    
    /// <summary>
    /// How wide is the agent we want to hide?
    /// </summary>
    public float AgentRadius
    {
        get => agentRadius;
        set => agentRadius = value;
    }
    
    /// <summary>
    /// A position with any of this physic layers objects is not empty ground to be a
    /// valid hiding point.
    /// </summary>
    public LayerMask NotEmptyGroundLayers
    {
        get => notEmptyGroundLayers;
        set => notEmptyGroundLayers = value;
    }

    /// <summary>
    /// List of hiding points currently detected.
    /// </summary>
    public List<Vector2> HidingPoints { get; private set; } = new();

    /// <summary>
    /// Minimum radius free from obstacles and other obstacles to be a valid hiding point.
    /// </summary>
    private float MinimumCleanRadius => SeparationFromObstacles + AgentRadius;
    
    private RaySensor _raySensor;
    private List<Vector2> _rayCollisionPoints = new();
    private List <(Vector2, Vector2)> _afterCollisionRayEnds = new();

    private void Awake()
    {
        _raySensor = GetComponent<RaySensor>();
    }

    private void Start()
    {
        InitRaySensor();
    }

    private void InitRaySensor()
    {
        _raySensor.SensorLayerMask = ObstaclesLayer;
        _raySensor.StartPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (_raySensor == null || Threat == null) return;

        UpdateRayCollisionPoints();
        UpdateCleanHidingPoints();
    }
    
    /// <summary>
    /// <p>Update the list of sight collision points between the threat agent and the
    /// obstacles in the level.</p>
    /// <p>This collision points is the raycast collision point between the threat
    /// agent position and the obstacle position.</p>
    /// </summary>
    private void UpdateRayCollisionPoints()
    {
        _rayCollisionPoints.Clear();
        _raySensor.StartPosition = threat.transform.position;
        foreach (Vector2 obstaclePosition in ObstaclesPositions)
        {
            _raySensor.TargetPosition = obstaclePosition;
            if (_raySensor.IsColliderDetected)
            {
                _rayCollisionPoints.Add(_raySensor.DetectedHit.point);
            }
        }
    }
    
    /// <summary>
    /// <p>Update the list of points after the obstacles that are clear enough to allow
    /// the agent to hide there.</p>
    /// <p>It uses a ShapeCast2D to continue sight right, after collision with obstacle,
    /// until it finds a valid hiding point free from the obstacle.</p>
    /// </summary>
    private void UpdateCleanHidingPoints()
    {
        _afterCollisionRayEnds.Clear();
        HidingPoints.Clear();
        foreach (Vector2 rayCollisionPoint in _rayCollisionPoints)
        {
            Vector2 rayDirection = (rayCollisionPoint - 
                                     (Vector2)threat.transform.position).normalized;
            float innerAdvance = 0;
            while (innerAdvance < maximumAdvanceAfterCollision)
            {
                innerAdvance += innerRayStep;
                Vector2 candidateHidingPoint = rayCollisionPoint +
                                               rayDirection * innerAdvance;
                if (IsCleanHidingPoint(candidateHidingPoint))
                {
                    // Candidate point zone is clean. We can place and agent there.
                    _afterCollisionRayEnds.Add((rayCollisionPoint, candidateHidingPoint));
                    HidingPoints.Add(candidateHidingPoint);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Whether this point is free from obstacles to be a valid hiding point.
    /// </summary>
    /// <param name="candidateHidingPoint">Position to check</param>
    /// <returns>True if position is free from obstacles, false otherwise</returns>
    private bool IsCleanHidingPoint(Vector2 candidateHidingPoint)
    {
        Collider2D detectedCollider = Physics2D.OverlapCircle(
            candidateHidingPoint,
            MinimumCleanRadius,
            NotEmptyGroundLayers);
        return detectedCollider == null;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw a Line from Threat to Obstacles position.
        Gizmos.color = gizmosColor;
        foreach (Vector2 obstaclePosition in ObstaclesPositions)
        {
            Gizmos.DrawLine((Vector2)threat.transform.position, obstaclePosition);
        }
        
        // Draw a circle at each ray collision point with the obstacle.
        foreach (Vector2 rayCollisionPoint in _rayCollisionPoints)
        {
            Gizmos.DrawWireSphere(rayCollisionPoint, collisionPointRadius);
        }
        
        // Draw a line from collision point to hiding point.
        foreach ((Vector2 start, Vector2 end) in _afterCollisionRayEnds)
        {
            Gizmos.color = gizmosColor;
            Gizmos.DrawLine(start, end);
            Gizmos.color = cleanRadiusColor;
            Gizmos.DrawWireSphere(end, hidingPointRadius);
            Gizmos.color = gizmosColor;
            Gizmos.DrawWireSphere(end, MinimumCleanRadius);
        }
    }
#endif
}
}

