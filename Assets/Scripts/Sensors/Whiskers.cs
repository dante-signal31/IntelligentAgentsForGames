using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// An array of ray sensors placed over a circular sector.
///
/// The sector is placed around the local Y axis, so forward direction for this sensor
/// is the local UP direction.
///
/// Be aware, that Unity cannot instance object while in prefab mode. So when this
/// script detects that you are in prefab mode it will just populate a list of placements shown
/// with gizmos, but it won't instance any sensor until this prefab is placed in the scene.
/// </summary>
[ExecuteAlways]
public class Whiskers : MonoBehaviour
{
    /// <summary>
    /// A class wrapping a list of ray sensors to make it easier to search for them.
    /// </summary>
    private class RaySensorList : IEnumerable<RaySensor>
    {
        // It should have 2N + 3 sensors.
        // Think in this array of sensors as looking to UP direction,
        // Inside this list:
        //  * Top left sensor is always at index 0.
        //  * Center sensor is always at the middle index.
        //  * Top right sensor is always at the end index.
        private List<RaySensor> _raySensors;
        
        /// <summary>
        /// Current amount of sensors in this list.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Get the center sensor.
        /// </summary>
        public RaySensor CenterSensor => _raySensors[Count / 2];
        
        /// <summary>
        /// Get the leftmost sensor (assuming whiskers locally looks to UP direction).
        /// </summary>
        public RaySensor LeftMostSensor => _raySensors[0];
        
        /// <summary>
        /// Get the rightmost sensor (assuming whiskers locally looks to UP direction).
        /// </summary>
        public RaySensor RightMostSensor => _raySensors[Count - 1];
        
        /// <summary>
        /// Get the sensor at the given index counting from the leftmost sensor to center.
        /// </summary>
        /// <param name="index">0 index is the leftmost sensor</param>
        /// <returns></returns>
        public RaySensor GetSensorFromLeft(int index) => _raySensors[index];
        
        /// <summary>
        ///  Get the sensor at the given index counting from the rightmost sensor to center.
        /// </summary>
        /// <param name="index">0 index is the rightmost sensor</param>
        /// <returns></returns>
        public RaySensor GetSensorFromRight(int index) => _raySensors[Count - 1 - index];

        public RaySensorList(List<RaySensor> raySensors)
        {
            _raySensors = raySensors;
            Count = _raySensors.Count;
        }

        /// <summary>
        /// Remove every sensor in the list.
        ///
        /// This method leaves list empty.
        /// </summary>
        public void Clear()
        {
            foreach (RaySensor sensor in _raySensors)
            {
#if UNITY_EDITOR
                if (sensor != null) DestroyImmediate(sensor.gameObject);
#else
                if (sensor != null) Destroy(sensor.gameObject);
#endif
            }
            _raySensors?.Clear();
            Count = 0;
        }

        public IEnumerator<RaySensor> GetEnumerator()
        {
            return _raySensors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    /// <summary>
    /// Struct to represent ray ends for every sensor in prefab local space.
    /// </summary>
    [Serializable]
    public struct RayEnds
    {
        public Vector3 start;
        public Vector3 end;
    }
    
    [Header("CONFIGURATION:")]
    [Tooltip("Ray sensor to instance.")]
    [SerializeField] private GameObject sensor;
    [Tooltip("Number of rays for this sensor: (sensorResolution * 2) + 3")]
    [Range(0.0f, 10.0f)]
    [SerializeField] private int sensorResolution = 1;
    [Tooltip("Angular width in degrees for this sensor.")]
    [Range(0.0f, 180.0f)]
    [SerializeField] private float semiConeDegrees = 45.0f;
    [Tooltip("Maximum range for these rays.")]
    [SerializeField] private float range = 1.0f;
    [Tooltip("Minimum range for these rays. Useful to make rays start not at the agent's center.")]
    [SerializeField] private float minimumRange = 0.2f;
    
    [Header("DEBUG")]
    [Tooltip("Whether to show gizmos for sensors.")]
    [SerializeField] private bool showGizmos = true;
    [Tooltip("Color for this scripts gizmos.")]
    [SerializeField] private Color gizmoColor = Color.yellow;

    /// <summary>
    /// Number of rays for this sensor: (sensorResolution * 2) + 3
    /// </summary>
    public int SensorResolution
    {
        get => sensorResolution;
        set
        {
            sensorResolution = value;
            _onValidationUpdatePending = true;
        }
    }

    /// <summary>
    /// Angular width in degrees for this sensor.
    /// </summary>
    public float SemiConeDegrees
    {
        get => semiConeDegrees;
        set
        {
            semiConeDegrees = value;
            _onValidationUpdatePending = true;
        }
    }

    /// <summary>
    /// Maximum range for these rays.
    /// </summary>
    public float Range
    {
        get => range; 
        set
        {
            range = value;
            _onValidationUpdatePending = true;
        }
    }

    /// <summary>
    /// Minimum range for these rays. Useful to make rays start not at the agent's center.
    /// </summary>
    public float MinimumRange
    {
        get => minimumRange;
        set
        {
            minimumRange = value;
            _onValidationUpdatePending = true;
        }
    }

    /// <summary>
    /// Number of rays for this sensor with given resolution.
    /// </summary>
    public int SensorAmount => (sensorResolution * 2) + 3;
    
    private RaySensorList _sensors;
    public List<RayEnds> _rayEnds;

    private bool _onValidationUpdatePending = false;

    private void Start()
    {
#if UNITY_EDITOR
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            // Script is executing in prefab mode
            // Nothing done at the moment.
        }
        else
        {
            // Script is executing in edit mode (but not in prefab mode, so in scene mode)
            // OR script is executing in Play mode
            UpdateSensorPlacement();
            UpdateGizmosConfiguration();
        }
#else
        SetupSensors();
        UpdateGizmosConfiguration();
#endif
    }
    
