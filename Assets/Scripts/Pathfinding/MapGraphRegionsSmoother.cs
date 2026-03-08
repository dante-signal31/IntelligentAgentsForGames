
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding
{
public class MapGraphRegionsSmoother : MonoBehaviour
{
    [Header("CONFIGURATION:")] 
    [Tooltip("Whether to use random seeds for the regions generation. If true, only " +
             "seeds set at MapGraphRegions will be used; whereas if false, then " +
             "random seeds will be generated from this class with the same influence.")]
    public bool randomSeeds = true;
    [Tooltip("Numbers of regions to generate.")]
    public uint randomSeedsAmount = 3;
    [Tooltip("Number of iterations for the Lloyd relaxation algorithm.")]
    public uint relaxationIterations = 3;

    [Header("WIRING:")] 
    public MapGraphRegions mapGraphRegions;
    
    /// <summary>
    /// Smooths regions in the map graph using iterative relaxation. Optionally,
    /// initializes the region seeds with random data if RandomSeeds is enabled.
    /// The smoothing applies a given number of relaxation iterations as specified
    /// by the RelaxationIterations property.
    /// </summary>
    /// <remarks>
    /// - If RandomSeeds is true, the method generates a list of randomized seeds
    /// before starting the smoothing process.
    /// - During each relaxation iteration, regions are updated, and seeds are
    /// relocated if more iterations remain.
    /// - The method depends on the MapGraphRegions object to handle the actual
    /// region generation based on the provided seeds.
    /// </remarks>
    public void SmoothRegions()
    {
        if (randomSeeds) mapGraphRegions.seeds = GenerateRandomSeeds();
        for (int i = 0; i < relaxationIterations; i++)
        {
            mapGraphRegions.GenerateRegions();
            if (i < relaxationIterations - 1) RelocateSeeds();
        }
    }
    
    /// <summary>
    /// Relocates region seeds to the nearest valid positions within their respective
    /// regions by calculating the average position of nodes in each region and snapping
    /// the seed to the nearest node to that position.
    /// </summary>
    private void RelocateSeeds()
    {
        foreach (RegionSeed seed in mapGraphRegions.seeds)
        {
            uint regionId = mapGraphRegions.GetRegionByPosition(seed.position);
            HashSet<uint> nodesInRegion = mapGraphRegions.NodesByRegion[regionId];
            Vector2 averagePosition = GetAveragePosition(nodesInRegion);
            // Average position can be inside an obstacle. So we must search for the
            // nearest node.
            PositionNode nearestNode = 
                mapGraphRegions.mapGraph.GetNodeAtNearestPosition(averagePosition);
            seed.position = nearestNode.Position;
        }
    }
    
    /// <summary>
    /// Calculates the average position of all nodes within a specified region.
    /// The average is computed by summing the positions of the nodes and dividing
    /// by the total number of nodes in the region.
    /// </summary>
    /// <param name="nodesInRegion">
    /// A collection of node IDs representing the nodes within the region
    /// whose average position is to be calculated.
    /// </param>
    /// <returns>
    /// A <see cref="Vector2"/> representing the calculated average position
    /// of all nodes in the specified region.
    /// </returns>
    /// <remarks>
    /// This method assumes that the positional data of nodes can be accessed
    /// via the MapGraph associated with the MapGraphRegions object. The average
    /// position may not account for obstacles or invalid positions, which must
    /// be handled by the calling process if necessary.
    /// </remarks>
    private Vector2 GetAveragePosition(HashSet<uint> nodesInRegion)
    {
        Vector2 positionsSum = Vector2.zero;
        uint positionsCount = 0;
        foreach (uint nodeId in nodesInRegion)
        {
            PositionNode node = 
                (PositionNode) mapGraphRegions.mapGraph.GetNodeById(nodeId);
            positionsSum += node.Position;
            positionsCount++;
        }
        return positionsSum / positionsCount;
    }
    
    /// <summary>
    /// Generates a collection of randomized region seeds to initialize the map graph
    /// regions. Each seed is assigned a unique spatial position and color, ensuring no
    /// duplicates.
    /// </summary>
    /// <returns>
    /// An array of generated RegionSeed objects, each carrying a position, an influence
    /// value, and a randomly assigned color.
    /// </returns>
    private List<RegionSeed> GenerateRandomSeeds()
    {
        List<RegionSeed> randomSeeds = new();
        Color mapGraphRegionsGizmoColor = mapGraphRegions.gizmosColor;
        HashSet<Color> selectedColors = new() { mapGraphRegionsGizmoColor };

        // Get all valid array positions from the graph nodes
        List<Vector2Int> allNodesArrayPositions = 
            mapGraphRegions.mapGraph.ArrayPositionsToNodes.Keys.ToList();

        HashSet<int> alreadySelectedIndices = new();
        // Generate random seeds
        for (int i = 0; i < randomSeedsAmount && i < allNodesArrayPositions.Count; i++)
        {
            // Select a random node from the graph.
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, allNodesArrayPositions.Count);
                // I've had problems with seed collision, so I made sure to avoid selecting
                // the same position.
            } while (alreadySelectedIndices.Contains(randomIndex));
            alreadySelectedIndices.Add(randomIndex);
            Vector2Int selectedArrayPosition = allNodesArrayPositions[randomIndex];
            PositionNode selectedNode = 
                mapGraphRegions.mapGraph.GetNodeAtArrayPosition(selectedArrayPosition);
            allNodesArrayPositions.RemoveAt(randomIndex);

            // Generate a random color that doesn't exist
            Color randomColor;
            do
            {
                randomColor = new Color(
                    Random.Range(0.0f, 1.0f),
                    Random.Range(0.0f, 1.0f),
                    Random.Range(0.0f, 1.0f)
                );
            } while (selectedColors.Contains(randomColor));
            selectedColors.Add(randomColor);

            // Create the region seed
            RegionSeed seed = new()
            {
                position = selectedNode.Position,
                influence = 1,
                gizmoColor = randomColor
            };
            randomSeeds.Add(seed);
        }
        return randomSeeds;
    }
}
}

