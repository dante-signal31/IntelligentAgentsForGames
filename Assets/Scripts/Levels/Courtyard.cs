using System.Collections.Generic;
using UnityEngine;

namespace Levels
{
public class Courtyard : MonoBehaviour
{
    public List<Vector2> ObstaclePositions { get; } = new();

    private void Start()
    {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("InnerObstacle");
        foreach (GameObject obstacle in obstacles)
        {
            ObstaclePositions.Add(obstacle.transform.position);
        }
    }
}
}

