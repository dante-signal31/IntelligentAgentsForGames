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

        private Transform _seekStartPosition;
        private Transform _alignStartPosition;
        private Transform _faceStartPosition;
        private Transform _fleeStartPosition;
        private Transform _pursuitStartPosition;
        private Transform _pursuitTargetStartPosition;
        private Transform _targetPosition;
        private Transform _targetPosition2;
        private Transform _targetPosition3;
        private Transform _targetPosition4;

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

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestLevelManagement.ReLoadScene(CurrentScene);

            if (_targetPosition == null)
                _targetPosition = GameObject.Find("Position1").transform;
            if (_targetPosition2 == null)
                _targetPosition2 = GameObject.Find("Position2").transform;
            if (_targetPosition3 == null)
                _targetPosition3 = GameObject.Find("Position3").transform;
            if (_targetPosition4 == null)
                _targetPosition4 = GameObject.Find("Position4").transform;
            if (_target == null)
            {
                _target = GameObject.Find("Target").GetComponent<TargetPlacement>();
                _target.TargetPosition = _targetPosition.position;
                _target.Enabled = false;
            }

            if (_seekStartPosition == null)
                _seekStartPosition = GameObject.Find("Position5").transform;
            if (_alignStartPosition == null)
                _alignStartPosition = GameObject.Find("Position6").transform;
            if (_faceStartPosition == null)
                _faceStartPosition = GameObject.Find("Position7").transform;
            if (_fleeStartPosition == null)
                _fleeStartPosition = GameObject.Find("Position8").transform;
            if (_pursuitStartPosition == null)
                _pursuitStartPosition = GameObject.Find("Position9").transform;
            if (_pursuitTargetStartPosition == null)
                _pursuitTargetStartPosition = GameObject.Find("Position10").transform;

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
            
            // if (_pursuitGameObject == null)
            // {
            //     _pursuitGameObject = GameObject.Find("PursuitMovingAgent");
            //     _pursuitGameObject.SetActive(false);
            // }
            // if (_evadeGameObject == null)
            // {
            //     _evadeGameObject = GameObject.Find("EvadeMovingAgent");
            //     _evadeGameObject.SetActive(false);
            // }
            if (_velocityMatchingGameObject == null)
            {
                _velocityMatchingGameObject = GameObject.Find("VelocityMatchingMovingAgent");
                _velocityMatchingGameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Test that SeekBehavior can reach a target.
        /// </summary>
        [UnityTest]
        public IEnumerator SeekBehaviourTest()
        {
            // Test setup.
            _seekGameObject.transform.position = _seekStartPosition.position;
            var seekSteeringBehavior =
                _seekGameObject.GetComponent<SeekSteeringBehavior>();
            var agentMover = _seekGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 10.0f;
            _target.Enabled = true;
            _target.TargetPosition = _targetPosition.position;
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            _seekGameObject.SetActive(true);

            // Give time for the seeker to get to the target.
            yield return new WaitForSeconds(2.0f);

            // Assert the target was reached.
            Assert.True(Vector3.Distance(_targetPosition.position,
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
            _arriveNLAGameObject.transform.position = _seekStartPosition.position;
            var arriveSteeringBehavior =
                _arriveNLAGameObject.GetComponent<ArriveSteeringBehaviorNLA>();
            var agentMover = _arriveNLAGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 5.0f;
            _target.Enabled = true;
            _target.TargetPosition = _targetPosition.position;
            arriveSteeringBehavior.Target = _target.gameObject;
            arriveSteeringBehavior.ArrivalDistance = 0.5f;
            arriveSteeringBehavior.AccelerationRadius = 2.0f;
            arriveSteeringBehavior.BrakingRadius = 3.0f;
            _arriveNLAGameObject.SetActive(true);

            // Check that agent is accelerating at the beginning.
            // Wait until agent starts is movement.
            yield return new WaitUntil(() =>
                (Vector3.Distance(_seekStartPosition.position,
                    _arriveNLAGameObject.transform.position) >= 0.1f) &&
                (Vector3.Distance(_seekStartPosition.position,
                     _arriveNLAGameObject.transform.position) <
                 arriveSteeringBehavior.AccelerationRadius));
            Assert.True(agentMover.CurrentSpeed > 0.0f &&
                        agentMover.CurrentSpeed < agentMover.MaximumSpeed);

            // Check that agent gets its full cruise speed. 
            yield return new WaitUntil(() =>
                Vector3.Distance(_seekStartPosition.position,
                    _arriveNLAGameObject.transform.position) >
                (arriveSteeringBehavior.AccelerationRadius + 0.1f));
            yield return null;
            yield return null;
            Assert.True(
                Mathf.Approximately(agentMover.CurrentSpeed,
                    agentMover.MaximumSpeed));

            // Check that agent is braking at the end.
            yield return new WaitUntil(() =>
                Vector3.Distance(_targetPosition.position,
                    _arriveNLAGameObject.transform.position) <
                (arriveSteeringBehavior.BrakingRadius - 0.2f));
            yield return null;
            yield return null;
            Assert.True(agentMover.CurrentSpeed > 0.0f &&
                        agentMover.CurrentSpeed < agentMover.MaximumSpeed);

            // Assert the target was reached.
            yield return new WaitForSeconds(3f);
            Assert.True(Vector3.Distance(_targetPosition.position,
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
            _arriveLAGameObject.transform.position = _seekStartPosition.position;
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
            _target.TargetPosition = _targetPosition.position;
            arriveSteeringBehavior.Target = _target.gameObject;
            arriveSteeringBehavior.ArrivalDistance = 0.5f;
            _arriveLAGameObject.SetActive(true);

            // Check that agent is accelerating at the beginning.
            // Wait until agent starts is movement.
            yield return new WaitUntil(() =>
                (Vector3.Distance(_seekStartPosition.position,
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
                Vector3.Distance(_targetPosition.position,
                    _arriveLAGameObject.transform.position) <=
                (arriveSteeringBehavior.BrakingRadius));
            yield return null;
            yield return null;
            Assert.True(agentMover.CurrentSpeed > 0.0f &&
                        agentMover.CurrentSpeed < agentMover.MaximumSpeed);

            // Assert the target was reached.
            yield return new WaitForSeconds(3f);
            Assert.True(Vector3.Distance(_targetPosition.position,
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
            _fleeGameObject.transform.position = _fleeStartPosition.position;
            var fleeSteeringBehavior =
                _fleeGameObject.GetComponent<FleeSteeringBehavior>();
            fleeSteeringBehavior.Threath = _target.gameObject;
            fleeSteeringBehavior.PanicDistance = 5.0f;
            var agentMover = _fleeGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 10.0f;
            _target.Enabled = true;
            _target.TargetPosition = _targetPosition.position;
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
            _seekGameObject.transform.position = _seekStartPosition.position;
            var seekSteeringBehavior =
                _seekGameObject.GetComponent<SeekSteeringBehavior>();
            var agentMover = _seekGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 2.0f;
            _target.Enabled = true;
            _target.TargetPosition = _targetPosition.position;
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            var seekColor = _seekGameObject.GetComponent<AgentColor>();
            seekColor.Color = Color.red;
            _alignGameObject.transform.position = _alignStartPosition.position;
            var alignSteeringBehavior =
                _alignGameObject.GetComponent<AlignSteeringBehavior>();
            alignSteeringBehavior.Target = _seekGameObject;
            _alignGameObject.SetActive(true);
            _seekGameObject.SetActive(true);

            // Move seeker to face the first target.
            _target.TargetPosition = _targetPosition.position;
            yield return new WaitForSeconds(2f);
            Assert.True(
                Mathf.Abs(
                    _seekGameObject.GetComponent<AgentMover>().Orientation -
                    _alignGameObject.GetComponent<AgentMover>().Orientation) <=
                agentMover.StopRotationThreshold);

            // Move seeker to face the second target.
            _target.TargetPosition = _targetPosition2.position;
            yield return new WaitForSeconds(2.2f);
            Assert.True(
                Mathf.Abs(
                    _seekGameObject.GetComponent<AgentMover>().Orientation -
                    _alignGameObject.GetComponent<AgentMover>().Orientation) <=
                agentMover.StopRotationThreshold);

            // Move seeker to face the third target.
            _target.TargetPosition = _targetPosition3.position;
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
            _seekGameObject.transform.position = _seekStartPosition.position;
            var seekSteeringBehavior =
                _seekGameObject.GetComponent<SeekSteeringBehavior>();
            var agentMover = _seekGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 2.0f;
            _target.Enabled = true;
            _target.TargetPosition = _targetPosition.position;
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            var seekColor = _seekGameObject.GetComponent<AgentColor>();
            seekColor.Color = Color.red;
            _faceGameObject.transform.position = _faceStartPosition.position;
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

        //     /// <summary>
        //     /// Test that PursuitBehavior can intercept its target.
        //     /// </summary>
        //     [UnityTest]
        //     public IEnumerator PursuitBehaviourTest()
        //     {
        //         // Test setup.
        //         _seekGameObject.transform.position = _pursuitTargetStartPosition.position;
        //         var seekSteeringBehavior = _seekGameObject.GetComponent<SeekSteeringBehavior>();
        //         var agentMover = _seekGameObject.GetComponent<AgentMover>();
        //         agentMover.MaximumSpeed = 2.0f;
        //         _target.Enabled = true;
        //         _target.TargetPosition = _targetPosition4.position;
        //         seekSteeringBehavior.Target = _target.gameObject;
        //         seekSteeringBehavior.ArrivalDistance = 0.2f;
        //         _pursuitGameObject.transform.position = _pursuitStartPosition.position;
        //         var pursuitSteeringBehavior = _pursuitGameObject.GetComponent<PursuitSteeringBehavior>();
        //         var pursuitAgentMover = _seekGameObject.GetComponent<AgentMover>();
        //         pursuitAgentMover.MaximumSpeed = 2.0f;
        //         pursuitSteeringBehavior.Target = _seekGameObject;
        //         _seekGameObject.SetActive(true);
        //         _pursuitGameObject.SetActive(true);
        //         
        //         // Give time for the chaser to get to the target.
        //         yield return new WaitForSeconds(3.0f);
        //         
        //         // Assert the target was reached.
        //         // We test for a distance equal to the radius of both agents, plus
        //         // a 0.1 of tolerance. That should be the distance of centers when
        //         // both agents are touching.
        //         Assert.True(Vector3.Distance(_seekGameObject.transform.position, 
        //                         _pursuitGameObject.transform.position) <= (1.1f));
        //         
        //         // Cleanup.
        //         _seekGameObject.SetActive(false);
        //         _pursuitGameObject.SetActive(false);
        //         _target.Enabled = false;
        //     }
        //     
        //     /// <summary>
        //     /// Test that EvadeBehavior can can keep away its agent from its chaser.
        //     /// </summary>
        //     [UnityTest]
        //     public IEnumerator EvadeBehaviourTest()
        //     {
        //         // Test setup.
        //         _evadeGameObject.transform.position = _fleeStartPosition.position;
        //         var evadeSteeringBehavior = _evadeGameObject.GetComponent<EvadeSteeringBehavior>();
        //         var evadeAgentMover = _evadeGameObject.GetComponent<AgentMover>();
        //         _seekGameObject.transform.position = _seekStartPosition.position;
        //         var seekSteeringBehavior = _seekGameObject.GetComponent<SeekSteeringBehavior>();
        //         var seekAgentMover = _seekGameObject.GetComponent<AgentMover>();
        //         seekAgentMover.MaximumSpeed = 2.0f;
        //         evadeAgentMover.MaximumSpeed = 2.0f;
        //         seekSteeringBehavior.Target = _evadeGameObject;
        //         evadeSteeringBehavior.Threath = _seekGameObject;
        //         evadeSteeringBehavior.PanicDistance = 3.0f;
        //         _evadeGameObject.SetActive(true);
        //         _seekGameObject.SetActive(true);
        //         
        //         // Give time for the chaser to try to reach evader.
        //         yield return new WaitForSeconds(4.0f);
        //         
        //         // Assert the evader was not reached.
        //         Assert.True(Vector3.Distance(_seekGameObject.transform.position, 
        //             _evadeGameObject.transform.position) >= (2.0f));
        //         
        //         // Cleanup.
        //         _seekGameObject.SetActive(false);
        //         _evadeGameObject.SetActive(false);
        //         _target.Enabled = false;
        //     }
    
        /// <summary>
        /// Test that VelocityMatchingBehavior can can copy its target's velocity.
        /// </summary>
        [UnityTest]
        public IEnumerator VelocityMatchingBehaviourTest()
        {
            // Test setup.
            _velocityMatchingGameObject.transform.position =
                _pursuitTargetStartPosition.position;
            var velocityMatchingSteeringBehavior = _velocityMatchingGameObject
                .GetComponent<VelocityMatchingSteeringBehavior>();
            var velocityMatchingRigidbody =
                _velocityMatchingGameObject.GetComponent<Rigidbody2D>();
            var velocityMatchingAgentMover =
                _velocityMatchingGameObject.GetComponent<AgentMover>();
            _arriveLAGameObject.transform.position = _alignStartPosition.position;
            var arriveSteeringBehavior =
                _arriveLAGameObject.GetComponent<ArriveSteeringBehaviorLA>();
            var arriveAgentMover = _arriveLAGameObject.GetComponent<AgentMover>();
            var arriveRigidbody = _arriveLAGameObject.GetComponent<Rigidbody2D>();
            arriveAgentMover.MaximumSpeed = 5.55f;
            arriveAgentMover.StopSpeed = 0.1f;
            arriveAgentMover.MaximumRotationalSpeed = 180f;
            arriveAgentMover.StopRotationThreshold = 1f;
            arriveAgentMover.MaximumAcceleration = 4f;
            arriveAgentMover.MaximumDeceleration = 4f;
            arriveSteeringBehavior.Target = _targetPosition.gameObject;
            var arriveColor = _arriveLAGameObject.GetComponent<AgentColor>();
            arriveColor.Color = Color.red;
            velocityMatchingAgentMover.MaximumSpeed = 5.55f;
            velocityMatchingAgentMover.StopSpeed = 0.1f;
            velocityMatchingAgentMover.MaximumRotationalSpeed = 180f;
            velocityMatchingAgentMover.StopRotationThreshold = 1f;
            velocityMatchingAgentMover.MaximumAcceleration = 40f;
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
            Debug.Log($"Difference: {Mathf.Abs(velocityMatchingAgentMover.Velocity.magnitude - arriveAgentMover.Velocity.magnitude)}");
            Assert.True(velocityMatchingAgentMover.Velocity.normalized == arriveAgentMover.Velocity.normalized &&
                        Mathf.Abs(velocityMatchingAgentMover.Velocity.magnitude - arriveAgentMover.Velocity.magnitude) < 0.2f);

            // Wait until arriver brakes and asserts that the VelocityMatcher
            // has braked too.
            yield return new WaitUntil(() =>
                Mathf.Approximately(arriveAgentMover.CurrentSpeed, 0));
            yield return new WaitForSecondsRealtime(velocityMatchingSteeringBehavior.TimeToMatch);
            Debug.Log($"Difference: {Mathf.Abs(velocityMatchingAgentMover.Velocity.magnitude - arriveAgentMover.Velocity.magnitude)}");
            Assert.True(Mathf.Abs(velocityMatchingAgentMover.Velocity.magnitude - arriveAgentMover.Velocity.magnitude) < 0.2f);

            // Cleanup.
            _velocityMatchingGameObject.SetActive(false);
            _arriveLAGameObject.SetActive(false);
        }
    }
}
