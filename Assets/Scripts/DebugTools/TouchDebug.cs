using System.Collections.Generic;
using Sensors;
using UnityEngine;

namespace DebugTools
{ 
/// <summary>
/// Debug class for TouchSensor.
/// </summary>
public class TouchDebug : MonoBehaviour
{
    [Header("WIRING:")] 
    [SerializeField] private TouchSensor touchSensor;

    private void Start()
    {
        touchSensor.ObjectEnteredSensor.AddListener(OnObjectEnteredSensor);
        touchSensor.ObjectLeftSensor.AddListener(OnObjectLeftSensor);
    }

    private void OnObjectLeftSensor(GameObject obj)
    {
        string objectNames = "";
        foreach (GameObject otherObject in touchSensor.DetectedObjects)
        {
            if (otherObject == obj) continue;
            objectNames += $"\t{otherObject.name}\n";
        }
        string debugMessage =
            $"=====================\n Object left sensor: {obj.name}\n " +
            $"Other objects in sensor:\n {objectNames}";
        Debug.Log(debugMessage);
        
    }

    private void OnObjectEnteredSensor(GameObject obj)
    {
        string objectNames = "";
        foreach (GameObject otherObject in touchSensor.DetectedObjects)
        {
            if (otherObject == obj) continue;
            objectNames += $"\t{otherObject.name}\n";
        }

        string debugMessage =
            $"=====================\n Object entered sensor: {obj.name}\n " +
            $"Other objects in sensor:\n {objectNames}";
        Debug.Log(debugMessage);
    }
}
}

