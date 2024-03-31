using System;
using UnityEngine;

/// <summary>
/// Configuration for SeekWallAvoiderSmoothedMovingAgent.
///
/// This agent uses "chase the rabbit" method to avoid obstacles without the angle jittering of
/// SeekWallAvoiderMovingAgent. Nevertheless it still suffers of lack os smoothing in its speeds
/// when it gets close to its "rabbit".
/// </summary>
public class SeekWallAvoiderSmoothedMovingAgentConfiguration : MonoBehaviour
{
    [Header("WIRING")] 
    [SerializeField] private PursuitSteeringBehavior pursuitSteeringBehavior;
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
        seekSteeringBehavior.target = target;
        seekSteeringBehavior.arrivalDistance = arrivalDistance;
        wallAvoidanceSteeringBehavior.AvoidLayerMask = layersToAvoid;
        pursuitSteeringBehavior.ArrivalDistance = easyness;
    }
}
