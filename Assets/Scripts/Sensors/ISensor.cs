using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Sensors
{
public interface ISensor
{
    /// <summary>
    /// Event when the sensor starts to detect an object.
    /// </summary>
    public UnityEvent<GameObject> ObjectEnteredSensor { get; }
    
    /// <summary>
    /// Event when the sensor stays inside an object.
    /// </summary>
    public UnityEvent<GameObject> ObjectStayedInSensor { get; }
    
    /// <summary>
    /// Event when an object ends to detect an object.
    /// </summary>
    public UnityEvent<GameObject> ObjectLeftSensor { get; }
    
    /// <summary>
    /// <p>List of objects currently inside this sensor range.</p>
    /// <p>Only are considered those objects included in the layer mask provided
    /// to ConeSensor.</p> 
    /// </summary>
    public HashSet<GameObject> DetectedObjects { get; }

    /// <summary>
    /// Whether there is any object detected by the sensor.
    /// </summary>
    public bool AnyObjectDetected { get; }
}
}