using System;
using System.Collections.Generic;
using Sensors;
using SteeringBehaviors;
using UnityEngine;

namespace Tools
{
/// <summary>
/// <p>Script to detect future collisions with other agents nearby.</p>
/// </summary>
[ExecuteAlways]
public class PotentialCollisionDetector : MonoBehaviour
{
    [Header("CONFIGURATION:")]
    [Tooltip("Range to detect possible collisions.")]
    [SerializeField] private float detectionRange = 10f;
    [Tooltip("Semicone angle for detection (in degrees).")]
    [Range(0f, 90f)]
    [SerializeField] private float detectionSemiconeAngle = 45f;
    [Tooltip("Specifies the physics layers that the will monitor for potential " +
             "collisions.")]
    [SerializeField] private LayerMask layersToDetect;
    [Tooltip("Represents the radius of an agent used for potential collision detection.")]
    [SerializeField] private float agentRadius;
    
    [Header("WIRING:")]
    [SerializeField] private GameObject sensor;
    [SerializeField] private ConeRange2D coneRange;

    /// <summary>
    /// Range to detect possible collisions.
    /// </summary>
    public float DetectionRange
    {
        get => detectionRange;
        set
        {
            detectionRange = value;
            UpdateDetectionArea();
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
    /// Represents the radius of an agent used for potential collision detection.
    /// </summary>
    public float AgentRadius
    {
        get => agentRadius;
        set
        {
            agentRadius = value;
            CollisionDistance = 2 * agentRadius;
        }
    }
    public bool PotentialCollisionDetected { get; private set; }
    
    public AgentMover PotentialCollisionAgent { get; private set; }
    
    public float TimeToPotentialCollision {get; private set; }
    
    public Vector2 RelativePositionAtPotentialCollision {get; private set; }
    
    public float SeparationAtPotentialCollision {get; private set; }
    
    public Vector2 CurrentRelativePositionToPotentialCollisionAgent {get; private set; }
    
    public Vector2 CurrentRelativeVelocityToPotentialCollisionAgent {get; private set; }
    
    public float CurrentDistanceToPotentialCollisionAgent => 
        CurrentRelativePositionToPotentialCollisionAgent.magnitude;
    
    public float CollisionDistance { get; private set; }
    
    private AgentMover _currentAgent;
    private List<AgentMover> _detectedAgents;
    private BoxRangeManager _boxRangeManager;
    private VolumetricSensor _volumetricSensor;

    /// <summary>
    /// Whether provided object layer is included in the provided LayerMask.
    /// </summary>
    /// <param name="obj">Object to check.</param>
    /// <param name="layerMask">List of layers.</param>
    /// <returns>True if object's layer is in layermask.</returns>
    private bool ObjectIsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layersToDetect.value & (1 << obj.layer)) != 0;
    }

    /// <summary>
    /// Whether a global position is inside cone range of the agent.
    /// </summary>
    /// <param name="position">Global position to check.</param>
    /// <returns>True if position is inside the cone./returns>
    private bool PositionIsInConeRange(Vector2 position)
    {
        float distance = Vector2.Distance(position, _currentAgent.transform.position);
        float heading = Vector2.Angle(_currentAgent.Forward,
            _currentAgent.transform.InverseTransformPoint(position));
        return distance <= DetectionRange && heading <= DetectionSemiconeAngle;
    }
    
    public void OnConeRangeUpdated(float range, float semiConeDegrees)
    {
        DetectionRange = range;
        DetectionSemiconeAngle = semiConeDegrees;
    }

    public void OnAgentAreaEntered(GameObject otherAgent)
    {
        if (ObjectIsInLayerMask(otherAgent, LayersToDetect))
        {
            AgentMover otherAgentMover = otherAgent.GetComponent<AgentMover>();
            
            if (otherAgentMover == null || 
                !PositionIsInConeRange(otherAgentMover.transform.position)) 
                return;
            
            _detectedAgents.Add(otherAgentMover);
        }
    }

    public void OnAgentAreaExited(GameObject otherAgent)
    {
        if (ObjectIsInLayerMask(otherAgent, LayersToDetect))
        {
            AgentMover otherAgentMover = otherAgent.GetComponent<AgentMover>();
            
            if (otherAgentMover == null ||
                !_detectedAgents.Contains(otherAgentMover)) return;
            
            _detectedAgents.Remove(otherAgentMover);
        }
    }
    
