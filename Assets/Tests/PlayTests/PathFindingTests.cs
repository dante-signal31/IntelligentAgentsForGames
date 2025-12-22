using System.Collections;
using NUnit.Framework;
using Pathfinding;
using SteeringBehaviors;
using Tests.PlayTests.Common;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayTests
{
public class PathFindingTests
{
    private const string CurrentScene = "TestPathFindingTiledYard";
    
    private Transform _position1;
    private Transform _position2;
    private Transform _position3;
    private GameObject _pathFollowingGameObject;
    private GameObject _dijkstraPathFindingGameObject;
    private GameObject _target;
    private PathFollowingSteeringBehavior _pathFollowingSteeringBehavior;
    private PathFinderSteeringBehavior _dijkstraPathFinderSteeringBehavior;
    private AgentMover _pathFollowingAgent;
    private AgentColor _pathFollowingAgentColor;
    private AgentColor _dijkstraPathFinderAgentColor;
    private GameObject _pathGameObject;
    private GameObject _path2GameObject;
    private Path _path;
    private Path _path2;
    
    
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Clean up any existing objects first
        _pathFollowingGameObject = null;
        _pathFollowingSteeringBehavior = null;
        _pathFollowingAgent = null;
        _pathFollowingAgentColor = null;
        _pathGameObject = null;
        _path = null;
        
        // Clean up any existing objects first.

        // Load the test scene
        yield return TestLevelManagement.ReLoadScene(CurrentScene);
        yield return null;
        
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
        
        if (_target == null)
            _target = GameObject.Find("Target");
        
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
        
        if (_pathFollowingAgent == null)
            _pathFollowingAgent = _pathFollowingGameObject.GetComponent<AgentMover>();
        
        if (_pathFollowingSteeringBehavior == null)
            _pathFollowingSteeringBehavior = 
                _pathFollowingGameObject.GetComponentInChildren<PathFollowingSteeringBehavior>();
        
        if (_dijkstraPathFinderSteeringBehavior == null)
            _dijkstraPathFinderSteeringBehavior = 
                _dijkstraPathFindingGameObject.GetComponentInChildren<PathFinderSteeringBehavior>();
        
        if (_pathFollowingAgentColor == null)
            _pathFollowingAgentColor = _pathFollowingGameObject.GetComponent<AgentColor>();
        
        if (_dijkstraPathFinderAgentColor == null)
            _dijkstraPathFinderAgentColor = _dijkstraPathFindingGameObject.GetComponent<AgentColor>();
        
        if (_path == null)
            _path = _pathGameObject.GetComponent<Path>();
        
        if (_path2 == null)
            _path2 = _path2GameObject.GetComponent<Path>();
    }
    
    
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_pathFollowingGameObject != null)
            _pathFollowingGameObject.SetActive(false);
        if (_pathGameObject != null)
            _pathGameObject.SetActive(false);
        if (_dijkstraPathFindingGameObject != null)
            _dijkstraPathFindingGameObject.SetActive(false);

        yield return null;
    }


    /// <summary>
    /// Test the path following behavior.
    /// </summary>
    [UnityTest]
    public IEnumerator PathFollowingBehaviorTest()
    {
        // Set up agents before the tests.
        _pathFollowingGameObject.transform.position = _position1.position;
        _pathFollowingAgent.MaximumSpeed = 4.0f;
        _pathFollowingAgent.StopSpeed = 0.01f;
        _pathFollowingAgent.MaximumRotationalSpeed = 1080f;
        _pathFollowingAgent.StopRotationThreshold = 1f;
        _pathFollowingAgentColor.Color = Color.green;
        _pathFollowingSteeringBehavior.FollowPath = _path;
        _pathFollowingSteeringBehavior.arrivalDistance = 0.3f;
        _pathGameObject.SetActive(true);
        _path.loop = false;
        _path.ShowGizmos = true;
        _pathFollowingGameObject.SetActive(true);
        
        // Start test.
        // Assert that the path following agent can reach every target position in the
        // path.
        float maximumWaitTime = 10;
        float waitStep = 0.001f;
        
        foreach (Vector2 targetPosition in _path.positions)
        {
            bool targetReached = false;
            float elapsedTime = 0;
            while (elapsedTime < maximumWaitTime)
            {
                yield return new WaitForSeconds(waitStep);
                elapsedTime += waitStep;
                targetReached = Vector2.Distance(_pathFollowingGameObject.transform.position, targetPosition) < 0.3f;
                if (targetReached) break;
            }
            Assert.True(targetReached);
        }
    }

    
    /// <summary>
    /// Test the path following behavior loop feature.
    /// </summary>
    [UnityTest]
    public IEnumerator LoopPathFollowingBehaviorTest()
    {
        // Set up agents before the tests.
        _pathFollowingGameObject.transform.position = _position1.position;
        _pathFollowingAgent.MaximumSpeed = 4.0f;
        _pathFollowingAgent.StopSpeed = 0.01f;
        _pathFollowingAgent.MaximumRotationalSpeed = 1080f;
        _pathFollowingAgent.StopRotationThreshold = 1f;
        _pathFollowingAgentColor.Color = Color.green;
        _pathFollowingSteeringBehavior.FollowPath = _path2;
        _pathFollowingSteeringBehavior.arrivalDistance = 0.3f;
        _path2GameObject.SetActive(true);
        _path2.loop = true;
        _path2.ShowGizmos = true;
        _pathFollowingGameObject.SetActive(true);

        // Start test.
        // Assert that the path following agent reach twice the firs target position
        // in the path.
        float maximumWaitTime = 10;
        float waitStep = 0.001f;

        Vector2 firstTargetPosition = _path2.positions[0];

        bool targetAlreadyReached = false;
        float elapsedTime = 0;
        uint timesReached = 0;
        while (elapsedTime < maximumWaitTime)
        {
            yield return new WaitForSeconds(waitStep);
            elapsedTime += waitStep;
            if (Vector2.Distance(_pathFollowingGameObject.transform.position,
                    firstTargetPosition) < 0.3f)
            {
                // We want to increment the counter just once every time we get near
                // the target.
                if (!targetAlreadyReached)
                {
                    targetAlreadyReached = true;
                    timesReached++;
                }
            }
            else
            {
                targetAlreadyReached = false;
            }

            if (timesReached >= 2) break;
        }

        Assert.True(timesReached > 1);
    }
    
    /// <summary>
    /// Test the Dijkstra pathfinder behavior.
    /// </summary>
    [UnityTest]
    public IEnumerator DijkstraPathFindingBehaviorTest()
    {
        // Set up agents before the tests.
        _dijkstraPathFindingGameObject.transform.position = _position1.position;
        _pathFollowingAgent.MaximumSpeed = 4.0f;
        _pathFollowingAgent.StopSpeed = 0.01f;
        _pathFollowingAgent.MaximumRotationalSpeed = 1080f;
        _pathFollowingAgent.StopRotationThreshold = 1f;
        _pathFollowingAgentColor.Color = Color.green;
        _pathFollowingSteeringBehavior.FollowPath = _path;
        _pathFollowingSteeringBehavior.arrivalDistance = 0.3f;
        _dijkstraPathFindingGameObject.SetActive(true);

        // Start test.
        // Assert that the pathfinder agent can reach the first target.
        _target.transform.position = _position2.position;
        yield return new WaitForSeconds(12);
        Assert.True(Vector2.Distance(_dijkstraPathFindingGameObject.transform.position, _position2.position) < 0.3f);
        
        // Assert that the pathfinder agent can reach the second target.
        _target.transform.position = _position3.position;
        yield return new WaitForSeconds(12);
        Assert.True(Vector2.Distance(_dijkstraPathFindingGameObject.transform.position, _position3.position) < 0.3f);
    }
}
}