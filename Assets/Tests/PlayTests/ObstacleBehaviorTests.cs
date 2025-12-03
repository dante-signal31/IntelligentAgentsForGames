using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Pathfinding;
using SteeringBehaviors;
using Tests.PlayTests.Common;
using UnityEngine;
using UnityEngine.TestTools;


namespace Tests.PlayTests
{
    public class ObstacleBehaviorTests
    {
        private const string CurrentScene = "TestObstacleTiledYard";
        
        private Transform _position1;
        private Transform _position2;
        private Transform _position3;
        private Transform _position4;
        private Transform _position5;
        private Transform _position6;

        private TargetPlacement _target;
        
        private GameObject _seekGameObject;
        private GameObject _hideGameObject;
        private GameObject _wallAvoiderGameObject;
        private GameObject _smoothedWallAvoiderGameObject;
        private GameObject _weightBlendedHideWallAvoiderGameObject;
        private GameObject _priorityWeightBlendedHideWallAvoiderGameObject;
        private GameObject _priorityDitheringBlendedHideWallAvoiderGameObject;
        private GameObject _pathFollowingGameObject;
        private GameObject _pathGameObject;
        
        private SeekSteeringBehavior _seekSteeringBehavior;
        private HideSteeringBehavior _hideSteeringBehavior;
        private ActiveWallAvoiderSteeringBehavior _wallAvoiderSteeringBehavior;
        private SmoothedWallAvoiderSteeringBehavior _smoothedWallAvoiderSteeringBehavior;
        
        private AgentMover _seekAgent;
        private AgentMover _hideAgent;
        private AgentMover _wallAvoiderAgent;
        private AgentMover _smoothedWallAvoiderAgent;
        private AgentMover _weightBlendedHideWallAvoiderAgent;
        private AgentMover _priorityWeightBlendedHideWallAvoiderAgent;
        private AgentMover _priorityDitheringBlendedHideWallAvoiderAgent;
        
        private AgentColor _seekAgentColor;
        private AgentColor _hideAgentColor;
        private AgentColor _wallAvoiderAgentColor;
        private AgentColor _smoothedWallAvoiderAgentColor;
        private AgentColor _weightBlendedHideWallAvoiderAgentColor;
        private AgentColor _priorityWeightBlendedHideWallAvoiderAgentColor;
        private AgentColor _priorityDitheringBlendedHideWallAvoiderAgentColor;
        
        

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Clean up any existing objects first
            _seekGameObject = null;
            _hideGameObject = null;
            _wallAvoiderGameObject = null;
            _seekSteeringBehavior = null;
            _hideSteeringBehavior = null;
            _wallAvoiderSteeringBehavior = null;
            _smoothedWallAvoiderSteeringBehavior = null;
            _seekAgent = null;
            _hideAgent = null;
            _wallAvoiderAgent = null;
            _smoothedWallAvoiderAgent = null;
            _weightBlendedHideWallAvoiderAgent = null;
            _seekAgentColor = null;
            _hideAgentColor = null;
            _wallAvoiderAgentColor = null;
            _smoothedWallAvoiderAgentColor = null;
            _weightBlendedHideWallAvoiderAgentColor = null;
            _priorityWeightBlendedHideWallAvoiderAgentColor = null;
            _target = null;
    
            // Load the test scene
            yield return TestLevelManagement.ReLoadScene(CurrentScene);
            yield return null;
            
            if (_position1 == null)
                _position1 = GameObject.Find("Position1").transform;
            if (_position2 == null)
                _position2 = GameObject.Find("Position2").transform;
            if (_position3 == null)
                _position3 = GameObject.Find("Position3").transform;
            if (_position4 == null)
                _position4 = GameObject.Find("Position4").transform;
            if (_position5 == null)
                _position5 = GameObject.Find("Position5").transform;
            if (_position6 == null)
                _position6 = GameObject.Find("Position6").transform;
            if (_target == null)
            {
                _target = GameObject.Find("Target").GetComponent<TargetPlacement>();
                _target.TargetPosition = _position1.position;
                _target.Enabled = false;
            }

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
            
