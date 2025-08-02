using UnityEngine;
using UnityEngine.Serialization;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Monobehaviour to offer a evade steering behaviour.</p>
/// <p>Evade steering behaviour makes the agent go away from another GameObject marked
/// as threath.</p>
/// </summary>
public class EvadeSteeringBehavior : SteeringBehavior
{
    [FormerlySerializedAs("threathAgent")]
    [Header("CONFIGURATION:")]
    [Tooltip("Agent to run from.")]
    [SerializeField] private AgentMover threatAgent;
    [Tooltip("Minimum distance to threath before fleeing.")]
    [SerializeField] private float panicDistance;
    
    [Header("WIRING:")]
    [SerializeField] private FleeSteeringBehavior fleeSteeringBehaviour;

    [Header("DEBUG:")]
    [Tooltip("Make visible position marker.")] 
    [SerializeField] private bool predictedPositionMarkerVisible = true;
    
    private GameObject _predictedPositionMarker;

    private Color _agentColor;
    private Color _targetColor;

    public AgentMover Threat
    {
        get => threatAgent;
        set
        {
            threatAgent = value;
            _targetColor = threatAgent.GetComponent<AgentColor>().Color;
        }
    }

    public float PanicDistance
    {
        get => panicDistance;
        set
        {
            panicDistance = value;
            if (fleeSteeringBehaviour != null)
                fleeSteeringBehaviour.PanicDistance = panicDistance;
        }
    }

    private void Awake()
    {
        _predictedPositionMarker = new GameObject();
        fleeSteeringBehaviour.PanicDistance = PanicDistance;
        fleeSteeringBehaviour.Threath = _predictedPositionMarker;
        _agentColor = GetComponentInParent<AgentColor>().Color;
    }

    private void Start()
    {
        if (Threat != null) _targetColor = Threat.GetComponent<AgentColor>().Color;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (Threat == null) return SteeringOutput.Zero;
    
        Vector2 currentPosition = args.Position;
        float maximumSpeed = args.MaximumSpeed;
        Vector2 threathPosition = Threat.transform.position;

        Vector2 toThreath = threathPosition - currentPosition;
    
        // The look-ahead time is proportional to the distance between the evader
        // and the pursuer; and is inversely proportional to the sum of the
        // agent's velocities
        float lookAheadTime = toThreath.magnitude / 
                              (maximumSpeed + Threat.Velocity.magnitude);
    
        // Place the marker where we think the chaser will be at the look-ahead
        // time.
        _predictedPositionMarker.transform.position = threathPosition + 
                                                      Threat.Velocity * lookAheadTime;

        // Make the flee behavior go away from the predicted position.
        return fleeSteeringBehaviour.GetSteering(args);
    }

    private void OnDrawGizmos()
    {
        if (predictedPositionMarkerVisible && _predictedPositionMarker != null)
        {
            Gizmos.color = _agentColor;
            Gizmos.DrawLine(transform.position, _predictedPositionMarker.transform.position);
            Gizmos.DrawWireSphere(_predictedPositionMarker.transform.position, 0.3f);
            Gizmos.color = _targetColor;
            Gizmos.DrawLine(threatAgent.transform.position, _predictedPositionMarker.transform.position);
        }
    }
}
}