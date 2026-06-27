using PropertyAttribute;
using Sensors;
using UnityEngine;

namespace DebugTools
{ 
/// <summary>
/// Debug class for ISensor compliant sensors.
/// </summary>
public class SensorDebug : MonoBehaviour
{
    [Header("WIRING:")] 
    [InterfaceCompliant(typeof(ISensor))]
    [SerializeField] private MonoBehaviour sensor;

    private ISensor _sensor;
    
    private void Start()
    {
        _sensor = (ISensor) sensor;
        _sensor.ObjectEnteredSensor.AddListener(OnObjectEnteredSensor);
        _sensor.ObjectLeftSensor.AddListener(OnObjectLeftSensor);
    }

    /// <summary>
    /// Handles the event triggered when an object leaves the sensor's detection area.
    /// Logs a message containing the name of the object that left and the names of all
    /// other objects currently detected by the sensor.
    /// </summary>
    /// <param name="obj">The object that has left the sensor's detection area.</param>
    private void OnObjectLeftSensor(GameObject obj)
    {
        string objectNames = GetObjectNamesString(obj);
        string debugMessage =
            $"=====================\n Object left sensor: {obj.name}\n " +
            $"Other objects in sensor:\n {objectNames}";
        Debug.Log(debugMessage);
        
    }

    /// <summary>
    /// Handles the event triggered when an object enters the sensor's detection area.
    /// Logs a message containing the name of the entered object and the names of all
    /// other objects currently detected by the sensor.
    /// </summary>
    /// <param name="obj">The object that has entered the sensor's detection
    /// area.</param>
    private void OnObjectEnteredSensor(GameObject obj)
    {
        string objectNames = GetObjectNamesString(obj);
        string debugMessage =
            $"=====================\n Object entered sensor: {obj.name}\n " +
            $"Other objects in sensor:\n {objectNames}";
        Debug.Log(debugMessage);
    }

    /// <summary>
    /// Constructs a formatted string containing the names of all objects currently
    /// detected by the sensor, excluding the specified object.
    /// </summary>
    /// <param name="obj">The object to be excluded from the list of detected
    /// objects.</param>
    /// <returns>A string containing the names of all other objects in the sensor,
    /// each on a new line.</returns>
    private string GetObjectNamesString(GameObject obj)
    {
        string objectNames = "";
        
        foreach (GameObject otherObject in _sensor.DetectedObjects)
        {
            if (otherObject == obj) continue;
            objectNames += $"\t{otherObject.name}\n";
        }

        return objectNames;
    }
}
}

