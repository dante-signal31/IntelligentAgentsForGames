using System.Collections;
using System.Collections.Generic;
using SteeringBehaviors;
using Tools;
using UnityEngine;
using UnityEngine.Events;

namespace Sensors
{
/// <summary>
/// <p>An array of ray sensors placed over a circular sector.</p>
///
/// <p>The sector is placed around the local Y axis, so the forward direction for this
/// sensor is the local UP direction.</p>
/// </summary>
[ExecuteAlways]
public class WhiskersSensor : MonoBehaviour, IGizmos, ISensor
{
    /// <summary>
    /// A class wrapping a list of ray sensors to make it easier to search for them.
    /// </summary>
    private class RaySensorList : IEnumerable<RaySensor>
    {
        // It should have 2N + 3 sensors.
        // Think of this array of sensors as looking in a UP direction,
        // Inside this list:
        //  * Top left sensor is always at index 0.
        //  * Center sensor is always at the middle index.
        //  * Top right sensor is always at the end index.
        private readonly List<RaySensor> _raySensors;
    
        /// <summary>
        /// Current number of sensors in this list.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Get the center sensor.
        /// </summary>
        public RaySensor CenterSensor => _raySensors[Count / 2];
    
        /// <summary>
        /// Get the leftmost sensor (assuming whiskers locally look in the UP direction).
        /// </summary>
        public RaySensor LeftMostSensor => _raySensors[0];
    
        /// <summary>
        /// Get the rightmost sensor (assuming whiskers locally look in the UP direction).
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

