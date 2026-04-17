using UnityEngine;
using UnityEngine.Serialization;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Mono behavior to offer a Flee steering behavior.</p>
///
/// <p>Flee steering behavior makes the agent go away from another GameObject marked
/// as a threat.</p>
/// </summary>
public class FleeSteeringBehavior : SteeringBehavior
{
    private const float MinimumPanicDistance = 0.3f;

    [FormerlySerializedAs("threath")]
    [Header("CONFIGURATION:")]
    [SerializeField] private GameObject threat;
    [Tooltip("Minimum distance to threat before fleeing.")]
    [Min(MinimumPanicDistance)]
    [SerializeField] private float panicDistance;
    
    [Header("WIRING:")]
    [Tooltip("Steering behavior to actually move this agent.")]
    [SerializeField] private SeekSteeringBehavior seekSteeringBehaviour;

    public GameObject Threat
    {
        get => threat;
        set
        {
            threat = value;
            if (seekSteeringBehaviour != null) 
                seekSteeringBehaviour.Target = value;
        }
    }

    public float PanicDistance
    {
        get => panicDistance;
        set => panicDistance = Mathf.Max(MinimumPanicDistance, value);
    }

    private void Awake()
    {
        if (Threat == null) return;
        seekSteeringBehaviour.Target = Threat;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (Threat == null) return SteeringOutput.Zero;
        
        if (Vector2.Distance(
                args.CurrentAgent.transform.position,
                Threat.transform.position) > PanicDistance)
        { // Out of panic distance, so we stop fleeing.
            return SteeringOutput.Zero;
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
}