
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Levels
{
public class Courtyard : MonoBehaviour
{
    [Header("CONFIGURATION:")]
    [SerializeField] private Tilemap _obstaclesTilemap;
    
    public List<Vector2> ObstaclePositions { get; } = new();

    private void Start()
    {
        // TODO: Implement.
    }
}
}

