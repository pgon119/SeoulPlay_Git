using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace SeoulPlay.Cutscenes
{
    [RequireComponent(typeof(Collider))]
    public class CutsceneTrigger : MonoBehaviour
    {
        [SerializeField] private CutsceneCameraBridge bridge;
        [SerializeField] private PlayableDirector timelineOverride;
        [SerializeField] private CinemachineVirtualCameraBase cameraOverride;
        [SerializeField] private float cameraHoldSeconds = 3f;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool playOnce = true;

        private bool hasPlayed;

        private void Reset()
        {
            bridge = FindObjectOfType<CutsceneCameraBridge>();
            var triggerCollider = GetComponent<Collider>();
            triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (playOnce && hasPlayed)
            {
                return;
            }

            if (!other.CompareTag(playerTag))
            {
                return;
            }

            if (bridge == null)
            {
                bridge = FindObjectOfType<CutsceneCameraBridge>();
            }

            if (bridge == null || bridge.IsPlaying)
            {
                return;
            }

            hasPlayed = true;
            if (cameraOverride != null)
            {
                bridge.PlayCamera(cameraOverride, cameraHoldSeconds);
            }
            else
            {
                bridge.Play(timelineOverride != null ? timelineOverride : bridge.TimelineDirector);
            }
        }
    }
}
