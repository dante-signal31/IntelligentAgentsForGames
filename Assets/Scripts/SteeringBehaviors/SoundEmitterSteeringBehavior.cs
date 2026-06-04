using Sensors;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// A steering behavior that dynamically manages the emission of sound signals based on
/// velocity, using an underlying steering behavior to determine movement logic.
/// </summary>
/// <remarks>
/// The behavior implements logic to activate or deactivate a sound signal emitter based
/// on the linear velocity of the agent. When the velocity exceeds a configurable
/// threshold, sound emission is activated; otherwise, it is deactivated. This behavior
/// is useful for AI agents where sound emission should reflect their activity level or
/// speed.
/// </remarks>
public class SoundEmitterSteeringBehavior: SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("Speed under which no sound is emitted.")]
    [SerializeField] public float soundSpeedThreshold = 0.3f;
    
    [Header("WIRING:")]
    [Tooltip("Steering behavior to determine movement logic.")]
    [SerializeField] private SteeringBehavior currentSteeringBehavior;
    [Tooltip("Sound emitter to control sound emission.")]
    [SerializeField] private RegionSenseSoundSignalEmitter soundEmitter;
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        SteeringOutput steering = currentSteeringBehavior.GetSteering(args);

        // Make sure we only emit sound when we are moving at a speed above the threshold.
        if (steering.Linear.magnitude < soundSpeedThreshold &&
            soundEmitter.IsEmissionActive)
        {
            soundEmitter.StopEmission();
        }
        else if (steering.Linear.magnitude >= soundSpeedThreshold &&
                 !soundEmitter.IsEmissionActive)
        {
            soundEmitter.StartEmission();
        }

        return steering;
    }
}
}