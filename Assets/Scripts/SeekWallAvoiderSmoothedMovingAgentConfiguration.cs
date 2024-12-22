using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Configuration for SeekWallAvoiderSmoothedMovingAgent.
///
/// This agent uses "chase the rabbit" method to avoid obstacles without the angle jittering of
/// SeekWallAvoiderMovingAgent. Nevertheless it still suffers of lack os smoothing in its speeds
/// when it gets close to its "rabbit".
/// </summary>
public class SeekWallAvoiderSmoothedMovingAgentConfiguration : MonoBehaviour
{
    [FormerlySerializedAs("pursuitSteeringBehavior")]
    [Header("WIRING")] 
    // [SerializeField] private SeekSteeringBehavior mainSeekSteeringBehavior;
    [SerializeField] private SeekSteeringBehavior seekSteeringBehavior;
    [SerializeField] private WallAvoidanceSteeringBehavior wallAvoidanceSteeringBehavior;

    [Header("CONFIGURATION:")]
    [Tooltip("Target to seek to.")]
    [SerializeField] private GameObject target;
    [Tooltip("Layers to avoid.")] 
    [SerializeField] private LayerMask layersToAvoid;
    [Tooltip("Distance to target to consider we have arrived.")]
    [Min(0.0f)]
    [SerializeField] private float arrivalDistance = 0.5f;
    [Tooltip("Distance to leave between SeekWallAvoider and Pursuit components. Higher values = mores smoothing but less precision.")]
    [Min(0.0f)]
    [SerializeField] private float easyness = 0.5f;

    private void Start()
    {
        seekSteeringBehavior.Target = target;
        seekSteeringBehavior.ArrivalDistance = arrivalDistance;
        wallAvoidanceSteeringBehavior.AvoidLayerMask = layersToAvoid;
        // mainSeekSteeringBehavior.ArrivalDistance = easyness;
    }
}
