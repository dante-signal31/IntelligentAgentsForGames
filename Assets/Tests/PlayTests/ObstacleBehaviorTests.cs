﻿using System.Collections;
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
        private Transform _position5;
        private Transform _position6;

        private TargetPlacement _target;
        
        private GameObject _seekGameObject;
        private GameObject _hideGameObject;
        private GameObject _wallAvoiderGameObject;
        private GameObject _smoothedWallAvoiderGameObject;
        
        private SeekSteeringBehavior _seekSteeringBehavior;
        private HideSteeringBehavior _hideSteeringBehavior;
        private WallAvoiderSteeringBehavior _wallAvoiderSteeringBehavior;
        private SmoothedWallAvoiderSteeringBehavior _smoothedWallAvoiderSteeringBehavior;
        
        private AgentMover _seekAgent;
        private AgentMover _hideAgent;
        private AgentMover _wallAvoiderAgent;
        private AgentMover _smoothedWallAvoiderAgent;
        
        private AgentColor _seekAgentColor;
        private AgentColor _hideAgentColor;
        private AgentColor _wallAvoiderAgentColor;
        private AgentColor _smoothedWallAvoiderAgentColor;
        

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
            _seekAgentColor = null;
            _hideAgentColor = null;
            _wallAvoiderAgentColor = null;
            _smoothedWallAvoiderAgentColor = null;
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

            // if (_wallAvoiderGameObject == null)
            // {
            //     _wallAvoiderGameObject = GameObject.Find("WallAvoiderMovingAgent");
            //     _wallAvoiderGameObject.SetActive(false);
            // }
            //
            // if (_smoothedWallAvoiderGameObject == null)
            // {
            //     _smoothedWallAvoiderGameObject = GameObject.Find("SmoothedWallAvoiderMovingAgent");
            //     _smoothedWallAvoiderGameObject.SetActive(false);
            // }
            
            if (_seekSteeringBehavior == null)
                _seekSteeringBehavior = 
                    _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            if (_hideSteeringBehavior == null)
                _hideSteeringBehavior = 
                    _hideGameObject.GetComponentInChildren<HideSteeringBehavior>();
            // if (_wallAvoiderSteeringBehavior == null)
            //     _wallAvoiderSteeringBehavior = 
            //         _wallAvoiderGameObject.GetComponent<WallAvoiderSteeringBehavior>();
            // if (_smoothedWallAvoiderSteeringBehavior == null)
            //     _smoothedWallAvoiderSteeringBehavior = 
            //         _smoothedWallAvoiderGameObject.GetComponent<SmoothedWallAvoiderSteeringBehavior>();
            if (_seekAgent == null)
                _seekAgent = _seekGameObject.GetComponent<AgentMover>();
            if (_hideAgent == null)
                _hideAgent = _hideGameObject.GetComponent<AgentMover>();
            // if (_wallAvoiderAgent == null)
            //     _wallAvoiderAgent = _wallAvoiderGameObject.GetComponent<AgentMover>();
            // if (_smoothedWallAvoiderAgent == null)
            //     _smoothedWallAvoiderAgent = 
            //     _smoothedWallAvoiderGameObject.GetComponent<AgentMover>();
            if (_seekAgentColor == null)
                _seekAgentColor = _seekGameObject.GetComponent<AgentColor>();
            if (_hideAgentColor == null)
                _hideAgentColor = _hideGameObject.GetComponent<AgentColor>();
        //     if (_wallAvoiderAgentColor == null)
        //         _wallAvoiderAgentColor = 
        //             _wallAvoiderGameObject.GetComponent<AgentColor>();
        //     if (_smoothedWallAvoiderAgentColor == null)
        //         _smoothedWallAvoiderAgentColor = 
        //             _smoothedWallAvoiderGameObject.GetComponent<AgentColor>();
        }
        
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_seekGameObject != null)
                _seekGameObject.SetActive(false);
            if (_hideGameObject != null)
                _hideGameObject.SetActive(false);
            // if (_wallAvoiderGameObject != null)
            //     _wallAvoiderGameObject.SetActive(false);
            // if (_smoothedWallAvoiderGameObject != null)
            //     _smoothedWallAvoiderGameObject.SetActive(false);
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
            yield return null;
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


        // TODO: Fix this automated test. When I run WallAvoiderBehavior test in Play Mode
        // it works fine, but when I run it from this test it launch the error:
        //
        // "Cannot instantiate objects with a parent which is persistent. New object will be created without a parent."
        //
        // This error happens when WhiskerSensors instantiates sensor and tries to parent
        // to him. Because of that sensors are missing.
        /// <summary>
        /// Test that WallAvoiderBehavior can move an agent from a point A to a point B
        /// avoiding obstacles.
        /// </summary>
        // [UnityTest]
        // public IEnumerator WallAvoiderBehaviorTest()
        // {
        //     // Setup agents before the tests.
        //     _target.Enabled = true;
        //     _target.TargetPosition = _position5.position;
        //     
        //     _wallAvoiderGameObject.transform.position = _position6.position;
        //     _wallAvoiderAgent.MaximumSpeed = 2.0f;
        //     _wallAvoiderAgent.StopSpeed = 0.01f;
        //     _wallAvoiderAgent.MaximumRotationalSpeed = 1080f;
        //     _wallAvoiderAgent.StopRotationThreshold = 1f;
        //     _wallAvoiderAgentColor.Color = Color.green;
        //     _wallAvoiderSteeringBehavior.Target = _target.gameObject;
        //     _wallAvoiderSteeringBehavior.AvoidLayerMask = 
        //         LayerMask.GetMask("Obstacles");
        //     _wallAvoiderGameObject.SetActive(true);
        //     
        //     
        //     // Start test.
        //     
        //     // Give hide agent time to hide.
        //     yield return new WaitForSeconds(1f);
        //     yield return new WaitForSeconds(6f);
        //     
        //     // Assert that wall avoider has reached target.
        //     Assert.True(Vector2.Distance(
        //         _wallAvoiderAgent.transform.position, 
        //         _target.TargetPosition) < 0.1f);
        // }
        
        // TODO: Fix this automated test. When I run SmoothedWallAvoiderBehavior test in Play Mode
        // it works fine, but when I run it from this test it launch the error:
        //
        // "Cannot instantiate objects with a parent which is persistent. New object will be created without a parent."
        //
        // This error happens when WhiskerSensors instantiates sensor and tries to parent
        // to him. Because of that sensors are missing.
        // [UnityTest]
        // public IEnumerator SmoothedWallAvoiderBehaviorTest()
        // {
        //     // Setup agents before the tests.
        //     _target.Enabled = true;
        //     _target.TargetPosition = _position5.position;
        //     
        //     _smoothedWallAvoiderGameObject.transform.position = _position6.position;
        //     _smoothedWallAvoiderAgent.MaximumSpeed = 2.0f;
        //     _smoothedWallAvoiderAgent.StopSpeed = 0.01f;
        //     _smoothedWallAvoiderAgent.MaximumRotationalSpeed = 1080f;
        //     _smoothedWallAvoiderAgent.StopRotationThreshold = 1f;
        //     _smoothedWallAvoiderAgentColor.Color = Color.green;
        //     _smoothedWallAvoiderSteeringBehavior.Target = _target.gameObject;
        //     _smoothedWallAvoiderSteeringBehavior.ShowGizmos = true;
        //     _smoothedWallAvoiderGameObject.SetActive(true);
        //     
        //     
        //     // Start test.
        //     
        //     // Give agent time to reach its target.
        //     yield return new WaitForSeconds(6f);
        //     
        //     // Assert that wall avoider has reached target.
        //     Assert.True(Vector2.Distance(
        //         _smoothedWallAvoiderAgent.transform.position, 
        //         _target.TargetPosition) < 0.1f);
        // }
    }
}