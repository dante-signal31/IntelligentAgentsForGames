using System.Collections.Generic;
using System.Timers;
using Levels;
using Pathfinding;
using Sensors;
using Tools;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// <p> MonoBehaviour to offer a hiding steering behavior. </p>
/// <p> Hiding makes an agent place itself after an obstacle between him and a
/// threat. </p>
/// </summary>
public class HideSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")] 
    [Tooltip("Agent to hide from.")]
    [SerializeField] private AgentMover threat;
    [Tooltip("Distance at which we give our goal as reached and we stop our agent.")]
    [SerializeField] private float arrivalDistance = 0.1f;
    [Tooltip("At which physics layers the obstacles belong to?")]
    [SerializeField] private LayerMask obstaclesLayer;
    [Tooltip("Maximum scene obstacles inner distance.")]
    [SerializeField] private float maximumInnerObstacleSpace = 3.0f;
    [Tooltip("How much separation our hiding point must show from obstacles?")]
    [SerializeField] private float separationFromObstacles = 1f;
    [Tooltip("How wide is the agent we want to hide?")] 
    [SerializeField] private float agentRadius = 0.5f;
    [Tooltip("Step length to advance the inner ray. The smaller value gives more " +
             "accuracy to calculate the hiding point but it's slower to calculate.")]
    [SerializeField] private float innerRayStep = 0.3f;
    [Tooltip("A position with any of this physic layers objects is not empty ground " +
             "to be a valid hiding point.")]
    [SerializeField] private LayerMask notEmptyGroundLayers;
    [Tooltip("Layer where threats are.")]
    [SerializeField] private LayerMask threatLayerMask;
    [Tooltip("Minimum time in seconds between hiding point path recalculations.")]
    [SerializeField] private float pathRecalculationTime = 0.5f;
    
    [Header("WIRING:")]
    [SerializeField] private RaySensor rayCastToThreat; 
    [SerializeField] private HidingPointsDetector hidingPointsDetector;
    [SerializeField] private MeshPathFinderSteeringBehavior meshPathFinderSteeringBehavior;
    
    
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
            if (rayCastToThreat != null)
                rayCastToThreat.SensorLayerMask = threatLayerMask | ObstaclesLayer;

        }
    }

    /// <summary>
    /// Distance at which we give our goal as reached, and we stop our agent.
    /// </summary>
    public float ArrivalDistance
    {
        get => arrivalDistance;
        set => arrivalDistance = value;
    }

    /// <summary>
    /// At which physics layers do the obstacles belong to?
    /// </summary>
    public LayerMask ObstaclesLayer
    {
        get => obstaclesLayer;
        set
        {
            obstaclesLayer = value;
            if (hidingPointsDetector != null) 
                hidingPointsDetector.ObstaclesLayer = value;
            InitRayCast();
        }
    }

    /// <summary>
    /// How much separation must our hiding point show from obstacles?
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
    /// A position with any of these physic layers objects is not empty ground to be a
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


    /// <summary>
    /// Defines the layer mask for identifying threats in the environment.
    /// </summary>
    public LayerMask ThreatLayerMask
    {
        get => threatLayerMask;
        set
        {
            threatLayerMask = value;
            InitRayCast();
        }
    }

    private Vector2 _hidingPoint;
    /// <summary>
    /// Current hiding point position selected by this behavior.
    /// </summary>
    private Vector2 HidingPoint
    {
        get => _hidingPoint;
        set
        {
            _hidingPoint = value;
            meshPathFinderSteeringBehavior.TargetPosition = value;
        }
    }

    /// <summary>
    /// Indicates whether the threat currently has a clear line of sight to the agent.
    /// </summary>
    public bool ThreatCanSeeUs => _threatCanSeeUs;
    
    private Courtyard _currentLevel;
    private Vector2 _previousThreatPosition = Vector2.zero;

    private bool ThreatHasJustMoved => 
        (Vector2)Threat.transform.position != _previousThreatPosition;

    // private bool HidingPointReached => meshPathFinderSteeringBehavior.IsPathEnded;
    
    private bool _threatCanSeeUs;
    private bool _hidingPointRecheckNeeded;
    private bool _hidingPointReached;
    private Timer _pathRecalculationTimer;
    private bool _pathRecalculationCooldownActive;

    private void Awake()
    {
        HidingPoint = transform.position;
        _pathRecalculationTimer = new Timer(pathRecalculationTime * 1000);
        _pathRecalculationTimer.Elapsed += OnRecalculationPathTimerTimeout;
        _pathRecalculationTimer.AutoReset = false;
    }

    private void Start()
    {
        _currentLevel = FindAnyObjectByType<Courtyard>();
        if (_currentLevel == null) return;
        InitRayCast();
        InitHidingPointDetector();
        InitNavigationAgent();
    }

    private void OnRecalculationPathTimerTimeout(object sender, ElapsedEventArgs e)
    {
        _pathRecalculationCooldownActive = false;
    }

    private void StartPathRecalculationTimer()
    {
        _pathRecalculationTimer.Stop();
        _pathRecalculationTimer.Start();
        _pathRecalculationCooldownActive = true;
    }

    private void InitRayCast()
    {
        if (rayCastToThreat == null) return;
        if (Threat != null) 
            rayCastToThreat.SensorLayerMask = threatLayerMask | ObstaclesLayer;
        rayCastToThreat.IgnoreCollidersOverlappingStartPoint = true;
    }

    private void InitHidingPointDetector()
    {
        if (Threat != null) hidingPointsDetector.Threat = Threat.gameObject;
        hidingPointsDetector.ObstaclesPositions = _currentLevel.ObstaclePositions;
        hidingPointsDetector.ObstaclesLayer = ObstaclesLayer;
        hidingPointsDetector.SeparationFromObstacles = SeparationFromObstacles;
        hidingPointsDetector.AgentRadius = AgentRadius;
        hidingPointsDetector.NotEmptyGroundLayers = NotEmptyGroundLayers;
        hidingPointsDetector.MaximumAdvanceAfterCollision = maximumInnerObstacleSpace;
        hidingPointsDetector.InnerRayStep = innerRayStep;
    }

    private void InitNavigationAgent()
    {
        meshPathFinderSteeringBehavior.TargetPosition = HidingPoint;
        meshPathFinderSteeringBehavior.agentRadius = AgentRadius;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (Threat == null || rayCastToThreat == null) return SteeringOutput.Zero;
        
        // Check if there is a line of sight with the threat.
        CheckThreatVisibility();

        // Starting threat position counts as ThreatHasJustMoved because
        // _previousThreatPosition is init as Vector2.Zero.
        if ((ThreatHasJustMoved && !_pathRecalculationCooldownActive) ||
            (ThreatCanSeeUs && _hidingPointReached))
        {
            _hidingPointRecheckNeeded = true;
        }
        _previousThreatPosition = Threat.transform.position;
        
        // Look for a new hiding point if the threat can see us and has just moved, or
        // if it is threat-first position (only once).
        if (_threatCanSeeUs && _hidingPointRecheckNeeded && 
            !_pathRecalculationCooldownActive) 
        {   // Search for the nearest hiding point.
            List<Vector2> hidingPoints = hidingPointsDetector.HidingPoints;
            if (hidingPoints.Count > 0)
            {
                float minimumDistance = float.MaxValue;
                Vector2 nearestHidingPoint = Vector2.zero;
                foreach (Vector2 candidatePoint in hidingPoints)
                {
                    PathData pathToCandidatePoint = 
                        meshPathFinderSteeringBehavior.
                            CurrentPathFinder.FindPath(candidatePoint);
                    float currentDistance = pathToCandidatePoint.PathLength;
                    if (currentDistance < minimumDistance)
                    {
                        minimumDistance = currentDistance;
                        nearestHidingPoint = candidatePoint;
                    }
                }
                HidingPoint = nearestHidingPoint;
                _hidingPointReached = false;
                _hidingPointRecheckNeeded = false;
            }
            // A path recalculation cooldown is needed, or the path will be recalculated
            // repeatedly while the threat moves without giving a useful hiding path until
            // the threat stops for the first time.
            StartPathRecalculationTimer();
        }
        
        if (Vector2.Distance(HidingPoint, transform.position) < arrivalDistance)
            _hidingPointReached = true;

        if (_hidingPointReached)
        {
            // If we don't need to hide, then return zero.
            return SteeringOutput.Zero;
        }
        return meshPathFinderSteeringBehavior.GetSteering(args);
    }

    private void CheckThreatVisibility()
    {
        rayCastToThreat.EndPosition = Threat.transform.position;
        if (rayCastToThreat.IsColliderDetected)
        {
            _threatCanSeeUs = 
                rayCastToThreat.DetectedCollider.gameObject == Threat.gameObject;
        }
        else
        {
            _threatCanSeeUs = false;
        }
    }
}
}
