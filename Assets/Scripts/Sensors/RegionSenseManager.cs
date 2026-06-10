using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Sensors
{
/// <summary>
/// Manages the registration of region sense sensors and orchestrates the emission
/// and distribution of signals to the registered sensors in the current scene.
/// </summary>
public class RegionSenseManager: MonoBehaviour
{
    /// <summary>
    /// Structure used to store information about a signal to be delivered to a
    /// specific sensor at a specific time.
    /// </summary>
    private struct SignalNotification
    {
        public double time;
        public IRegionSenseSensor sensor;
        public RegionSenseSignal signal;
    }

    /// <summary>
    /// Provides comparison logic for SignalNotification structs based on their
    /// scheduled times, signal strengths, and unique source identifiers.
    /// </summary>
    /// <remarks>
    /// This comparer is used to maintain a sorted order of SignalNotification instances
    /// in collections like SortedSet, based on their time properties. If the times are
    /// identical, the comparison is extended to the signal's strength and then to
    /// the source's unique identifier to ensure distinct elements in the sorted
    /// collection.
    /// </remarks>
    private class SignalNotificationComparer : IComparer<SignalNotification>
    {
        public int Compare(SignalNotification x, SignalNotification y)
        {
            int result = x.time.CompareTo(y.time);
            // If times are equal, we must not return 0, otherwise SortedSet 
            // thinks they are the same element and won't add the new one.
            
            if (result == 0 && x.signal.source != y.signal.source)
                result = x.signal.strength.CompareTo(y.signal.strength);

            if (result == 0 && x.signal.source != y.signal.source)
                result = x.signal.source.GetEntityId()
                    .CompareTo(y.signal.source.GetEntityId());
            
            return result;
        }
    }
    
    [Header("CONFIGURATION:")]
    [Tooltip("Time in seconds between signal emissions.")]
    [SerializeField] private float sendingPeriod = 0.1f;
    
    [Header("WIRING:")]
    [Tooltip("Timer that controls the emission of the signal.")]
    [SerializeField] private CustomTimer senderTimer;
    
    /// <summary>
    /// Time in seconds between signal emissions.
    /// </summary>
    public float SendingPeriod
    {
        get => sendingPeriod;
        set
        {
            sendingPeriod = value;
            if (senderTimer == null) return;
            senderTimer.waitTime = value;
        }
    }
    
    private readonly HashSet<IRegionSenseSensor> _registeredSensors = new();
    private readonly SortedSet<SignalNotification> _signalQueue = 
        new(new SignalNotificationComparer());

    protected virtual void Start()
    {
        InitializeSenderTimer();
    }

    private void InitializeSenderTimer()
    {
        senderTimer.waitTime = SendingPeriod;
        senderTimer.oneShot = false;
        senderTimer.timeout.AddListener(OnTimerElapsed);
        senderTimer.StartTimer();
    }
    
    private void OnTimerElapsed()
    {
        SendSignals();
    }
    
    /// <summary>
    /// Register a sensor to receive signals from this RegionSenseManager.
    /// </summary>
    /// <param name="sensor">Sensor interested in receiving signals.</param>
    public virtual void RegisterSensor(IRegionSenseSensor sensor)
    {
        _registeredSensors.Add(sensor);
    }
    
    /// <summary>
    /// Unregister a sensor from receiving signals from this RegionSenseManager.
    /// </summary>
    /// <param name="sensor">Sensor no longer interested in receiving signals.</param>
    public virtual void UnregisterSensor(IRegionSenseSensor sensor)
    {
        _registeredSensors.Remove(sensor);
    }

    /// <summary>
    /// Called by signal sources to send a signal to the sensors.
    /// </summary>
    /// <param name="signal">Signal to be sent.</param>
    public virtual void RegisterSignal(RegionSenseSignal signal)
    {
        foreach (IRegionSenseSensor sensor in _registeredSensors)
        {
            // Is this sensor interested in this signal modality?
            if (!sensor.SensesModality(signal.modality)) continue;

            // Is this sensor near enough?
            float distance =
                Vector2.Distance(
                    signal.source.transform.position, 
                    sensor.Position);
            if (distance > signal.modality.MaximumRange) continue;

            // Is the signal powerful enough to be perceived by the sensor?
            float receivedPower = signal.strength *
                                  MathF.Pow(signal.modality.Attenuation, distance);
            if (receivedPower < sensor.ModalityThreshold(signal.modality)) continue;

            // Now, let's perform the specific checks for this modality.
            if (!signal.modality.ExtraChecks(signal, sensor)) continue;

            // OK, if we got here, then the signal should be delivered to this sensor,
            // but when? We must calculate the time it takes the signal to reach
            // the sensor. 
            float timeToSensor = distance * signal.modality.InverseTransmissionSpeed;
            double deliveryTime =
                (DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch).TotalSeconds +
                timeToSensor;

            // Now create a signal notification and add it to the queue to be sent.
            SignalNotification notification = new()
            {
                time = deliveryTime,
                sensor = sensor,
                signal = signal,
            };
            _signalQueue.Add(notification);
        }
    }

    /// <summary>
    /// Send every signal in the signal queue that is due to be sent.
    /// </summary>
    private void SendSignals()
    {
        double currentTime = 
            (DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch).TotalSeconds;
        
        while (_signalQueue.Count > 0 && _signalQueue.Min.time <= currentTime)
        {
            SignalNotification notification = _signalQueue.Min;
            _signalQueue.Remove(notification);
            notification.sensor.NotifySignal(notification.signal);
        }
    }
}
}