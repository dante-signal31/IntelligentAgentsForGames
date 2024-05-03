using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tests.PlayTests.Common
{
    public static class TestLevelManagement
    {
        /// <summary>
        /// Load a test scene.
        /// </summary>
        /// <param name="sceneName">Name of the desired scene.</param>
        public static IEnumerator LoadScene(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(
                sceneName, 
                LoadSceneMode.Single);
            yield return new WaitUntil(() => asyncLoad.isDone);
        }

        /// <summary>
        /// Unload give scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to Unload.</param>
        public static IEnumerator UnloadScene(string sceneName)
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(
                    sceneName, 
                    UnloadSceneOptions.None);
                if (asyncUnload != null)
                    yield return new WaitUntil(() => asyncUnload.isDone);
                else
                    yield return null;
            }
        }

        /// <summary>
        /// Unload given scene and load it again.
        /// </summary>
        /// <param name="sceneName">Name of the scene to reload.</param>
        public static IEnumerator ReLoadScene(string sceneName)
        {
            _ = UnloadScene(sceneName);
            yield return LoadScene(sceneName);
        }
    }
}