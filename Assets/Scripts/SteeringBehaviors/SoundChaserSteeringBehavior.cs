using Sensors;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// Represents a steering behavior responsible for directing an agent toward
/// the source of the strongest sound signal detected within its sensing range.
/// The behavior uses a sound sensor to identify signals and dynamically
/// updates a pathfinding target for navigation.
/// </summary>
public class SoundChaserSteeringBehavior: SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("Distance at which we give our goal as reached and we stop our agent.")]
    [SerializeField] public float arrivalDistance = 100f;
    
    [Header("WIRING:")]
    [SerializeField] private RegionSenseSoundSensor soundSensor;
    [SerializeField] private MeshPathFinderSteeringBehavior meshPathFinderSteeringBehavior;
    
    private GameObject _target;
    private Vector2 _currentTargetPosition;

    private void Start()
    {
        _target = new GameObject($"{name} - Target");
        
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (!soundSensor.AnyObjectDetected) return SteeringOutput.zero;
        
        // Chase the strongest signal.
        RegionSenseSignal strongestSignal = soundSensor.DetectedSignals.Max.signal;

        if (Vector2.Distance(
                _currentTargetPosition, 
                strongestSignal.source.transform.position) >
            arrivalDistance)
        {
            _target.transform.position = strongestSignal.source.transform.position;
            meshPathFinderSteeringBehavior.Target = _target;
            _currentTargetPosition = _target.transform.position;
        }
        
        return meshPathFinderSteeringBehavior.GetSteering(args);
    }
}
}