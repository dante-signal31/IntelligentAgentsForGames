using UnityEngine;

namespace Tests.PlayTests.Common
{
    public static class TestGameObjectManagement
    {
        /// <summary>
        /// Instantiate given prefab at a given position.
        ///
        /// WARNING: prefab should be in Resources folder or it won't
        /// be instantiated.
        /// </summary>
        /// <param name="path">Prefab path at Assets folder.</param>
        /// <param name="spawnPosition">Position where to spawn this game
        /// object.</param>
        /// <param name="testModuleName">(optional) Test module name to be
        /// used at error logs.</param>
        /// <param name="testMethodName">(optional) Test method name to be
        /// used at error logs.</param>
        /// <returns>Spawned game object or null if prefab not found.</returns>
        public static GameObject InstantiatePrefabByPath(
            string path, 
            Vector3 spawnPosition,
            string testModuleName = "",
            string testMethodName = "")
        {
            GameObject prefab = Resources.Load<GameObject>(path);

            if (prefab != null)
            {
                GameObject spawnedGameObject = GameObject.Instantiate(
                    prefab, 
                    spawnPosition, 
                    Quaternion.identity);
                return spawnedGameObject; 
            }
            
            Debug.LogError($"[{testModuleName} - {testMethodName}] " +
                           $"Prefab not found at " +
                           $"path: {path}");
            return null;
        }
    }
}