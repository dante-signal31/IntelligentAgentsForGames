using Tools;
using UnityEngine;

namespace Sensors
{
/// <summary>
/// An example of a component that can emit a sound signal like modality.
/// </summary>
public class RegionSenseSoundSignalEmitter: MonoBehaviour
{
    /// <summary>
    /// Internal class that describes the sound modality this node is emitting.
    /// </summary>
    public class SoundModality : RegionSenseModality
    {
        public SoundModality(
            float maximumRange, float attenuation, float inverseTransmissionSpeed): 
            base(maximumRange, attenuation, inverseTransmissionSpeed)
        { }
    }

    [Header("CONFIGURATION:")] 
    [Tooltip("Maximum range of the modality.")]
    [SerializeField] private float modalityMaximumRange = 500f;
    
    [Tooltip("Attenuation factor by unit of distance for this modality.")]
    [SerializeField] private float modalityAttenuation = 0.9f;
    
    [Tooltip("How long it will take (in seconds) for the signal to travel one unit of " +
             "distance.")]
    [SerializeField] private float modalityInverseTransmissionSpeed = 0.2f;

    [Tooltip("Time in seconds between emissions of the signal.")]
    [SerializeField] private float emissionPeriod = 0.5f;
    
    [Tooltip("Whether to automatically start the emission of the signal. If you set " +
             "this value to false, you will have to call StartEmission() from code.")]
    [SerializeField] public bool autoStartEmission = true;
    
    [Tooltip("Signal strength.")]
    [SerializeField] public float signalStrength = 100f;
    
    [Header("WIRING:")]
    [Tooltip("Timer that controls the emission of the signal.")]
    // I could not use a standard Timer because it failed to call RegisterSignal from
    // OnEmissionElapsed. That happened because RegionSenseManager is a MonoBehavior, so
    // it cannot be called from outside the game-loop. The standard Timer callbacks are
    // executed out the game-loop, whereas the CustomTimer is coroutine-based, and it is
    // executed inside the game-loop.
    [SerializeField] public CustomTimer emissionTimer;
    
    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos;
    [SerializeField] private Color maximumRangeColor = Color.red; 
    
    /// <summary>
    /// Maximum range of the modality.
    /// </summary>
    public float ModalityMaximumRange
    {
        get => modalityMaximumRange;
        set
        {
            modalityMaximumRange = value;
            GenerateModality();
        }
    }
    
    /// <summary>
    /// Attenuation factor by unit of distance for this modality.
    /// </summary>
    public float ModalityAttenuation
    {
        get => modalityAttenuation;
        set
        {
            modalityAttenuation = value;
            GenerateModality();
        }
    }
    
    /// <summary>
    /// How long it will take (in seconds) for the signal to travel one unit of distance.
    /// </summary>
    /// <remarks>
    /// Using inverse transmission speed is more useful than uninverted because this way
    /// we can represent almost infinite speeds just with an inverse transmission speed of
    /// zero.
    /// </remarks>
    public float ModalityInverseTransmissionSpeed
    {
        get => modalityInverseTransmissionSpeed;
        set
        {
            modalityInverseTransmissionSpeed = value;
            GenerateModality();
        }
    }
    
    /// <summary>
    /// Time in seconds between emissions of the signal.
    /// </summary>
    public float EmissionPeriod
    {
        get => emissionPeriod;
        set
        {
            emissionPeriod = value;
            InitializeEmissionTimer();
        }
    }
    
    /// <summary>
    /// Indicates whether the signal is currently being emitted.
    /// </summary>
    public bool IsEmissionActive => emissionTimer.IsRunning;

    private SoundModality _currentModality;
    private float _maximumAttenuatedRange;
    private RegionSenseManager _regionSenseManager;

    /// <summary>
    /// Generates a new modality based on the current configuration.
    /// </summary>
    private void GenerateModality()
    {
        _currentModality = new SoundModality(
            ModalityMaximumRange, 
            ModalityAttenuation, 
            ModalityInverseTransmissionSpeed);
    }
    
    /// <summary>
    /// Starts the emission of the signal.
    /// </summary>
    public void StartEmission()
    {
        emissionTimer.StartTimer();
    }
    
    /// <summary>
    /// Stops the emission of the signal.
    /// </summary>
    public void StopEmission()
    {
        emissionTimer.StopTimer();
    }

    private void Start()
    {
        GenerateModality();
        InitializeEmissionTimer();
        _regionSenseManager = FindAnyObjectByType<RegionSenseManager>();
    }

    private void InitializeEmissionTimer()
    {
        emissionTimer.waitTime = EmissionPeriod;
        emissionTimer.oneShot = false;
        emissionTimer.timeout.AddListener(OnEmissionElapsed);
        if (autoStartEmission) emissionTimer.StartTimer();
    }

    private void OnEmissionElapsed()
    {
        RegionSenseSignal signal = new()
        {
            modality = _currentModality,
            source = gameObject,
            strength = signalStrength
        };
        _regionSenseManager.RegisterSignal(signal);
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = maximumRangeColor;
        Gizmos.DrawWireSphere(transform.position, modalityMaximumRange);
    }
#endif
}
}