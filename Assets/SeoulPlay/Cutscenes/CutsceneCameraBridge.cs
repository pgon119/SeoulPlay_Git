using System.Collections;
using Cinemachine;
using Invector.vCamera;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace SeoulPlay.Cutscenes
{
    public class CutsceneCameraBridge : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private vThirdPersonCamera gameplayCamera;
        [SerializeField] private vThirdPersonInput playerInput;
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [SerializeField] private CinemachineVirtualCameraBase handoffCamera;
        [SerializeField] private PlayableDirector timelineDirector;

        [Header("Blend")]
        [SerializeField] private int handoffPriority = 1000;
        [SerializeField] private int cutscenePriority = 900;
        [SerializeField] private float returnBlendDuration = 1.25f;
        [SerializeField] private bool useUnscaledTime;

        [Header("Player Control")]
        [SerializeField] private bool lockPlayerInput = true;
        [SerializeField] private bool lockCameraInput = true;
        [SerializeField] private bool stopPlayerOnStart = true;
        [SerializeField] private bool freezeGameplayCamera = true;

        [Header("Events")]
        public UnityEvent OnCutsceneStarted;
        public UnityEvent OnCutsceneFinished;

        private Coroutine playRoutine;
        private Pose gameplayCameraPose;
        private float gameplayCameraFov;
        private bool gameplayCameraWasEnabled;
        private bool cinemachineBrainWasEnabled;
        private bool playerLockInputWasEnabled;
        private bool playerLockMoveInputWasEnabled;
        private bool playerLockCameraInputWasEnabled;
        private int handoffCameraOriginalPriority;
        private CinemachineVirtualCameraBase activeCutsceneCamera;
        private int activeCutsceneCameraOriginalPriority;
        private PlayableDirector activeDirector;

        public bool IsPlaying => playRoutine != null;
        public PlayableDirector TimelineDirector => timelineDirector;

        private void Reset()
        {
            ResolveMissingReferences();
        }

        private void Awake()
        {
            ResolveMissingReferences();
        }

        public void Play()
        {
            Play(timelineDirector);
        }

        public void Play(PlayableDirector director)
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                FinishCutscene();
            }

            playRoutine = StartCoroutine(PlayTimelineRoutine(director));
        }

        public void PlayCamera(CinemachineVirtualCameraBase cutsceneCamera, float holdSeconds)
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                FinishCutscene();
            }

            playRoutine = StartCoroutine(PlayCameraRoutine(cutsceneCamera, holdSeconds));
        }

        public void Stop()
        {
            if (playRoutine == null)
            {
                return;
            }

            StopCoroutine(playRoutine);
            FinishCutscene();
            playRoutine = null;
        }

        private IEnumerator PlayTimelineRoutine(PlayableDirector director)
        {
            BeginCutscene();
            activeDirector = director;

            if (director != null && director.playableAsset != null)
            {
                director.time = 0;
                director.Play();

                while (director.state == PlayState.Playing)
                {
                    yield return null;
                }
            }

            yield return ReturnToGameplayRoutine();
            FinishCutscene();
            playRoutine = null;
        }

        private IEnumerator PlayCameraRoutine(CinemachineVirtualCameraBase cutsceneCamera, float holdSeconds)
        {
            BeginCutscene();

            activeCutsceneCamera = cutsceneCamera;
            if (activeCutsceneCamera != null)
            {
                activeCutsceneCameraOriginalPriority = activeCutsceneCamera.Priority;
                activeCutsceneCamera.Priority = cutscenePriority;
            }

            yield return Wait(holdSeconds);
            yield return ReturnToGameplayRoutine();
            FinishCutscene();
            playRoutine = null;
        }

        private void BeginCutscene()
        {
            ResolveMissingReferences();

            if (mainCamera != null)
            {
                gameplayCameraPose = new Pose(mainCamera.transform.position, mainCamera.transform.rotation);
                gameplayCameraFov = mainCamera.fieldOfView;
            }

            if (handoffCamera != null)
            {
                handoffCameraOriginalPriority = handoffCamera.Priority;
                handoffCamera.transform.SetPositionAndRotation(gameplayCameraPose.position, gameplayCameraPose.rotation);
                handoffCamera.Priority = handoffPriority;
            }

            if (cinemachineBrain != null)
            {
                cinemachineBrainWasEnabled = cinemachineBrain.enabled;
                cinemachineBrain.enabled = true;
            }

            if (gameplayCamera != null)
            {
                gameplayCameraWasEnabled = gameplayCamera.enabled;
                if (freezeGameplayCamera)
                {
                    gameplayCamera.FreezeCamera();
                }
                gameplayCamera.enabled = false;
            }

            if (playerInput != null)
            {
                playerLockInputWasEnabled = playerInput.lockInput;
                playerLockMoveInputWasEnabled = playerInput.lockMoveInput;
                playerLockCameraInputWasEnabled = playerInput.lockCameraInput;

                if (lockPlayerInput)
                {
                    playerInput.SetLockAllInput(true);
                    playerInput.lockMoveInput = true;
                }

                if (lockCameraInput)
                {
                    playerInput.SetLockCameraInput(true);
                }

                if (stopPlayerOnStart && playerInput.cc != null)
                {
                    playerInput.cc.StopCharacter();
                }
            }

            OnCutsceneStarted.Invoke();
        }

        private IEnumerator ReturnToGameplayRoutine()
        {
            if (activeCutsceneCamera != null)
            {
                activeCutsceneCamera.Priority = activeCutsceneCameraOriginalPriority;
                activeCutsceneCamera = null;
            }

            if (handoffCamera != null)
            {
                handoffCamera.transform.SetPositionAndRotation(gameplayCameraPose.position, gameplayCameraPose.rotation);
                handoffCamera.Priority = handoffPriority;
            }

            if (mainCamera != null && cinemachineBrain != null && cinemachineBrain.enabled)
            {
                yield return Wait(returnBlendDuration);
            }
        }

        private void FinishCutscene()
        {
            if (activeDirector != null && activeDirector.state == PlayState.Playing)
            {
                activeDirector.Stop();
            }
            activeDirector = null;

            if (handoffCamera != null)
            {
                handoffCamera.Priority = handoffCameraOriginalPriority;
            }

            if (mainCamera != null)
            {
                mainCamera.transform.SetPositionAndRotation(gameplayCameraPose.position, gameplayCameraPose.rotation);
                mainCamera.fieldOfView = gameplayCameraFov;
            }

            if (cinemachineBrain != null)
            {
                cinemachineBrain.enabled = cinemachineBrainWasEnabled;
            }

            if (gameplayCamera != null)
            {
                gameplayCamera.enabled = gameplayCameraWasEnabled;
                if (freezeGameplayCamera)
                {
                    gameplayCamera.UnFreezeCamera();
                }
            }

            if (playerInput != null)
            {
                playerInput.lockInput = playerLockInputWasEnabled;
                playerInput.lockMoveInput = playerLockMoveInputWasEnabled;
                playerInput.SetLockCameraInput(playerLockCameraInputWasEnabled);
            }

            OnCutsceneFinished.Invoke();
        }

        private IEnumerator Wait(float seconds)
        {
            if (seconds <= 0f)
            {
                yield break;
            }

            if (useUnscaledTime)
            {
                var endTime = Time.unscaledTime + seconds;
                while (Time.unscaledTime < endTime)
                {
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(seconds);
            }
        }

        private void ResolveMissingReferences()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (gameplayCamera == null)
            {
                gameplayCamera = FindObjectOfType<vThirdPersonCamera>();
            }

            if (playerInput == null)
            {
                playerInput = FindObjectOfType<vThirdPersonInput>();
            }

            if (cinemachineBrain == null && mainCamera != null)
            {
                cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
            }
        }
    }
}
