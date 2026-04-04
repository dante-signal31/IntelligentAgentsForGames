using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Sensors
{
/// <summary>
/// This component emits events when objects are detected by the volumetric sensor it is
/// attached to. 
/// </summary>
[ExecuteAlways]
public class VolumetricSensor : MonoBehaviour, ISensor
{
    [Header("CONFIGURATION:")]
    [Tooltip("Layers to be detected by this sensor.")]
    [SerializeField] public LayerMask detectionLayers;
    [Tooltip("Ignore detected collider if it is the same transform than sensor or in " +
             "any of its parents.")]
    [SerializeField] public bool ignoreOwner = true;
    
    [Header("EVENTS:")] 
    [FormerlySerializedAs("objectEnteredDetectionArea")]
    [Tooltip("Subscribers for detection events.")] 
    [SerializeField] private UnityEvent<GameObject> objectEnteredSensor;
    
    [FormerlySerializedAs("objectStayDetectionArea")]
    [Tooltip("Subscribers to object staying in volumetric area.")]
    [SerializeField] private UnityEvent<GameObject> objectStayedInSensor;
    
    [FormerlySerializedAs("objectLeftDetectionArea")]
    [Tooltip("Subscribers to object leaving volumetric area.")] 
    [SerializeField] private UnityEvent<GameObject> objectLeftSensor;

    [Header("WIRING:")] 
    [Tooltip("Volumetric collider trigger.")] 
    [SerializeField] private Collider2D volumetricCollider;

    /// <summary>
    /// Set of GameObjects under sensor range.
    /// </summary>
    public HashSet<GameObject> DetectedObjects { get; } = new ();

    public UnityEvent<GameObject> ObjectEnteredSensor => objectEnteredSensor;

    public UnityEvent<GameObject> ObjectStayedInSensor => objectStayedInSensor;

    public UnityEvent<GameObject> ObjectLeftSensor => objectLeftSensor;
    

    /// <summary>
    /// If the sensor has any detected object under its range.
    /// </summary>
    public bool AnyObjectDetected => DetectedObjects.Count > 0;

    /// <summary>
    /// This sensor collider. Useful to find approximate contact points.
    /// </summary>
    protected Collider2D SensorCollider => volumetricCollider;
    
    private void AddDetectedObject(GameObject obj)
    {
        DetectedObjects.Add(obj);
    }

    private void RemoveDetectedObject(GameObject obj)
    {
        DetectedObjects.Remove(obj);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsColliderLayerInDetectionLayers(other)) return;
        if (ignoreOwner && transform.IsChildOf(other.transform)) return;
        GameObject detectedGameObject = other.gameObject;
        AddDetectedObject(detectedGameObject);
        objectEnteredSensor?.Invoke(detectedGameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsColliderLayerInDetectionLayers(other)) return;
        if (ignoreOwner && transform.IsChildOf(other.transform)) return;
        
        GameObject detectedGameObject = other.gameObject;
        RemoveDetectedObject(detectedGameObject);
        objectLeftSensor?.Invoke(detectedGameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsColliderLayerInDetectionLayers(other)) return;
        if (ignoreOwner && transform.IsChildOf(other.transform)) return;

        GameObject detectedGameObject = other.gameObject;
        if (!DetectedObjects.Contains(detectedGameObject)) 
            AddDetectedObject(detectedGameObject);
        objectStayedInSensor?.Invoke(detectedGameObject);
    }
    
    /// <summary>
    /// Whether the provided object layer is included in the detection layers LayerMask.
    /// </summary>
    /// <param name="other">Object to check.</param>
    /// <returns>True if the object's layer is in the layer mask.</returns>
    private bool IsColliderLayerInDetectionLayers(Collider2D other)
    {
        return (detectionLayers & (1 << other.gameObject.layer)) != 0;
    }
}
}
