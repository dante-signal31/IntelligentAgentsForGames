using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// <p>An array of ray sensors placed over a circular sector.</p>
///
/// <p>The sector is placed around the local Y axis, so forward direction for this sensor
/// is the local UP direction.</p>
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
        ///  Get the sensor at the given index counting from the rightmost sensor to
        /// center.
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
        /// This method leaves the list empty.
        /// </summary>
        public void Clear()
        {
            foreach (RaySensor sensor in _raySensors)
            {
                if (sensor != null) Destroy(sensor.gameObject);
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
    [Serializable] public struct RayEnds
    {
        public Vector3 start;
        public Vector3 end;
        
        public RayEnds(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
        }
    }
    
    [Header("CONFIGURATION:")]
    [Tooltip("Ray sensor to instance.")]
    [SerializeField] private GameObject sensor;
    [Tooltip("Layers to be detected by this sensor.")] 
    [SerializeField] private LayerMask layerMask;

    [Tooltip("Whether this sensor should detect its own agent if its ray sensors start " +
             "inside him.")]
    public bool ignoreOwnerAgent = true;
    [Tooltip("Number of rays for this sensor: (sensorResolution * 2) + 3")]
    [Range(0.0f, 10.0f)]
    [SerializeField] private int sensorResolution = 1;
    [Tooltip("Angular width in degrees for this sensor.")]
    [Range(0.0f, 180.0f)]
    [SerializeField] private float semiConeDegrees = 45.0f;
    [Tooltip("Maximum range for these rays.")]
    [SerializeField] private float range = 1.0f;
    [Tooltip("Minimum range for these rays. Useful to make rays start not at the " +
             "agent's center.")]
    [SerializeField] private float minimumRange = 0.2f;
    [Tooltip("Range proportion for whiskers at left side. 0.0 = leftmost sensor, " +
             "1.0 = center sensor.")]
    [SerializeField] private AnimationCurve leftRangeSemiCone = AnimationCurve.EaseInOut(
        0.0f, 
        0.2f, 
        1.0f, 
        1.0f);
    [Tooltip("Range proportion for whiskers at right side. 0.0 = center sensor, " +
             "1.0 = rightmost sensor.")]
    [SerializeField] private AnimationCurve rightRangeSemiCone = AnimationCurve.EaseInOut(
        0.0f,
        1.0f,
        1.0f,
        0.2f);
    [Space]
    [Tooltip("Event to trigger when a collider is detected by this sensor.")]    
    [SerializeField] private UnityEvent<Collider2D> colliderDetected;
    [Tooltip("Event to trigger when no collider is detected by this sensor.")]
    [SerializeField] private UnityEvent noColliderDetected;
    [Header("WIRING:")] 
    [SerializeField] private SectorRange sectorRange;
    [Header("DEBUG")]
    [Tooltip("Whether to show gizmos for sensors.")]
    [SerializeField] private bool showGizmos = true;
    [Tooltip("Color for this script gizmos.")]
    [SerializeField] private Color gizmoColor = Color.yellow;
    [Tooltip("Radius for the sensor ends.")]
    [SerializeField] private float gizmoRadius = 0.1f;

    /// <summary>
    /// This sensor layer mask.
    /// </summary>
    public LayerMask SensorsLayerMask
    {
        get => layerMask;
        set
        {
            layerMask = value;
            if (_sensors == null) return;
            foreach (RaySensor raySensor in _sensors)
            {
                raySensor.SensorLayerMask = value;
            }
        }
    }
    
    /// <summary>
    /// Number of rays for this sensor: (sensorResolution * 2) + 3
    /// </summary>
    public int SensorResolution
    {
        get => sensorResolution;
        set
        {
            if (sensorResolution == value) return;
            sensorResolution = value;
            UpdateRayEnds();
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
            if (sectorRange == null) return;
            semiConeDegrees = value;
            // Guard needed to avoid infinite calls between this component and sectorRange
            // when changing the semiConeDegrees.
            _parameterSetFromHere = true;
            sectorRange.SemiConeDegrees = value;
            UpdateRayEnds();
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
            if (sectorRange == null) return;
            range = value;
            // Guard needed to avoid infinite calls between this component and sectorRange
            // when changing the range.
            _parameterSetFromHere = true;
            sectorRange.Range = value;
            UpdateRayEnds();
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
            if (sectorRange == null) return;
            minimumRange = value;
            // Guard needed to avoid infinite calls between this component and sectorRange
            // when changing the minimumRange.
            _parameterSetFromHere = true;
            sectorRange.MinimumRange = value;
            UpdateRayEnds();
        }
    }

    private AnimationCurve _leftRangeSemiCone;
    /// <summary>
    /// <p>Range proportion for whiskers at left side.</p>
    /// <ul>0.0 = leftmost sensor.</ul>
    /// <ul>1.0 = center sensor.</ul>
    /// </summary>
    private AnimationCurve LeftRangeSemiCone
    {
        get => _leftRangeSemiCone;
        set
        {
            _leftRangeSemiCone = value;
            UpdateRayEnds();
        }
    }
    
    private AnimationCurve _rightRangeSemiCone;
    /// <summary>
    /// <p>Range proportion for whiskers at right side.</p>
    /// <ul>0.0 = center sensor</ul>
    /// <ul>1.0 = right sensor.</ul>
    /// </summary>
    private AnimationCurve RightRangeSemiCone
    {
        get => _rightRangeSemiCone;
        set
        {
            _rightRangeSemiCone = value;
            UpdateRayEnds();
        }
    }

    /// <summary>
    /// Number of rays for this sensor with given resolution.
    /// </summary>
    public int SensorAmount => (sensorResolution * 2) + 3;

    /// <summary>
    /// Whether this sensor detects any collider.
    /// </summary>
    public bool IsAnyColliderDetected
    {
        get
        {
            if (_sensors == null) return false;
            foreach (RaySensor sensor in _sensors)
            {
                if (sensor.IsColliderDetected) return true;
            }
            return false;
        }
    }
    
    /// <summary>
    /// <p>Set of detected objects.</p>
    /// <p>It offers a tuple of (Collider2D, int) where the int is the sensor index.</p>
    /// </summary>
    public HashSet<(Collider2D, int)> DetectedColliders
    {
        get
        {
            HashSet<(Collider2D, int)> detectedColliders = new();
            int sensorIndex = 0;
            foreach (RaySensor sensor in _sensors)
            {
                if (sensor.IsColliderDetected) 
                    detectedColliders.Add((sensor.DetectedCollider, sensorIndex));
                sensorIndex++;
            }
            return detectedColliders;
        } 
    }

    /// <summary>
    /// <p>List of detected hits.</p>
    /// <p>It's got as a list of tuples of (hit, detecting sensor index).</p>
    /// </summary>
    public List<(RaycastHit2D, int)> DetectedHits
    {
        get
        {
            var detectedHits = new List<(RaycastHit2D, int)>();
            int sensorIndex = 0;
            foreach (RaySensor sensor in _sensors)
            {
                if (sensor.IsColliderDetected) detectedHits.Add(
                    (sensor.DetectedHit, sensorIndex));
                sensorIndex++;
            }
            return detectedHits;
        }
    }
    
    /// <summary>
    /// <p>This sensor Forward vector.</p>
    /// </summary>
    public Vector2 Forward
    {
        get
        {
            if (sectorRange == null) return transform.up;
            return sectorRange.Forward;
        }
    }
    
    private bool _parameterSetFromHere;
    private RaySensorList _sensors;
    public List<RayEnds> _rayEnds;

    private bool _onValidationUpdatePending;
    
    private void SubscribeToSectorRangeEvents()
    {
        if (sectorRange == null) return;
        sectorRange.Updated.AddListener(OnSectorRangeUpdated);
    }

    private void OnSectorRangeUpdated()
    {
        if (_parameterSetFromHere)
        {
            _parameterSetFromHere = false;
            return;
        }
        UpdateRayEnds();
    }

    private void Start()
    {
#if UNITY_EDITOR
        // If in editor then only place gizmos. And link to sector range to set up
        // fields.
        if (sectorRange == null) return;
        SubscribeToSectorRangeEvents();
        UpdateRayEnds();
#else
        // If not in editor then create real sensors.
        UpdateRayEnds();
        SetupSensors();
        SubscribeToSensorsEvents();
#endif
    }

    private void OnEnable()
    {
        // OnEnable runs before Start, so first time OnEnable is called, sensors are not
        // initialized. That's why I call to SubscribeToSensorsEvents in Start.
        // Nevertheless, I call it here too just in case object is disabled and then
        // enabled again.
        SubscribeToSensorsEvents();
    }
    
    private void OnDisable()
    {
        UnsubscribeFromSensorsEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromSensorsEvents();
    }
    
    /// <summary>
    /// Bind a listener to the colliderDetected event.
    /// </summary>
    /// <param name="action">Method to bind.</param>
    public void SubscribeToColliderDetected(UnityAction<Collider2D> action)
    {
        colliderDetected.AddListener(action);
    }
    
    /// <summary>
    /// Unbind a listener from the colliderDetected event.
    /// </summary>
    /// <param name="action">Method to unbind.</param>
    public void UnsubscribeFromColliderDetected(UnityAction<Collider2D> action)
    {
        colliderDetected.RemoveListener(action);
    }
    
    /// <summary>
    /// Bind a listener to the noColliderDetected event.
    /// </summary>
    /// <param name="action">Method to bind.</param>
    public void SubscribeToNoColliderDetected(UnityAction action)
    {
        noColliderDetected.AddListener(action);
    }
    
    /// <summary>
    /// Unbind a listener from the noColliderDetected event.
    /// </summary>
    /// <param name="action">Method to unbind.</param>
    public void UnsubscribeFromNoColliderDetected(UnityAction action)
    {
        noColliderDetected.RemoveListener(action);
    }

    /// <summary>
    /// Called when a collider is detected.
    /// </summary>
    /// <param name="detectionSensor">RaySensor that detected collider.</param>
    public void OnColliderDetected(RaySensor detectionSensor)
    {
        if (colliderDetected != null) 
            colliderDetected.Invoke(detectionSensor.DetectedCollider);;
    }
    
    /// <summary>
    /// Called when no collider is detected.
    /// </summary>
    public void OnColliderNoLongerDetected()
    {
        if (DetectedColliders.Count == 0) noColliderDetected.Invoke();
    }

    /// <summary>
    /// Create a new list of sensors and place them.
    /// </summary>
    private void SetupSensors()
    {
        PopulateSensors();
        PlaceSensors();
        foreach (RaySensor raySensor in _sensors)
        {
            raySensor.detectionLayers = SensorsLayerMask;
            raySensor.IgnoreCollidersOverlappingStartPoint = ignoreOwnerAgent;
            raySensor.ShowGizmos = showGizmos;
        }
    }

    /// <summary>
    /// Create a new list of sensors.
    /// </summary>
    private void PopulateSensors()
    {
        _sensors?.Clear();
        List<RaySensor> raySensors = new List<RaySensor>();
        for (int i = 0; i < SensorAmount; i++)
        {
            GameObject sensorInstance = Instantiate(sensor, transform);
            sensorInstance.transform.SetParent(transform);
            raySensors.Add(sensorInstance.GetComponent<RaySensor>());
        }
        _sensors = new RaySensorList(raySensors);
    }

    /// <summary>
    /// Subscribe to sensors events.
    /// </summary>
    private void SubscribeToSensorsEvents()
    {
        if (_sensors == null) return;
        foreach (RaySensor raySensor in _sensors)
        {
            raySensor.colliderDetected.AddListener(OnColliderDetected);
            raySensor.noColliderDetected.AddListener(OnColliderNoLongerDetected);
        }
    }

    /// <summary>
    /// Unsubscribe from sensors events.
    /// </summary>
    private void UnsubscribeFromSensorsEvents()
    {
        if (_sensors == null) return;
        foreach (RaySensor raySensor in _sensors)
        {
            raySensor.colliderDetected.RemoveListener(OnColliderDetected);
            raySensor.noColliderDetected.RemoveListener(OnColliderNoLongerDetected);
        }
    }
    
    /// <summary>
    /// Place sensors in the correct positions for current resolution and current range
    /// sector.
    /// </summary>
    private void PlaceSensors()
    {
        List<RayEnds> rayEnds = _rayEnds;

        int i = 0;
        foreach (RayEnds rayEnd in rayEnds)
        {
            RaySensor sensor = _sensors.GetSensorFromLeft(i);
            sensor.StartPosition = transform.TransformPoint(rayEnd.start);
            sensor.EndPosition = transform.TransformPoint(rayEnd.end);
            i++;
        }
    }
    
    
    /// <summary>
    /// <p>Refresh positions for sensor ends.</p>
    /// <p>These positions are local to the current agent</p>
    /// </summary>
    /// <returns>New list for sensor ends local positions.</returns>
    private void UpdateRayEnds()
    {
        if (leftRangeSemiCone == null || 
            rightRangeSemiCone == null) 
            return;
        List<RayEnds> rayEnds = new();

        float totalPlacementAngle = SemiConeDegrees * 2;
        float placementAngleInterval = totalPlacementAngle / (SensorAmount - 1);

        for (int i = 0; i < SensorAmount; i++)
        {
            float currentAngle = SemiConeDegrees - (placementAngleInterval * i);
            Vector2 placementVector = Quaternion.Euler(0, 0, currentAngle) * 
                                      Forward;
            Vector2 placementVectorStart = placementVector * MinimumRange;
            Vector2 placementVectorEnd =
                placementVector * (MinimumRange + GetSensorLength(i));
            
            RayEnds rayEnd = new RayEnds(placementVectorStart, placementVectorEnd);
            
            rayEnds.Add(rayEnd);
        }
        
        _rayEnds = rayEnds;
    }
    
    
    /// <summary>
    /// Whether this index is the one of the center sensor.
    /// </summary>
    /// <param name="index">Sensor index</param>
    /// <returns>True if the center sensor has this index.</returns>
    public bool IsCenterSensor(int index)=> index == SensorAmount / 2;
    
    /// <summary>
    /// Calculates and returns the length of a sensor based on the sensor index provided.
    ///
    /// It uses index to use the proper proportion curve for left and right side.
    /// </summary>
    /// <param name="sensorIndex">Index of this sensor</param>
    /// <returns>This sensor length from minimum range.</returns>
    public float GetSensorLength(int sensorIndex)
    {
        int middleSensorIndex = SensorAmount / 2;
        if (sensorIndex < middleSensorIndex)
        {
            return GetLeftSensorLength(sensorIndex, middleSensorIndex);
        }
        return GetRightSensorLength(sensorIndex, middleSensorIndex);
    }

    /// <summary>
    /// Calculate the length of the left sensor based on the sensor index using left range semi cone
    /// curve.
    /// </summary>
    /// <param name="sensorIndex">Index of this sensor.</param>
    /// <param name="middleSensorIndex">Middle sensor index.</param>
    /// <returns>This sensor length from minimum range.</returns>
    private float GetLeftSensorLength(int sensorIndex, int middleSensorIndex)
    {
        float curvePoint = Mathf.InverseLerp(0, middleSensorIndex, sensorIndex);
        float curvePointRange = leftRangeSemiCone.Evaluate(curvePoint) * (range-minimumRange);
        return curvePointRange;
    }
    
    /// <summary>
    /// Calculate the length of the right sensor based on the sensor index using right range semi cone
    /// curve.
    /// </summary>
    /// <param name="sensorIndex">Index of this sensor.</param>
    /// <param name="middleSensorIndex">Middle sensor index.</param>
    /// <returns>This sensor length from minimum range.</returns>
    private float GetRightSensorLength(int sensorIndex, int middleSensorIndex)
    {
        float curvePoint = Mathf.InverseLerp( middleSensorIndex, SensorAmount-1, sensorIndex);
        float curvePointRange = rightRangeSemiCone.Evaluate(curvePoint) * (range-minimumRange);
        return curvePointRange;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw gizmos only if in editor. If in scene or play mode the gizmos
        // I'm interested in are those drawn from the RaySensors.
        if (showGizmos && _rayEnds != null)
        {
            Gizmos.color = gizmoColor;
            foreach (RayEnds rayEnds in _rayEnds)
            {
                Gizmos.color = gizmoColor;
                Gizmos.DrawLine(
                    transform.TransformPoint(rayEnds.start), 
                    transform.TransformPoint(rayEnds.end));
                Gizmos.DrawWireSphere(
                    transform.TransformPoint(rayEnds.start),
                    gizmoRadius);
                Gizmos.DrawWireSphere(
                    transform.TransformPoint(rayEnds.end),
                    gizmoRadius);
            }
        }
    }
#endif
}
