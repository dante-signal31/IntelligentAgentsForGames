﻿using UnityEngine;

/// <summary>
/// <p>Monobehaviour to offer a Flee steering behaviour.</p>
///
/// <p>Flee steering behaviour makes the agent go away from another GameObject marked
/// as threath.</p>
/// </summary>
[RequireComponent(typeof(SeekSteeringBehavior))]
public class FleeSteeringBehavior : SteeringBehavior
{
    private const float MinimumPanicDistance = 0.3f;
    
    [Header("CONFIGURATION:")]
    [SerializeField] private GameObject threath;
    [Tooltip("Minimum distance to threath before fleeing.")]
    [Min(MinimumPanicDistance)]
    [SerializeField] private float panicDistance;
    
    [Header("WIRING:")] 
    [SerializeField] private SeekSteeringBehavior seekSteeringBehaviour; 
    
    public GameObject Threath
    {
        get => threath;
        set
        {
            threath = value;
            if (seekSteeringBehaviour != null) 
                seekSteeringBehaviour.Target = value;
        }
    }

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
        seekSteeringBehaviour.Target = threath;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (Vector2.Distance(
                args.CurrentAgent.transform.position,
                Threath.transform.position) > panicDistance)
        { // Out of panic distance, so we stop fleeing.
            return new SteeringOutput();
        }
        else
        { // Threat inside panic distance, so run in the opposite direction seek
          // would advise. 
            SteeringOutput approachSteeringOutput = 
                seekSteeringBehaviour.GetSteering(args);
            SteeringOutput fleeSteeringOutput = new SteeringOutput(
                -approachSteeringOutput.Linear,
                approachSteeringOutput.Angular
            );
            return fleeSteeringOutput;
        }
    }
}