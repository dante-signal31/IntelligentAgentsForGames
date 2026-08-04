using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;
using Random = System.Random;

namespace Pathfinding
{
/// <summary>
/// Represents a node in a graph data structure, uniquely identified by an ID
/// and capable of maintaining connections to other nodes.
/// </summary>
/// <remarks>
/// This class is core to a graph representation and is designed to store
/// connections (or edges) to other nodes in the graph. Each connection is
/// directional and associated with a specific orientation and cost.
/// </remarks>
[Serializable]
public class GraphNode: IEquatable<GraphNode>
{
    private static readonly HashSet<uint> AssignedIds = new();
    private static readonly Random RandomGenerator = new();

    [SerializeField] private uint id = GenerateUniqueId();
    
    /// <summary>
    /// This node unique identifier.
    /// </summary>
    public uint Id {
        get => id;
        set => id = value; 
    }
    
    // Don't make this field readonly, or it will break serialization.
    [SerializeField] 
    private CustomUnityDictionaries.UintGraphConnectionDictionary connections = 
        new ();
    
    /// <summary>
    /// This node connections to other graph nodes.
    /// <remarks>
    /// <ul><b>Key:</b> An integer that can map to any enum you need.
    /// E.g. <see cref="Orientation"/> enum.</ul>
    /// <ul><b>Value:</b> A <see cref="GraphConnection"/> object that represents the
    /// connection between the current node and the target node.</ul>
    /// </remarks>
    /// </summary>
    public CustomUnityDictionaries.UintGraphConnectionDictionary Connections 
    { 
        get => connections;
        private set => connections = value;
    }

    /// <summary>
    /// Connection IDs currently present.
    /// </summary>
    public uint[] ConnectionIds => connections.Keys.ToArray();

    /// <summary>
    /// Generates a unique identifier for a node by creating a random 32-bit unsigned
    /// integer that is not currently assigned to any other node. Ensures identifier
    /// uniqueness by checking against a set of already assigned IDs.
    /// </summary>
    /// <returns>A 32-bit unsigned integer representing a unique identifier for the
    /// node.</returns>
    private static uint GenerateUniqueId()
    {
        uint newId;
        byte[] buffer = new byte[4];
        do
        {
            RandomGenerator.NextBytes(buffer);
            newId = BitConverter.ToUInt32(buffer, 0);
        } while (AssignedIds.Contains(newId));

        AssignedIds.Add(newId);
        return newId;
    }
    
    /// <summary>
    /// Recreate the assigned ID.
    /// </summary>
    /// <remarks> Nodes created from the inspector do not run GenerateUniqueId() over
    /// its id field. So it must be run manually. MonoBehaviours that create nodes
    /// must run this method in everyone.</remarks>
    public void RegenerateId()
    {
        id = GenerateUniqueId();
    }
    
    /// <summary>
    /// Adds a connection between the current node and a specified destination node
    /// with a given cost, orientation, and optionally bidirectional behavior.
    /// </summary>
    /// <param name="endNodeKey">The destination node id to which the connection should
    /// point.</param>
    /// <param name="cost">The cost or weight associated with traversing the edge.</param>
    /// <param name="orientation">The orientation of the edge connecting the two
    /// nodes.</param>
    public void AddConnection(
        uint endNodeKey,
        float cost, 
        uint orientation)
    {
        GraphConnection graphConnection = new(Id, endNodeKey, cost);
        Connections[orientation] = graphConnection;
    }
    
    /// <summary>
    /// Determines whether the node has a connection with the given ID.
    /// </summary>
    /// <param name="connectionId">The ID of the connection to check for.</param>
    /// <returns>True if a connection exists with the given ID; otherwise,
    /// false.</returns>
    public bool HasConnection(uint connectionId)
    {
        return Connections.ContainsKey(connectionId);
    }

    /// <summary>
    /// Retrieves the connection associated with the given ID, if it exists.
    /// </summary>
    /// <param name="connectionId">The ID associated with the desired
    /// connection.</param>
    /// <returns>The connection object if a connection exists with the specified
    /// ID; otherwise, null.</returns>
    public GraphConnection GetConnection(uint connectionId)
    {
        return HasConnection(connectionId) ? Connections[connectionId]: null;
    }


    /// <summary>
    /// Gets the next available unique connection ID that does not conflict with
    /// any of the existing connection IDs. This method scans the existing
    /// connection IDs, determines the first missing ID in numerical order, and
    /// returns it as the next available value.
    /// </summary>
    /// <returns>A 32-bit unsigned integer representing the next available connection
    /// ID that is not already used.</returns>
    public uint GetNextAvailableConnectionId()
    {
        List<uint> connectionIds = ConnectionIds.ToList();
        connectionIds.Sort();

        uint nextId = 0;
        foreach (uint usedId in connectionIds)
        {
            if (usedId != nextId)
            {
                return nextId;
            }
            nextId++;
        }
        return nextId;
    }
    
    public bool Equals(GraphNode other)
    {
        if (other is null) return false;
        return Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        return obj is GraphNode other && Equals(other);
    }
    
    public static bool operator ==(GraphNode left, GraphNode right)
    {
        return EqualityComparer<GraphNode>.Default.Equals(left, right);
    }

    public static bool operator !=(GraphNode left, GraphNode right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}    
}