    /// <summary>
    /// When a value changes on Inspector, update prefab appearance.
    /// </summary>
    private void LateUpdate()
    {
        if (_onValidationUpdatePending)
        {
            UpdateSensorPlacement();
            UpdateGizmosConfiguration();
            _onValidationUpdatePending = false;
        }
    }

    /// <summary>
    /// Create a new list of sensors and place them.
    /// </summary>
    private void SetupSensors()
    {
        PopulateSensors();
        PlaceSensors();
    }

    /// <summary>
    /// Create a new list of sensors.
    /// </summary>
    private void PopulateSensors()
    {
        if (_sensors != null) _sensors.Clear();
        List<RaySensor> raySensors = new List<RaySensor>();
        for (int i = 0; i < SensorAmount; i++)
        {
            GameObject sensorInstance = Instantiate(sensor, transform);
            raySensors.Add(sensorInstance.GetComponent<RaySensor>());
        }
        _sensors = new RaySensorList(raySensors);
    }
    

    /// <summary>
    /// Place sensors in the correct positions for current resolution and current range sector
    /// </summary>
    private void PlaceSensors()
    {
        List<RayEnds> rayEnds = _rayEnds;

        int i = 0;
        foreach (RayEnds rayEnd in rayEnds)
        {
            _sensors.GetSensorFromLeft(i).SetRayOrigin(transform.TransformPoint(rayEnd.start));
            _sensors.GetSensorFromLeft(i).SetRayTarget(transform.TransformPoint(rayEnd.end));
            i++;
        }
    }

    /// <summary>
    /// If in prefab mode then recalculate sensor ends positions. If in scene or play mode
    /// then it recalculates sensor ends positions and instantiate sensors in those positions.
    /// </summary>
    private void UpdateSensorPlacement()
    {
#if UNITY_EDITOR
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            // Script is executing in prefab mode
            PopulateRayEnds();
        }
        else
        {
            // Script is executing in edit mode (but not necessarily prefab mode)
            // OR script is executing in Play mode
            PopulateRayEnds();
            SetupSensors();
        }
#else
        PopulateRayEnds();
        SetupSensors();
#endif
    }
    
    /// <summary>
    /// Calculate local positions for sensor ends and store them to be serialized along the prefab.
    /// </summary>
    /// <returns>New list for sensor ends local positions.</returns>
    private List<RayEnds> GetRayEnds()
    {
        List<RayEnds> rayEnds = new List<RayEnds>();
        
        if (transform == null) return rayEnds;
        
        float totalPlacementAngle = semiConeDegrees * 2;
        float placementAngleInterval = totalPlacementAngle / (SensorAmount - 1);
        // Vector3 currentPosition = transform.position;
        
        // Remember: local forward is UP direction in local space.
        Vector3 forwardSensorPlacement = Vector3.up * minimumRange;

        for (int i = 0; i < SensorAmount; i++)
        {
            float currentAngle = semiConeDegrees - (placementAngleInterval * i);
            Vector3 placementVector = Quaternion.AngleAxis(currentAngle, Vector3.forward) * forwardSensorPlacement;
            Vector3 placementVectorEnd = placementVector.normalized * range;
            
            // Vector3 sensorStart = currentPosition + transform.TransformDirection(placementVector);
            // Vector3 sensorEnd = currentPosition + transform.TransformDirection(placementVectorEnd);
            
            Vector3 sensorStart = placementVector;
            Vector3 sensorEnd = placementVectorEnd;
            
            RayEnds newRayEnds = new RayEnds();
            newRayEnds.start = sensorStart;
            newRayEnds.end = sensorEnd;
            rayEnds.Add(newRayEnds);
        }

        return rayEnds;
    }

    /// <summary>
    /// Update gizmos configuration of every sensor in the list.
    /// </summary>
    private void UpdateGizmosConfiguration()
    {
        foreach (RaySensor raySensor in _sensors)
        {
            raySensor.ShowGizmos = showGizmos;
        }
    }

    /// <summary>
    /// Calculate sensor ends and store them to be serialized along the prefab.
    /// </summary>
    private void PopulateRayEnds()
    {
        _rayEnds = GetRayEnds();
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Remember: You cannot create objects from OnValidate(). So just mark the update as pending
        // and create those objects from LateUpdate().
        _onValidationUpdatePending = true;
    }
    
    private void OnDrawGizmos()
    {
        // Draw gizmos only if in prefab mode. If in scene or play mode the gizmos
        // I'm interested in are those drawn from the sensors.
        if (showGizmos && _rayEnds != null && PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            Gizmos.color = gizmoColor;
            foreach (RayEnds rayEnds in _rayEnds)
            {
                Gizmos.DrawLine(rayEnds.start, rayEnds.end);
            }
        }
    }
#endif
}
