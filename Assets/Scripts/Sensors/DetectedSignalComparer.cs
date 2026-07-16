using System.Collections.Generic;

namespace Sensors
{
/// <summary>
/// Provides a comparison logic for sorting or ordering instances of
/// <see cref="DetectedSignal"/> based on signal strength, source, and detection
/// timestamp.
/// </summary>
/// <remarks>
/// This comparer enforces a comparison hierarchy for detected signals:
/// 1. Signals are primarily ordered by their strength in ascending order.
/// 2. If two signals have the same strength, their source identifiers are compared to
/// ensure uniqueness.
/// 3. If both strength and source identifiers match, the signals are ordered by their
/// detection timestamp.
/// This ensures that no two signals are treated as identical by collections
/// like <see cref="SortedSet{T}"/>.
/// </remarks>
public class DetectedSignalComparer : IComparer<DetectedSignal>
{
    public int Compare(DetectedSignal x, DetectedSignal y)
    {
        int result = x.signal.strength.CompareTo(y.signal.strength);
        // If strengths are equal, we must not return 0, otherwise SortedSet 
        // thinks they are the same element and won't add the new one.
        
        if (result == 0 && x.signal.source != y.signal.source)
            result = x.signal.source.GetEntityId()
                .CompareTo(y.signal.source.GetEntityId());
        
        if (result == 0 && x.signal.source == y.signal.source)
            result = x.detectionTimeStamp.CompareTo(y.detectionTimeStamp);
        
        return result;
    }
}
}