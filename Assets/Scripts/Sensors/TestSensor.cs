using UnityEngine;

public class TestSensor : MonoBehaviour
{

    public void OnObjectEnteredDetectionArea(GameObject obj)
    {
        Debug.Log($"Object entered detection area: {obj.name}");
    }
    
    public void OnObjectLeftDetectionArea(GameObject obj)
    {
        Debug.Log($"Object left detection area: {obj.name}");
    }
}
