using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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

        private TargetPlacement _target;
        
        private GameObject _seekGameObject;
        private GameObject _hideGameObject;
        
        private SeekSteeringBehavior _seekSteeringBehavior;
        private HideSteeringBehavior _hideSteeringBehavior;
        private AgentMover _seekAgent;
        private AgentMover _hideAgent;
        private AgentColor _seekAgentColor;
        private AgentColor _hideAgentColor;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestLevelManagement.ReLoadScene(CurrentScene);
            
            if (_position1 == null)
                _position1 = GameObject.Find("Position1").transform;
            if (_position2 == null)
                _position2 = GameObject.Find("Position2").transform;
            if (_position3 == null)
                _position3 = GameObject.Find("Position3").transform;
            if (_position4 == null)
                _position4 = GameObject.Find("Position4").transform;
            if (_target == null)
            {
                _target = GameObject.Find("Target").GetComponent<TargetPlacement>();
                _target.TargetPosition = _position1.position;
                _target.Enabled = false;
            }
            
            if (_seekGameObject == null)
                _seekGameObject = GameObject.Find("SeekMovingAgent");
            if (_hideGameObject == null)
                _hideGameObject = GameObject.Find("HideMovingAgent");
            
            if (_seekSteeringBehavior == null)
                _seekSteeringBehavior = _seekGameObject.GetComponent<SeekSteeringBehavior>();
            if (_hideSteeringBehavior == null)
                _hideSteeringBehavior = _hideGameObject.GetComponent<HideSteeringBehavior>();
            if (_seekAgent == null)
                _seekAgent = _seekGameObject.GetComponent<AgentMover>();
            if (_hideAgent == null)
                _hideAgent = _hideGameObject.GetComponent<AgentMover>();
            if (_seekAgentColor == null)
                _seekAgentColor = _seekGameObject.GetComponent<AgentColor>();
            if (_hideAgentColor == null)
                _hideAgentColor = _hideGameObject.GetComponent<AgentColor>();
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
            
            // Cleanup.
            _seekGameObject.SetActive(false);
            _hideGameObject.SetActive(false);
            _target.Enabled = false;
        }

    }
}