            if (_seekSteeringBehavior == null)
                _seekSteeringBehavior = 
                    _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            if (_hideSteeringBehavior == null)
                _hideSteeringBehavior = 
                    _hideGameObject.GetComponentInChildren<HideSteeringBehavior>();
            if (_wallAvoiderSteeringBehavior == null)
                _wallAvoiderSteeringBehavior = 
                    _wallAvoiderGameObject.GetComponentInChildren<ActiveWallAvoiderSteeringBehavior>();
            if (_smoothedWallAvoiderSteeringBehavior == null)
                _smoothedWallAvoiderSteeringBehavior = 
                    _smoothedWallAvoiderGameObject.GetComponentInChildren<SmoothedWallAvoiderSteeringBehavior>();
            if (_seekAgent == null)
                _seekAgent = _seekGameObject.GetComponent<AgentMover>();
            if (_hideAgent == null)
                _hideAgent = _hideGameObject.GetComponent<AgentMover>();
            if (_wallAvoiderAgent == null)
                _wallAvoiderAgent = _wallAvoiderGameObject.GetComponent<AgentMover>();
            if (_smoothedWallAvoiderAgent == null)
                _smoothedWallAvoiderAgent = 
                _smoothedWallAvoiderGameObject.GetComponent<AgentMover>();
            if (_weightBlendedHideWallAvoiderAgent == null)
                _weightBlendedHideWallAvoiderAgent = 
                    _weightBlendedHideWallAvoiderGameObject.GetComponent<AgentMover>();
            if (_priorityWeightBlendedHideWallAvoiderAgent == null)
                _priorityWeightBlendedHideWallAvoiderAgent =
                    _priorityWeightBlendedHideWallAvoiderGameObject
                        .GetComponent<AgentMover>();
            if (_priorityDitheringBlendedHideWallAvoiderAgent == null)
                _priorityDitheringBlendedHideWallAvoiderAgent =
                    _priorityDitheringBlendedHideWallAvoiderGameObject
                        .GetComponent<AgentMover>();
            
