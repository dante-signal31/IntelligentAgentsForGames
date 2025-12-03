using System.Collections;
using NUnit.Framework;
using Pathfinding;
using SteeringBehaviors;
using Tests.PlayTests.Common;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayTests
{
    public class PathFollowingTests
    {
        private const string CurrentScene = "TestObstacleTiledYard";
        
        private Transform _position7;
        private GameObject _pathFollowingGameObject;
        private PathFollowingSteeringBehavior _pathFollowingSteeringBehavior;
        private AgentMover _pathFollowingAgent;
        private AgentColor _pathFollowingAgentColor;
        private GameObject _pathGameObject;
        private Path _path;
        
        private GameObject _seekGameObject;
        private GameObject _hideGameObject;
        private GameObject _wallAvoiderGameObject;
        private GameObject _smoothedWallAvoiderGameObject;
        private GameObject _weightBlendedHideWallAvoiderGameObject;
        private GameObject _priorityWeightBlendedHideWallAvoiderGameObject;
        private GameObject _priorityDitheringBlendedHideWallAvoiderGameObject;
        
        
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
            
            // Clean up any existing objects first
            _seekGameObject = null;
            _hideGameObject = null;
            _wallAvoiderGameObject = null;
    
            // Load the test scene
            yield return TestLevelManagement.ReLoadScene(CurrentScene);
            yield return null;
            
            if (_position7 == null)
                _position7 = GameObject.Find("Position7").transform;

            if (_pathFollowingGameObject == null)
            {
                _pathFollowingGameObject = GameObject.Find("PathFollowingMovingAgent");
                _pathFollowingGameObject.SetActive(false);
            }
            
            if (_pathGameObject == null)
            {
                _pathGameObject = GameObject.Find("Path");
                _pathGameObject.SetActive(false);
            }
            
            if (_pathFollowingAgent == null)
                _pathFollowingAgent = _pathFollowingGameObject.GetComponent<AgentMover>();
            
            if (_pathFollowingSteeringBehavior == null)
                _pathFollowingSteeringBehavior = 
                    _pathFollowingGameObject.GetComponentInChildren<PathFollowingSteeringBehavior>();
            
            if (_pathFollowingAgentColor == null)
                _pathFollowingAgentColor = _pathFollowingGameObject.GetComponent<AgentColor>();
            
            if (_path == null)
                _path = _pathGameObject.GetComponent<Path>();
            
            if (_seekGameObject == null)
            {
                _seekGameObject = GameObject.Find("SeekMovingAgent");
                _seekGameObject.SetActive(false);
            }

            if (_hideGameObject == null)
            {
                _hideGameObject = GameObject.Find("HideMovingAgent");
                _hideGameObject.SetActive(false);
            }

            if (_wallAvoiderGameObject == null)
            {
                _wallAvoiderGameObject = GameObject.Find("WallAvoiderMovingAgent");
                _wallAvoiderGameObject.SetActive(false);
            }
            
            if (_smoothedWallAvoiderGameObject == null)
            {
                _smoothedWallAvoiderGameObject = 
                    GameObject.Find("SmoothedWallAvoiderMovingAgent");
                _smoothedWallAvoiderGameObject.SetActive(false);
            }

            if (_weightBlendedHideWallAvoiderGameObject == null)
            {
                _weightBlendedHideWallAvoiderGameObject = 
                    GameObject.Find("WeightBlendedHideWallAvoiderMovingAgent");
                _weightBlendedHideWallAvoiderGameObject.SetActive(false);
            }

            if (_priorityWeightBlendedHideWallAvoiderGameObject == null)
            {
                _priorityWeightBlendedHideWallAvoiderGameObject = 
                    GameObject.Find("PriorityWeightBlendedHideWallAvoiderMovingAgent");
                _priorityWeightBlendedHideWallAvoiderGameObject.SetActive(false);
            }

            if (_priorityDitheringBlendedHideWallAvoiderGameObject == null)
            {
                _priorityDitheringBlendedHideWallAvoiderGameObject = 
                    GameObject.Find("PriorityDitheringBlendedHideWallAvoiderMovingAgent");
                _priorityDitheringBlendedHideWallAvoiderGameObject.SetActive(false);
            }
        }
        
        
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_pathFollowingGameObject != null)
                _pathFollowingGameObject.SetActive(false);
            if (_pathGameObject != null)
                _pathGameObject.SetActive(false);

            yield return null;
        }


        /// <summary>
        /// Test the path following behavior.
        /// </summary>
        [UnityTest]
        public IEnumerator PathFollowingBehaviorTest()
        {
            // Setup agents before the tests.
            _pathFollowingGameObject.transform.position = _position7.position;
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
            // Setup agents before the tests.
            _pathFollowingGameObject.transform.position = _position7.position;
            _pathFollowingAgent.MaximumSpeed = 4.0f;
            _pathFollowingAgent.StopSpeed = 0.01f;
            _pathFollowingAgent.MaximumRotationalSpeed = 1080f;
            _pathFollowingAgent.StopRotationThreshold = 1f;
            _pathFollowingAgentColor.Color = Color.green;
            _pathFollowingSteeringBehavior.FollowPath = _path;
            _pathFollowingSteeringBehavior.arrivalDistance = 0.3f;
            _pathGameObject.SetActive(true);
            _path.loop = true;
            _path.ShowGizmos = true;
            _pathFollowingGameObject.SetActive(true);

            // Start test.
            // Assert that the path following agent reach twice the firs target position
            // in the path.
            float maximumWaitTime = 10;
            float waitStep = 0.001f;

            Vector2 firstTargetPosition = _path.positions[0];

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
    }
}