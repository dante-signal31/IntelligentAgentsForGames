using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Sensors
{
/// <summary>
/// <p>A sensor component that detects GameObjects within its touch range.</p>
/// <p>The detection area is defined by a 2D collider attached to the same GameObject as
/// the sensor.</p>
/// </summary>
/// <remarks>
/// You can use this script in the same game object as the one of the agent main collider.
/// This way, you would use the same collider to give shape to the agent and to detect
/// touch events. The problem is that, in that case, you would mess the agent collision
/// matrix with the sensor collision matrix; those layer masks do not have to be
/// compatible.So, if you finally decide to use this script with its own collider, you
/// must be sure to make this collider of the same shape as the collider used by the
/// agent for its movement collisions.
/// </remarks>
[RequireComponent(typeof(Collider2D))]
public class TouchSensor : MonoBehaviour, ISensor
{
    [Header("CONFIGURATION:")]
    [Tooltip("Layers of the objects to be detected by this sensor.")]
    [SerializeField] private LayerMask detectionLayers;
    [Tooltip("Event to emit when an object is detected by this sensor.")]
    [SerializeField] private UnityEvent<GameObject> objectEnteredSensor = new();
    [Tooltip("Event to emit when an object is is already detected by this sensor.")]
    [SerializeField] private UnityEvent<GameObject> objectStayedInSensor = new();
    [Tooltip("Event to emit when an object no longer is detected by this sensor.")]
    [SerializeField] private UnityEvent<GameObject> objectLeftSensor = new();

    public UnityEvent<GameObject> ObjectEnteredSensor => objectEnteredSensor;
    public UnityEvent<GameObject> ObjectStayedInSensor => objectStayedInSensor;
    public UnityEvent<GameObject> ObjectLeftSensor => objectLeftSensor;

    public HashSet<GameObject> DetectedObjects { get; } = new();

    public bool AnyObjectDetected => DetectedObjects.Count > 0;

    private Collider2D _sensorCollider;
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!DetectedObjects.Add(other.gameObject)) return;
        objectEnteredSensor?.Invoke(other.gameObject);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (!DetectedObjects.Remove(other.gameObject)) return;
        objectLeftSensor?.Invoke(other.gameObject);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        objectStayedInSensor?.Invoke(other.gameObject);
    }
    
    private void Awake()
    {
        _sensorCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // By default, Unity uses the Layer Collision Settings from the Layer Mask. To
        // set collision layers manually, we need to use the includeLayers and
        // excludeLayers. This way we can set a collision matrix only for this object 
        // without messing with the global collision matrix.
        _sensorCollider.includeLayers = detectionLayers;
        _sensorCollider.excludeLayers = ~detectionLayers;
    }
}
}

