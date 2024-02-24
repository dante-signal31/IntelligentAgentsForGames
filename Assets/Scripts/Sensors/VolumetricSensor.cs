using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VolumetricSensor : MonoBehaviour
{
    [Header("WIRING:")] 
    [Tooltip("Volumetric collider trigger.")] 
    [SerializeField] private BoxCollider2D volumetricCollider;
    
    [Header("CONFIGURATION:")] 
    [Tooltip("Subscribers for detection events.")] 
    [SerializeField] private UnityEvent<GameObject> objectEnteredDetectionArea;
    [Tooltip("Subscribers to object leaving volumetric area.")] 
    [SerializeField] private UnityEvent<GameObject> objectLeftDetectionArea;

    private HashSet<GameObject> _objectsDetected;

    /// <summary>
    /// Set of GameObjects under sensor range.
    /// </summary>
    public HashSet<GameObject> ObjectsDetected => _objectsDetected;

    /// <summary>
    /// If sensor has any detected object under its range.
    /// </summary>
    public bool anyObjectDetected => ObjectsDetected.Count > 0;
    
    private void AddDetectedObject(GameObject obj)
    {
        ObjectsDetected.Add(obj);
    }

    private void RemoveDetectedObject(GameObject obj)
    {
        ObjectsDetected.Remove(obj);
    }

    private GameObject GetGameObject(Collider2D col)
    {
        return col.transform.root.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject detectedGameObject = GetGameObject(other);
        AddDetectedObject(detectedGameObject);
        if (objectEnteredDetectionArea != null) objectEnteredDetectionArea.Invoke(detectedGameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject detectedGameObject = GetGameObject(other);
        RemoveDetectedObject(detectedGameObject);
        if (objectLeftDetectionArea != null) objectLeftDetectionArea.Invoke(detectedGameObject);
    }

    // Start is called before the first frame update
    void Awake()
    {
        _objectsDetected = new HashSet<GameObject>();
    }
}
