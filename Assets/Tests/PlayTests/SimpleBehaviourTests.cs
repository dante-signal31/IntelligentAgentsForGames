using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SteeringBehaviors;
using Tests.PlayTests.Common;
using Tools;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayTests
{
    public class SimpleBehaviourTests
    {
        private const string CurrentScene = "TestClearTiledYard";
        
        private Transform _position5;
        private Transform _position6;
        private Transform _position7;
        private Transform _position8;
        private Transform _position9;
        private Transform _position10;
        private Transform _position1;
        private Transform _position2;
        private Transform _position3;
        private Transform _position4;
        private Transform _position11;
        private Transform _position12;
        private Transform _position13;
        private Transform _position14;

        private TargetPlacement _target;

        private GameObject _seekGameObject;
        private GameObject _alignGameObject;
        private GameObject _faceGameObject;
        private GameObject _fleeGameObject;
        private GameObject _arriveNLAGameObject;
        private GameObject _arriveLAGameObject;
        private GameObject _pursuitGameObject;
        private GameObject _evadeGameObject;
        private GameObject _velocityMatchingGameObject;
        private GameObject _interposeGameObject;
        private GameObject _separationGameObject;
        private GameObject _groupAlignGameObject;
        private GameObject _cohesionGameObject;
        private GameObject _wanderGameObject;
        private GameObject _offsetFollowGameObject;
        private GameObject _agentAvoiderGameObject;

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

            if (_position5 == null)
                _position5 = GameObject.Find("Position5").transform;
            if (_position6 == null)
                _position6 = GameObject.Find("Position6").transform;
            if (_position7 == null)
                _position7 = GameObject.Find("Position7").transform;
            if (_position8 == null)
                _position8 = GameObject.Find("Position8").transform;
            if (_position9 == null)
                _position9 = GameObject.Find("Position9").transform;
            if (_position10 == null)
                _position10 = GameObject.Find("Position10").transform;
            if (_position11 == null)
                _position11 = GameObject.Find("Position11").transform;
            if (_position12 == null)
                _position12 = GameObject.Find("Position12").transform;
            if (_position13 == null)
                _position13 = GameObject.Find("Position13").transform;
            if (_position14 == null)
                _position14 = GameObject.Find("Position14").transform;

            if (_seekGameObject == null)
            {
                _seekGameObject = GameObject.Find("SeekMovingAgent");
                _seekGameObject.SetActive(false);
            }

            if (_alignGameObject == null)
            {
                _alignGameObject = GameObject.Find("AlignMovingAgent");
                _alignGameObject.SetActive(false);
            }

            if (_faceGameObject == null)
            {
                _faceGameObject = GameObject.Find("FaceMovingAgent");
                _faceGameObject.SetActive(false);
            }

            if (_fleeGameObject == null)
            {
                _fleeGameObject = GameObject.Find("FleeMovingAgent");
                _fleeGameObject.SetActive(false);
            }

            if (_arriveNLAGameObject == null)
            {
                _arriveNLAGameObject = GameObject.Find("ArriveNLAMovingAgent");
                _arriveNLAGameObject.SetActive(false);
            }
            
            if (_arriveLAGameObject == null)
            {
                _arriveLAGameObject = GameObject.Find("ArriveLAMovingAgent");
                _arriveLAGameObject.SetActive(false);
            }
            
            if (_pursuitGameObject == null)
            {
                _pursuitGameObject = GameObject.Find("PursuitMovingAgent");
                _pursuitGameObject.SetActive(false);
            }
            if (_evadeGameObject == null)
            {
                _evadeGameObject = GameObject.Find("EvadeMovingAgent");
                _evadeGameObject.SetActive(false);
            }
            if (_velocityMatchingGameObject == null)
            {
                _velocityMatchingGameObject = GameObject.Find("VelocityMatchingMovingAgent");
                _velocityMatchingGameObject.SetActive(false);
            }
            if (_interposeGameObject == null)
            {
                _interposeGameObject = GameObject.Find("InterposeMovingAgent");
                _interposeGameObject.SetActive(false);
            }
            
            if (_separationGameObject == null)
            {
                _separationGameObject = GameObject.Find("SeparationMovingAgent");
                _separationGameObject.SetActive(false);
            }
            
            if (_groupAlignGameObject == null)
            {
                _groupAlignGameObject = GameObject.Find("GroupAlignMovingAgent");
                _groupAlignGameObject.SetActive(false);
            }
            
            if (_cohesionGameObject == null)
            {
                _cohesionGameObject = GameObject.Find("CohesionMovingAgent");
                _cohesionGameObject.SetActive(false);
            }
            if (_wanderGameObject == null)
            {
                _wanderGameObject = GameObject.Find("WanderMovingAgent");
                _wanderGameObject.SetActive(false);
            }
            if (_offsetFollowGameObject == null)
            {
                _offsetFollowGameObject = GameObject.Find("OffsetFollowMovingAgent");
                _offsetFollowGameObject.SetActive(false);
            }
            if (_agentAvoiderGameObject == null)
            {
                _agentAvoiderGameObject = GameObject.Find("ActiveAgentAvoiderMovingAgent");
                _agentAvoiderGameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Test that SeekBehavior can reach a target.
        /// </summary>
        [UnityTest]
        public IEnumerator SeekBehaviourTest()
        {
            // Test setup.
            _seekGameObject.transform.position = _position5.position;
            var seekSteeringBehavior =
                _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            var agentMover = _seekGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 10.0f;
            _target.Enabled = true;
            _target.TargetPosition = _position1.position;
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            _seekGameObject.SetActive(true);

            // Give time for the seeker to get to the target.
            yield return new WaitForSeconds(2.0f);

            // Assert the target was reached.
            Assert.True(Vector3.Distance(_position1.position,
                            _seekGameObject.transform.position) <=
                        (seekSteeringBehavior.ArrivalDistance + 0.1));

            // Cleanup.
            _seekGameObject.SetActive(false);
            _target.Enabled = false;
        }

        /// <summary>
        /// Test that ArriveBehaviorNLA can reach a target and that it accelerates
        /// at the beginning and brakes at the end.
        /// </summary>
        [UnityTest]
        public IEnumerator ArriveBehaviourNLATest()
        { // TODO: This test fails too often. There's something wrong with. Must check it.
            // Test setup.
            _arriveNLAGameObject.transform.position = _position5.position;
            var arriveSteeringBehavior =
                _arriveNLAGameObject.GetComponentInChildren<ArriveSteeringBehaviorNLA>();
            var arriveMover = _arriveNLAGameObject.GetComponent<AgentMover>();
            var arriveMoverRigidBody = _arriveNLAGameObject.GetComponent<Rigidbody2D>();
            arriveMoverRigidBody.linearVelocity = Vector2.zero;
            arriveMover.MaximumSpeed = 6.0f;
            _target.Enabled = true;
            _target.TargetPosition = _position1.position;
            arriveSteeringBehavior.Target = _target.gameObject;
            arriveSteeringBehavior.ArrivalDistance = 0.5f;
            arriveSteeringBehavior.AccelerationRadius = 2.0f;
            arriveSteeringBehavior.BrakingRadius = 3.0f;
            _arriveNLAGameObject.SetActive(true);

            // Check that agent is accelerating at the beginning.
            // Wait until agent starts is movement.
            yield return new WaitUntil(() =>
                (Vector3.Distance(_position5.position,
                    _arriveNLAGameObject.transform.position) >= 0.1f) &&
                (Vector3.Distance(_position5.position,
                     _arriveNLAGameObject.transform.position) <
                 arriveSteeringBehavior.AccelerationRadius));
            yield return new WaitForSeconds(0.03f);
            Assert.True(arriveMover.CurrentSpeed > 0.0f &&
                        arriveMover.CurrentSpeed < arriveMover.MaximumSpeed);

            // Check that agent gets its full cruise speed. 
            yield return new WaitUntil(() =>
                Vector3.Distance(_position5.position,
                    _arriveNLAGameObject.transform.position) >
                (arriveSteeringBehavior.AccelerationRadius + 0.1f));
            yield return new WaitForSeconds(0.06f);
            Assert.True(
                Mathf.Approximately(arriveMover.CurrentSpeed,
                    arriveMover.MaximumSpeed));

            // Check that agent is braking at the end.
            yield return new WaitUntil(() =>
                Vector3.Distance(_position1.position,
                    _arriveNLAGameObject.transform.position) <
                (arriveSteeringBehavior.BrakingRadius - 0.2f));
            yield return new WaitForSeconds(0.06f);
            Assert.True(arriveMover.CurrentSpeed > 0.0f &&
                        arriveMover.CurrentSpeed < arriveMover.MaximumSpeed);

            // Assert the target was reached.
            yield return new WaitForSeconds(3f);
            Assert.True(Vector3.Distance(_position1.position,
                            _arriveNLAGameObject.transform.position) <=
                        (arriveSteeringBehavior.ArrivalDistance));

            // Cleanup.
            _arriveNLAGameObject.SetActive(false);
            _target.Enabled = false;
        }
        
        /// <summary>
        /// Test that ArriveBehaviorLA can reach a target and that it accelerates
        /// at the beginning and brakes at the end.
        /// </summary>
        [UnityTest]
        public IEnumerator ArriveBehaviourLATest()
        {
            // Test setup.
            _arriveLAGameObject.transform.position = _position5.position;
            var arriveSteeringBehavior =
                _arriveLAGameObject.GetComponentInChildren<ArriveSteeringBehaviorLA>();
            var agentMover = _arriveLAGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 6.0f;
            agentMover.StopSpeed = 0.01f;
            agentMover.MaximumRotationalSpeed = 1080f;
            agentMover.StopRotationThreshold = 1f;
            agentMover.MaximumAcceleration = 4f;
            agentMover.MaximumDeceleration = 4f;
            _target.Enabled = true;
            _target.TargetPosition = _position1.position;
            arriveSteeringBehavior.Target = _target.gameObject;
            arriveSteeringBehavior.ArrivalDistance = 0.5f;
            _arriveLAGameObject.SetActive(true);

            // Check that agent is accelerating at the beginning.
            // Wait until agent starts is movement.
            yield return new WaitUntil(() =>
                (Vector3.Distance(_position5.position,
                    _arriveLAGameObject.transform.position) >= 0.1f));
            yield return null;
            yield return null;
            Assert.True(agentMover.CurrentSpeed > 0.0f &&
                        agentMover.CurrentSpeed < agentMover.MaximumSpeed);

            // Check that agent gets its full cruise speed. 
            yield return new WaitUntil(() =>
                Mathf.Approximately(agentMover.CurrentSpeed, agentMover.MaximumSpeed));
            Assert.True(
                Mathf.Approximately(agentMover.CurrentSpeed,
                    agentMover.MaximumSpeed));

            // Check that agent is braking at the end.
            yield return new WaitUntil(() =>
                Vector3.Distance(_position1.position,
                    _arriveLAGameObject.transform.position) <=
                (arriveSteeringBehavior.BrakingRadius));
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            Assert.True(agentMover.CurrentSpeed > 0.0f &&
                        agentMover.CurrentSpeed < agentMover.MaximumSpeed);

            // Assert the target was reached.
            yield return new WaitForSeconds(3f);
            Assert.True(Vector3.Distance(_position1.position,
                            _arriveLAGameObject.transform.position) <=
                        (arriveSteeringBehavior.ArrivalDistance));

            // Cleanup.
            _arriveLAGameObject.SetActive(false);
            _target.Enabled = false;
        }

        /// <summary>
        /// Test that FleeBehavior makes agent go away from its threath.
        /// </summary>
        [UnityTest]
        public IEnumerator FleeBehaviourTest()
        {
            // Test setup.
            _fleeGameObject.transform.position = _position8.position;
            var fleeSteeringBehavior =
                _fleeGameObject.GetComponentInChildren<FleeSteeringBehavior>();
            fleeSteeringBehavior.Threath = _target.gameObject;
            fleeSteeringBehavior.PanicDistance = 5.0f;
            var agentMover = _fleeGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 10.0f;
            _target.Enabled = true;
            _target.TargetPosition = _position1.position;
            _fleeGameObject.SetActive(true);

            // Place 5 targets in random positions and check that the agent flees.
            int testsSamples = 5;
            for (int i = 0; i < testsSamples; i++)
            {
                // Place target in a random position inside the flee range.
                _target.TargetPosition = _fleeGameObject.transform.position +
                                         (Vector3)(Random.insideUnitCircle *
                                                   fleeSteeringBehavior.PanicDistance);
                // Let the agent flee from the target.
                yield return new WaitForSeconds(1.0f);
                // Assert the fled agent is now farther from the target than before.
                Assert.True(Vector3.Distance(_target.TargetPosition,
                                _fleeGameObject.transform.position) >
                            fleeSteeringBehavior.PanicDistance);
            }

            // Cleanup.
            _fleeGameObject.SetActive(false);
            _target.Enabled = false;
        }

        /// <summary>
        /// Test that AlignBehavior can face in the same direction as other GameObject.
        /// </summary>
        [UnityTest]
        public IEnumerator AlignBehaviourTest()
        {
            // Test setup.
            _seekGameObject.transform.position = _position5.position;
            var seekSteeringBehavior =
                _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            var agentMover = _seekGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 2.0f;
            _target.Enabled = true;
            _target.TargetPosition = _position1.position;
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            var seekColor = _seekGameObject.GetComponent<AgentColor>();
            seekColor.Color = Color.red;
            _alignGameObject.transform.position = _position6.position;
            var alignSteeringBehavior =
                _alignGameObject.GetComponentInChildren<AlignSteeringBehavior>();
            alignSteeringBehavior.Target = _seekGameObject;
            _alignGameObject.SetActive(true);
            _seekGameObject.SetActive(true);

            // Move seeker to face the first target.
            _target.TargetPosition = _position1.position;
            yield return new WaitForSeconds(2f);
            Assert.True(
                Mathf.Abs(
                    _seekGameObject.GetComponent<AgentMover>().Orientation -
                    _alignGameObject.GetComponent<AgentMover>().Orientation) <=
                agentMover.StopRotationThreshold);

            // Move seeker to face the second target.
            _target.TargetPosition = _position2.position;
            yield return new WaitForSeconds(2.2f);
            Assert.True(
                Mathf.Abs(
                    _seekGameObject.GetComponent<AgentMover>().Orientation -
                    _alignGameObject.GetComponent<AgentMover>().Orientation) <=
                agentMover.StopRotationThreshold);

            // Move seeker to face the third target.
            _target.TargetPosition = _position3.position;
            yield return new WaitForSeconds(2.2f);
            Assert.True(
                Mathf.Abs(
                    _seekGameObject.GetComponent<AgentMover>().Orientation -
                    _alignGameObject.GetComponent<AgentMover>().Orientation) <=
                agentMover.StopRotationThreshold);

            // Cleanup.
            _seekGameObject.SetActive(false);
            _alignGameObject.SetActive(false);
            _target.Enabled = false;
        }

        /// <summary>
        /// Test that FaceBehavior can face towards a target while it moves.
        /// </summary>
        [UnityTest]
        public IEnumerator FaceBehaviourTest()
        {
            // Test setup.
            _seekGameObject.transform.position = _position5.position;
            var seekSteeringBehavior =
                _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            var agentMover = _seekGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 2.0f;
            _target.Enabled = true;
            _target.TargetPosition = _position1.position;
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            var seekColor = _seekGameObject.GetComponent<AgentColor>();
            seekColor.Color = Color.red;
            _faceGameObject.transform.position = _position7.position;
            var faceSteeringBehavior =
                _faceGameObject.GetComponentInChildren<FaceMatchingSteeringBehavior>();
            faceSteeringBehavior.Target = _seekGameObject;
            _faceGameObject.SetActive(true);
            _seekGameObject.SetActive(true);

            // Sample the tested agent alignment in some moments of the
            // seeker travel, to check if it is still facing the seeker.
            float totalTestTimeInSeconds = 5.0f;
            int numberOfSamples = 5;
            float sampleInterval = totalTestTimeInSeconds / numberOfSamples;
            foreach (int _ in Enumerable.Range(1, numberOfSamples))
            {
                yield return new WaitForSeconds(sampleInterval);
                float currentAngle = Vector3.Angle(Vector3.up,
                    _seekGameObject.transform.position -
                    _faceGameObject.transform.position);
                // 5 degrees tolerance, because target is constantly moving.
                Assert.True(
                    Mathf.Abs(
                        currentAngle -
                        _faceGameObject.GetComponent<AgentMover>().Orientation
                    ) <= 5);
            }

            // Cleanup.
            _seekGameObject.SetActive(false);
            _faceGameObject.SetActive(false);
            _target.Enabled = false;
        }

        /// <summary>
        /// Test that PursuitBehavior can intercept its target.
        /// </summary>
        [UnityTest]
        public IEnumerator PursuitBehaviourTest()
        {
            // Test setup.
            _seekGameObject.transform.position = _position10.position;
            _target.Enabled = true;
            _target.TargetPosition = _position4.position;
            var targetMovingAgent = _seekGameObject.GetComponent<AgentMover>();
            targetMovingAgent.MaximumSpeed = 2.0f;
            targetMovingAgent.StopSpeed = 0.1f;
            targetMovingAgent.MaximumRotationalSpeed = 180f;
            targetMovingAgent.StopRotationThreshold = 1f;
            targetMovingAgent.MaximumAcceleration = 1.8f;
            targetMovingAgent.MaximumDeceleration = 1.8f;
            var targetMovingAgentColor = _seekGameObject.GetComponent<AgentColor>();
            targetMovingAgentColor.Color = Color.red;
            var seekSteeringBehavior = 
                _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            _pursuitGameObject.transform.position = _position1.position;
            var pursueAgentMover = _pursuitGameObject.GetComponent<AgentMover>();
            pursueAgentMover.MaximumSpeed = 2.5f;
            pursueAgentMover.MaximumAcceleration = 4.0f;
            pursueAgentMover.MaximumRotationalSpeed = 180f;
            pursueAgentMover.StopRotationThreshold = 1f;
            pursueAgentMover.StopSpeed = 0.1f;
            pursueAgentMover.MaximumAcceleration = 2;
            pursueAgentMover.MaximumDeceleration = 4;
            var pursueSteeringBehavior = 
                _pursuitGameObject.GetComponentInChildren<PursuitSteeringBehavior>();
            pursueSteeringBehavior.Target = targetMovingAgent;
            _seekGameObject.SetActive(true);
            _pursuitGameObject.SetActive(true);
            
            // Give time for the chaser to get to the target.
            yield return new WaitForSeconds(2.7f);
            
            // Assert the target was reached.
            // We test for a distance equal to the radius of both agents, plus
            // a 0.1 of tolerance. That should be the distance of centers when
            // both agents are touching.
            Assert.True(Vector3.Distance(_seekGameObject.transform.position, 
                            _pursuitGameObject.transform.position) <= (1.1f));
            
            // Cleanup.
            _seekGameObject.SetActive(false);
            _pursuitGameObject.SetActive(false);
            _target.Enabled = false;
        }
        
        /// <summary>
        /// Test that OffsetFollowBehavior can follow its target keeping its offset.
        /// </summary>
        [UnityTest]
        public IEnumerator OffsetFollowBehaviourTest()
        {
            // Test setup.
            _seekGameObject.transform.position = _position10.position;
            _target.Enabled = true;
            _target.TargetPosition = _position4.position;
            var targetMovingAgent = _seekGameObject.GetComponent<AgentMover>();
            targetMovingAgent.MaximumSpeed = 2.0f;
            targetMovingAgent.StopSpeed = 0.1f;
            targetMovingAgent.MaximumRotationalSpeed = 180f;
            targetMovingAgent.StopRotationThreshold = 1f;
            targetMovingAgent.MaximumAcceleration = 1.8f;
            targetMovingAgent.MaximumDeceleration = 1.8f;
            var targetMovingAgentColor = _seekGameObject.GetComponent<AgentColor>();
            targetMovingAgentColor.Color = Color.red;
            var seekSteeringBehavior = 
                _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            
            _offsetFollowGameObject.transform.position = _position1.position;
            var pursueAgentMover = _offsetFollowGameObject.GetComponent<AgentMover>();
            pursueAgentMover.MaximumSpeed = 2.5f;
            pursueAgentMover.MaximumAcceleration = 4.0f;
            pursueAgentMover.MaximumRotationalSpeed = 180f;
            pursueAgentMover.StopRotationThreshold = 1f;
            pursueAgentMover.StopSpeed = 0.1f;
            pursueAgentMover.MaximumAcceleration = 2;
            pursueAgentMover.MaximumDeceleration = 4;
            var offsetFollowBehavior = 
                _offsetFollowGameObject.GetComponentInChildren<OffsetFollowSteeringBehavior>();
            offsetFollowBehavior.Target = targetMovingAgent.gameObject;
            GameObject offsetFromTargetMarker = 
                offsetFollowBehavior.gameObject.transform.Find("OffsetFromTargetMarker").gameObject;
            
            Vector2 offsetFromTarget = new Vector2(1, -1);
            offsetFromTargetMarker.transform.position =
                targetMovingAgent.transform.TransformPoint(offsetFromTarget);
            offsetFollowBehavior.UpdateOffsetFromTarget();
            
            _seekGameObject.SetActive(true);
            _offsetFollowGameObject.SetActive(true);
            
            // Give time for the follower to get to the target.
            yield return new WaitForSeconds(3.5f);
            
            // Assert follow agent is at the offset position from target agent.
            Assert.True(Vector3.Distance(_offsetFollowGameObject.transform.position,
                targetMovingAgent.transform.TransformPoint(offsetFromTarget)) <= (1.5f));
            
            // Move again target agent.
            _target.TargetPosition = _position1.position;
            
            // Give time for the follower to get to the target.
            yield return new WaitForSeconds(3.0f);
            
            // Assert follow agent is at the offset position from target agent.
            Assert.True(Vector3.Distance(_offsetFollowGameObject.transform.position,
                targetMovingAgent.transform.TransformPoint(offsetFromTarget)) <= (1.5f));
            
            // Cleanup.
            _seekGameObject.SetActive(false);
            _offsetFollowGameObject.SetActive(false);
            _target.Enabled = false;
        }
        
        /// <summary>
        /// Test that EvadeBehavior can can keep away its agent from its chaser.
        /// </summary>
        [UnityTest]
        public IEnumerator EvadeBehaviourTest()
        {
            // Test setup.
            var evadeAgentMover = _evadeGameObject.GetComponent<AgentMover>();
            var evadeSteeringBehavior = _evadeGameObject.GetComponentInChildren<EvadeSteeringBehavior>();
            var seekSteeringBehavior = _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            var seekAgentMover = _seekGameObject.GetComponent<AgentMover>();
            _evadeGameObject.transform.position = _position8.position;
            evadeAgentMover.MaximumSpeed = 2.0f;
            evadeSteeringBehavior.Threat = seekAgentMover;
            evadeSteeringBehavior.PanicDistance = 3.0f;
            _seekGameObject.transform.position = _position10.position;
            seekAgentMover.GetComponent<AgentColor>().Color = Color.red;
            seekAgentMover.MaximumSpeed = 2.0f;
            seekSteeringBehavior.Target = _evadeGameObject;
            _evadeGameObject.SetActive(true);
            _seekGameObject.SetActive(true);
            
            // Give time for the chaser to try to reach evader.
            yield return new WaitForSeconds(4.0f);
            
            // Assert the evader was not reached.
            Assert.True(Vector3.Distance(_seekGameObject.transform.position, 
                _evadeGameObject.transform.position) >= 
                        (evadeSteeringBehavior.PanicDistance));
            
            // Cleanup.
            _seekGameObject.SetActive(false);
            _evadeGameObject.SetActive(false);
            _target.Enabled = false;
        }
    
        /// <summary>
        /// Test that VelocityMatchingBehavior can copy its target's velocity.
        /// </summary>
        [UnityTest]
        public IEnumerator VelocityMatchingBehaviourTest()
        {
            // Get references to components.
            var velocityMatchingSteeringBehavior = _velocityMatchingGameObject
                .GetComponentInChildren<VelocityMatchingSteeringBehavior>();
            var velocityMatchingRigidbody =
                _velocityMatchingGameObject.GetComponent<Rigidbody2D>();
            var velocityMatchingAgentMover =
                _velocityMatchingGameObject.GetComponent<AgentMover>();
            var arriveSteeringBehavior =
                _arriveLAGameObject.GetComponentInChildren<ArriveSteeringBehaviorLA>();
            var arriveAgentMover = _arriveLAGameObject.GetComponent<AgentMover>();
            var arriveRigidbody = _arriveLAGameObject.GetComponent<Rigidbody2D>();
            
            // Setup agents before the test.
            _arriveLAGameObject.transform.position = _position6.position;
            arriveAgentMover.MaximumSpeed = 5.55f;
            arriveAgentMover.StopSpeed = 0.1f;
            arriveAgentMover.MaximumRotationalSpeed = 180f;
            arriveAgentMover.StopRotationThreshold = 1f;
            arriveAgentMover.MaximumAcceleration = 4f;
            arriveAgentMover.MaximumDeceleration = 4f;
            arriveSteeringBehavior.Target = _position1.gameObject;
            var arriveColor = _arriveLAGameObject.GetComponent<AgentColor>();
            arriveColor.Color = Color.red;
            _velocityMatchingGameObject.transform.position =
                _position10.position;
            velocityMatchingAgentMover.MaximumSpeed = 5.55f;
            velocityMatchingAgentMover.StopSpeed = 0.1f;
            velocityMatchingAgentMover.MaximumRotationalSpeed = 180f;
            velocityMatchingAgentMover.StopRotationThreshold = 1f;
            velocityMatchingAgentMover.MaximumAcceleration = 10f;
            velocityMatchingAgentMover.MaximumDeceleration = 200f;
            velocityMatchingSteeringBehavior.TimeToMatch = 0.1f;
            velocityMatchingSteeringBehavior.Target = arriveAgentMover;
            velocityMatchingRigidbody.linearVelocity = Vector2.zero;
            arriveRigidbody.linearVelocity = Vector2.zero;
            _velocityMatchingGameObject.SetActive(true);
            _arriveLAGameObject.SetActive(true);

            // Give time for the followed agent to try to reach its target
            // cruise velocity and assert velocity matcher agent has matched the velocity
            // of its target.
            yield return new WaitUntil(() =>
                Mathf.Approximately(
                    arriveAgentMover.CurrentSpeed,
                    arriveAgentMover.MaximumSpeed));
            yield return new WaitForSecondsRealtime(velocityMatchingSteeringBehavior.TimeToMatch);
            Assert.True(velocityMatchingAgentMover.Velocity.normalized == arriveAgentMover.Velocity.normalized &&
                        Mathf.Abs(velocityMatchingAgentMover.Velocity.magnitude - arriveAgentMover.Velocity.magnitude) < 0.01f);

            // Wait until arriver brakes and asserts that the VelocityMatcher
            // has braked too.
            yield return new WaitUntil(() =>
                Mathf.Approximately(arriveAgentMover.CurrentSpeed, 0));
            yield return new WaitForSecondsRealtime(velocityMatchingSteeringBehavior.TimeToMatch);
            Assert.True(Mathf.Abs(velocityMatchingAgentMover.Velocity.magnitude - arriveAgentMover.Velocity.magnitude) < 0.01f);

            // Cleanup.
            _velocityMatchingGameObject.SetActive(false);
            _arriveLAGameObject.SetActive(false);
        }
        
        /// <summary>
        /// Test that InterposeMatchingBehavior can place and agent between two
        /// moving agents.
        /// </summary>
        [UnityTest]
        public IEnumerator InterposeBehaviourTest()
        {
            // Get references to components.
            var velocityMatchingSteeringBehavior = _velocityMatchingGameObject
                .GetComponentInChildren<VelocityMatchingSteeringBehavior>();
            var velocityMatchingRigidbody =
                _velocityMatchingGameObject.GetComponent<Rigidbody2D>();
            var velocityMatchingAgentMover =
                _velocityMatchingGameObject.GetComponent<AgentMover>();
            var arriveSteeringBehavior =
                _arriveLAGameObject.GetComponentInChildren<ArriveSteeringBehaviorLA>();
            var arriveAgentMover = _arriveLAGameObject.GetComponent<AgentMover>();
            var arriveRigidbody = _arriveLAGameObject.GetComponent<Rigidbody2D>();
            var interposeSteeringBehavior =
                _interposeGameObject.GetComponentInChildren<InterposeSteeringBehavior>();
            var interposeAgentMover = _interposeGameObject.GetComponent<AgentMover>();
            var interposeRigidbody = _interposeGameObject.GetComponent<Rigidbody2D>();
            
            // Setup agents before the test.
            _arriveLAGameObject.transform.position = _position6.position;
            arriveAgentMover.MaximumSpeed = 5.55f;
            arriveAgentMover.StopSpeed = 0.1f;
            arriveAgentMover.MaximumRotationalSpeed = 180f;
            arriveAgentMover.StopRotationThreshold = 1f;
            arriveAgentMover.MaximumAcceleration = 4f;
            arriveAgentMover.MaximumDeceleration = 4f;
            arriveSteeringBehavior.Target = _position1.gameObject;
            var arriveColor = _arriveLAGameObject.GetComponent<AgentColor>();
            arriveColor.Color = Color.red;
            _velocityMatchingGameObject.transform.position =
                _position10.position;
            velocityMatchingAgentMover.MaximumSpeed = 5.55f;
            velocityMatchingAgentMover.StopSpeed = 0.1f;
            velocityMatchingAgentMover.MaximumRotationalSpeed = 180f;
            velocityMatchingAgentMover.StopRotationThreshold = 1f;
            velocityMatchingAgentMover.MaximumAcceleration = 10f;
            velocityMatchingAgentMover.MaximumDeceleration = 200f;
            velocityMatchingSteeringBehavior.TimeToMatch = 0.1f;
            velocityMatchingSteeringBehavior.Target = arriveAgentMover;
            var velocityMatchingColor = _velocityMatchingGameObject.GetComponent<AgentColor>();
            velocityMatchingColor.Color = Color.red;
            _interposeGameObject.transform.position = _position11.position;
            interposeAgentMover.MaximumSpeed = 5.55f;
            interposeAgentMover.StopSpeed = 0.1f;
            interposeAgentMover.MaximumRotationalSpeed = 180f;
            interposeAgentMover.StopRotationThreshold = 1f;
            interposeAgentMover.MaximumAcceleration = 10f;
            interposeAgentMover.MaximumDeceleration = 200f;
            interposeSteeringBehavior.AgentA = arriveAgentMover;
            interposeSteeringBehavior.AgentB = velocityMatchingAgentMover;
            interposeRigidbody.linearVelocity = Vector2.zero;
            velocityMatchingRigidbody.linearVelocity = Vector2.zero;
            arriveRigidbody.linearVelocity = Vector2.zero;
            _velocityMatchingGameObject.SetActive(true);
            _arriveLAGameObject.SetActive(true);
            _interposeGameObject.SetActive(true);

            // Start test.
            
            // Give time for the followed agent to try to reach its target
            // cruise velocity and assert interposed agent in the middle of the two agents.
            yield return new WaitUntil(() =>
                Mathf.Approximately(
                    arriveAgentMover.CurrentSpeed,
                    arriveAgentMover.MaximumSpeed));
            yield return new WaitForSecondsRealtime(velocityMatchingSteeringBehavior.TimeToMatch);
            Assert.True(
                ((Vector2) _interposeGameObject.transform.position -
                InterposeSteeringBehavior.GetMidPoint(
                    _velocityMatchingGameObject.transform.position, 
                    _arriveLAGameObject.transform.position)).magnitude < 0.2f);

            // Wait until arriver brakes and asserts that the interpose agent stays in
            // the middle.
            yield return new WaitUntil(() =>
                Mathf.Approximately(arriveAgentMover.CurrentSpeed, 0));
            yield return new WaitForSecondsRealtime(velocityMatchingSteeringBehavior.TimeToMatch);
            Assert.True(
                ((Vector2) _interposeGameObject.transform.position -
                 InterposeSteeringBehavior.GetMidPoint(
                     _velocityMatchingGameObject.transform.position, 
                     _arriveLAGameObject.transform.position)).magnitude < 0.2f);
            
            // Cleanup.
            _velocityMatchingGameObject.SetActive(false);
            _arriveLAGameObject.SetActive(false);
            _interposeGameObject.SetActive(false);
        }
        
         /// <summary>
         /// Test that SeparationMatchingBehavior can separate its agent from two moving agents
         /// using a linear algorithm.
        /// </summary>
        [UnityTest]
        public IEnumerator SeparationBehaviourLinearTest()
        {
            // Get references to components.
            var velocityMatchingSteeringBehavior = _velocityMatchingGameObject
                .GetComponentInChildren<VelocityMatchingSteeringBehavior>();
            var velocityMatchingRigidbody =
                _velocityMatchingGameObject.GetComponent<Rigidbody2D>();
            var velocityMatchingAgentMover =
                _velocityMatchingGameObject.GetComponent<AgentMover>();
            var arriveSteeringBehavior =
                _arriveLAGameObject.GetComponentInChildren<ArriveSteeringBehaviorLA>();
            var arriveAgentMover = _arriveLAGameObject.GetComponent<AgentMover>();
            var arriveRigidbody = _arriveLAGameObject.GetComponent<Rigidbody2D>();
            var separationSteeringBehavior =
                _separationGameObject.GetComponentInChildren<SeparationSteeringBehavior>();
            var separationAgentMover = _separationGameObject.GetComponent<AgentMover>();
            var separationRigidbody = _separationGameObject.GetComponent<Rigidbody2D>();
            
            // Setup agents before the test.
            _arriveLAGameObject.transform.position = _position6.position;
            arriveAgentMover.MaximumSpeed = 5.55f;
            arriveAgentMover.StopSpeed = 0.1f;
            arriveAgentMover.MaximumRotationalSpeed = 180f;
            arriveAgentMover.StopRotationThreshold = 1f;
            arriveAgentMover.MaximumAcceleration = 4f;
            arriveAgentMover.MaximumDeceleration = 4f;
            arriveSteeringBehavior.Target = _position9.gameObject;
            var arriveColor = _arriveLAGameObject.GetComponent<AgentColor>();
            arriveColor.Color = Color.red;
            _velocityMatchingGameObject.transform.position =
                _position10.position;
            velocityMatchingAgentMover.MaximumSpeed = 5.55f;
            velocityMatchingAgentMover.StopSpeed = 0.1f;
            velocityMatchingAgentMover.MaximumRotationalSpeed = 180f;
            velocityMatchingAgentMover.StopRotationThreshold = 1f;
            velocityMatchingAgentMover.MaximumAcceleration = 10f;
            velocityMatchingAgentMover.MaximumDeceleration = 200f;
            velocityMatchingSteeringBehavior.TimeToMatch = 0.1f;
            velocityMatchingSteeringBehavior.Target = arriveAgentMover;
            var velocityMatchingColor = _velocityMatchingGameObject.GetComponent<AgentColor>();
            velocityMatchingColor.Color = Color.red;
            _separationGameObject.transform.position = _position12.position;
            separationAgentMover.MaximumSpeed = 5.55f;
            separationAgentMover.StopSpeed = 0.1f;
            separationAgentMover.MaximumRotationalSpeed = 180f;
            separationAgentMover.StopRotationThreshold = 1f;
            separationAgentMover.MaximumAcceleration = 10f;
            separationAgentMover.MaximumDeceleration = 200f;
            separationSteeringBehavior.Threats.Add(arriveAgentMover);
            separationSteeringBehavior.Threats.Add(velocityMatchingAgentMover);
            separationSteeringBehavior.SeparationThreshold = 4f;
            separationSteeringBehavior.DecayCoefficient = 2f;
            separationSteeringBehavior.SeparationAlgorithm = 
                SeparationSteeringBehavior.SeparationAlgorithms.Linear;
            separationRigidbody.linearVelocity = Vector2.zero;
            velocityMatchingRigidbody.linearVelocity = Vector2.zero;
            arriveRigidbody.linearVelocity = Vector2.zero;
            _velocityMatchingGameObject.SetActive(true);
            _arriveLAGameObject.SetActive(true);
            _separationGameObject.SetActive(true);

            // Start test.
            
            // Assert that both agents start under separation threshold.
            Assert.True(
                ( _velocityMatchingGameObject.transform.position -
                _separationGameObject.transform.position).magnitude <= 
                separationSteeringBehavior.SeparationThreshold);
            Assert.True(
                ( _arriveLAGameObject.transform.position -
                  _separationGameObject.transform.position).magnitude <= 
                separationSteeringBehavior.SeparationThreshold);

            // Let separation agent time to go away from the two agents.
            yield return new WaitForSecondsRealtime(4);

            // Assert that both agents are above separation threshold.
            Assert.True(
                ( _velocityMatchingGameObject.transform.position -
                  _separationGameObject.transform.position).magnitude >= 
                separationSteeringBehavior.SeparationThreshold);
            Assert.True(
                ( _arriveLAGameObject.transform.position -
                  _separationGameObject.transform.position).magnitude >= 
                separationSteeringBehavior.SeparationThreshold);
            
            // Cleanup.
            _velocityMatchingGameObject.SetActive(false);
            _arriveLAGameObject.SetActive(false);
            _separationGameObject.SetActive(false);
        }
         
        /// <summary>
        /// Test that SeparationMatchingBehavior can separate its agent from two
        /// moving agents using an inverse square algorithm.
        /// </summary>
        [UnityTest]
        public IEnumerator SeparationBehaviourInverseSquareTest()
        {
            // Get references to components.
            var velocityMatchingSteeringBehavior = _velocityMatchingGameObject
                .GetComponentInChildren<VelocityMatchingSteeringBehavior>();
            var velocityMatchingRigidbody =
                _velocityMatchingGameObject.GetComponent<Rigidbody2D>();
            var velocityMatchingAgentMover =
                _velocityMatchingGameObject.GetComponent<AgentMover>();
            var arriveSteeringBehavior =
                _arriveLAGameObject.GetComponentInChildren<ArriveSteeringBehaviorLA>();
            var arriveAgentMover = _arriveLAGameObject.GetComponent<AgentMover>();
            var arriveRigidbody = _arriveLAGameObject.GetComponent<Rigidbody2D>();
            var separationSteeringBehavior =
                _separationGameObject.GetComponentInChildren<SeparationSteeringBehavior>();
            var separationAgentMover = _separationGameObject.GetComponent<AgentMover>();
            var separationRigidbody = _separationGameObject.GetComponent<Rigidbody2D>();
            
            // Setup agents before the test.
            _arriveLAGameObject.transform.position = _position6.position;
            arriveAgentMover.MaximumSpeed = 5.55f;
            arriveAgentMover.StopSpeed = 0.1f;
            arriveAgentMover.MaximumRotationalSpeed = 180f;
            arriveAgentMover.StopRotationThreshold = 1f;
            arriveAgentMover.MaximumAcceleration = 4f;
            arriveAgentMover.MaximumDeceleration = 4f;
            arriveSteeringBehavior.Target = _position9.gameObject;
            var arriveColor = _arriveLAGameObject.GetComponent<AgentColor>();
            arriveColor.Color = Color.red;
            _velocityMatchingGameObject.transform.position =
                _position10.position;
            velocityMatchingAgentMover.MaximumSpeed = 5.55f;
            velocityMatchingAgentMover.StopSpeed = 0.1f;
            velocityMatchingAgentMover.MaximumRotationalSpeed = 180f;
            velocityMatchingAgentMover.StopRotationThreshold = 1f;
            velocityMatchingAgentMover.MaximumAcceleration = 10f;
            velocityMatchingAgentMover.MaximumDeceleration = 200f;
            velocityMatchingSteeringBehavior.TimeToMatch = 0.1f;
            velocityMatchingSteeringBehavior.Target = arriveAgentMover;
            var velocityMatchingColor = _velocityMatchingGameObject.GetComponent<AgentColor>();
            velocityMatchingColor.Color = Color.red;
            _separationGameObject.transform.position = _position12.position;
            separationAgentMover.MaximumSpeed = 5.55f;
            separationAgentMover.StopSpeed = 0.1f;
            separationAgentMover.MaximumRotationalSpeed = 180f;
            separationAgentMover.StopRotationThreshold = 1f;
            separationAgentMover.MaximumAcceleration = 10f;
            separationAgentMover.MaximumDeceleration = 200f;
            separationSteeringBehavior.Threats.Add(arriveAgentMover);
            separationSteeringBehavior.Threats.Add(velocityMatchingAgentMover);
            separationSteeringBehavior.SeparationThreshold = 4f;
            separationSteeringBehavior.DecayCoefficient = 2f;
            separationSteeringBehavior.SeparationAlgorithm = 
                SeparationSteeringBehavior.SeparationAlgorithms.InverseSquare;
            separationRigidbody.linearVelocity = Vector2.zero;
            velocityMatchingRigidbody.linearVelocity = Vector2.zero;
            arriveRigidbody.linearVelocity = Vector2.zero;
            _velocityMatchingGameObject.SetActive(true);
            _arriveLAGameObject.SetActive(true);
            _separationGameObject.SetActive(true);

            // Start test.
            
            // Assert that both agents start under separation threshold.
            Assert.True(
                ( _velocityMatchingGameObject.transform.position -
                _separationGameObject.transform.position).magnitude <= 
                separationSteeringBehavior.SeparationThreshold);
            Assert.True(
                ( _arriveLAGameObject.transform.position -
                  _separationGameObject.transform.position).magnitude <= 
                separationSteeringBehavior.SeparationThreshold);

            // Let separation agent time to go away from the two agents.
            yield return new WaitForSecondsRealtime(4);

            // Assert that both agents are above separation threshold.
            Assert.True(
                ( _velocityMatchingGameObject.transform.position -
                  _separationGameObject.transform.position).magnitude >= 
                separationSteeringBehavior.SeparationThreshold);
            Assert.True(
                ( _arriveLAGameObject.transform.position -
                  _separationGameObject.transform.position).magnitude >= 
                separationSteeringBehavior.SeparationThreshold);
            
            // Cleanup.
            _velocityMatchingGameObject.SetActive(false);
            _arriveLAGameObject.SetActive(false);
            _separationGameObject.SetActive(false);
        }
        
        /// <summary>
        /// Test that GroupAlignBehavior calculates the average between two target agent's
        /// orientations.
        /// </summary>
        [UnityTest]
        public IEnumerator GroupAlignBehaviorTest()
        {
            // Get references to components.
            var groupAlignSteeringBehavior = _groupAlignGameObject
                .GetComponentInChildren<GroupAlignSteeringBehavior>();
            var groupAlignRigidbody =
                _groupAlignGameObject.GetComponent<Rigidbody2D>();
            var groupAlignAgentMover =
                _groupAlignGameObject.GetComponent<AgentMover>();
            var arriveSteeringBehavior =
                _arriveLAGameObject.GetComponentInChildren<ArriveSteeringBehaviorLA>();
            var arriveAgentMover = _arriveLAGameObject.GetComponent<AgentMover>();
            var arriveRigidbody = _arriveLAGameObject.GetComponent<Rigidbody2D>();
            var seekSteeringBehavior =
                _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            var seekAgentMover = _seekGameObject.GetComponent<AgentMover>();
            var seekRigidbody = _seekGameObject.GetComponent<Rigidbody2D>();
            
            // Setup agents before the test.
            _arriveLAGameObject.transform.position = _position10.position;
            arriveAgentMover.MaximumSpeed = 5.55f;
            arriveAgentMover.StopSpeed = 0.1f;
            arriveAgentMover.MaximumRotationalSpeed = 180f;
            arriveAgentMover.StopRotationThreshold = 1f;
            arriveAgentMover.MaximumAcceleration = 4f;
            arriveAgentMover.MaximumDeceleration = 4f;
            arriveSteeringBehavior.Target = _position4.gameObject;
            var arriveColor = _arriveLAGameObject.GetComponent<AgentColor>();
            arriveColor.Color = Color.red;
            _groupAlignGameObject.transform.position =
                _position8.position;
            groupAlignAgentMover.MaximumSpeed = 5.55f;
            groupAlignAgentMover.StopSpeed = 0.1f;
            groupAlignAgentMover.MaximumRotationalSpeed = 180f;
            groupAlignAgentMover.StopRotationThreshold = 3f;
            groupAlignAgentMover.MaximumAcceleration = 10f;
            groupAlignAgentMover.MaximumDeceleration = 200f;
            groupAlignSteeringBehavior.Targets.Add(_arriveLAGameObject);
            groupAlignSteeringBehavior.Targets.Add(_seekGameObject);
            _seekGameObject.transform.position = _position1.position;
            seekAgentMover.MaximumSpeed = 0f;
            seekAgentMover.StopSpeed = 0.1f;
            seekAgentMover.MaximumRotationalSpeed = 180f;
            seekAgentMover.StopRotationThreshold = 1f;
            seekAgentMover.MaximumAcceleration = 10f;
            seekAgentMover.MaximumDeceleration = 200f;
            var seekColor = _seekGameObject.GetComponent<AgentColor>();
            seekColor.Color = Color.red;
            seekRigidbody.linearVelocity = Vector2.zero;
            groupAlignRigidbody.linearVelocity = Vector2.zero;
            arriveRigidbody.linearVelocity = Vector2.zero;
            _groupAlignGameObject.SetActive(true);
            _arriveLAGameObject.SetActive(true);
            _seekGameObject.SetActive(true);

            // Start test.
            
            // Assert that group align agent starts with 0 rotation.
            Assert.True(Mathf.Approximately(groupAlignAgentMover.Orientation, 0));
            
            // Let time arrive agent to go to its target.
            yield return new WaitForSecondsRealtime(4);
            
            // Assert that group align agent is no longer at 0 rotation but at the average
            // of other two agents rotation.
            Assert.False(Mathf.Approximately(groupAlignAgentMover.Orientation, 0));
            float rotationAverage = (arriveAgentMover.Orientation + 
                seekAgentMover.Orientation) / 2f;
            Assert.True(Mathf.Abs(groupAlignAgentMover.Orientation - 
                rotationAverage) <= groupAlignAgentMover.StopRotationThreshold);
            
            // Cleanup.
            _groupAlignGameObject.SetActive(false);
            _arriveLAGameObject.SetActive(false);
            _seekGameObject.SetActive(false);
        }
        
        /// <summary>
        /// Test that CohesionMatchingBehavior can place and agent in the center of mass of
        /// a 3 agent group.
        /// </summary>
        [UnityTest]
        public IEnumerator CohesionBehaviourTest()
        {
            // Get references to components.
            var velocityMatchingSteeringBehavior = _velocityMatchingGameObject
                .GetComponentInChildren<VelocityMatchingSteeringBehavior>();
            var velocityMatchingRigidbody =
                _velocityMatchingGameObject.GetComponent<Rigidbody2D>();
            var velocityMatchingAgentMover =
                _velocityMatchingGameObject.GetComponent<AgentMover>();
            var arriveSteeringBehavior =
                _arriveLAGameObject.GetComponentInChildren<ArriveSteeringBehaviorLA>();
            var arriveAgentMover = _arriveLAGameObject.GetComponent<AgentMover>();
            var arriveRigidbody = _arriveLAGameObject.GetComponent<Rigidbody2D>();
            var seekSteeringBehavior =
                _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            var seekAgentMover = _seekGameObject.GetComponent<AgentMover>();
            var seekRigidbody = _seekGameObject.GetComponent<Rigidbody2D>();
            var cohesionSteeringBehavior =
                _cohesionGameObject.GetComponentInChildren<CohesionSteeringBehavior>();
            var cohesionAgentMover = _cohesionGameObject.GetComponent<AgentMover>();
            var cohesionRigidbody = _cohesionGameObject.GetComponent<Rigidbody2D>();
            
            // Setup agents before the test.
            _arriveLAGameObject.transform.position = _position9.position;
            arriveAgentMover.MaximumSpeed = 5.55f;
            arriveAgentMover.StopSpeed = 0.1f;
            arriveAgentMover.MaximumRotationalSpeed = 180f;
            arriveAgentMover.StopRotationThreshold = 1f;
            arriveAgentMover.MaximumAcceleration = 4f;
            arriveAgentMover.MaximumDeceleration = 4f;
            arriveSteeringBehavior.Target = _position1.gameObject;
            var arriveColor = _arriveLAGameObject.GetComponent<AgentColor>();
            arriveColor.Color = Color.red;
            _velocityMatchingGameObject.transform.position =
                _position5.position;
            velocityMatchingAgentMover.MaximumSpeed = 5.55f;
            velocityMatchingAgentMover.StopSpeed = 0.1f;
            velocityMatchingAgentMover.MaximumRotationalSpeed = 180f;
            velocityMatchingAgentMover.StopRotationThreshold = 1f;
            velocityMatchingAgentMover.MaximumAcceleration = 10f;
            velocityMatchingAgentMover.MaximumDeceleration = 200f;
            velocityMatchingSteeringBehavior.TimeToMatch = 0.1f;
            velocityMatchingSteeringBehavior.Target = arriveAgentMover;
            var velocityMatchingColor = _velocityMatchingGameObject.GetComponent<AgentColor>();
            velocityMatchingColor.Color = Color.red;
            _seekGameObject.transform.position =
                _position8.position;
            seekAgentMover.MaximumSpeed = 5.55f;
            seekAgentMover.StopSpeed = 0.1f;
            seekAgentMover.MaximumRotationalSpeed = 180f;
            seekAgentMover.StopRotationThreshold = 1f;
            seekAgentMover.MaximumAcceleration = 10f;
            seekAgentMover.MaximumDeceleration = 200f;
            seekSteeringBehavior.ArrivalDistance = 0.1f;
            seekSteeringBehavior.Target = _position3.gameObject;
            var seekColor = _seekGameObject.GetComponent<AgentColor>();
            seekColor.Color = Color.red;
            _cohesionGameObject.transform.position = _position7.position;
            cohesionAgentMover.MaximumSpeed = 5.55f;
            cohesionAgentMover.StopSpeed = 0.1f;
            cohesionAgentMover.MaximumRotationalSpeed = 180f;
            cohesionAgentMover.StopRotationThreshold = 1f;
            cohesionAgentMover.MaximumAcceleration = 10f;
            cohesionAgentMover.MaximumDeceleration = 200f;
            cohesionSteeringBehavior.Targets.Clear();
            cohesionSteeringBehavior.Targets.Add(_arriveLAGameObject);
            cohesionSteeringBehavior.Targets.Add(_velocityMatchingGameObject);
            cohesionSteeringBehavior.Targets.Add(_seekGameObject);
            cohesionSteeringBehavior.ArrivalDistance = 0.1f;
            cohesionRigidbody.linearVelocity = Vector2.zero;
            velocityMatchingRigidbody.linearVelocity = Vector2.zero;
            arriveRigidbody.linearVelocity = Vector2.zero;
            seekRigidbody.linearVelocity = Vector2.zero;
            _velocityMatchingGameObject.SetActive(true);
            _arriveLAGameObject.SetActive(true);
            _cohesionGameObject.SetActive(true);
            _seekGameObject.SetActive(true);

            // Start test.
            
            // Check that agent starts out of place.
            Assert.False(
                ((Vector2) _cohesionGameObject.transform.position -
                 cohesionSteeringBehavior.AveragePosition).magnitude <= 
                cohesionSteeringBehavior.ArrivalDistance);
            
            // Let it time to reach its position.
            yield return new WaitForSecondsRealtime(3f);
            
            // Check that agent now is in place.
            Assert.True(
                ((Vector2) _cohesionGameObject.transform.position -
                 cohesionSteeringBehavior.AveragePosition).magnitude <= 
                cohesionSteeringBehavior.ArrivalDistance);
            
            // Cleanup.
            _velocityMatchingGameObject.SetActive(false);
            _arriveLAGameObject.SetActive(false);
            _cohesionGameObject.SetActive(false);
            _seekGameObject.SetActive(false);
        }
        
        /// <summary>
        /// Test that WanderSteeringBehavior constantly changes position and orientation.
        /// </summary>
        [UnityTest]
        public IEnumerator WanderBehaviourTest()
        {
            // Test setup.
            int numberOfTestSamples = 5;
            _wanderGameObject.transform.position = _position13.position;
            var wanderSteeringBehavior =
                _wanderGameObject.GetComponentInChildren<WanderSteeringBehavior>();
            var wanderMover = _wanderGameObject.GetComponent<AgentMover>();
            var wanderRigidbody = _seekGameObject.GetComponent<Rigidbody2D>();
            wanderMover.MaximumSpeed = 1.0f;
            wanderMover.StopSpeed = 0.1f;
            wanderMover.MaximumRotationalSpeed = 1080f;
            wanderMover.StopRotationThreshold = 1f;
            wanderRigidbody.linearVelocity = Vector2.zero;
            wanderSteeringBehavior.ArrivalDistance = 0.1f;
            wanderSteeringBehavior.WanderRadius = 3f;
            wanderSteeringBehavior.WanderDistance = 6;
            wanderSteeringBehavior.WanderRecalculationTime = 0.2f;
            _wanderGameObject.SetActive(true);
            
            // Start test.
            List<Vector2> previousVelocities = new();
            foreach (var _ in Enumerable.Range(0, numberOfTestSamples))
            {
                // Give time the wander agent to move.
                yield return new WaitForSeconds(1.0f);
                // Sample its velocity.
                Vector2 currentVelocity = wanderMover.Velocity;
                // Check that velocity is different than the previous ones.
                Assert.False(previousVelocities.Contains(currentVelocity));
                // Store current velocity to be checked against in the next samples.
                previousVelocities.Add(currentVelocity);
            }

            // Cleanup.
            _wanderGameObject.SetActive(false);
            _target.Enabled = false;
        }
        
        /// <summary>
        /// Test that AgentAvoiderBehavior can reach its target without touching another
        /// moving agent that goes across its path.
        /// </summary>
        [UnityTest]
        public IEnumerator AgentAvoiderBehaviorTestFirstScenario()
        {
            // Test setup.
            var obstacleMovingAgent = _seekGameObject.GetComponent<AgentMover>();
            obstacleMovingAgent.MaximumSpeed = 2.0f;
            obstacleMovingAgent.StopSpeed = 0.1f;
            obstacleMovingAgent.MaximumRotationalSpeed = 180f;
            obstacleMovingAgent.StopRotationThreshold = 1f;
            obstacleMovingAgent.MaximumAcceleration = 1.8f;
            obstacleMovingAgent.MaximumDeceleration = 1.8f;
            var obstacleMovingAgentColor = _seekGameObject.GetComponent<AgentColor>();
            obstacleMovingAgentColor.Color = Color.red;
            var seekSteeringBehavior = 
                _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            
            var agentAvoider = _agentAvoiderGameObject.GetComponent<AgentMover>();
            agentAvoider.MaximumSpeed = 2.7f;
            agentAvoider.MaximumAcceleration = 4.0f;
            agentAvoider.MaximumRotationalSpeed = 180f;
            agentAvoider.StopRotationThreshold = 1f;
            agentAvoider.StopSpeed = 0.1f;
            agentAvoider.MaximumAcceleration = 2;
            agentAvoider.MaximumDeceleration = 4;
            var agentAvoiderSeekSteeringBehavior = _agentAvoiderGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            
            // FIRST SCENARIO:
            _target.TargetPosition = _position11.position;
            _agentAvoiderGameObject.transform.position = _position14.position;
            _seekGameObject.transform.position = _position10.position;
            seekSteeringBehavior.Target = _position1.gameObject;
            agentAvoiderSeekSteeringBehavior.Target = _target.gameObject;
            _target.Enabled = true;
            _seekGameObject.SetActive(true);
            _agentAvoiderGameObject.SetActive(true);
            
            // Assert we move without touching the obstacle agent.
            int steps = 6;
            for (int i=0; i < steps; i++)
            {
                yield return new WaitForSeconds(1.0f);
                Assert.True(Vector3.Distance(_seekGameObject.transform.position, 
                    _agentAvoiderGameObject.transform.position) >= (1.5f));
            }
            
            // Assert we reached target.
            Assert.True(Vector3.Distance(_agentAvoiderGameObject.transform.position, 
                    _target.transform.position) <= (0.7f));
            
            // Cleanup.
            _seekGameObject.SetActive(false);
            _agentAvoiderGameObject.SetActive(false);
            _target.Enabled = false;
        }
        
        /// <summary>
        /// Test that AgentAvoiderBehavior can reach its target without touching another
        /// moving agent that goes across its path.
        /// </summary>
        [UnityTest]
        public IEnumerator AgentAvoiderBehaviorTestSecondScenario()
        {
            // Test setup.
            var obstacleMovingAgent = _seekGameObject.GetComponent<AgentMover>();
            obstacleMovingAgent.MaximumSpeed = 2.7f;
            obstacleMovingAgent.StopSpeed = 0.1f;
            obstacleMovingAgent.MaximumRotationalSpeed = 180f;
            obstacleMovingAgent.StopRotationThreshold = 1f;
            obstacleMovingAgent.MaximumAcceleration = 1.8f;
            obstacleMovingAgent.MaximumDeceleration = 1.8f;
            var obstacleMovingAgentColor = _seekGameObject.GetComponent<AgentColor>();
            obstacleMovingAgentColor.Color = Color.red;
            var seekSteeringBehavior = 
                _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            
            var agentAvoider = _agentAvoiderGameObject.GetComponent<AgentMover>();
            agentAvoider.MaximumSpeed = 2.0f;
            agentAvoider.MaximumAcceleration = 4.0f;
            agentAvoider.MaximumRotationalSpeed = 180f;
            agentAvoider.StopRotationThreshold = 1f;
            agentAvoider.StopSpeed = 0.1f;
            agentAvoider.MaximumAcceleration = 2;
            agentAvoider.MaximumDeceleration = 4;
            var agentAvoiderSeekSteeringBehavior = _agentAvoiderGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            
            // SECOND SCENARIO:
            _target.TargetPosition = _position11.position;
            _agentAvoiderGameObject.transform.position = _position14.position;
            _seekGameObject.transform.position = _position1.position;
            seekSteeringBehavior.Target = _position10.gameObject;
            agentAvoiderSeekSteeringBehavior.Target = _target.gameObject;
            _target.Enabled = true;
            _seekGameObject.SetActive(true);
            _agentAvoiderGameObject.SetActive(true);
            
            // Assert we move without touching the obstacle agent.
            int steps = 7;
            for (int i=0; i < steps; i++)
            {
                yield return new WaitForSeconds(1.2f);
                Assert.True(Vector3.Distance(_seekGameObject.transform.position, 
                    _agentAvoiderGameObject.transform.position) >= (1.1f));
            }
            
            // Assert we reached target.
            Assert.True(Vector3.Distance(_agentAvoiderGameObject.transform.position, 
                    _target.transform.position) <= (0.5f));
            
            // Cleanup.
            _seekGameObject.SetActive(false);
            _agentAvoiderGameObject.SetActive(false);
            _target.Enabled = false;
        }
        
        /// <summary>
        /// Test that AgentAvoiderBehavior can reach its target without touching another
        /// moving agent that goes across its path.
        /// </summary>
        [UnityTest]
        public IEnumerator AgentAvoiderBehaviorTestThirdScenario()
        {
            // Test setup.
            var obstacleMovingAgent = _seekGameObject.GetComponent<AgentMover>();
            obstacleMovingAgent.MaximumSpeed = 2.5f;
            obstacleMovingAgent.StopSpeed = 0.1f;
            obstacleMovingAgent.MaximumRotationalSpeed = 180f;
            obstacleMovingAgent.StopRotationThreshold = 1f;
            obstacleMovingAgent.MaximumAcceleration = 1.8f;
            obstacleMovingAgent.MaximumDeceleration = 1.8f;
            var obstacleMovingAgentColor = _seekGameObject.GetComponent<AgentColor>();
            obstacleMovingAgentColor.Color = Color.red;
            var seekSteeringBehavior = 
                _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            
            var agentAvoider = _agentAvoiderGameObject.GetComponent<AgentMover>();
            agentAvoider.MaximumSpeed = 2.7f;
            agentAvoider.MaximumAcceleration = 4.0f;
            agentAvoider.MaximumRotationalSpeed = 180f;
            agentAvoider.StopRotationThreshold = 1f;
            agentAvoider.StopSpeed = 0.1f;
            agentAvoider.MaximumAcceleration = 2;
            agentAvoider.MaximumDeceleration = 4;
            var agentAvoiderSeekSteeringBehavior = _agentAvoiderGameObject.GetComponentInChildren<SeekSteeringBehavior>();

            
            // THIRD SCENARIO:
            _target.TargetPosition = _position14.position;
            _agentAvoiderGameObject.transform.position = _position7.position;
            _seekGameObject.transform.position = _position1.position;
            seekSteeringBehavior.Target = _position10.gameObject;
            agentAvoiderSeekSteeringBehavior.Target = _target.gameObject;
            _target.Enabled = true;
            _seekGameObject.SetActive(true);
            _agentAvoiderGameObject.SetActive(true);
            
            // Assert we move without touching the obstacle agent.
            int steps = 6;
            for (int i=0; i < steps; i++)
            {
                yield return new WaitForSeconds(1.0f);
                Assert.True(Vector3.Distance(_seekGameObject.transform.position, 
                    _agentAvoiderGameObject.transform.position) >= (1.5f));
            }
            
            // Assert we reached target.
            Assert.True(Vector3.Distance(_agentAvoiderGameObject.transform.position, 
                    _target.transform.position) <= (0.1f));
            
            // Cleanup.
            _seekGameObject.SetActive(false);
            _agentAvoiderGameObject.SetActive(false);
            _target.Enabled = false;
        }
        
        /// <summary>
        /// Test that AgentAvoiderBehavior can reach its target without touching another
        /// moving agent that goes across its path.
        /// </summary>
        [UnityTest]
        public IEnumerator AgentAvoiderBehaviorTestFourthScenario()
        {
            // Test setup.
            var obstacleMovingAgent = _seekGameObject.GetComponent<AgentMover>();
            obstacleMovingAgent.MaximumSpeed = 2.0f;
            obstacleMovingAgent.StopSpeed = 0.1f;
            obstacleMovingAgent.MaximumRotationalSpeed = 180f;
            obstacleMovingAgent.StopRotationThreshold = 1f;
            obstacleMovingAgent.MaximumAcceleration = 1.8f;
            obstacleMovingAgent.MaximumDeceleration = 1.8f;
            var obstacleMovingAgentColor = _seekGameObject.GetComponent<AgentColor>();
            obstacleMovingAgentColor.Color = Color.red;
            var seekSteeringBehavior = 
                _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            
            var agentAvoider = _agentAvoiderGameObject.GetComponent<AgentMover>();
            agentAvoider.MaximumSpeed = 2.7f;
            agentAvoider.MaximumAcceleration = 4.0f;
            agentAvoider.MaximumRotationalSpeed = 180f;
            agentAvoider.StopRotationThreshold = 1f;
            agentAvoider.StopSpeed = 0.1f;
            agentAvoider.MaximumAcceleration = 2;
            agentAvoider.MaximumDeceleration = 4;
            var agentAvoiderSeekSteeringBehavior = _agentAvoiderGameObject.GetComponentInChildren<SeekSteeringBehavior>();

            
            // FOURTH SCENARIO:
            _target.TargetPosition = _position14.position;
            agentAvoiderSeekSteeringBehavior.Target = _target.gameObject;
            _agentAvoiderGameObject.transform.position = _position10.position;
            _seekGameObject.transform.position = _position14.position;
            seekSteeringBehavior.Target = _position10.gameObject;
            _target.Enabled = true;
            _seekGameObject.SetActive(true);
            _agentAvoiderGameObject.SetActive(true);
            
            // Assert we move without touching the obstacle agent.
            int steps = 5;
            for (int i=0; i < steps; i++)
            {
                yield return new WaitForSeconds(1.0f);
                Assert.True(Vector3.Distance(_seekGameObject.transform.position, 
                    _agentAvoiderGameObject.transform.position) >= (1.5f));
            }
            
            // Assert we reached target.
            Assert.True(Vector3.Distance(_agentAvoiderGameObject.transform.position, 
                    _target.transform.position) <= (0.1f));
            
            // Cleanup.
            _seekGameObject.SetActive(false);
            _agentAvoiderGameObject.SetActive(false);
            _target.Enabled = false;
        }
        
        /// <summary>
        /// Test that AgentAvoiderBehavior can reach its target without touching another
        /// moving agent that goes across its path.
        /// </summary>
        [UnityTest]
        public IEnumerator AgentAvoiderBehaviorTestFifthScenario()
        {
            // Test setup.
            var obstacleMovingAgent = _seekGameObject.GetComponent<AgentMover>();
            obstacleMovingAgent.MaximumSpeed = 2.0f;
            obstacleMovingAgent.StopSpeed = 0.1f;
            obstacleMovingAgent.MaximumRotationalSpeed = 180f;
            obstacleMovingAgent.StopRotationThreshold = 1f;
            obstacleMovingAgent.MaximumAcceleration = 1.8f;
            obstacleMovingAgent.MaximumDeceleration = 1.8f;
            var obstacleMovingAgentColor = _seekGameObject.GetComponent<AgentColor>();
            obstacleMovingAgentColor.Color = Color.red;
            var seekSteeringBehavior = 
                _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            
            var agentAvoider = _agentAvoiderGameObject.GetComponent<AgentMover>();
            agentAvoider.MaximumSpeed = 2.7f;
            agentAvoider.MaximumAcceleration = 4.0f;
            agentAvoider.MaximumRotationalSpeed = 180f;
            agentAvoider.StopRotationThreshold = 1f;
            agentAvoider.StopSpeed = 0.1f;
            agentAvoider.MaximumAcceleration = 2;
            agentAvoider.MaximumDeceleration = 4;
            var agentAvoiderSeekSteeringBehavior = _agentAvoiderGameObject.GetComponentInChildren<SeekSteeringBehavior>();

            
            // FIFTH SCENARIO:
            _target.TargetPosition = _position12.position;
            agentAvoiderSeekSteeringBehavior.Target = _target.gameObject;
            _agentAvoiderGameObject.transform.position = _position2.position;
            _seekGameObject.transform.position = _position12.position;
            seekSteeringBehavior.Target = _position2.gameObject;
            _target.Enabled = true;
            _seekGameObject.SetActive(true);
            _agentAvoiderGameObject.SetActive(true);
            
            // Assert we move without touching the obstacle agent.
            int steps = 5;
            for (int i=0; i < steps; i++)
            {
                yield return new WaitForSeconds(1.0f);
                Assert.True(Vector3.Distance(_seekGameObject.transform.position, 
                    _agentAvoiderGameObject.transform.position) >= (1.5f));
            }
            
            // Assert we reached target.
            Assert.True(Vector3.Distance(_agentAvoiderGameObject.transform.position, 
                    _target.transform.position) <= (0.1f));
            
            // Cleanup.
            _seekGameObject.SetActive(false);
            _agentAvoiderGameObject.SetActive(false);
            _target.Enabled = false;
        }
    }
}
