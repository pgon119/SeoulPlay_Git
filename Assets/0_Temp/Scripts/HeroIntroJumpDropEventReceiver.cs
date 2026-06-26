using UnityEngine;

namespace SeoulPlay
{
    public sealed class HeroIntroJumpDropEventReceiver : MonoBehaviour
    {
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Animator sourceAnimator;
        [SerializeField, Min(0f)] private float additionalDropHeight = 0.6f;
        [SerializeField] private float forwardDropDistance = 0.6f;
        [SerializeField, Min(0.01f)] private float dropDuration = 0.2f;
        [SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve forwardCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private bool useAnimatorStateFallback = true;
        [SerializeField] private string jumpStateName = "Jumping Down";
        [SerializeField, Range(0f, 1f)] private float fallbackDropNormalizedTime = 0.95f;

        private Vector3 cachedLocalPosition;
        private bool hasCachedLocalPosition;
        private bool hasAppliedDrop;
        private bool wasInJumpState;
        private int jumpStateHash;
        private Vector3 currentDropOffset;
        private Coroutine dropRoutine;

        private void Awake()
        {
            if (targetTransform == null)
            {
                targetTransform = transform;
            }

            if (sourceAnimator == null)
            {
                sourceAnimator = GetComponent<Animator>();
            }

            jumpStateHash = Animator.StringToHash(jumpStateName);
        }

        private void Update()
        {
            if (!useAnimatorStateFallback || sourceAnimator == null)
            {
                return;
            }

            var stateInfo = sourceAnimator.GetCurrentAnimatorStateInfo(0);
            var isInJumpState = stateInfo.shortNameHash == jumpStateHash || stateInfo.IsName(jumpStateName);
            if (!isInJumpState)
            {
                wasInJumpState = false;
                return;
            }

            if (!wasInJumpState)
            {
                CacheCurrentLocalPosition();
                wasInJumpState = true;
            }

            if (!hasAppliedDrop && stateInfo.normalizedTime >= fallbackDropNormalizedTime)
            {
                ApplyAdditionalDrop();
            }
        }

        private void LateUpdate()
        {
            ApplyRenderedDropOffset();
        }

        public void CacheJumpDropStart()
        {
            CacheCurrentLocalPosition();
        }

        public void HeroJumpDropStart()
        {
            CacheCurrentLocalPosition();
        }

        public void KeepJumpHeight()
        {
            CacheCurrentLocalPosition();
        }

        public void ApplyJumpDropOffset()
        {
            ApplyAdditionalDrop();
        }

        public void HeroJumpDropEnd()
        {
            ApplyAdditionalDrop();
        }

        public void LowerJumpHeight()
        {
            ApplyAdditionalDrop();
        }

        private void CacheCurrentLocalPosition()
        {
            if (targetTransform == null)
            {
                return;
            }

            if (dropRoutine != null)
            {
                StopCoroutine(dropRoutine);
                dropRoutine = null;
            }

            ResetDropOffset();
            cachedLocalPosition = targetTransform.localPosition;
            hasCachedLocalPosition = true;
            hasAppliedDrop = false;
        }

        private void ApplyAdditionalDrop()
        {
            if (targetTransform == null || hasAppliedDrop)
            {
                return;
            }

            if (!hasCachedLocalPosition)
            {
                CacheCurrentLocalPosition();
            }

            hasAppliedDrop = true;
            StartDropRoutine();
        }

        private void StartDropRoutine()
        {
            if (dropRoutine != null)
            {
                StopCoroutine(dropRoutine);
            }

            dropRoutine = StartCoroutine(ApplyAdditionalDropSmoothly());
        }

        private System.Collections.IEnumerator ApplyAdditionalDropSmoothly()
        {
            var startOffset = currentDropOffset;
            var endOffset = GetTargetDropOffset();
            var elapsed = 0f;

            while (elapsed < dropDuration)
            {
                elapsed += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(elapsed / dropDuration);
                currentDropOffset = LerpDropOffset(startOffset, endOffset, EvaluateDropProgress(normalizedTime));
                ApplyRenderedDropOffset();
                yield return null;
            }

            currentDropOffset = endOffset;
            ApplyRenderedDropOffset();
            dropRoutine = null;
        }

        private void ApplyRenderedDropOffset()
        {
            if (targetTransform == null || !hasCachedLocalPosition || currentDropOffset == Vector3.zero)
            {
                return;
            }

            targetTransform.localPosition = cachedLocalPosition + currentDropOffset;
        }

        private Vector3 GetTargetDropOffset()
        {
            var forwardDirection = Vector3.forward;
            if (targetTransform != null)
            {
                forwardDirection = targetTransform.parent != null
                    ? targetTransform.parent.InverseTransformDirection(targetTransform.forward)
                    : targetTransform.forward;
            }

            return forwardDirection.normalized * forwardDropDistance + Vector3.down * additionalDropHeight;
        }

        private Vector3 EvaluateDropProgress(float normalizedTime)
        {
            var forwardTime = forwardCurve != null ? forwardCurve.Evaluate(normalizedTime) : normalizedTime;
            var verticalTime = dropCurve != null ? dropCurve.Evaluate(normalizedTime) : normalizedTime;
            return new Vector3(forwardTime, verticalTime, forwardTime);
        }

        private static Vector3 LerpDropOffset(Vector3 startOffset, Vector3 endOffset, Vector3 progress)
        {
            return new Vector3(
                Mathf.LerpUnclamped(startOffset.x, endOffset.x, progress.x),
                Mathf.LerpUnclamped(startOffset.y, endOffset.y, progress.y),
                Mathf.LerpUnclamped(startOffset.z, endOffset.z, progress.z));
        }

        private void ResetDropOffset()
        {
            if (targetTransform != null && hasCachedLocalPosition && currentDropOffset != Vector3.zero)
            {
                targetTransform.localPosition = cachedLocalPosition;
            }

            currentDropOffset = Vector3.zero;
        }
    }
}
