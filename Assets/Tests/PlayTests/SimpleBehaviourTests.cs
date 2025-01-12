using System.Collections;
using System.Linq;
using NUnit.Framework;
using Tests.PlayTests.Common;
using UnityEngine;
using UnityEngine.PlayerLoop;
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
        private Transform _position10;
        private Transform _position1;
        private Transform _position2;
        private Transform _position3;
        private Transform _position4;
        private Transform _position11;

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
            if (_position10 == null)
                _position10 = GameObject.Find("Position10").transform;
            if (_position11 == null)
                _position11 = GameObject.Find("Position11").transform;

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
                _seekGameObject.GetComponent<SeekSteeringBehavior>();
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
        {
            // Test setup.
            _arriveNLAGameObject.transform.position = _position5.position;
            var arriveSteeringBehavior =
                _arriveNLAGameObject.GetComponent<ArriveSteeringBehaviorNLA>();
            var agentMover = _arriveNLAGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 5.0f;
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
            Assert.True(agentMover.CurrentSpeed > 0.0f &&
                        agentMover.CurrentSpeed < agentMover.MaximumSpeed);

            // Check that agent gets its full cruise speed. 
            yield return new WaitUntil(() =>
                Vector3.Distance(_position5.position,
                    _arriveNLAGameObject.transform.position) >
                (arriveSteeringBehavior.AccelerationRadius + 0.1f));
            yield return null;
            yield return null;
            Assert.True(
                Mathf.Approximately(agentMover.CurrentSpeed,
                    agentMover.MaximumSpeed));

            // Check that agent is braking at the end.
            yield return new WaitUntil(() =>
                Vector3.Distance(_position1.position,
                    _arriveNLAGameObject.transform.position) <
                (arriveSteeringBehavior.BrakingRadius - 0.2f));
            yield return null;
            yield return null;
            Assert.True(agentMover.CurrentSpeed > 0.0f &&
                        agentMover.CurrentSpeed < agentMover.MaximumSpeed);

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
                _arriveLAGameObject.GetComponent<ArriveSteeringBehaviorLA>();
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
                _fleeGameObject.GetComponent<FleeSteeringBehavior>();
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
                // Assert fleer is now farther from the target than before.
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
                _seekGameObject.GetComponent<SeekSteeringBehavior>();
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
                _alignGameObject.GetComponent<AlignSteeringBehavior>();
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
                _seekGameObject.GetComponent<SeekSteeringBehavior>();
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
                _faceGameObject.GetComponent<FaceMatchingSteeringBehavior>();
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
            var seekSteeringBehavior = _seekGameObject.GetComponent<SeekSteeringBehavior>();
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
            var pursueSteeringBehavior = _pursuitGameObject.GetComponent<PursuitSteeringBehavior>();
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
        /// Test that EvadeBehavior can can keep away its agent from its chaser.
        /// </summary>
        [UnityTest]
        public IEnumerator EvadeBehaviourTest()
        {
            // Test setup.
            var evadeAgentMover = _evadeGameObject.GetComponent<AgentMover>();
            var evadeSteeringBehavior = _evadeGameObject.GetComponent<EvadeSteeringBehavior>();
            var seekSteeringBehavior = _seekGameObject.GetComponent<SeekSteeringBehavior>();
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
                _evadeGameObject.transform.position) >= (2.0f));
            
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
                .GetComponent<VelocityMatchingSteeringBehavior>();
            var velocityMatchingRigidbody =
                _velocityMatchingGameObject.GetComponent<Rigidbody2D>();
            var velocityMatchingAgentMover =
                _velocityMatchingGameObject.GetComponent<AgentMover>();
            var arriveSteeringBehavior =
                _arriveLAGameObject.GetComponent<ArriveSteeringBehaviorLA>();
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
                .GetComponent<VelocityMatchingSteeringBehavior>();
            var velocityMatchingRigidbody =
                _velocityMatchingGameObject.GetComponent<Rigidbody2D>();
            var velocityMatchingAgentMover =
                _velocityMatchingGameObject.GetComponent<AgentMover>();
            var arriveSteeringBehavior =
                _arriveLAGameObject.GetComponent<ArriveSteeringBehaviorLA>();
            var arriveAgentMover = _arriveLAGameObject.GetComponent<AgentMover>();
            var arriveRigidbody = _arriveLAGameObject.GetComponent<Rigidbody2D>();
            var interposeSteeringBehavior =
                _interposeGameObject.GetComponent<InterposeSteeringBehavior>();
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
    }
}
