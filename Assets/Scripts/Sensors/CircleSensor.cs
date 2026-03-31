using UnityEngine;

namespace Sensors
{
public class CircleSensor: VolumetricSensor
{
    [Header("CIRCLE SENSOR CONFIGURATION:")]
    [SerializeField] private float radius;

    public float Radius
    {
        get => radius;
        set
        {
            radius = value;
            if (CircleCollider == null) return; 
            CircleCollider.radius = value;
        }
    }
    
    private CircleCollider2D CircleCollider => (CircleCollider2D) SensorCollider;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        Radius = radius;
    }
#endif
}
}