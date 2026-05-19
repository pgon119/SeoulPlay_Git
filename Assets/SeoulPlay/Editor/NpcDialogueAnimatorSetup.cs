using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class NpcDialogueAnimatorSetup
{
    private const string ControllerPath = "Assets/SeoulPlay/Animator/Animator_Npc_Man_1.controller";
    private const string IdleStateName = "Holding Idle";
    private const string TurnStateName = "Left Turn";
    private const string IsTurningParameter = "IsTurning";
    private const string TurnPlaybackSpeedParameter = "TurnPlaybackSpeed";
    private const float TransitionDuration = 0.18f;

    static NpcDialogueAnimatorSetup()
    {
        EditorApplication.delayCall += ConfigureNpcManAnimator;
    }

    [MenuItem("SeoulPlay/Setup NPC Man Dialogue Animator")]
    public static void ConfigureNpcManAnimator()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null || controller.layers.Length == 0)
        {
            return;
        }

        EnsureParameter(controller, IsTurningParameter, AnimatorControllerParameterType.Bool, 0f);
        EnsureParameter(controller, TurnPlaybackSpeedParameter, AnimatorControllerParameterType.Float, 1f);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState idleState = FindState(stateMachine, IdleStateName);
        AnimatorState turnState = FindState(stateMachine, TurnStateName);

        if (idleState == null)
        {
            return;
        }

        if (turnState == null)
        {
            turnState = stateMachine.AddState(TurnStateName, new Vector3(310f, 300f, 0f));
        }

        AnimationClip turnClip = FindAnimationClip(TurnStateName);
        if (turnClip != null)
        {
            turnState.motion = turnClip;
        }

        turnState.speedParameterActive = true;
        turnState.speedParameter = TurnPlaybackSpeedParameter;

        EnsureBoolTransition(idleState, turnState, IsTurningParameter, true);
        EnsureBoolTransition(turnState, idleState, IsTurningParameter, false);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    private static void EnsureParameter(
        AnimatorController controller,
        string parameterName,
        AnimatorControllerParameterType parameterType,
        float defaultFloat)
    {
        if (controller.parameters.Any(parameter => parameter.name == parameterName))
        {
            return;
        }

        controller.AddParameter(new AnimatorControllerParameter
        {
            name = parameterName,
            type = parameterType,
            defaultFloat = defaultFloat
        });
    }

    private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
    {
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            if (childState.state != null && childState.state.name == stateName)
            {
                return childState.state;
            }
        }

        return null;
    }

    private static AnimationClip FindAnimationClip(string clipName)
    {
        string[] guids = AssetDatabase.FindAssets($"{clipName} t:AnimationClip", new[]
        {
            "Assets/SeoulPlay/Animaition/Npc",
            "Assets/SeoulPlay/Modeling/NPC/Man1"
        });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is AnimationClip clip && clip.name == clipName)
                {
                    return clip;
                }
            }
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is AnimationClip clip && clip.name.StartsWith(clipName))
                {
                    return clip;
                }
            }
        }

        return null;
    }

    private static void EnsureBoolTransition(
        AnimatorState source,
        AnimatorState destination,
        string parameterName,
        bool targetValue)
    {
        AnimatorConditionMode mode = targetValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot;

        AnimatorStateTransition transition = source.transitions.FirstOrDefault(existingTransition =>
            existingTransition.destinationState == destination
            && existingTransition.conditions.Length == 1
            && existingTransition.conditions[0].parameter == parameterName
            && existingTransition.conditions[0].mode == mode);

        if (transition == null)
        {
            transition = source.AddTransition(destination);
            transition.AddCondition(mode, 0f, parameterName);
        }

        transition.hasExitTime = false;
        transition.duration = TransitionDuration;
        transition.hasFixedDuration = true;
        transition.canTransitionToSelf = false;
    }
}
