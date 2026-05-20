using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class LoadingSceneController : MonoBehaviour
    {
        [SerializeField] private string targetSceneName = "SeoulPlay_Street1";

        private AsyncOperation loadOperation;
        private int progressPercent;

        private IEnumerator Start()
        {
            if (string.IsNullOrWhiteSpace(targetSceneName))
            {
                yield break;
            }

            yield return null;

            loadOperation = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
            while (loadOperation != null && !loadOperation.isDone)
            {
                progressPercent = Mathf.RoundToInt(Mathf.Clamp01(loadOperation.progress / 0.9f) * 100f);
                yield return null;
            }
        }

        private void OnGUI()
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, Color.black, 0f, 0f);

            const int width = 360;
            const int height = 80;

            var rect = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height);

            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 32,
                normal = { textColor = Color.white }
            };

            GUI.Label(rect, $"Loading {progressPercent}%", style);
        }
    }
}
