using System.Collections;
using NUnit.Framework;
using Pathfinding;
using SteeringBehaviors;
using Tests.PlayTests.Common;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayTests
{
public class NotInformedPathFindingTests
{
    private const string CurrentScene = "TestPathFindingTiledYard";
    
    private Transform _position1;
    private Transform _position2;
    private Transform _position3;
    private GameObject _pathFollowingGameObject;
    private GameObject _dijkstraPathFindingGameObject;
    private GameObject _aStarPathFindingGameObject;
    private GameObject _smoothedAStarPathFindingGameObject;
    private GameObject _breathFirstPathFindingGameObject;
    private GameObject _depthFirstPathFindingGameObject;
    private GameObject _meshPathFindingGameObject;
    private GameObject _unityNavMeshMovingAgentGameObject;
    private GameObject _smoothPathFinderCurrentPathGameObject;
    private GameObject _target;
    private PathFinderSteeringBehavior _breathFirstPathFinderSteeringBehavior;
    private PathFinderSteeringBehavior _depthFirstPathFinderSteeringBehavior;
    private AgentMover _breathFirstPathFinderAgent;
    private AgentMover _depthFirstPathFinderAgent;
    private AgentColor _breathFirstPathFinderAgentColor;
    private AgentColor _depthFirstPathFinderAgentColor;
    private GameObject _pathGameObject;
    private GameObject _path2GameObject;
    
    
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Clean up any existing objects first
        _pathFollowingGameObject = null;
        _dijkstraPathFindingGameObject = null;
        _aStarPathFindingGameObject = null;
        _smoothedAStarPathFindingGameObject = null;
        _breathFirstPathFindingGameObject = null;
        _depthFirstPathFindingGameObject = null;
        _meshPathFindingGameObject = null;
        _meshPathFindingGameObject = null;
        _smoothPathFinderCurrentPathGameObject = null;
        
        // Clean up any existing objects first.

        // Load the test scene
        yield return TestLevelManagement.ReLoadScene(CurrentScene);
        yield return null;
        
        if (_target == null)
        {
            _target = GameObject.Find("Target");
            _target.SetActive(false);
        }
        
        if (_position1 == null)
            _position1 = GameObject.Find("Position1").transform;
        if (_position2 == null)
            _position2 = GameObject.Find("Position2").transform;
        if (_position3 == null)
            _position3 = GameObject.Find("Position3").transform;

        if (_pathFollowingGameObject == null)
        {
            _pathFollowingGameObject = GameObject.Find("PathFollowingMovingAgent");
            _pathFollowingGameObject.SetActive(false);
        }
        
        if (_dijkstraPathFindingGameObject == null)
        {
            _dijkstraPathFindingGameObject = GameObject.Find("DijkstraPathFinderMovingAgent");
            _dijkstraPathFindingGameObject.SetActive(false);
        }
        
        if (_aStarPathFindingGameObject == null)
        {
            _aStarPathFindingGameObject = GameObject.Find("AStarPathFinderMovingAgent");
            _aStarPathFindingGameObject.SetActive(false);
        }

        if (_smoothedAStarPathFindingGameObject == null)
        {
            _smoothedAStarPathFindingGameObject =
                GameObject.Find("SmoothedAStarPathFinderMovingAgent");
            _smoothedAStarPathFindingGameObject.SetActive(false);
        }

        if (_breathFirstPathFindingGameObject == null)
        {
            _breathFirstPathFindingGameObject = GameObject.Find("BreathFirstPathFinderMovingAgent");
            _breathFirstPathFindingGameObject.SetActive(false);
        }
        
        if (_depthFirstPathFindingGameObject == null)
        {
            _depthFirstPathFindingGameObject = GameObject.Find("DepthFirstPathFinderMovingAgent");
            _depthFirstPathFindingGameObject.SetActive(false);
        }
        
        if (_meshPathFindingGameObject == null)
        {
            _meshPathFindingGameObject = GameObject.Find("MeshPathFinderMovingAgent");
            _meshPathFindingGameObject.SetActive(false);
        }

        if (_unityNavMeshMovingAgentGameObject == null)
        {
            _unityNavMeshMovingAgentGameObject = GameObject.Find("UnityNavMeshMovingAgent");
            _unityNavMeshMovingAgentGameObject.SetActive(false);
        }
        
        if (_pathGameObject == null)
        {
            _pathGameObject = GameObject.Find("Path");
            _pathGameObject.SetActive(false);
        }
        
        if (_path2GameObject == null)
        {
            _path2GameObject = GameObject.Find("Path_2");
            _path2GameObject.SetActive(false);
        }
        
        if (_breathFirstPathFinderAgent == null)
            _breathFirstPathFinderAgent = _breathFirstPathFindingGameObject.GetComponent<AgentMover>();
        
        if (_depthFirstPathFinderAgent == null)
            _depthFirstPathFinderAgent = _depthFirstPathFindingGameObject.GetComponent<AgentMover>();
        
        if (_breathFirstPathFinderSteeringBehavior == null)
            _breathFirstPathFinderSteeringBehavior = _breathFirstPathFindingGameObject.GetComponentInChildren<PathFinderSteeringBehavior>();
        
        if (_depthFirstPathFinderSteeringBehavior == null)
            _depthFirstPathFinderSteeringBehavior = _depthFirstPathFindingGameObject.GetComponentInChildren<PathFinderSteeringBehavior>();
        
        if (_breathFirstPathFinderAgentColor == null)
            _breathFirstPathFinderAgentColor = _breathFirstPathFindingGameObject.GetComponent<AgentColor>();
        
        if (_depthFirstPathFinderAgentColor == null)
            _depthFirstPathFinderAgentColor = _depthFirstPathFindingGameObject.GetComponent<AgentColor>();
    }
    
    
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_target != null)
            _target.SetActive(false);
        if (_breathFirstPathFindingGameObject != null)
            _breathFirstPathFindingGameObject.SetActive(false);
        if (_depthFirstPathFindingGameObject != null)
            _depthFirstPathFindingGameObject.SetActive(false);
        
        yield return null;
    }
    
    /// <summary>
    /// Test the Breath First pathfinder behavior.
    /// </summary>
    [UnityTest]
    public IEnumerator BreathFirstPathFindingBehaviorTest()
    {
        // Set up agents before the tests.
        _breathFirstPathFindingGameObject.transform.position = _position1.position;
        _breathFirstPathFinderAgent.MaximumSpeed = 6.0f;
        _breathFirstPathFinderAgent.StopSpeed = 0.01f;
        _breathFirstPathFinderAgent.MaximumRotationalSpeed = 1080f;
        _breathFirstPathFinderAgent.StopRotationThreshold = 1f;
        _breathFirstPathFinderAgentColor.Color = Color.green;
        _breathFirstPathFinderSteeringBehavior.ShowGizmos = true;
        _breathFirstPathFindingGameObject.SetActive(true);
        _target.SetActive(true);
        
    
        // Start test.
        // Assert that the pathfinder agent can reach the first target.
        _target.transform.position = _position2.position;
        yield return new WaitForSeconds(5);
        Assert.True(Vector2.Distance(_breathFirstPathFindingGameObject.transform.position, _position2.position) < 0.3f);
        
        // Assert that the pathfinder agent can reach the second target.
        _target.transform.position = _position3.position;
        yield return new WaitForSeconds(5);
        Assert.True(Vector2.Distance(_breathFirstPathFindingGameObject.transform.position, _position3.position) < 0.3f);
    }
    
    /// <summary>
    /// Test the Depth First pathfinder behavior.
    /// </summary>
    [UnityTest]
    public IEnumerator DepthFirstPathFindingBehaviorTest()
    {
        // Set up agents before the tests.
        _depthFirstPathFindingGameObject.transform.position = _position1.position;
        _depthFirstPathFinderAgent.MaximumSpeed = 6.0f;
        _depthFirstPathFinderAgent.StopSpeed = 0.01f;
        _depthFirstPathFinderAgent.MaximumRotationalSpeed = 1080f;
        _depthFirstPathFinderAgent.StopRotationThreshold = 1f;
        _depthFirstPathFinderAgentColor.Color = Color.green;
        _depthFirstPathFinderSteeringBehavior.ShowGizmos = true;
        _depthFirstPathFindingGameObject.SetActive(true);
        _target.SetActive(true);
        
    
        // Start test.
        // Assert that the pathfinder agent can reach the first target.
        _target.transform.position = _position2.position;
        yield return new WaitForSeconds(5);
        Assert.True(Vector2.Distance(_depthFirstPathFindingGameObject.transform.position, _position2.position) < 0.3f);
        
        // Assert that the pathfinder agent can reach the second target.
        _target.transform.position = _position3.position;
        yield return new WaitForSeconds(7);
        Assert.True(Vector2.Distance(_depthFirstPathFindingGameObject.transform.position, _position3.position) < 0.3f);
    }
}
}