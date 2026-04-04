using System.Collections.Generic;

namespace Tools
{
/// <summary>
/// Represents a fixed-size moving window for storing and processing a sequence of
/// float values.
/// </summary>
public class MovingWindow
{
    private readonly Queue<float> _queue;

    /// <summary>
    /// Returns the maximum number of elements that can be stored in the moving window.
    /// </summary>
    public int Capacity { get; }
    
    /// <summary>
    /// Returns the number of elements in the moving window.
    /// </summary>
    public int Count => _queue.Count;
    
    /// <summary>
    /// Array of current values in the moving window.
    /// </summary>
    public float[] Values => _queue.ToArray();
    
    /// <summary>
    /// Creates a new moving window with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of elements that can be stored in the
    /// moving window.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown if the specified capacity is less than 1.
    /// </exception>
    public MovingWindow(int capacity)
    {
        Capacity = capacity > 0 ? 
            capacity : 
            throw new System.ArgumentOutOfRangeException(nameof(capacity));
        _queue = new Queue<float>(capacity);
    }

    /// <summary>
    /// Adds a new float value to the moving window. If the window is at full capacity,
    /// the oldest value is removed before adding the new value. Updates the sum of the
    /// elements in the window.
    /// </summary>
    /// <param name="value">The float value to add to the moving window.</param>
    public void Add(float value)
    {
        if (_queue.Count == Capacity)
            _queue.Dequeue();

        _queue.Enqueue(value);
    }
}
}