    private void UpdateDetectionArea()
    {
        if (_boxRangeManager == null) return;
        _boxRangeManager.Range = DetectionRange;
        _boxRangeManager.Width = DetectionRange * 
                                 Mathf.Sin(DetectionSemiconeAngle * 
                                           Mathf.Deg2Rad) * 2;
    }
    
    private void Awake()
    {
        _currentAgent = GetComponentInParent<AgentMover>();
        CollisionDistance = 2 * agentRadius;
        _boxRangeManager = sensor.GetComponent<BoxRangeManager>();
        _volumetricSensor = sensor.GetComponent<VolumetricSensor>();
    }

    private void Start()
    {
        coneRange.Initialize(detectionRange, detectionSemiconeAngle);
        UpdateDetectionArea();
    }

    private void FixedUpdate()
    {
        PotentialCollisionDetected = false;
        if (_detectedAgents.Count == 0) return;
        
        float shortestTimeToCollision = float.MaxValue;
        Vector2 relativePositionAtPotentialCollision = Vector2.zero;
        float minSeparationAtClosestCollisionCandidate = float.MaxValue;
        AgentMover closestCollidingAgentCandidate = null;
        Vector2 currentRelativePositionToPotentialCollisionAgent = Vector2.zero;
        Vector2 currentRelativeVelocityToPotentialCollisionAgent = Vector2.zero;

        foreach (AgentMover target in _detectedAgents)
        {
            // Calculate time to collision.
            Vector2 relativePosition = target.transform.position - 
                                       _currentAgent.transform.position;
            Vector2 relativeVelocity = target.Velocity - _currentAgent.Velocity;
            float relativeSpeed = relativeVelocity.magnitude;
            
            // I've used Millington algorithm as reference, but here mine differs his.
            // Millington algorithm uses de positive dot product between relativePosition
            // and relativeVelocity. I guess it's an error because, in my calculations,
            // that would get a positive result for a non-collision approach,
            // that wouldn't be correct because timeToClosestPosition should be negative
            // if agents go away from each other and positive if they go towards each
            // other.
            // Besides, I've found sources where this formula is defined and they 
            // multiply by -1.0 the numerator:
            // https://medium.com/@knave/collision-avoidance-the-math-1f6cdf383b5c
            //
            // So, I've multiplied by -1.0 the numerator.
            float timeToClosestPosition = Vector2.Dot(
                                              -relativePosition, 
                                              relativeVelocity) / 
                                          (float) Mathf.Pow(relativeSpeed, 2.0f);
            
            // They are moving away, so no collision possible.
            if (timeToClosestPosition < 0) continue;

            // Here too, my implementation differs from Millington's. He calculates
            // miSeparation substracting relativeSpeed * timeToClosestPosition from the 
            // modulus of relative position. My tests show that you must do instead the
            // operations summing with vectors and later get the module.
            Vector2 minRelativePosition =
                relativePosition + relativeVelocity * timeToClosestPosition; 
            float minSeparation = minRelativePosition.magnitude;
            
            // If minSeparation is greater than _collisionDistance then we have no
            // collision at all, so we assess next target.
            if (minSeparation > CollisionDistance) continue;
            
            // OK, we have a candidate potential collision, but is it the nearest?
            if (timeToClosestPosition < shortestTimeToCollision)
            {
                shortestTimeToCollision = timeToClosestPosition;
                closestCollidingAgentCandidate = target;
                relativePositionAtPotentialCollision = minRelativePosition;
                minSeparationAtClosestCollisionCandidate = minSeparation;
                currentRelativePositionToPotentialCollisionAgent = relativePosition;
                currentRelativeVelocityToPotentialCollisionAgent = relativeVelocity;
            }
        }
        
        // Offer data of the current nearest agent collision candidate.
        TimeToPotentialCollision = shortestTimeToCollision;
        RelativePositionAtPotentialCollision = relativePositionAtPotentialCollision;
        SeparationAtPotentialCollision = minSeparationAtClosestCollisionCandidate;
        PotentialCollisionAgent = closestCollidingAgentCandidate;
        CurrentRelativePositionToPotentialCollisionAgent = 
            currentRelativePositionToPotentialCollisionAgent;
        CurrentRelativeVelocityToPotentialCollisionAgent = 
            currentRelativeVelocityToPotentialCollisionAgent;
        PotentialCollisionDetected = PotentialCollisionAgent != null;
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateDetectionArea();
    }
#endif
}
}

