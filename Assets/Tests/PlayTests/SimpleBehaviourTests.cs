using System.Collections;
using NUnit.Framework;
using Tests.PlayTests.Common;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayTests
{
    public class SimpleBehaviourTests
    {
        private string _currentScene = "ClearCourtyard";
        private string _seekPrefabPath = "Assets/Prefabs/SteeringBehaviors/SeekMovingAgent.prefab";

        private Transform _seekStartPosition;
        private Transform _targetPosition;

        private TargetPlacement _target;

        private GameObject _seekerGameObject;
    
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestLevelManagement.ReLoadScene(_currentScene);

            if (_targetPosition == null)
                _targetPosition = GameObject.Find("TargetPosition").transform;
            if (_target == null)
            {
                _target = GameObject.Find("Target").GetComponent<TargetPlacement>();
                _target.TargetPosition = _targetPosition.position;
                _target.Enabled = false;
            }
            
            if (_seekStartPosition == null) 
                _seekStartPosition = GameObject.Find("SeekStartPosition").transform;
            
            if (_seekerGameObject == null)
            {
                _seekerGameObject = GameObject.Find("SeekMovingAgent");
                // _seekerGameObject.GetComponent<SeekSteeringBehavior>().Target = _target.gameObject;
                _seekerGameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Test that SeekBehavior can seek to a target.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator SeekBehaviourTest()
        {
            // Test setup.
            _seekerGameObject.transform.position = _seekStartPosition.position;
            var seekSteeringBehavior = _seekerGameObject.GetComponent<SeekSteeringBehavior>();
            var agentMover = _seekerGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 40.0f;
            _target.Enabled = true;
            _target.TargetPosition = _targetPosition.position;
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.5f;
            _seekerGameObject.SetActive(true);
            
            // Give time for the seeker to get to the target.
            yield return new WaitForSeconds(1.0f);
            
            // Assert the target was reached.
            Assert.True(Vector3.Distance(_targetPosition.position, 
                _seekerGameObject.transform.position) < 1.0f);
            
            // Cleanup.
            _seekerGameObject.SetActive(false);
            _target.Enabled = false;
        }
    }
}
