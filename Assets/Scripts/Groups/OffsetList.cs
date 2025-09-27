using UnityEngine;

namespace Groups
{
/// <summary>
/// <p>A class that encapsulates a collection of offsets represented as a list of Vector2
/// values.</p>
/// <p>Every offset represents a relative point position.</p>
/// </summary>
[CreateAssetMenu(fileName = "OffsetList", menuName = "Scriptable Objects/OffsetList", order = 0)]
public class OffsetList : ScriptableObject
{
    [SerializeField] private Vector2[] offsets;
    
    /// <summary>
    /// Relative offsets registered.
    /// </summary>
    public Vector2[] Offsets => offsets;
}
}