        /// <summary>
        /// Returns an enumerator that iterates through the collection of RaySensor
        /// objects.
        /// </summary>
        /// <returns>An enumerator for the collection of RaySensor objects.</returns>
        public IEnumerator<RaySensor> GetEnumerator()
        {
            return _raySensors.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of RaySensor
        /// objects.
        /// </summary>
        /// <returns>An enumerator for the collection of RaySensor objects.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
    [Tooltip("Event to trigger when an object is detected by this sensor.")]    
    [SerializeField] private UnityEvent<GameObject> objectEnteredSensor;
    [Tooltip("Event to trigger when an object keeps being detected by this sensor.")]
    [SerializeField] private UnityEvent<GameObject> objectStayedInSensor;
    [Tooltip("Event to trigger when an object is no longer detected by this sensor.")]
    [SerializeField] private UnityEvent<GameObject> objectLeftSensor;
    [Tooltip("Event to trigger when no object is detected by this sensor.")]
    [SerializeField] private UnityEvent noObjectDetected;
    [Header("WIRING:")] 
    [SerializeField] private SectorRange sectorRange;
    [Header("DEBUG")]
    [Tooltip("Whether to show gizmos for sensors.")]
    [SerializeField] private bool showGizmos = true;
    [Tooltip("Color for this script gizmos.")]
    [SerializeField] private Color gizmosColor = Color.yellow;
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
            if (Application.isPlaying) 
                UpdateSensors();
            else 
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
            if (Application.isPlaying)
                UpdateSensors();
            else
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
            if (Application.isPlaying)
                UpdateSensors();
            else
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
            if (Application.isPlaying)
                UpdateSensors();
            else
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
    /// Number of rays for this sensor with the given resolution.
    /// </summary>
    public int SensorAmount => (sensorResolution * 2) + 3;

    /// <summary>
    /// Whether this sensor detects any object.
    /// </summary>
    public bool IsAnyObjectDetected
    {
        get
        {
            if (_sensors == null) return false;
            foreach (RaySensor currentSensor in _sensors)
            {
                if (currentSensor.AnyObjectDetected) return true;
            }
            return false;
        }
    }
    
    /// <summary>
    /// Sensor indices where an object is detected. True if detected.
    /// </summary>
    public List<bool> DetectionMask
    {
        get
        {
            List<bool> detectionMask = new();
            foreach (RaySensor currentSensor in _sensors)
            {
                detectionMask.Add(currentSensor.AnyObjectDetected); 
            }
            return detectionMask;
        }
    }

    /// <summary>
    /// <p>Set of detected objects.</p>
    /// <p>It offers a tuple of (GameObject, int) where the int is the sensor index.</p>
    /// </summary>
    public HashSet<(GameObject, int)> DetectingSensors
    {
        get
        {
            HashSet<(GameObject, int)> detectingSensors = new();
            int sensorIndex = 0;
            foreach (RaySensor currentSensor in _sensors)
            {
                if (currentSensor.AnyObjectDetected) 
                    detectingSensors.Add((currentSensor.FirstDetectedObject, sensorIndex));
                sensorIndex++;
            }
            return detectingSensors;
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
            foreach (RaySensor currentSensor in _sensors)
            {
                // I had to use the HasValue-Value syntax because
                // currentSensor.FirstDetectedHit is nullable.
                if (currentSensor.FirstDetectedHit.HasValue)
                    detectedHits.Add((currentSensor.FirstDetectedHit.Value, sensorIndex));
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
    
    public bool ShowGizmos
    {
        get => showGizmos;
        set => showGizmos = value;
    }

    public Color GizmosColor
    {
        get => gizmosColor;
        set => gizmosColor = value;
    }
    
    public readonly List<RayEnds> rayEnds = new();

    public UnityEvent<GameObject> ObjectEnteredSensor => objectEnteredSensor;

    public UnityEvent<GameObject> ObjectStayedInSensor => objectStayedInSensor;

    public UnityEvent<GameObject> ObjectLeftSensor => objectLeftSensor;

    public HashSet<GameObject> DetectedObjects { get; } = new();

    public bool AnyObjectDetected => DetectedObjects.Count > 0;
    
    private bool _parameterSetFromHere;
    private RaySensorList _sensors;

    public void OnSectorRangeUpdated()
    {
        if (sectorRange == null) return;
        
        if (_parameterSetFromHere)
        {
            _parameterSetFromHere = false;
            return;
        }
        
        Range = sectorRange.Range;
        MinimumRange = sectorRange.MinimumRange;
        SemiConeDegrees = sectorRange.SemiConeDegrees;
        
        UpdateRayEnds();
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            // If not in play mode, then create real sensors.
            UpdateSensors();
        }
        else
        {
            // If in editor mode, then only place gizmos. 
            if (sectorRange == null) return;
            UpdateRayEnds();
        }
    }

    /// <summary>
    /// <p>Recreate sensors with current settings.</p>
    /// <p>It's automatically called when any of the setting properties is used.</p> 
    /// </summary>
    private void UpdateSensors()
    {
        UpdateRayEnds();
        SetupSensors();
        SubscribeToSensorsEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromSensorsEvents();
        RemoveSensors();
    }

    /// <summary>
    /// Bind a listener to the objectEnteredSensor event.
    /// </summary>
    /// <param name="action">Method to bind.</param>
    public void SubscribeToObjectDetected(UnityAction<GameObject> action)
    {
        objectEnteredSensor.AddListener(action);
    }

    /// <summary>
    /// Unbind a listener from the objectEnteredSensor event.
    /// </summary>
    /// <param name="action">Method to unbind.</param>
    public void UnsubscribeFromObjectDetected(UnityAction<GameObject> action)
    {
        objectEnteredSensor.RemoveListener(action);
    }

    /// <summary>
    /// Bind a listener to the noObjectDetected event.
    /// </summary>
    /// <param name="action">Method to bind.</param>
    public void SubscribeToNoObjectDetected(UnityAction action)
    {
        noObjectDetected.AddListener(action);
    }

    /// <summary>
    /// Unbind a listener from the noObjectDetected event.
    /// </summary>
    /// <param name="action">Method to unbind.</param>
    public void UnsubscribeFromNoObjectDetected(UnityAction action)
    {
        noObjectDetected.RemoveListener(action);
    }

    /// <summary>
    /// Called when a collider is detected.
    /// </summary>
    /// <param name="detectedObject">Object detected by the sensor</param>
    public void OnObjectDetected(GameObject detectedObject)
    {
        objectEnteredSensor?.Invoke(detectedObject);
        DetectedObjects.Add(detectedObject);
    }

    /// <summary>
    /// Called when no collider is detected.
    /// </summary>
    public void OnObjectNoLongerDetected(GameObject disappearedObject)
    {
        objectLeftSensor?.Invoke(disappearedObject);
        DetectedObjects.Remove(disappearedObject);
        if (DetectedObjects.Count == 0) noObjectDetected?.Invoke();
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
        RemoveSensors();
        List<RaySensor> raySensors = new List<RaySensor>();
        for (int i = 0; i < SensorAmount; i++)
        {
            // It's really odd, but this line works right when the game is normally
            // played, but when I run automated tests, it doesn't work because
            // sensorInstance is not parented to whiskersSensors. So, the sensor stays
            // floating at the hierarchy root or even disappears from
            // the automated test scene.
            GameObject sensorInstance = Instantiate(sensor,gameObject.transform);
            raySensors.Add(sensorInstance.GetComponent<RaySensor>());
        }
        _sensors = new RaySensorList(raySensors);
    }

    /// <summary>
    /// Clear the current list of sensors.
    /// </summary>
    private void RemoveSensors()
    {
        if (_sensors == null) return;
        foreach (RaySensor raySensor in _sensors)
        {
            Destroy(raySensor.gameObject);
        }
        _sensors.Clear();
    }

    /// <summary>
    /// Subscribe to sensor events.
    /// </summary>
    private void SubscribeToSensorsEvents()
    {
        if (_sensors == null) return;
        foreach (RaySensor raySensor in _sensors)
        {
            raySensor.ObjectEnteredSensor.AddListener(OnObjectDetected);
            raySensor.ObjectLeftSensor.AddListener(OnObjectNoLongerDetected);
        }
    }

    /// <summary>
    /// Unsubscribe from sensor events.
    /// </summary>
    private void UnsubscribeFromSensorsEvents()
    {
        if (_sensors == null) return;
        foreach (RaySensor raySensor in _sensors)
        {
            raySensor.ObjectEnteredSensor.RemoveListener(OnObjectDetected);
            raySensor.ObjectLeftSensor.RemoveListener(OnObjectNoLongerDetected);
        }
    }

    /// <summary>
    /// Place sensors in the correct positions for current resolution and current range
    /// sector.
    /// </summary>
    private void PlaceSensors()
    {
        // List<RayEnds> rayEnds = this.rayEnds;

        int i = 0;
        foreach (RayEnds currentRayEnd in rayEnds)
        {
            RaySensor currentSensor = _sensors.GetSensorFromLeft(i);
            currentSensor.GlobalStartPosition = transform.TransformPoint(currentRayEnd.start);
            currentSensor.GlobalEndPosition = transform.TransformPoint(currentRayEnd.end);
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
        List<RayEnds> newRayEnds = new();

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
        
            newRayEnds.Add(rayEnd);
        }
    
        rayEnds.Clear();
        rayEnds.AddRange(newRayEnds);
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
    /// <returns>This sensor length from the minimum range.</returns>
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
    /// Calculate the length of the left sensor based on the sensor index using the left
    /// range semi-cone curve.
    /// </summary>
    /// <param name="sensorIndex">Index of this sensor.</param>
    /// <param name="middleSensorIndex">Middle sensor index.</param>
    /// <returns>This sensor length from the minimum range.</returns>
    private float GetLeftSensorLength(int sensorIndex, int middleSensorIndex)
    {
        float curvePoint = Mathf.InverseLerp(0, middleSensorIndex, sensorIndex);
        float curvePointRange = leftRangeSemiCone.Evaluate(curvePoint) * range;
        return curvePointRange;
    }

    /// <summary>
    /// Calculate the length of the right sensor based on the sensor index using the right
    /// range semi-cone curve.
    /// </summary>
    /// <param name="sensorIndex">Index of this sensor.</param>
    /// <param name="middleSensorIndex">Middle sensor index.</param>
    /// <returns>This sensor length from the minimum range.</returns>
    private float GetRightSensorLength(int sensorIndex, int middleSensorIndex)
    {
        float curvePoint = Mathf.InverseLerp(
            middleSensorIndex, 
            SensorAmount-1, 
            sensorIndex);
        float curvePointRange = rightRangeSemiCone.Evaluate(curvePoint) * range;
        return curvePointRange;
    }

    private void FixedUpdate()
    {
        if (DetectedObjects.Count == 0) return;

        foreach (GameObject detectedObject in DetectedObjects)
        {
            ObjectStayedInSensor?.Invoke(detectedObject);    
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        // Force the use of properties to make them update cone range.
        MinimumRange = minimumRange;
        Range = range; 
        SemiConeDegrees = semiConeDegrees;
    }

    private void OnDrawGizmos()
    {
        // Draw gizmos only if in editor mode. 
        if (!Application.isPlaying && showGizmos && rayEnds != null)
        {
            Gizmos.color = gizmosColor;
            foreach (RayEnds currentRayEnds in rayEnds)
            {
                Gizmos.color = gizmosColor;
                Gizmos.DrawLine(
                    transform.TransformPoint(currentRayEnds.start), 
                    transform.TransformPoint(currentRayEnds.end));
                Gizmos.DrawWireSphere(
                    transform.TransformPoint(currentRayEnds.start),
                    gizmoRadius);
                Gizmos.DrawWireSphere(
                    transform.TransformPoint(currentRayEnds.end),
                    gizmoRadius);
            }
        }
        // If in play mode, the gizmos I'm interested in are those drawn from the
        // RaySensors.
    }
#endif
}
}
