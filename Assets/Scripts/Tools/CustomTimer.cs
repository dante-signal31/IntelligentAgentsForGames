using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Tools
{
/// <summary>
/// <p>Timer to launch events after a provided time elapses.</p>
/// <p><b>WARNING:</b> I've implemented this component for an article, as a possible usage
/// of coroutines. But actually, this component is like reinventing the wheel because
/// C# has System.Timers.Timer that gives this functionality off the shelf. So,
/// you'd better use that built-in component than this script.</p>
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
    public float waitTime;
    
    [Tooltip("If true, this timer will stop after reaching the end. If false, it will " +
             "repeat forever.")]
    public bool oneShot;
    
    [Tooltip("If true, this timer will start automatically when the scene starts. If " +
             "false, you'll have to call StartTimer() manually.")]
    public bool autoStart;
    
    [Tooltip("Time scale to use for this timer.")]
    public TimeScale timeScale;
    
    [Tooltip("Event to launch when the timer reaches the end.")]
    public UnityEvent timeoutEvent = new();

    private Coroutine _currentCoroutine;
    
    private void Awake()
    {
        if (autoStart) StartTimer();
    }

    public void StartTimer()
    {
        _currentCoroutine = StartCoroutine(TimerCoroutine());
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
            
            timeoutEvent.Invoke();
        
            if (oneShot) break;
        }
    }

    public void StopTimer()
    {
        StopCoroutine(_currentCoroutine);
    }
}
}

