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
    public GameObject threathAgent;
    [Tooltip("Prefab used to mark next position to reach by pursuer.")]
    [SerializeField] private GameObject positionMarker;
    [Tooltip("Minimum distance to threath before fleeing.")]
    [Min(MinimumPanicDistance)]
    [SerializeField] private float panicDistance = 1.0f;
    // public float panicDistance = 1.0f;
    
    private Rigidbody2D _threathRigidBody;
    private Vector2 _threathPosition;
    private GameObject _currentThreath;

    private float _cosAheadSemiConeRadians;
    private float _cosComingToUsSemiConeRadians;
    private GameObject _positionMarker;

    public float PanicDistance
    {
        get => panicDistance;
        set
        {
            panicDistance = Mathf.Max(MinimumPanicDistance, value);
            fleeSteeringBehaviour.panicDistance = panicDistance;
        }
    }
    
    private void Start()
    {
        fleeSteeringBehaviour.threath = positionMarker;
        fleeSteeringBehaviour.panicDistance = panicDistance;
        _positionMarker = Instantiate(positionMarker, Vector2.zero, Quaternion.identity);
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
        _positionMarker.transform.position = _threathPosition + _threathRigidBody.velocity * lookAheadTime;
        fleeSteeringBehaviour.threath = _positionMarker;
        return fleeSteeringBehaviour.GetSteering(args);

    }
}