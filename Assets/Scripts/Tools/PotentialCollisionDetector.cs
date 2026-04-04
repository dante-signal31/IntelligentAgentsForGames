using System.Collections.Generic;
using System.Linq;
using PropertyAttribute;
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
    [Tooltip("Represents the radius of an agent used for potential collision detection.")]
    [SerializeField] private float agentRadius;
    
    [Header("WIRING:")]
    [Tooltip("Sensor to detect potential collisions.")]
    [InterfaceCompliant(typeof(ISensor))]
    [SerializeField] private MonoBehaviour iSensor;
    
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
    /// Agent whose heading can collide with us.
    /// </summary>
    public AgentMover PotentialCollisionAgent { get; private set; }
    
    /// <summary>
    /// Time for potential collision with the other agent.
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
    /// The present vector from us to the agent we can collide with.
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
    
    /// <summary>
    /// Indicates whether the system is currently evaluating any potential collisions.
    /// </summary>
    public bool EvaluatingPotentialCollision => _detectedAgents.Count > 0;
    
    private AgentMover _currentAgent;
    private readonly HashSet<AgentMover> _detectedAgents = new();
    private ISensor sensor;

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
        
        // Object may already be in the list if it entered the sensor area, but if it 
        // was inside when sensor started, then this method is the way to register it as
        // detected.
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
    /// Assess if the current agent can collide with any of the detected agents, with
    /// the given velocity and radius.
    /// </summary>
    /// <param name="currentVelocity">Velocity for the current agent.</param>
    /// <param name="currentRadius">Radius for the current agent.</param>
    /// <param name="collisionTime">If a potential collision is detected, returns time
    /// to suffer that collision if the given velocity is applied to the current agent.
    /// If no potential collision is detected, then a float.PositiveInfinity is
    /// returned in this parameter.</param>
    /// <returns>True if any potential collision is detected. False otherwise.</returns>
    public bool IsCollidingVelocity(
        Vector2 currentVelocity, 
        float currentRadius, 
        out float collisionTime)
    {
        List<AgentMover> otherAgents = GetDetectedAgents();
        foreach (AgentMover otherAgent in otherAgents)
        {
            if (IsGoingToHappenACollision(
                    otherAgent, 
                    currentVelocity,
                    currentRadius + otherAgent.Radius,
                    out var _, 
                    out var _, 
                    out var timeToClosestPosition, 
                    out var _, 
                    out var _))
            {
                collisionTime = timeToClosestPosition;
                return true;
            }
        }
        // No collision detected for that currentVelocity with that currentRadius.
        collisionTime = float.PositiveInfinity;
        return false;
    }
    
    /// <summary>
    /// Gets all agents detected by the sensor.
    /// </summary>
    /// <returns>An array with the MovingAgent components of detected agents.</returns>
    private List<AgentMover> GetDetectedAgents()
    {
        return new List<AgentMover>(
            sensor.DetectedObjects.Where(x => 
                    x.GetComponent<AgentMover>() != null && 
                    // Don't take in count our own agent.
                    (x.GetComponent<AgentMover>()).name != _currentAgent.name)
                .Select(x => x.GetComponent<AgentMover>())
                .ToArray());
    }


    /// <summary>
    /// Determines whether a collision is likely to occur between the current agent and
    /// a target agent based on their velocities and positions.
    /// </summary>
    /// <param name="target">The agent that is being checked for a potential
    /// collision.</param>
    /// <param name="currentVelocity">The velocity of the current agent.</param>
    /// <param name="collisionDistance">The minimum distance at which a collision is
    /// considered to occur.</param>
    /// <param name="relativePosition">The relative position vector between the current
    /// agent and the target.</param>
    /// <param name="relativeVelocity">The relative velocity vector between the current
    /// agent and the target.</param>
    /// <param name="timeToClosestPosition">The calculated time at which the two agents
    /// will be closest to each other, given their current velocities.</param>
    /// <param name="minRelativePosition">The relative position vector at the time of
    /// closest approach.</param>
    /// <param name="minSeparation">The minimum separation distance between the two
    /// agents at the time of closest approach.</param>
    /// <returns>
    /// True if a collision is predicted to occur, otherwise false.
    /// </returns>
    private bool IsGoingToHappenACollision(
        AgentMover target,
        Vector2 currentVelocity,
        float collisionDistance,
        out Vector2 relativePosition,
        out Vector2 relativeVelocity,
        out float timeToClosestPosition,
        out Vector2 minRelativePosition,
        out float minSeparation)
    {
        // Calculate time to collision.
        relativePosition = target.transform.position - 
                                   _currentAgent.transform.position;
        relativeVelocity = target.Velocity - currentVelocity;
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
        timeToClosestPosition = -1.0f * Vector2.Dot(
                                    relativePosition, 
                                    relativeVelocity) / 
                                Mathf.Pow(relativeSpeed, 2.0f);
        
        // Here too, my implementation differs from Millington's. He calculates
        // minSeparation subtracting relativeSpeed * timeToClosestPosition from the 
        // modulus of relative position. My tests show that you must do instead the
        // operations summing with vectors and later get the module.
        minRelativePosition = relativePosition + relativeVelocity * timeToClosestPosition; 
        minSeparation = minRelativePosition.magnitude;
        
        // They are moving away, so no collision possible.
        if (timeToClosestPosition < 0)
        {
            minSeparation = float.MaxValue;
            return false;
        }
        
        // If timeToClosestPosition is 0, then the two agents
        // If minSeparation is greater than _collisionDistance, then we have no
        // collision at all, so we assess the next target.
        if (minSeparation > collisionDistance) return false;
        return true;
    }

    private void Awake()
    {
        _currentAgent = GetComponentInParent<AgentMover>();
        CollisionDistance = 2 * agentRadius;
    }

    private void Start()
    {
        sensor = (ISensor) iSensor;
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
            // If it isn't going to happen with that target, then continue to the next
            // target.
            if (!IsGoingToHappenACollision(
                    target, 
                    _currentAgent.Velocity,
                    CollisionDistance,
                    out var relativePosition, 
                    out var relativeVelocity, 
                    out var timeToClosestPosition, 
                    out var minRelativePosition, 
                    out var minSeparation)) continue;
            
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
}
}

