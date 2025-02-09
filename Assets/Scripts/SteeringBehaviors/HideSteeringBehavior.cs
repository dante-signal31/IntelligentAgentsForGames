using System.Collections.Generic;
using Levels;
using Pathfinding;
using Tools;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// <p> Monobehaviour to offer a hiding steering behaviour. </p>
/// <p> Hiding makes an agent to place itself after an obstacle between him and a
/// threat. </p>
/// </summary>
[RequireComponent(typeof(SeekSteeringBehavior), typeof(AgentMover))]
public class HideSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")] 
    [Tooltip("Agent to hide from.")]
    [SerializeField] private AgentMover threat;
    [Tooltip("Distance at which we give our goal as reached and we stop our agent.")]
    [SerializeField] private float arrivalDistance;
    [Tooltip("At which physics layers the obstacles belong to?")]
    [SerializeField] private LayerMask obstaclesLayer;
    [Tooltip("How much separation our hiding point must show from obstacles?")]
    [SerializeField] private float separationFromObstacles = 1f;
    [Tooltip("How wide is the agent we want to hide?")] 
    [SerializeField] private float agentRadius = 0.5f;
    [Tooltip("A position with any of this physic layers objects is not empty ground " +
             "to be a valid hiding point.")]
    [SerializeField] private LayerMask notEmptyGroundLayers;

    [Header("DEBUG:")]
    [Tooltip("Show gizmos for debugging.")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmosColor = Color.green;

    [Header("WIRING:")]
    [SerializeField] private RaySensor rayCast; 
    [SerializeField] private NavigationAgent navigationAgent;
    [SerializeField] private HidingPointsDetector hidingPointsDetector;

    /// <summary>
    /// Agent to hide from.
    /// </summary>
    public AgentMover Threat
    {
        get => threat;
        set
        {
            threat = value;
            if (hidingPointsDetector != null) 
                hidingPointsDetector.Threat = threat.gameObject;
            if (rayCast != null)
                rayCast.SensorLayerMask = Threat.CollisionLayer | ObstaclesLayer;

        }
    }

    /// <summary>
    /// Distance at which we give our goal as reached and we stop our agent.
    /// </summary>
    public float ArrivalDistance
    {
        get => arrivalDistance;
        set => arrivalDistance = value;
    }

    /// <summary>
    /// At which physics layers the obstacles belong to?
    /// </summary>
    public LayerMask ObstaclesLayer
    {
        get => obstaclesLayer;
        set
        {
            obstaclesLayer = value;
            if (hidingPointsDetector != null) 
                hidingPointsDetector.ObstaclesLayer = value;
        }
    }

    /// <summary>
    /// How much separation our hiding point must show from obstacles?
    /// </summary>
    public float SeparationFromObstacles
    {
        get => separationFromObstacles;
        set
        {
            separationFromObstacles = value;
            if (hidingPointsDetector != null) 
                hidingPointsDetector.SeparationFromObstacles = value;
        }
    }

    /// <summary>
    /// How wide is the agent we want to hide?
    /// </summary>
    public float AgentRadius
    {
        get => agentRadius;
        set
        {
            agentRadius = value;
            if (hidingPointsDetector != null) 
                hidingPointsDetector.AgentRadius = value;
        }
    }

    /// <summary>
    /// A position with any of this physic layers objects is not empty ground to be a
    /// valid hiding point.
    /// </summary>
    public LayerMask NotEmptyGroundLayers
    {
        get => notEmptyGroundLayers;
        set
        {
            notEmptyGroundLayers = value;
            if (hidingPointsDetector != null) 
                hidingPointsDetector.NotEmptyGroundLayers = value;
        }
    }

    private Vector2 _hidingPoint;
    /// <summary>
    /// Current hiding point position selected by this behavior.
    /// </summary>
    public Vector2 HidingPoint
    {
        get => _hidingPoint;
        private set
        {
            _hidingPoint = value;
            if (navigationAgent != null)
                navigationAgent.TargetPosition = value;
        }
    }

    // private INavigationAgent _navigationAgent;
    private SeekSteeringBehavior _seekSteeringBehavior;
    private Courtyard _currentLevel;
    private AgentMover _parentAgentMover;
    private Vector2 _previousThreatPosition = Vector2.zero;

    private bool ThreatHasJustMoved => 
        (Vector2)Threat.transform.position != _previousThreatPosition;

    private bool _threatCanSeeUs;
    private bool _hidingPointRecheckNeeded;
    private GameObject _nextMovementTarget;

    private void Awake()
    {
        if (_nextMovementTarget == null)
            _nextMovementTarget = new GameObject("NextMovementTarget");
        HidingPoint = transform.position;
        _parentAgentMover = GetComponent<AgentMover>();
        _seekSteeringBehavior = GetComponent<SeekSteeringBehavior>();
    }

    private void Start()
    {
        _currentLevel = FindAnyObjectByType<Courtyard>();
        if (_currentLevel == null) return;
        InitRayCast();
        InitSeekSteeringBehavior();
        InitHidingPointDetector();
        InitNavigationAgent();
    }

    private void InitRayCast()
    {
        if (rayCast == null) return;
        if (Threat != null) 
            rayCast.SensorLayerMask = Threat.CollisionLayer | ObstaclesLayer;
        // TODO: Asses RaySensor implementation to make it similar to Godot Raycast api.
        // TODO: Some configurations are missing here compared with Godot version. They may be needed.
    }

    private void InitSeekSteeringBehavior()
    {
        _seekSteeringBehavior.Target = _nextMovementTarget;
        _seekSteeringBehavior.ArrivalDistance = ArrivalDistance;
    }

    private void InitHidingPointDetector()
    {
        hidingPointsDetector.Threat = Threat.gameObject;
        hidingPointsDetector.ObstaclesPositions = _currentLevel.ObstaclePositions;
        hidingPointsDetector.ObstaclesLayer = ObstaclesLayer;
        hidingPointsDetector.SeparationFromObstacles = SeparationFromObstacles;
        hidingPointsDetector.AgentRadius = AgentRadius;
        hidingPointsDetector.NotEmptyGroundLayers = NotEmptyGroundLayers;
    }

    private void InitNavigationAgent()
    {
        navigationAgent.TargetPosition = HidingPoint;
        navigationAgent.Radius = AgentRadius;
    }

    private void OnDestroy()
    {
        if (_nextMovementTarget != null) Destroy(_nextMovementTarget);
    }

    private void FixedUpdate()
    {
        if (Threat == null || rayCast == null) return;
    
        // Check if there is a line of sight with the threat.
        rayCast.TargetPosition = Threat.transform.position;
        if (rayCast.IsColliderDetected)
        {
            _threatCanSeeUs = rayCast.DetectedCollider.gameObject == Threat.gameObject;
        }
        else
        {
            _threatCanSeeUs = false;
        }
    
        // Starting threat position counts as ThreatHasJustMoved because
        // _previousThreatPosition is init as Vector2.Zero.
        if (ThreatHasJustMoved) _hidingPointRecheckNeeded = true;
        _previousThreatPosition = Threat.transform.position;
    
        // TODO: Check if this is actually needed.
        // Do not query when the map has never synchronized and is empty.
        if (!navigationAgent.IsReady) return;
        // Only query when the navigation agent has not reached the target yet.
        if (!navigationAgent.IsNavigationFinished)
            _nextMovementTarget.transform.position = navigationAgent.GetNextPathPosition();
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        // Look for a new hiding point if the threat can see us and if it is threat first 
        // position (only once) or has just moved.
        if (_threatCanSeeUs && _hidingPointRecheckNeeded)
        {   // Search for the nearest hiding point.
            List<Vector2> hidingPoints = hidingPointsDetector.HidingPoints;
            if (hidingPoints.Count > 0)
            {
                float minimumDistance = float.MaxValue;
                Vector2 nearestHidingPoint = Vector2.zero;
                foreach (Vector2 candidatePoint in hidingPoints)
                {
                    navigationAgent.TargetPosition = candidatePoint;
                    float currentDistance = navigationAgent.DistanceToTarget();
                    if (currentDistance < minimumDistance)
                    {
                        minimumDistance = currentDistance;
                        nearestHidingPoint = candidatePoint;
                    }
                }
                HidingPoint = nearestHidingPoint;
                _hidingPointRecheckNeeded = false;
            }
        }
    
        // Head to the next point in the path to the heading target. That next point
        // position is updated in FixedUpdate().
        return _seekSteeringBehavior.GetSteering(args);
    }
}
}
