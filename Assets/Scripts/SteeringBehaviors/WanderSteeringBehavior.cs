using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


/// <summary>
/// <p>Monobehaviour to offer a wandering steering behaviour.</p>
///
/// <p>Wandering steering behaviour makes the agent move randomly around the scene.</p>
/// </summary>
[RequireComponent(typeof(SeekSteeringBehavior))]
public class WanderSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("Distance at which we give our goal as reached and we stop our agent.")]
    [SerializeField] private float arrivalDistance;
    [Tooltip("This is the radius of the constraining circle. KEEP IT UNDER " +
             "wanderDistance!")] 
    [SerializeField] private float wanderRadius;
    [Tooltip("This is the distance the wander circle is projected in front of the " +
             "agent. KEEP IT OVER wanderRadius!")]
    [SerializeField] private float wanderDistance;
    [Tooltip("Maximum amount of random displacement that can be added to the target " +
             "each second. KEEP IT OVER wanderRadius.")]
    [SerializeField] private float wanderJitter;
    [Tooltip("Time in seconds to recalculate the wander position.")]
    [SerializeField] private float wanderRecalculationTime;
    
    [Header("DEBUG:")]
    [Tooltip("Make visible position marker.")] 
    [SerializeField] private bool predictedPositionMarkerVisible = true;

    /// <summary>
    /// Distance at which we give our goal as reached and we stop our agent.
    /// </summary>
    public float ArrivalDistance
    {
        get => arrivalDistance;
        set
        {
            arrivalDistance = value;
            if (_seekSteeringBehaviour != null)
                _seekSteeringBehaviour.ArrivalDistance = value;
        }
    }

    /// <summary>
    /// This is the radius of the constraining circle. KEEP IT UNDER wanderDistance!
    /// </summary>
    public float WanderRadius
    {
        get => wanderRadius;
        set
        {
            wanderRadius = value;
        }
    }

    /// <summary>
    /// This is the distance the wander circle is projected in front of the agent. KEEP
    /// IT OVER wanderRadius!
    /// </summary>
    public float WanderDistance
    {
        get => wanderDistance;
        set
        {
            wanderDistance = value;
        }
    }

    /// <summary>
    /// Maximum amount of random displacement that can be added to the target each
    /// second. KEEP IT OVER wanderRadius.
    /// </summary>
    public float WanderJitter
    {
        get => wanderJitter;
        set
        {
            wanderJitter = value;
        }
    }

    /// <summary>
    /// Time in seconds to recalculate the wander position.
    /// </summary>
    public float WanderRecalculationTime
    {
        get => wanderRecalculationTime;
        set
        {
            wanderRecalculationTime = value;
        }
    }
    
    private GameObject _marker;
    private Vector2 _wanderLocalPosition;

    private SeekSteeringBehavior _seekSteeringBehaviour;
    private SteeringBehaviorArgs _currentSteeringBehaviorArgs;

    private Color _agentColor;

    private void Awake()
    {
        _marker = new GameObject();
        _seekSteeringBehaviour = GetComponent<SeekSteeringBehavior>();
        _seekSteeringBehaviour.Target = _marker;
        _seekSteeringBehaviour.ArrivalDistance = arrivalDistance;
        
        _agentColor = GetComponent<AgentColor>().Color;
        
        // Place WanderPosition in a point constrained to the edge of a circle of
        // radius wanderRadius.
        _wanderLocalPosition = GetRandomCircunferencePoint(Vector2.zero, 
            wanderRadius);
    }

    private void Start()
    {
        // Run wander position update every wanderRecalculationTime seconds.
        InvokeRepeating(
            nameof(WanderPositionUpdate), 
            0f, 
            wanderRecalculationTime);
    }

    private void OnDestroy()
    {
        Destroy(_marker);
    }

    /// <summary>
    /// Get a random point on the edge of a circle.
    /// </summary>
    /// <param name="center">Center of a circle.</param>
    /// <param name="radius">Radius of the circle.</param>
    /// <returns>Random point.</returns>
    private static Vector2 GetRandomCircunferencePoint(Vector2 center, float radius)
    {
        return center + Random.insideUnitCircle.normalized * radius;
    } 
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        _currentSteeringBehaviorArgs = args;

        return _seekSteeringBehaviour.GetSteering(args);
    }

    /// <summary>
    /// Update the wander position based on the given steering behavior arguments.
    /// </summary>
    /// <param name="args">Steering behavior arguments</param>
    private void WanderPositionUpdate()
    {
        if (_currentSteeringBehaviorArgs == null) return;
        
        SteeringBehaviorArgs args = _currentSteeringBehaviorArgs;
        
        // Add random displacement over an area of a circle of radius wanderJitter. This
        // circle is around current wander local position.
        _wanderLocalPosition += Random.insideUnitCircle * WanderJitter;
        
        // Reproject this new vector back onto a unit circle. This circle is around
        // current agent.
        _wanderLocalPosition = _wanderLocalPosition.normalized * WanderRadius;
        
        // Create a targetLocal into a position WanderDist distance in front of the agent.
        // Remember Y local axis is our forward axis.
        Vector2 targetLocal = _wanderLocalPosition + new Vector2(0, WanderDistance);
        
        // Place targetLocal as relative to agent.
        _marker.transform.position = args.CurrentAgent.transform.TransformPoint(targetLocal);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (predictedPositionMarkerVisible && _marker != null)
        {
            Vector3 _markerPosition = _marker.transform.position;
            Gizmos.color = _agentColor;
            Gizmos.DrawWireSphere(_markerPosition, 0.2f);
            Gizmos.DrawLine(_markerPosition, transform.position);
        }
    }
#endif    
}