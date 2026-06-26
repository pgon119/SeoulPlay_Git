using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class TitleAnyButtonSceneLoader : MonoBehaviour
    {
        [SerializeField] private string targetSceneName = "SeoulPlay_WorldMap";

        private bool isLoading;

        private void Update()
        {
            if (!isLoading && AnyInputDown())
            {
                LoadTargetScene();
            }
        }

        public void LoadTargetScene()
        {
            if (isLoading)
            {
                return;
            }

            isLoading = true;

            if (!string.IsNullOrWhiteSpace(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
                return;
            }

            var nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex, LoadSceneMode.Single);
            }
        }

        private static bool AnyInputDown()
        {
            if (Input.anyKeyDown)
            {
                return true;
            }

            for (var i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