            if (_seekAgentColor == null)
                _seekAgentColor = _seekGameObject.GetComponent<AgentColor>();
            if (_hideAgentColor == null)
                _hideAgentColor = _hideGameObject.GetComponent<AgentColor>();
            if (_wallAvoiderAgentColor == null)
                _wallAvoiderAgentColor = 
                    _wallAvoiderGameObject.GetComponent<AgentColor>();
            if (_smoothedWallAvoiderAgentColor == null)
                _smoothedWallAvoiderAgentColor = 
                    _smoothedWallAvoiderGameObject.GetComponent<AgentColor>();
            if (_weightBlendedHideWallAvoiderAgentColor == null)
                _weightBlendedHideWallAvoiderAgentColor = 
                    _weightBlendedHideWallAvoiderGameObject.GetComponent<AgentColor>();
            if (_priorityWeightBlendedHideWallAvoiderAgentColor == null)
                _priorityWeightBlendedHideWallAvoiderAgentColor = 
                    _priorityWeightBlendedHideWallAvoiderGameObject.GetComponent<AgentColor>();
            if (_priorityDitheringBlendedHideWallAvoiderAgentColor == null)
                _priorityDitheringBlendedHideWallAvoiderAgentColor = 
                    _priorityDitheringBlendedHideWallAvoiderGameObject.GetComponent<AgentColor>();
        }
        
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_seekGameObject != null)
                _seekGameObject.SetActive(false);
            if (_hideGameObject != null)
                _hideGameObject.SetActive(false);
            if (_wallAvoiderGameObject != null)
                _wallAvoiderGameObject.SetActive(false);
            if (_smoothedWallAvoiderGameObject != null)
                _smoothedWallAvoiderGameObject.SetActive(false);
            if (_weightBlendedHideWallAvoiderGameObject != null)
                _weightBlendedHideWallAvoiderGameObject.SetActive(false);
            if (_priorityWeightBlendedHideWallAvoiderGameObject != null)
                _priorityWeightBlendedHideWallAvoiderGameObject.SetActive(false);
            if (_priorityDitheringBlendedHideWallAvoiderGameObject != null)
                _priorityDitheringBlendedHideWallAvoiderGameObject.SetActive(false);
            if (_target != null)
                _target.Enabled = false;

            yield return null;
        }


        /// <summary>
        /// Test that HideBehavior hides from a moving SeekBehavior.
        /// </summary>
        [UnityTest]
        public IEnumerator HideBehaviorTest()
        {
            // Setup agents before the tests.
            _target.Enabled = true;
            _target.TargetPosition = _position2.position;
            _seekGameObject.transform.position = _position1.position;
            _seekAgent.MaximumSpeed = 6.0f;
            _seekAgent.StopSpeed = 0.01f;
            _seekAgent.MaximumRotationalSpeed = 1080f;
            _seekAgent.StopRotationThreshold = 1f;
            _seekAgentColor.Color = Color.red;
            _seekSteeringBehavior.Target = _target.gameObject;
            _seekSteeringBehavior.ArrivalDistance = 0.3f;
            _seekGameObject.SetActive(true);
            
            _hideGameObject.transform.position = _position2.position;
            _hideAgent.MaximumSpeed = 7.0f;
            _hideAgent.StopSpeed = 0.01f;
            _hideAgent.MaximumRotationalSpeed = 1080f;
            _hideAgent.StopRotationThreshold = 1f;
            _hideAgentColor.Color = Color.green;
            _hideSteeringBehavior.Threat = _seekAgent;
            _hideSteeringBehavior.ArrivalDistance = 0.3f;
            _hideSteeringBehavior.ObstaclesLayer = LayerMask.GetMask("Obstacles");
            _hideSteeringBehavior.SeparationFromObstacles = 0.3f;
            _hideSteeringBehavior.AgentRadius = 0.5f;
            _hideSteeringBehavior.NotEmptyGroundLayers = LayerMask.GetMask("Obstacles");
            _hideSteeringBehavior.ThreatLayerMask = LayerMask.GetMask("Agents");
            _hideGameObject.SetActive(true);
            
            // Start test.
            // Assert that seek agent can see hide agent.
            yield return new WaitForSeconds(0.1f);
            Assert.True(_hideSteeringBehavior.ThreatCanSeeUs);
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(2f);
            
            // Assert that seek agent can no longer see hide agent.
            Assert.False(_hideSteeringBehavior.ThreatCanSeeUs);
            
            // Move seek agent to another position.
            _target.transform.position = _position3.position;
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(2f);
            
            // Assert that seek agent can no longer see hide agent.
            Assert.False(_hideSteeringBehavior.ThreatCanSeeUs);
            
            // Move seek agent to another position.
            _target.transform.position = _position4.position;
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(2f);
            
            // Assert that seek agent can no longer see hide agent.
            Assert.False(_hideSteeringBehavior.ThreatCanSeeUs);
        }

        
        /// <summary>
        /// Test that WallAvoiderBehavior can move an agent from a point A to a point B
        /// avoiding obstacles.
        /// </summary>
        [UnityTest]
        public IEnumerator WallAvoiderBehaviorTest()
        {
            // Setup agents before the tests.
            _target.Enabled = true;
            _target.TargetPosition = _position5.position;
            
            _wallAvoiderGameObject.transform.position = _position6.position;
            _wallAvoiderAgent.MaximumSpeed = 1.0f;
            _wallAvoiderAgent.StopSpeed = 0.01f;
            _wallAvoiderAgent.MaximumRotationalSpeed = 1080f;
            _wallAvoiderAgent.StopRotationThreshold = 1f;
            _wallAvoiderAgent.AutoSmooth = false;
            _wallAvoiderAgentColor.Color = Color.green;
            _wallAvoiderSteeringBehavior.Target = _target.gameObject;
            _wallAvoiderGameObject.SetActive(true);
            
            
            // Start test.
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(19f);
            
            // Assert that wall avoider has reached target.
            Assert.True(Vector2.Distance(
                _wallAvoiderAgent.transform.position, 
                _target.TargetPosition) < 0.3f);
        }
        
        /// <summary>
        /// Test that WallAvoiderBehavior can move an agent from a point A to a point B
        /// avoiding obstacles and using auto-smoothed movement.
        /// </summary>
        [UnityTest]
        public IEnumerator AutoSmoothedWallAvoiderBehaviorTest()
        {
            // Setup agents before the tests.
            _target.Enabled = true;
            _target.TargetPosition = _position5.position;
            
            _wallAvoiderGameObject.transform.position = _position6.position;
            _wallAvoiderAgent.MaximumSpeed = 2.0f;
            _wallAvoiderAgent.StopSpeed = 0.01f;
            _wallAvoiderAgent.MaximumRotationalSpeed = 1080f;
            _wallAvoiderAgent.StopRotationThreshold = 1f;
            _wallAvoiderAgent.AutoSmooth = true;
            _wallAvoiderAgent.AutoSmoothSamples = 10;
            _wallAvoiderAgentColor.Color = Color.green;
            _wallAvoiderSteeringBehavior.Target = _target.gameObject;
            _wallAvoiderGameObject.SetActive(true);
            
            
            // Start test.
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(8f);
            
            // Assert that wall avoider has reached the target.
            Assert.True(Vector2.Distance(
                _wallAvoiderAgent.transform.position, 
                _target.TargetPosition) < 0.3f);
        }
        
        [UnityTest]
        public IEnumerator SmoothedWallAvoiderBehaviorTest()
        {
            // Setup agents before the tests.
            _target.Enabled = true;
            _target.TargetPosition = _position5.position;
            
            _smoothedWallAvoiderGameObject.transform.position = _position1.position;
            _smoothedWallAvoiderAgent.MaximumSpeed = 1.0f;
            _smoothedWallAvoiderAgent.StopSpeed = 0.01f;
            _smoothedWallAvoiderAgent.MaximumRotationalSpeed = 1080f;
            _smoothedWallAvoiderAgent.StopRotationThreshold = 1f;
            _smoothedWallAvoiderAgentColor.Color = Color.green;
            _smoothedWallAvoiderSteeringBehavior.Target = _target.gameObject;
            _smoothedWallAvoiderSteeringBehavior.ShowGizmos = true;
            _smoothedWallAvoiderGameObject.SetActive(true);
            
            
            // Start test.
            
            // Give agent time to reach its target.
            yield return new WaitForSeconds(18f);
            
            // Assert that wall avoider has reached target.
            Assert.True(Vector2.Distance(
                _smoothedWallAvoiderAgent.transform.position, 
                _target.TargetPosition) < 0.3f);
        }
        
        /// <summary>
        /// Test that WeightBlendedHideWallAvoiderBehavior can hide from a moving SeekBehavior
        /// and reach its final destination.
        /// </summary>
        [UnityTest]
        public IEnumerator WeightBlendedHideWallAvoiderBehaviorTest()
        {
            // Setup agents before the tests.
            _target.Enabled = true;
            _target.TargetPosition = _position2.position;
            _seekGameObject.transform.position = _position1.position;
            _seekAgent.MaximumSpeed = 2.0f;
            _seekAgent.StopSpeed = 0.01f;
            _seekAgent.MaximumRotationalSpeed = 1080f;
            _seekAgent.StopRotationThreshold = 1f;
            _seekAgentColor.Color = Color.red;
            _seekSteeringBehavior.Target = _target.gameObject;
            _seekSteeringBehavior.ArrivalDistance = 0.3f;
            _seekGameObject.SetActive(true);
            
            _weightBlendedHideWallAvoiderAgent.transform.position = _position3.position;
            _weightBlendedHideWallAvoiderAgent.MaximumSpeed = 2.0f;
            _weightBlendedHideWallAvoiderAgent.StopSpeed = 0.01f;
            _weightBlendedHideWallAvoiderAgent.MaximumRotationalSpeed = 1080f;
            _weightBlendedHideWallAvoiderAgent.StopRotationThreshold = 1f;
            _weightBlendedHideWallAvoiderAgentColor.Color = Color.green;
            
            HideSteeringBehavior hideSteeringBehavior = 
                _weightBlendedHideWallAvoiderAgent.GetComponentInChildren<HideSteeringBehavior>();
            hideSteeringBehavior.Threat = _seekAgent;
            hideSteeringBehavior.ArrivalDistance = 0.3f;
            hideSteeringBehavior.ObstaclesLayer = LayerMask.GetMask("Obstacles");
            hideSteeringBehavior.SeparationFromObstacles = 0.2f;
            hideSteeringBehavior.AgentRadius = 0.5f;
            hideSteeringBehavior.NotEmptyGroundLayers = LayerMask.GetMask("Obstacles");
            hideSteeringBehavior.ThreatLayerMask = LayerMask.GetMask("Agents");
            
            // Set HiderWallAvoider agent its final destination.
            ActiveWallAvoiderSteeringBehavior wallAvoiderSteeringBehavior = 
                _weightBlendedHideWallAvoiderAgent.GetComponentInChildren<ActiveWallAvoiderSteeringBehavior>();
            wallAvoiderSteeringBehavior.Target = _position2.gameObject;;
            
            _weightBlendedHideWallAvoiderGameObject.SetActive(true);
            
            // Start test.
            // Assert that seek agent can see hide agent.
            yield return new WaitForSeconds(0.2f);
            Assert.True(hideSteeringBehavior.ThreatCanSeeUs);
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(3f);
            
            // Assert that seek agent can no longer see hide agent.
            Assert.False(hideSteeringBehavior.ThreatCanSeeUs);
            
            // Move seek agent to another position.
            _target.transform.position = _position3.position;
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(5f);
            
            // Assert that seek agent can no longer see hide agent.
            Assert.False(hideSteeringBehavior.ThreatCanSeeUs);
            
            // Move seek agent to another position.
            _target.transform.position = _position4.position;
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(3f);
            
            // Assert that seek agent can no longer see hide agent.
            Assert.False(hideSteeringBehavior.ThreatCanSeeUs);
            
            // Give agent time to reach its final destination.
            yield return new WaitForSeconds(3f);
            
            // Assert that seek agent can no longer see hide agent.
            Assert.True(Vector2.Distance(_weightBlendedHideWallAvoiderAgent.transform.position, 
                _position2.position) < 0.3f);
        }
        
        /// <summary>
        /// Test that PriorityWeightBlendedHideWallAvoiderBehavior can hide from a
        /// moving SeekBehavior and reach its final destination.
        /// </summary>
        [UnityTest]
        public IEnumerator PriorityWeightBlendedHideWallAvoiderBehaviorTest()
        {
            // Setup agents before the tests.
            _target.Enabled = true;
            _target.TargetPosition = _position2.position;
            _seekGameObject.transform.position = _position1.position;
            _seekAgent.MaximumSpeed = 2.0f;
            _seekAgent.StopSpeed = 0.01f;
            _seekAgent.MaximumRotationalSpeed = 1080f;
            _seekAgent.StopRotationThreshold = 1f;
            _seekAgentColor.Color = Color.red;
            _seekSteeringBehavior.Target = _target.gameObject;
            _seekSteeringBehavior.ArrivalDistance = 0.3f;
            _seekGameObject.SetActive(true);
            
            _priorityWeightBlendedHideWallAvoiderAgent.transform.position = _position3.position;
            _priorityWeightBlendedHideWallAvoiderAgent.MaximumSpeed = 2.0f;
            _priorityWeightBlendedHideWallAvoiderAgent.StopSpeed = 0.01f;
            _priorityWeightBlendedHideWallAvoiderAgent.MaximumRotationalSpeed = 1080f;
            _priorityWeightBlendedHideWallAvoiderAgent.StopRotationThreshold = 1f;
            _priorityWeightBlendedHideWallAvoiderAgentColor.Color = Color.green;
            
            HideSteeringBehavior hideSteeringBehavior = 
                _priorityWeightBlendedHideWallAvoiderAgent.GetComponentInChildren<HideSteeringBehavior>();
            hideSteeringBehavior.Threat = _seekAgent;
            hideSteeringBehavior.ArrivalDistance = 0.3f;
            hideSteeringBehavior.ObstaclesLayer = LayerMask.GetMask("Obstacles");
            hideSteeringBehavior.SeparationFromObstacles = 0.2f;
            hideSteeringBehavior.AgentRadius = 0.5f;
            hideSteeringBehavior.NotEmptyGroundLayers = LayerMask.GetMask("Obstacles");
            hideSteeringBehavior.ThreatLayerMask = LayerMask.GetMask("Agents");
            
            // Set HiderWallAvoider agent its final destination.
            ArriveSteeringBehaviorLA arriveSteeringBehavior = 
                _priorityWeightBlendedHideWallAvoiderAgent.GetComponentInChildren<ArriveSteeringBehaviorLA>();
            arriveSteeringBehavior.Target = _position2.gameObject;;
            
            _priorityWeightBlendedHideWallAvoiderGameObject.SetActive(true);
            
            // Start test.
            // Assert that seek agent can see hide agent.
            yield return new WaitForSeconds(0.2f);
            Assert.True(hideSteeringBehavior.ThreatCanSeeUs);
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(3f);
            
            // Assert that seek agent can no longer see hide agent.
            Assert.False(hideSteeringBehavior.ThreatCanSeeUs);
            
            // Move seek agent to another position.
            _target.transform.position = _position3.position;
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(5f);
            
            // Assert that seek agent can no longer see hide agent.
            Assert.False(hideSteeringBehavior.ThreatCanSeeUs);
            
            // Move seek agent to another position.
            _target.transform.position = _position4.position;
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(3f);
            
            // Assert that seek agent can no longer see hide agent.
            Assert.False(hideSteeringBehavior.ThreatCanSeeUs);
            
            // Give agent time to reach its final destination.
            yield return new WaitForSeconds(3f);
            
            // Assert we reached our target.
            Assert.True(Vector2.Distance(_priorityWeightBlendedHideWallAvoiderAgent.transform.position, 
                _position2.position) < 1.5f);
        }
        
        /// <summary>
        /// Test that PriorityDitheringBlendedHideWallAvoiderBehavior can hide from a
        /// moving SeekBehavior and reach its final destination.
        /// </summary>
        [UnityTest]
        public IEnumerator PriorityDitheringBlendedHideWallAvoiderBehaviorTest()
        {
            // Set up agents before the tests.
            _target.Enabled = true;
            _target.TargetPosition = _position2.position;
            _seekGameObject.transform.position = _position1.position;
            _seekAgent.MaximumSpeed = 2.0f;
            _seekAgent.StopSpeed = 0.01f;
            _seekAgent.MaximumRotationalSpeed = 1080f;
            _seekAgent.StopRotationThreshold = 1f;
            _seekAgentColor.Color = Color.red;
            _seekSteeringBehavior.Target = _target.gameObject;
            _seekSteeringBehavior.ArrivalDistance = 0.3f;
            _seekGameObject.SetActive(true);
            
            _priorityDitheringBlendedHideWallAvoiderAgent.transform.position = _position3.position;
            _priorityDitheringBlendedHideWallAvoiderAgent.MaximumSpeed = 2.0f;
            _priorityDitheringBlendedHideWallAvoiderAgent.StopSpeed = 0.01f;
            _priorityDitheringBlendedHideWallAvoiderAgent.MaximumRotationalSpeed = 1080f;
            _priorityDitheringBlendedHideWallAvoiderAgent.StopRotationThreshold = 1f;
            _priorityDitheringBlendedHideWallAvoiderAgentColor.Color = Color.green;
            
            HideSteeringBehavior hideSteeringBehavior = 
                _priorityDitheringBlendedHideWallAvoiderAgent.GetComponentInChildren<HideSteeringBehavior>();
            hideSteeringBehavior.Threat = _seekAgent;
            hideSteeringBehavior.ArrivalDistance = 0.3f;
            hideSteeringBehavior.ObstaclesLayer = LayerMask.GetMask("Obstacles");
            hideSteeringBehavior.SeparationFromObstacles = 0.2f;
            hideSteeringBehavior.AgentRadius = 0.5f;
            hideSteeringBehavior.NotEmptyGroundLayers = LayerMask.GetMask("Obstacles");
            hideSteeringBehavior.ThreatLayerMask = LayerMask.GetMask("Agents");
            
            // Set HiderWallAvoider agent its final destination.
            SeekSteeringBehavior hideSeekSteeringBehavior = 
                _priorityDitheringBlendedHideWallAvoiderAgent.GetComponentInChildren<SeekSteeringBehavior>();
            hideSeekSteeringBehavior.Target = _position2.gameObject;
            
            _priorityDitheringBlendedHideWallAvoiderGameObject.SetActive(true);
            
            // Start test.
            // Assert that seek agent can see hide agent.
            yield return new WaitForSeconds(0.2f);
            Assert.True(hideSteeringBehavior.ThreatCanSeeUs);
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(3f);
            
            // Assert that seek agent can no longer see hide agent.
            Assert.False(hideSteeringBehavior.ThreatCanSeeUs);
            
            // Move seek agent to another position.
            _target.transform.position = _position3.position;
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(5f);
            
            // Assert that seek agent can no longer see hide agent.
            Assert.False(hideSteeringBehavior.ThreatCanSeeUs);
            
            // Move seek agent to another position.
            _target.transform.position = _position4.position;
            
            // Give hide agent time to hide.
            yield return new WaitForSeconds(4f);
            
            // Assert that seek agent can no longer see hide agent.
            Assert.False(hideSteeringBehavior.ThreatCanSeeUs);
            
            // Give the agent time to reach its final destination.
            yield return new WaitForSeconds(4f);
            
            // Assert we reached our target.
            Assert.True(Vector2.Distance(_priorityDitheringBlendedHideWallAvoiderAgent.transform.position, 
                _position2.position) < 1.5f);
        }
    }
}