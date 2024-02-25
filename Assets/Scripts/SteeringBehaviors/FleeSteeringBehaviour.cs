using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Monobehaviour to offer a Flee steering behaviour.
/// </summary>
[RequireComponent(typeof(SeekSteeringBehavior))]
public class FleeSteeringBehavior : SteeringBehavior
{
    private const float MinimumPanicDistance = 0.3f;
    
    [Header("WIRING:")] 
    [SerializeField] private SeekSteeringBehavior seekSteeringBehaviour; 
    
    [Header("CONFIGURATION:")]
    public GameObject threath;
    [Tooltip("Minimum distance to threath before fleeing.")]
    [Min(MinimumPanicDistance)]
    [SerializeField] private float panicDistance;

    private GameObject _currentThreath;
    private Vector2 _threathPosition;
    
    public float PanicDistance
    {
        get => panicDistance;
        set
        {
            panicDistance = Mathf.Max(MinimumPanicDistance, value);
        }
    }
    
    private void Start()
    {
        seekSteeringBehaviour.target = threath;
    }
    
    /// <summary>
    /// Load target data.
    /// </summary>
    private void UpdateThreathData()
    {
        _currentThreath = threath;
        _threathPosition = _currentThreath.transform.position;
        seekSteeringBehaviour.target = _currentThreath;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        UpdateThreathData();
        if (Vector2.Distance(
                args.CurrentAgent.transform.position,
                _threathPosition) > panicDistance)
        { // Out of panic distance, so we stop accelerating.
            return new SteeringOutput();
        }
        else
        { // Threath inside panic distance, so run in the opposite direction seek would advise. 
            SteeringOutput approachSteeringOutput = seekSteeringBehaviour.GetSteering(args);
            SteeringOutput fleeSteeringOutput = new SteeringOutput(
                approachSteeringOutput.Linear * -1,
                approachSteeringOutput.Angular
            );
            return fleeSteeringOutput;
        }
    }

    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, PanicDistance);
    // }
}