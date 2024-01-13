using UnityEngine;


/// <summary>
/// Monobehaviour to offer a Wandering steering behaviour.
/// </summary>
[RequireComponent(typeof(SeekSteeringBehavior))]
public class WanderingSteeringBehavior : SteeringBehavior
{
    [Header("WIRING:")] 
    [SerializeField] private SeekSteeringBehavior seekSteeringBehaviour; 
    
    [Header("CONFIGURATION:")]
    [Tooltip("Prefab used to mark next position to reach.")]
    [SerializeField] private GameObject marker;
    [Tooltip("Make visible position marker.")] 
    [SerializeField] private bool positionMarkerVisible = true;
    [Tooltip("Distance at which we give our goal as reached and we stop our agent.")]
    [SerializeField] private float arrivalDistance;
    [Tooltip("This is the radius of the constraining circle. KEEP IT UNDER wanderDistance!")] 
    [SerializeField] private float wanderRadius;
    [Tooltip("This is the distance the wander ircle is projected in front of the agent. KEEP IT OVER wanderRadius!")]
    [SerializeField] private float wanderDistance;
    [Tooltip("Maximum amount of random displacement that can be added to the target each second.")]
    [SerializeField] private float wanderJitter;
    
    private GameObject _marker;
    // private Vector2 _markerPosition;
    // private Vector2 _wanderPosition;
    private Vector2 _wanderLocalPosition;

    private void Awake()
    {
        _marker = Instantiate(marker, Vector2.zero, Quaternion.identity);
        seekSteeringBehaviour.target = _marker;
        seekSteeringBehaviour.arrivalDistance = arrivalDistance;
        
        // WanderPosition is a point constrained to the edge of a circle of radius wanderRadius.
        _wanderLocalPosition = GetRandomCircunferencePoint(Vector2.zero, 
            wanderRadius);
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
        // Add random displacement over an area of a circle or radius wanderJitter.
        _wanderLocalPosition += Random.insideUnitCircle * wanderJitter;
        
        // Reproject this new vector back onto a unit circle.
        _wanderLocalPosition = _wanderLocalPosition.normalized * wanderRadius;
        
        // Create a targetLocal into a position WanderDist distance in front of the agent.
        // Remember Y local axis is our forward axis.
        Vector2 targetLocal = _wanderLocalPosition + new Vector2(0, wanderDistance);
        
        // Place targetLocal as relative to agent.
        _marker.transform.position = args.CurrentAgent.transform.TransformPoint(targetLocal);
        
        return seekSteeringBehaviour.GetSteering(args);
    }
    
}