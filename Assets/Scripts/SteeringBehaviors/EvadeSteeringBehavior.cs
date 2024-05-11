using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Monobehaviour to offer a Pursuit steering behaviour.
/// </summary>
[RequireComponent(typeof(FleeSteeringBehavior))]
public class EvadeSteeringBehavior : SteeringBehavior
{
    private const float MinimumPanicDistance = 0.3f;
    
    [FormerlySerializedAs("seekSteeringBehaviour")]
    [Header("WIRING:")] 
    [SerializeField] private FleeSteeringBehavior fleeSteeringBehaviour;
    
    [Header("CONFIGURATION:")]
    [Tooltip("Agent to run from.")]
    [SerializeField] private GameObject threathAgent;
    [Tooltip("Minimum distance to threath before fleeing.")]
    [Min(MinimumPanicDistance)]
    [SerializeField] private float panicDistance;
    
    [Header("DEBUG:")]
    [Tooltip("Make visible position marker.")] 
    [SerializeField] private bool predictedPositionMarkerVisible = true;
    
    private Rigidbody2D _threathRigidBody;
    private Vector2 _threathPosition;
    private GameObject _currentThreath;

    private float _cosAheadSemiConeRadians;
    private float _cosComingToUsSemiConeRadians;
    private GameObject _predictedPositionMarker;
    
    private Color _agentColor;
    private Color _targetColor;

    public GameObject Threath
    {
        get => threathAgent;
        set => threathAgent = value;
    }
    
    public float PanicDistance
    {
        get => panicDistance;
        set
        {
            panicDistance = Mathf.Max(MinimumPanicDistance, value);
            fleeSteeringBehaviour.PanicDistance = panicDistance;
        }
    }
    
    private void Start()
    {
        fleeSteeringBehaviour.Threath = threathAgent;
        fleeSteeringBehaviour.PanicDistance = panicDistance;
        _predictedPositionMarker = new GameObject();
        _agentColor = GetComponent<AgentColor>().Color;
        _targetColor = threathAgent.GetComponent<AgentColor>().Color;
    }

    /// <summary>
    /// Load target data.
    /// </summary>
    private void UpdateThreathData()
    {
        if (threathAgent != _currentThreath)
        {
            _threathRigidBody = threathAgent.GetComponentInChildren<Rigidbody2D>();
            _currentThreath = threathAgent;
        }
        _threathPosition = threathAgent.transform.position;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        UpdateThreathData();
        Vector2 currentPosition = args.Position;
        float maximumSpeed = args.MaximumSpeed;

        Vector2 toThreath = _threathPosition - currentPosition;
        
        //The look-ahead time is proportional to the distance between the evader
        //and the pursuer; and is inversely proportional to the sum of the
        //agents' velocities
        float lookAheadTime = toThreath.magnitude / (maximumSpeed + _threathRigidBody.velocity.magnitude);
        _predictedPositionMarker.transform.position = _threathPosition + _threathRigidBody.velocity * lookAheadTime;
        fleeSteeringBehaviour.Threath = _predictedPositionMarker;
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
            Gizmos.DrawLine(threathAgent.transform.position, _predictedPositionMarker.transform.position);
        }
    }
}