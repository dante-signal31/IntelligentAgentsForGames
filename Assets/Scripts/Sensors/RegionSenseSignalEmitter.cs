using Tools;
using UnityEngine;

namespace Sensors
{
/// <summary>
/// Component that can emit a signal through a RegionSenseManager.
/// </summary>
public abstract class RegionSenseSignalEmitter<T, TU>: MonoBehaviour 
    where T: RegionSenseModality
    where TU: RegionSenseManager
{
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
            _currentModality = GenerateModality();
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
            _currentModality = GenerateModality();
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
            _currentModality = GenerateModality();
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

    protected T _currentModality;
    private float _maximumAttenuatedRange;
    private TU _regionSenseManager;

    /// <summary>
    /// Generates a new modality based on the current configuration.
    /// </summary>
    protected abstract T GenerateModality();
    
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
        _currentModality = GenerateModality();
        InitializeEmissionTimer();
        _regionSenseManager = FindAnyObjectByType<TU>();
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
            emissionPosition = transform.position,
            source = gameObject,
            strength = signalStrength
        };
        _regionSenseManager.RegisterSignal(signal);
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Draw maximum range circle.
        Gizmos.color = maximumRangeColor;
        Gizmos.DrawWireSphere(transform.position, modalityMaximumRange);
    }
#endif
}
}