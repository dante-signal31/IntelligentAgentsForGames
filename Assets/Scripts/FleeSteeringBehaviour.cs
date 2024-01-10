using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Monobehaviour to offer a Flee steering behaviour.
/// </summary>
[RequireComponent(typeof(SeekSteeringBehavior))]
public class FleeSteeringBehavior : SteeringBehavior
{
    [Header("WIRING:")] 
    [SerializeField] private SeekSteeringBehavior seekSteeringBehaviour; 
    
    [FormerlySerializedAs("target")] [Header("CONFIGURATION:")]
    public GameObject threath;
    [Tooltip("Minimum distance to threath before fleeing.")]
    [SerializeField] private float PanicDistance;

    private void Start()
    {
        seekSteeringBehaviour.target = threath;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (Vector2.Distance(
                args.CurrentAgent.transform.position,
                threath.transform.position) > PanicDistance)
        { // Out of panic distance, so we stop accelerating.
            return new SteeringOutput();
        }
        else
        { // Threath inside panic distance, so run. 
            SteeringOutput approachSteeringOutput = seekSteeringBehaviour.GetSteering(args);
            SteeringOutput fleeSteeringOutput = new SteeringOutput(
                approachSteeringOutput.Linear * -1,
                approachSteeringOutput.Angular
            );
            return fleeSteeringOutput;
        }
    }
}