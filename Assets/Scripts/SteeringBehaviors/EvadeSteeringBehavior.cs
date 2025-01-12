using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// <p>Monobehaviour to offer a evade steering behaviour.</p>
/// <p>Evade steering behaviour makes the agent go away from another GameObject marked
/// as threath.</p>
/// </summary>
[RequireComponent(typeof(FleeSteeringBehavior))]
public class EvadeSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("Agent to run from.")]
    // TODO: Theath is mispelled. Fix it to threat.
    [SerializeField] private AgentMover threathAgent;
    [Tooltip("Minimum distance to threath before fleeing.")]
    [SerializeField] private float panicDistance;
    
    [Header("DEBUG:")]
    [Tooltip("Make visible position marker.")] 
    [SerializeField] private bool predictedPositionMarkerVisible = true;
    
    private FleeSteeringBehavior _fleeSteeringBehaviour;
    private GameObject _predictedPositionMarker;
    
    private Color _agentColor;
    private Color _targetColor;

    public AgentMover Threath
    {
        get => threathAgent;
        set
        {
            threathAgent = value;
            _targetColor = threathAgent.GetComponent<AgentColor>().Color;
        }
    }
    
    public float PanicDistance
    {
        get => panicDistance;
        set
        {
            panicDistance = value;
            _fleeSteeringBehaviour.PanicDistance = panicDistance;
        }
    }

    private void Awake()
    {
        _predictedPositionMarker = new GameObject();
        _fleeSteeringBehaviour = GetComponent<FleeSteeringBehavior>();
        _fleeSteeringBehaviour.PanicDistance = PanicDistance;
        _fleeSteeringBehaviour.Threath = _predictedPositionMarker;
        _agentColor = GetComponent<AgentColor>().Color;
    }

    private void Start()
    {
        if (Threath != null) _targetColor = Threath.GetComponent<AgentColor>().Color;
    }
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (Threath == null) return new SteeringOutput(Vector2.zero, 0);
        
        Vector2 currentPosition = args.Position;
        float maximumSpeed = args.MaximumSpeed;
        Vector2 threathPosition = Threath.transform.position;

        Vector2 toThreath = threathPosition - currentPosition;
        
        // The look-ahead time is proportional to the distance between the evader
        // and the pursuer; and is inversely proportional to the sum of the
        // agent's velocities
        float lookAheadTime = toThreath.magnitude / 
                              (maximumSpeed + Threath.Velocity.magnitude);
        
        _predictedPositionMarker.transform.position = threathPosition + 
                                                      Threath.Velocity * lookAheadTime;

        return _fleeSteeringBehaviour.GetSteering(args);
    }
    
    private void OnDrawGizmos()
    {
        if (predictedPositionMarkerVisible && _predictedPositionMarker != null)
        {
            Gizmos.color = _agentColor;
            Gizmos.DrawLine(transform.position, _predictedPositionMarker.transform.position);
            Gizmos.DrawWireSphere(_predictedPositionMarker.transform.position, 0.3f);
            Gizmos.color = _targetColor;
            Gizmos.DrawLine(threathAgent.transform.position, _predictedPositionMarker.transform.position);
        }
    }
}