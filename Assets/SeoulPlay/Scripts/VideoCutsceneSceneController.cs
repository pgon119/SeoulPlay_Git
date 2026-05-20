using System.Collections;
using EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class VideoCutsceneSceneController : MonoBehaviour
    {
        [Header("Video")]
        [SerializeField] private VideoClip videoClip;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private AudioSource audioSource;

        [Header("Scene Flow")]
        [SerializeField] private string nextSceneName = "SeoulPlay_Street1";
        [SerializeField] private bool preloadNextSceneDuringVideo;
        [SerializeField, Min(0f)] private float preloadDelayAfterVideoStarts = 1.5f;
        [SerializeField] private bool allowSkip = true;
        [SerializeField] private TransitionSettings transition;
        [SerializeField, Min(0f)] private float transitionStartDelay;

        private VideoPlayer videoPlayer;
        private AsyncOperation preloadOperation;
        private ThreadPriority originalBackgroundLoadingPriority;
        private bool isLoadingNextScene;

        private IEnumerator Start()
        {
            originalBackgroundLoadingPriority = Application.backgroundLoadingPriority;

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                targetCamera = gameObject.AddComponent<Camera>();
                targetCamera.tag = "MainCamera";
            }

            if (audioSource == null)
            {
                audioSource = gameObject.GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (videoClip == null)
            {
                Debug.LogWarning($"{nameof(VideoCutsceneSceneController)}: Video clip is not assigned.", this);
                LoadNextScene();
                yield break;
            }

            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            ConfigureVideoPlayerForPlayback();

            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            videoPlayer.Play();

            if (preloadNextSceneDuringVideo)
            {
                StartCoroutine(PreloadNextSceneAfterVideoStarts());
            }
        }

        private void Update()
        {
            if (!allowSkip || isLoadingNextScene)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Submit") ||
                Input.GetButtonDown("Cancel") ||
                Input.GetButtonDown("Start"))
            {
                LoadNextScene();
            }
        }

        private void OnDestroy()
        {
            Application.backgroundLoadingPriority = originalBackgroundLoadingPriority;

            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= HandleVideoFinished;
            }
        }

        private void HandleVideoFinished(VideoPlayer source)
        {
            LoadNextScene();
        }

        private void ConfigureVideoPlayerForPlayback()
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.isLooping = false;
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = videoClip;
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            videoPlayer.targetCamera = targetCamera;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audioSource);
            videoPlayer.loopPointReached -= HandleVideoFinished;
            videoPlayer.loopPointReached += HandleVideoFinished;
        }

        private IEnumerator PreloadNextSceneAfterVideoStarts()
        {
            while (videoPlayer != null && !videoPlayer.isPlaying)
            {
                yield return null;
            }

            if (preloadDelayAfterVideoStarts > 0f)
            {
                yield return new WaitForSeconds(preloadDelayAfterVideoStarts);
            }

            StartPreloadingNextScene();
        }

        private void StartPreloadingNextScene()
        {
            if (preloadOperation != null || string.IsNullOrWhiteSpace(nextSceneName))
            {
                return;
            }

            originalBackgroundLoadingPriority = Application.backgroundLoadingPriority;
            Application.backgroundLoadingPriority = ThreadPriority.Low;

            preloadOperation = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
            if (preloadOperation != null)
            {
                preloadOperation.allowSceneActivation = false;
            }
            else
            {
                Application.backgroundLoadingPriority = originalBackgroundLoadingPriority;
            }
        }

        private void LoadNextScene()
        {
            if (isLoadingNextScene || string.IsNullOrWhiteSpace(nextSceneName))
            {
                return;
            }

            isLoadingNextScene = true;

            if (preloadOperation == null)
            {
                if (transition != null && TransitionManager.Instance() != null)
                {
                    TransitionManager.Instance().Transition(nextSceneName, transition, transitionStartDelay);
                    return;
                }

                SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
                return;
            }

            StartCoroutine(ActivateWhenPreloadReady());
        }

        private IEnumerator ActivateWhenPreloadReady()
        {
            while (preloadOperation.progress < 0.9f)
            {
                yield return null;
            }

            Application.backgroundLoadingPriority = originalBackgroundLoadingPriority;
            preloadOperation.allowSceneActivation = true;
        }
    }
}
