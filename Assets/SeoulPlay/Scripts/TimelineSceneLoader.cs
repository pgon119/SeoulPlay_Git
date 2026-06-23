using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class TimelineSceneLoader : MonoBehaviour
    {
        [SerializeField] private PlayableDirector director;
        [SerializeField] private string targetSceneName = "SeoulPlay_Street1";
        [SerializeField, Min(0f)] private double triggerTime = 4.3016683350016685;
        [SerializeField] private bool evaluateFirstFrameOnStart = true;

        private double previousTime;
        private bool hasTriggered;

        private void Awake()
        {
            if (director == null)
            {
                director = GetComponent<PlayableDirector>();
            }

            previousTime = director != null ? director.time : 0d;
        }

        private void OnEnable()
        {
            hasTriggered = false;
            previousTime = director != null ? director.time : 0d;
        }

        private void Start()
        {
            if (!evaluateFirstFrameOnStart || director == null)
            {
                return;
            }

            director.time = 0d;
            director.Evaluate();
            previousTime = 0d;
        }

        private void Update()
        {
            if (hasTriggered || director == null || string.IsNullOrWhiteSpace(targetSceneName))
            {
                return;
            }

            var currentTime = director.time;
            if (currentTime < previousTime)
            {
                hasTriggered = false;
            }

            if (previousTime < triggerTime && currentTime >= triggerTime)
            {
                hasTriggered = true;
                LoadTargetScene();
                return;
            }

            previousTime = currentTime;
        }

        public void LoadTargetScene()
        {
            if (!string.IsNullOrWhiteSpace(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
            }
        }
    }
}
