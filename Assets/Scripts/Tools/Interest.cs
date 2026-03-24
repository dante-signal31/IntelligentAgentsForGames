using UnityEngine;

namespace Tools
{
/// <summary>
/// Represents an interest with a value and a directional vector.
/// </summary>
/// <remarks>
/// This struct is used to store the interest value, typically associated with some kind
/// of decision-making or steering behavior, as well as its corresponding directional
/// vector in local or global coordinates.
/// </remarks>
public struct Interest
{
    public float value;
    public Vector2 direction;
}
}