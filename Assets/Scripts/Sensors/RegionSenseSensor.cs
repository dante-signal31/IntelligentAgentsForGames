using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Sensors
{
/// <summary>
/// Represents a sound-based region sense sensor that detects and processes signals
/// within a specified region. This class is specialized for detecting sound and
/// smell modality signals and provides functionality to filter, buffer, and track
/// detected objects.
/// </summary>
/// <remarks>
/// T is the type of the modality this sensor is interested in. TU is the type of the
/// RegionSenseManager this sensor is registered to.
/// </remarks>
public class RegionSenseSensor<T, TU>: 
    MonoBehaviour, IRegionSenseSensor, ISensor, ISignalSensor
    where T: RegionSenseModality
    where TU: RegionSenseManager
{
    [FormerlySerializedAs("DetectionBufferSize")]
    [Header("CONFIGURATION:")]
    [Tooltip("How many received signal to keep in memory.")]
    [SerializeField] public int detectionBufferSize = 100;
    
    [FormerlySerializedAs("MinimumStrengthDetectionThreshold")]
    [Tooltip("Minimum strength threshold for a signal to be considered detected.")]
    [SerializeField] public float minimumStrengthDetectionThreshold = 40f;
    
    [FormerlySerializedAs("DetectionExpirationTime")]
    [Tooltip("How long to keep a signal in memory (in seconds).")]
    [SerializeField] public float detectionExpirationTime = 1f;

    [FormerlySerializedAs("CleaningPeriod")]
    [Tooltip("How often to clean the buffer of old signals (in seconds).")]
    [SerializeField] public float cleaningPeriod = 0.3f;
    
    [Header("WIRING:")]
    [Tooltip("Timer that controls the cleaning of the buffer of old signals.")]
    [SerializeField] private CustomTimer cleaningTimer;
    
    [Header("EVENTS:")]
    [Tooltip("Event triggered when a new object enters the sensor.")]
    [SerializeField] public UnityEvent<GameObject> objectEnteredSensor;
    [Tooltip("Event triggered when an object stays in the sensor.")]
    [SerializeField] public UnityEvent<GameObject> objectStayedInSensor;
    [Tooltip("Event triggered when an object leaves the sensor.")]
    [SerializeField] public UnityEvent<GameObject> objectLeftSensor;

    public UnityEvent<GameObject> ObjectEnteredSensor => objectEnteredSensor;
    public UnityEvent<GameObject> ObjectStayedInSensor => objectStayedInSensor;
    public UnityEvent<GameObject> ObjectLeftSensor => objectLeftSensor;
    
    public Vector2 Position => transform.position;

    public HashSet<GameObject> DetectedObjects
    {
        get
        {
            // I got errors traversing _detectionBuffer elements in the foreach loop
            // because elements changed while traversing. So, I take a copy of the
            // elements and I traverse through that static copy.
            DetectedSignal[] detectedSignals = _detectionBuffer.ToArray();
            HashSet<GameObject> detectedObjects = new();
            
            foreach (var detectedSignal in detectedSignals)
            {
                detectedObjects.Add(detectedSignal.signal.source);
            }

            return detectedObjects;
        }
    } 

    public bool AnyObjectDetected => DetectedObjects.Count > 0;
    
    /// <summary>
    /// Priority queue of detected signals, sorted by signal strength. The strongest
    /// signal, the first.
    /// </summary>
    public SortedSet<DetectedSignal> DetectedSignals {
        get
        {
            SortedSet<DetectedSignal> detectedSignals = 
                new(_detectionBuffer, new DetectedSignalComparer());
            
            return detectedSignals;
        }
    }
    
    private readonly Queue<DetectedSignal> _detectionBuffer = new();
    private TU _regionSenseManager;

    private void Start()
    {
        InitializeCleaningTimer();
        _regionSenseManager = FindAnyObjectByType<TU>();
        _regionSenseManager.RegisterSensor(this);
    }
    
    private void InitializeCleaningTimer()
    {
        cleaningTimer.waitTime = cleaningPeriod;
        cleaningTimer.oneShot= false;
        cleaningTimer.timeout.AddListener(OnCleaningTimerElapsed);
        cleaningTimer.StartTimer();
    }

    private void OnCleaningTimerElapsed()
    {
        CleanDetectionBuffer();
    }
    
    /// <summary>
    /// Cleans the detection buffer by removing expired signals and updating the
    /// detected objects set. If objects are no longer detected due to signal expiration,
    /// the appropriate event is triggered.
    /// </summary>
    private void CleanDetectionBuffer()
    {
        // Remove expired signals from the front of the queue
        while (_detectionBuffer.Count > 0 && 
               (DateTimeOffset.UtcNow - 
                _detectionBuffer.Peek().detectionTimeStamp).TotalSeconds > 
               detectionExpirationTime)
        {
            DetectedSignal removedSignal = _detectionBuffer.Dequeue();
            
            // Check if signal source is still in the set of detected objects, because a
            // newer signal from that source is still in the _detectionBuffer.
            if (DetectedObjects.Contains(removedSignal.signal.source)) continue;
            
            // Signal source is not in the set of detected objects, so it must have left.
            ObjectLeftSensor?.Invoke(removedSignal.signal.source);
        }
    }
    
    /// <summary>
    /// Adds a detected signal to the internal detection buffer, ensuring that the buffer
    /// size remains bounded.
    /// </summary>
    /// <param name="signal">The signal to be added to the detection buffer.</param>
    private void AddDetectedSignal(RegionSenseSignal signal)
    {
        // Keep the buffer size bounded.
        if (_detectionBuffer.Count >= detectionBufferSize)
            _detectionBuffer.Dequeue();
        
        // Check if signal source is new.
        if (!DetectedObjects.Contains(signal.source))
            ObjectEnteredSensor?.Invoke(signal.source);
        
        // Add the signal to the buffer.
        _detectionBuffer.Enqueue(
            new DetectedSignal
            {
                signal = signal, 
                detectionTimeStamp = DateTimeOffset.Now
            });
    }

    public float ModalityThreshold(RegionSenseModality modality)
    {
        if (modality is T)
        {
            return minimumStrengthDetectionThreshold;
        }
        // I'm only interested in sound modality. Any other one is ignored. One way
        // to do this is through SensesModality(), another one is setting an infinite
        // strength threshold in those modalities we are not interested in.
        return float.PositiveInfinity;
    }
    
    public bool SensesModality(RegionSenseModality modality)
    {
        return modality is T;
    }

    public void NotifySignal(RegionSenseSignal signal)
    {
        AddDetectedSignal(signal);
    }

    private void FixedUpdate()
    {
        foreach (GameObject detectedObject in DetectedObjects)
        {
            ObjectStayedInSensor?.Invoke(detectedObject);
        }
    }
}
}