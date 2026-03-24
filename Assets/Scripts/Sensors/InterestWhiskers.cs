
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Sensors
{
public class InterestWhiskers : MonoBehaviour
{
    private readonly List<RayEnds> _interestWhiskers = new();
    private readonly List<float> _interests = new();
    
    public int Count => _interestWhiskers.Count;

    public void ReloadWhiskers(List<RayEnds> interestEnds)
    {
        _interestWhiskers.Clear();
        _interestWhiskers.AddRange(interestEnds);
        _interests.Clear();
        _interests.AddRange(new float[_interestWhiskers.Count]);
    }

    public void CalculateInterests(Vector2 idealVelocity)
    {
        // idealVelocity is in global coordinates, while ray ends come in local
        // coordinates.
        int index = 0;
        foreach (RayEnds interestWhisker in _interestWhiskers)
        {
            Vector2 normalizedInterestWhisker = 
                (transform.TransformPoint(interestWhisker.end) - 
                 transform.TransformPoint(interestWhisker.start))
                .normalized;
            _interests[index] = 
                Mathf.Max(0, Vector2.Dot(
                    normalizedInterestWhisker,
                    idealVelocity));
            index++;
        }
    }
    
    public Interest GetInterest(int index)
    {
        return new Interest
        {
            Value = _interests[index], 
            Direction = transform.TransformPoint(_interestWhiskers[index].end) - 
                        transform.TransformPoint(_interestWhiskers[index].start)
        };
    }

    public List<Interest> GetInterests()
    {
        List<Interest> interests = new();
        for (int i = 0; i < Count; i++)
        {
            interests.Add(GetInterest(i));
        }
        return interests;
    }
}
}

