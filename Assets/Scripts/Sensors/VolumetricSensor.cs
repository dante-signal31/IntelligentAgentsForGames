using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Sensors
{
/// <summary>
/// This component emits events when objects are detected by the volumetric sensor it is
/// attached to. 
/// </summary>
[ExecuteAlways]
public class VolumetricSensor : MonoBehaviour
{
    [Header("EVENTS:")] 
    [Tooltip("Subscribers for detection events.")] 
    [SerializeField] private UnityEvent<GameObject> objectEnteredDetectionArea;
    [Tooltip("Subscribers to object staying in volumetric area.")]
    [SerializeField] private UnityEvent<GameObject> objectStayDetectionArea;
    [Tooltip("Subscribers to object leaving volumetric area.")] 
    [SerializeField] private UnityEvent<GameObject> objectLeftDetectionArea;

    [Header("WIRING:")] 
    [Tooltip("Volumetric collider trigger.")] 
    [SerializeField] private Collider2D volumetricCollider;

    private HashSet<GameObject> _objectsDetected;
    private HashSet<ContactPoint2D> _contactPoints;

    /// <summary>
    /// Set of GameObjects under sensor range.
    /// </summary>
    public HashSet<GameObject> ObjectsDetected => _objectsDetected;

    /// <summary>
    /// If sensor has any detected object under its range.
    /// </summary>
    public bool anyObjectDetected => ObjectsDetected.Count > 0;

    /// <summary>
    /// This sensor collider. Useful to find approximate contact points.
    /// </summary>
    public Collider2D SensorCollider => volumetricCollider;

    private void Awake()
    {
        _objectsDetected = new HashSet<GameObject>();
        _contactPoints = new HashSet<ContactPoint2D>();
    }

    private void AddDetectedObject(GameObject obj)
    {
        ObjectsDetected.Add(obj);
    }

    private void RemoveDetectedObject(GameObject obj)
    {
        ObjectsDetected.Remove(obj);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject detectedGameObject = other.gameObject;
        AddDetectedObject(detectedGameObject);
        if (objectEnteredDetectionArea != null) objectEnteredDetectionArea.Invoke(detectedGameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject detectedGameObject = other.gameObject;
        RemoveDetectedObject(detectedGameObject);
        if (objectLeftDetectionArea != null) objectLeftDetectionArea.Invoke(detectedGameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        GameObject detectedGameObject = other.gameObject;
        if (!ObjectsDetected.Contains(detectedGameObject)) AddDetectedObject(detectedGameObject);
        if (objectStayDetectionArea != null) objectStayDetectionArea.Invoke(detectedGameObject);
    }
}
}
