using System.Collections;
using HeneGames.DialogueSystem;
using UnityEngine;

public class DialogueNpcTurnController : MonoBehaviour, IDialogueStartHandler
{
    [SerializeField] private Transform characterRoot;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform fallbackLookTarget;
    [SerializeField] private string turnStateName = "Left Turn";
    [SerializeField] private string isTurningParameterName = "IsTurning";
    [SerializeField] private string turnPlaybackSpeedParameterName = "TurnPlaybackSpeed";
    [SerializeField, Min(1f)] private float turnAnimationReferenceAngle = 180f;
    [SerializeField, Min(0.1f)] private float minTurnAnimationSpeed = 0.5f;
    [SerializeField, Min(0.1f)] private float maxTurnAnimationSpeed = 3f;
    [SerializeField, Min(0.1f)] private float conversationFacingAngle = 45f;
    [SerializeField, Min(0.1f)] private float maxTurnTime = 2f;
    [SerializeField] private bool disableAnimatorRootMotionDuringTurn;

    private bool defaultAnimatorApplyRootMotion;

    private void Awake()
    {
        if (characterRoot == null)
        {
            characterRoot = transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            defaultAnimatorApplyRootMotion = animator.applyRootMotion;
        }
    }

    public IEnumerator BeforeDialogueStarts(DialogueManager dialogueManager, DialogueTrigger dialogueTrigger)
    {
        Transform target = dialogueTrigger != null ? dialogueTrigger.transform : fallbackLookTarget;
        if (target == null)
        {
            yield break;
        }

        float startAngle = GetYawAngleToPosition(target.position);
        float turnDuration = PlayTurnAnimation(startAngle);
        yield return WaitUntilFacingTarget(target, conversationFacingAngle, turnDuration);
        PlayIdleAnimation();
        RestoreAnimatorDefaults();
    }

    public void AfterDialogueEnds(DialogueManager dialogueManager, DialogueTrigger dialogueTrigger)
    {
        RestoreAnimatorDefaults();
    }

    private IEnumerator WaitUntilFacingTarget(Transform target, float facingAngle, float turnDuration)
    {
        if (target == null || GetYawAngleToPosition(target.position) <= facingAngle)
        {
            yield break;
        }

        float elapsedTime = 0f;
        float duration = Mathf.Max(0.01f, turnDuration);

        while (elapsedTime < duration)
        {
            if (GetYawAngleToPosition(target.position) <= facingAngle)
            {
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private float GetYawAngleToPosition(Vector3 worldPosition)
    {
        Vector3 direction = worldPosition - characterRoot.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return 0f;
        }

        Vector3 forward = characterRoot.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= 0.0001f)
        {
            return 0f;
        }

        return Vector3.Angle(forward.normalized, direction.normalized);
    }

    private float PlayTurnAnimation(float turnAngle)
    {
        float clipDuration = GetTurnAnimationDuration();
        float playbackSpeed = GetTurnAnimationSpeed(turnAngle);

        if (animator != null && !string.IsNullOrWhiteSpace(turnStateName))
        {
            if (disableAnimatorRootMotionDuringTurn)
            {
                animator.applyRootMotion = false;
            }

            SetAnimatorFloat(turnPlaybackSpeedParameterName, playbackSpeed);
            SetAnimatorBool(isTurningParameterName, true);
        }

        return Mathf.Max(0.01f, clipDuration / playbackSpeed);
    }

    private void PlayIdleAnimation()
    {
        if (animator != null)
        {
            SetAnimatorBool(isTurningParameterName, false);
        }
    }

    private void RestoreAnimatorDefaults()
    {
        if (animator != null)
        {
            animator.applyRootMotion = defaultAnimatorApplyRootMotion;
            SetAnimatorFloat(turnPlaybackSpeedParameterName, 1f);
        }
    }

    private float GetTurnAnimationSpeed(float turnAngle)
    {
        if (turnAngle <= conversationFacingAngle)
        {
            return 1f;
        }

        float unclampedSpeed = turnAnimationReferenceAngle / Mathf.Max(1f, turnAngle);
        return Mathf.Clamp(unclampedSpeed, minTurnAnimationSpeed, maxTurnAnimationSpeed);
    }

    private float GetTurnAnimationDuration()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return maxTurnTime;
        }

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == turnStateName)
            {
                return Mathf.Max(0.01f, clip.length);
            }
        }

        return maxTurnTime;
    }

    private void SetAnimatorBool(string parameterName, bool value)
    {
        if (animator != null && HasAnimatorParameter(parameterName))
        {
            animator.SetBool(parameterName, value);
        }
    }

    private void SetAnimatorFloat(string parameterName, float value)
    {
        if (animator != null && HasAnimatorParameter(parameterName))
        {
            animator.SetFloat(parameterName, value);
        }
    }

    private bool HasAnimatorParameter(string parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName)
            {
                return true;
            }
        }

        return false;
    }
}
