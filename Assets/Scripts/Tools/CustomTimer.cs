using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Tools
{
/// <summary>
/// <p>Timer to launch events after a provided time elapses.</p>
/// <p><b>WARNING:</b> At first glance, this component would be like reinventing the wheel
/// because C# has System.Timers.Timer that gives this functionality off the shelf. So,
/// you'd better use that built-in component than this script. But actually, it happens
/// that you cannot call de Unity API from a conventional Timer call back, because that
/// Timer is executed in a thread apart from game-loop. So, calling Unity API from a Timer
/// callback could get you in some nasty silent bugs. That is the reason to use something
/// like this CustomTimer.</p>
/// </summary>
public class CustomTimer : MonoBehaviour
{
    /// <summary>
    /// <p>GameTime means this timer follows the current game time. So if the game is in
    /// bullet time, this timer will last longer.</p>
    /// <p>RealTime means this timer follows real world's time.</p>
    /// </summary>
    public enum TimeScale
    {
        GameTime,
        RealTime
    }
    
    [Header("CONFIGURATION:")] 
    
    [Tooltip("Seconds to wait before launching events.")]
    [SerializeField] public float waitTime;
    
    [Tooltip("If true, this timer will stop after reaching the end. If false, it will " +
             "repeat forever.")]
    [SerializeField] public bool oneShot;
    
    [Tooltip("If true, this timer will start automatically when the scene starts. If " +
             "false, you'll have to call Start() manually.")]
    [SerializeField] public bool autoStart;
    
    [Tooltip("Time scale to use for this timer.")]
    [SerializeField] public TimeScale timeScale;
    
    [Tooltip("Event to launch when the timer reaches the end.")]
    [SerializeField] public UnityEvent timeout = new();
    
    /// <summary>
    /// True if this timer is running.
    /// </summary>
    public bool IsRunning { get; private set; }

    private Coroutine _currentCoroutine;
    
    private void Awake()
    {
        if (autoStart) StartTimer();
    }

    
    public void StartTimer()
    {
        _currentCoroutine = StartCoroutine(TimerCoroutine());
        IsRunning = true;
    }

    private IEnumerator TimerCoroutine()
    {
        while (true)
        {
            switch (timeScale)
            {
                case TimeScale.GameTime:
                    yield return new WaitForSeconds(waitTime);
                    break;
                case TimeScale.RealTime:
                    yield return new WaitForSecondsRealtime(waitTime);
                    break;
            }
            
            timeout.Invoke();
        
            if (oneShot) break;
        }
    }

    public void StopTimer()
    {
        StopCoroutine(_currentCoroutine);
        IsRunning = false;
    }
}
}

