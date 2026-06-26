using EasyTransition;
using UnityEngine;
using UnityEngine.Playables;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class SeoulPlayFrameTransitionTrigger : MonoBehaviour
    {
        [Header("Transition")]
        [Tooltip("Scene transition manager that owns the EasyTransitions template prefab.")]
        [SerializeField] private TransitionManager transitionManager;
        [Tooltip("EasyTransitions settings asset to play. Use Assets/EasyTransitions/Transitions/Noise/Noise.asset for the noise transition.")]
        [SerializeField] private TransitionSettings transition;

        [Header("Timing")]
        [Tooltip("Optional timeline. When Use Timeline Time is enabled, Trigger Frame is evaluated against this director's current time.")]
        [SerializeField] private PlayableDirector director;
        [Tooltip("Use PlayableDirector.time instead of elapsed scene time.")]
        [SerializeField] private bool useTimelineTime;
        [Tooltip("Frame number after this scene starts. For example, frame 30 at 30 fps triggers after 1 second.")]
        [SerializeField, Min(0)] private int triggerFrame;
        [Tooltip("Frame rate used to convert Trigger Frame into seconds.")]
        [SerializeField, Min(1f)] private float framesPerSecond = 30f;
        [Tooltip("Automatically play the transition after the configured frame delay.")]
        [SerializeField] private bool playOnStart = true;
        [Tooltip("Use unscaled time so the transition timing is not affected by Time.timeScale.")]
        [SerializeField] private bool useRealtime = true;

        private float elapsedTime;
        private double previousTimelineTime;
        private bool hasTriggered;

        public float TriggerTimeSeconds => triggerFrame / framesPerSecond;

        private void OnEnable()
        {
            elapsedTime = 0f;
            previousTimelineTime = director != null ? director.time : 0d;
            hasTriggered = false;
        }

        private void Update()
        {
            if (!playOnStart || hasTriggered)
            {
                return;
            }

            if (useTimelineTime && director != null)
            {
                UpdateTimelineTrigger();
                return;
            }

            elapsedTime += useRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (elapsedTime >= TriggerTimeSeconds)
            {
                Trigger();
            }
        }

        private void UpdateTimelineTrigger()
        {
            var currentTime = director.time;
            if (currentTime < previousTimelineTime)
            {
                hasTriggered = false;
            }

            if (previousTimelineTime < TriggerTimeSeconds && currentTime >= TriggerTimeSeconds)
            {
                Trigger();
                return;
            }

            previousTimelineTime = currentTime;
        }

        [ContextMenu("Trigger Transition Now")]
        public void Trigger()
        {
            if (hasTriggered)
            {
                return;
            }

            if (transition == null)
            {
                Debug.LogWarning($"{nameof(SeoulPlayFrameTransitionTrigger)}: Transition is not assigned.", this);
                return;
            }

            if (transitionManager == null)
            {
                transitionManager = TransitionManager.Instance();
            }

            if (transitionManager == null)
            {
                Debug.LogWarning($"{nameof(SeoulPlayFrameTransitionTrigger)}: TransitionManager is not available.", this);
                return;
            }

            hasTriggered = true;
            transitionManager.Transition(transition, 0f);
        }
    }
}
