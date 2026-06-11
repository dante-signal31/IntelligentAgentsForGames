using PropertyAttribute;
using Sensors;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// Represents a steering behavior responsible for directing an agent toward
/// the source of the strongest signal detected within its sensing range.
/// The behavior uses a sensor to identify signals and dynamically
/// updates a pathfinding target for navigation.
/// </summary>
/// /// <remarks>
/// This sensor can be used both for sound modalities, through a RegionSenseManager, and
/// smell modalities, through a FEMSenseManager.
/// </remarks>
public class SignalChaserSteeringBehavior: SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("Distance at which we give our goal as reached and we stop our agent.")]
    [SerializeField] public float arrivalDistance = 1f;
    
    [Header("WIRING:")]
    [InterfaceCompliant(typeof(ISensor), typeof(ISignalSensor))]
    [SerializeField] private MonoBehaviour sensor;
    [SerializeField] private MeshPathFinderSteeringBehavior meshPathFinderSteeringBehavior;
    
    private ISensor _sensor;
    private ISignalSensor _signalSensor;
    private GameObject _target;
    private Vector2 _currentTargetPosition;

    private void Start()
    {
        _sensor = (ISensor)sensor;
        _signalSensor = (ISignalSensor)sensor;
        _target = new GameObject($"{name} - Target");
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (!_sensor.AnyObjectDetected) return SteeringOutput.zero;
        
        // Chase the strongest signal.
        RegionSenseSignal strongestSignal = _signalSensor.DetectedSignals.Max.signal;

        if (Vector2.Distance(
                _currentTargetPosition, 
                strongestSignal.emissionPosition) >
            arrivalDistance)
        {
            _target.transform.position = strongestSignal.emissionPosition;
            meshPathFinderSteeringBehavior.Target = _target;
            _currentTargetPosition = _target.transform.position;
        }
        
        return meshPathFinderSteeringBehavior.GetSteering(args);
    }
}
}