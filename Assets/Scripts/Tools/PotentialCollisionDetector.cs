using System.Collections.Generic;
using Sensors;
using SteeringBehaviors;
using UnityEngine;

namespace Tools
{
/// <summary>
/// <p>Script to detect potential collisions with other agents nearby.</p>
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
    [SerializeField] private ConeSensor sensor;

    /// <summary>
    /// Range to detect possible collisions.
    /// </summary>
    public float DetectionRange
    {
        get => sensor.DetectionRange;
        set => sensor.DetectionRange = value;
    }
    
    /// <summary>
    /// Semicone angle for detection (in degrees).
    /// </summary>
    public float DetectionSemiconeAngle
    {
        get => sensor.DetectionSemiconeAngle;
        set => sensor.DetectionSemiconeAngle = value;
    }
    
    /// <summary>
    /// Specifies the physics layers that the will monitor for potential collisions.
    /// </summary>
    public LayerMask LayersToDetect
    {
        get => sensor.LayersToDetect;
        set => sensor.LayersToDetect = value;
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
    
    /// <summary>
    /// Whether there is a near agent on a potential collision heading.
    /// </summary>
    public bool PotentialCollisionDetected { get; private set; }
    
    /// <summary>
    /// Agent whose heading con collide with us.
    /// </summary>
    public AgentMover PotentialCollisionAgent { get; private set; }
    
    /// <summary>
    /// Time to potential collision with the other agent.
    /// </summary>
    public float TimeToPotentialCollision {get; private set; }
    
    /// <summary>
    /// Vector from us to the agent we can collide with at the point with minimum
    /// separation.
    /// </summary>
    public Vector2 RelativePositionAtPotentialCollision {get; private set; }
    
    /// <summary>
    /// Separation between us and the agent we can collide with at the point with minimum
    /// separation.
    /// </summary>
    public float SeparationAtPotentialCollision {get; private set; }
    
    /// <summary>
    /// Present vector from us to the agent we can collide with.
    /// </summary>
    public Vector2 CurrentRelativePositionToPotentialCollisionAgent {get; private set; }
    
    /// <summary>
    /// Difference between agent we can collide with velocity and our own velocity. 
    /// </summary>
    public Vector2 CurrentRelativeVelocityToPotentialCollisionAgent {get; private set; }
    
    /// <summary>
    /// Present distance from us to the agent we can collide with.
    /// </summary>
    public float CurrentDistanceToPotentialCollisionAgent => 
        CurrentRelativePositionToPotentialCollisionAgent.magnitude;
    
    /// <summary>
    /// Minimum distance under with we assume two agents have collided.
    /// </summary>
    public float CollisionDistance { get; private set; }
    
    private AgentMover _currentAgent;
    private HashSet<AgentMover> _detectedAgents = new();

    /// <summary>
    /// Event handler to use when another agent enters our detection area.
    /// </summary>
    /// <param name="otherObject">The agent who enters our detection area.</param>
    public void OnObjectEnteredSensor(GameObject otherObject)
    {
        AgentMover otherAgentMover = otherObject.GetComponent<AgentMover>();
        
        if (otherAgentMover == null) return;
        
        _detectedAgents.Add(otherAgentMover);
    }

    /// <summary>
    /// Event handler to use when an agent stays within the detection area.
    /// </summary>
    /// <param name="otherObject">The agent that remains within the detection
    /// area.</param>
    public void OnObjectStaySensor(GameObject otherObject)
    {
        AgentMover otherAgentMover = otherObject.GetComponent<AgentMover>();
        
        if (otherAgentMover == null) return;
        
        if (_detectedAgents.Contains(otherAgentMover)) return;
        _detectedAgents.Add(otherAgentMover);
    }

    /// <summary>
    /// Event handler to use when another agent exits our detection area.
    /// </summary>
    /// <param name="otherObject">The agent who exits our detection area.</param>
    public void OnObjectExitedSensor(GameObject otherObject)
    {
        AgentMover otherAgentMover = otherObject.GetComponent<AgentMover>();
        
        if (otherAgentMover == null ||
            !_detectedAgents.Contains(otherAgentMover)) return;
        
        _detectedAgents.Remove(otherAgentMover);
    }

    /// <summary>
    /// Event handler to use when the cone sensor changes its dimensions.
    /// </summary>
    /// <param name="range">New range.</param>
    /// <param name="semiConeDegrees">New semiConeDegrees.</param>
    public void OnSensorDimensionsChanged(float range, float semiConeDegrees)
    {
        if (!Mathf.Approximately(detectionRange, range)) 
            detectionRange = range;
        if (!Mathf.Approximately(detectionSemiconeAngle, semiConeDegrees)) 
            detectionSemiconeAngle = semiConeDegrees;
    }
    
    private void UpdateSensorConfiguration()
    {
        sensor.DetectionRange = detectionRange; 
        sensor.DetectionSemiconeAngle = detectionSemiconeAngle;
        sensor.LayersToDetect = layersToDetect;
    }
    
    private void Awake()
    {
        _currentAgent = GetComponentInParent<AgentMover>();
        CollisionDistance = 2 * agentRadius;
    }

    private void Start()
    {
        UpdateSensorConfiguration();
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
            
            // I've used Millington algorithm as a reference, but here mine differs from
            // his. Millington algorithm uses de positive dot product between
            // relativePosition and relativeVelocity. I guess it's an error.
            // In my calculations, that would get a positive result for a non-collision
            // approach, that wouldn't be correct because timeToClosestPosition should
            // be negative if agents go away from each other and positive if they go
            // towards each other.
            // Besides, I've found sources where this formula is defined, and they 
            // multiply by -1.0 the numerator:
            // https://medium.com/@knave/collision-avoidance-the-math-1f6cdf383b5c
            //
            // So, I've multiplied by -1.0 the numerator.
            float timeToClosestPosition = Vector2.Dot(
                                              -relativePosition, 
                                              relativeVelocity) / 
                                          Mathf.Pow(relativeSpeed, 2.0f);
            
            // They are moving away, so no collision possible.
            if (timeToClosestPosition < 0) continue;

            // Here too, my implementation differs from Millington's. He calculates
            // miSeparation substracting relativeSpeed * timeToClosestPosition from the 
            // modulus of relative position. My tests show that you must do instead the
            // operations summing with vectors and later get the module.
            Vector2 minRelativePosition =
                relativePosition + relativeVelocity * timeToClosestPosition; 
            float minSeparation = minRelativePosition.magnitude;
            
            // If minSeparation is greater than _collisionDistance, then we have no
            // collision at all, so we assess the next target.
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
        UpdateSensorConfiguration();
    }
#endif
}
}

