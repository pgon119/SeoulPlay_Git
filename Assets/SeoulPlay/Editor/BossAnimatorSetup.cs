using System.IO;
using System.Linq;
using SeoulPlay;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace SeoulPlay.Editor
{
    public static class BossAnimatorSetup
    {
        private const string BossPrefabPath = "Assets/SeoulPlay/Prefab/Monster_Boss_1.prefab";
        private const string BossModelPath = "Assets/SeoulPlay/Modeling/Monster/Boss_Monster/Monster_Boss_1.fbx";
        private const string IdleClipPath = "Assets/SeoulPlay/Animaition/Monster_Boss_1/Monster_Boss_1_Idle_1.fbx";
        private const string GeneratedFolder = "Assets/SeoulPlay/Generated/Boss";
        private const string ControllerPath = GeneratedFolder + "/Monster_Boss_1_BossAnimator.controller";
        private const string PlaceholderFolder = GeneratedFolder + "/PlaceholderClips";
        private const string PlaceholderAnchorName = "AnimationPlaceholderAnchor";
        private const string AutoBuildEditorPrefKey = "SeoulPlay.BossAnimatorSetup.AutoBuilt.Monster_Boss_1.v4";
        private const float BossMaxHealth = 100f;

        [InitializeOnLoadMethod]
        private static void AutoBuildOnceAfterCompile()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorPrefs.GetBool(AutoBuildEditorPrefKey, false) &&
                    AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
                {
                    return;
                }

                if (AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath) == null)
                {
                    return;
                }

                RebuildMonsterBoss1Animator();
                EditorPrefs.SetBool(AutoBuildEditorPrefKey, true);
            };
        }

        [MenuItem("SeoulPlay/Boss/Rebuild Monster Boss 1 Animator")]
        public static void RebuildMonsterBoss1Animator()
        {
            EnsureFolder(GeneratedFolder);
            EnsureFolder(PlaceholderFolder);
            ConfigureBossModelImports();

            var idleClip = LoadPrimaryClip(IdleClipPath);
            var clips = CreatePlaceholderClips();
            var controller = CreateController(idleClip, clips);
            var avatar = LoadPrimaryAvatar(BossModelPath) ?? LoadPrimaryAvatar(IdleClipPath);
            ApplyToBossPrefab(controller, avatar);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Monster_Boss_1 animator controller rebuilt and assigned.");
        }

        private static AnimatorController CreateController(AnimationClip idleClip, BossClips clips)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }

            ResetController(controller);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Attack01", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Attack02", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Attack03", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Stagger", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Enrage", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);

            var stateMachine = controller.layers[0].stateMachine;

            var idle = stateMachine.AddState("Idle", new Vector3(0f, 0f, 0f));
            idle.motion = idleClip != null ? idleClip : clips.IdleFallback;
            idle.writeDefaultValues = false;
            stateMachine.defaultState = idle;

            var move = stateMachine.AddState("Move", new Vector3(240f, 0f, 0f));
            move.motion = CreateMoveBlendTree(controller, idle.motion, clips.Move);
            move.writeDefaultValues = false;

            var attack01 = AddState(stateMachine, "Attack_01", clips.Attack01, new Vector3(240f, -160f, 0f));
            var attack02 = AddState(stateMachine, "Attack_02", clips.Attack02, new Vector3(480f, -160f, 0f));
            var attack03 = AddState(stateMachine, "Attack_03", clips.Attack03, new Vector3(720f, -160f, 0f));
            var hit = AddState(stateMachine, "Hit", clips.Hit, new Vector3(240f, 160f, 0f));
            var stagger = AddState(stateMachine, "Stagger", clips.Stagger, new Vector3(480f, 160f, 0f));
            var enrage = AddState(stateMachine, "Enrage", clips.Enrage, new Vector3(720f, 160f, 0f));
            var death = AddState(stateMachine, "Death", clips.Death, new Vector3(960f, 0f, 0f));

            AddIdleMoveTransitions(idle, move);
            AddAnyStateTrigger(stateMachine, death, "Death", 0f, true);
            AddAnyStateTrigger(stateMachine, hit, "Hit", 0.03f, false);
            AddAnyStateTrigger(stateMachine, stagger, "Stagger", 0.05f, false);
            AddAnyStateTrigger(stateMachine, enrage, "Enrage", 0.08f, false);
            AddAnyStateTrigger(stateMachine, attack01, "Attack01", 0.08f, false);
            AddAnyStateTrigger(stateMachine, attack02, "Attack02", 0.08f, false);
            AddAnyStateTrigger(stateMachine, attack03, "Attack03", 0.08f, false);

            AddReturnTransitions(attack01, idle, move, 0.9f);
            AddReturnTransitions(attack02, idle, move, 0.92f);
            AddReturnTransitions(attack03, idle, move, 0.92f);
            AddReturnTransitions(hit, idle, move, 0.85f);
            AddReturnTransitions(stagger, idle, move, 0.9f);
            AddReturnTransitions(enrage, idle, move, 0.95f);

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void ResetController(AnimatorController controller)
        {
            controller.parameters = System.Array.Empty<AnimatorControllerParameter>();

            var layers = controller.layers;
            if (layers == null || layers.Length == 0 || layers[0].stateMachine == null)
            {
                var stateMachine = new AnimatorStateMachine { name = "Base Layer" };
                AssetDatabase.AddObjectToAsset(stateMachine, controller);
                controller.layers = new[]
                {
                    new AnimatorControllerLayer
                    {
                        name = "Base Layer",
                        defaultWeight = 1f,
                        stateMachine = stateMachine
                    }
                };
                return;
            }

            var baseLayer = layers[0];
            baseLayer.defaultWeight = 1f;
            layers[0] = baseLayer;
            controller.layers = layers;

            var stateMachineToReset = controller.layers[0].stateMachine;
            stateMachineToReset.states = System.Array.Empty<ChildAnimatorState>();
            stateMachineToReset.anyStateTransitions = System.Array.Empty<AnimatorStateTransition>();
            stateMachineToReset.entryTransitions = System.Array.Empty<AnimatorTransition>();
            stateMachineToReset.stateMachines = System.Array.Empty<ChildAnimatorStateMachine>();
            EditorUtility.SetDirty(stateMachineToReset);
        }

        private static BlendTree CreateMoveBlendTree(
            AnimatorController controller,
            Motion idleMotion,
            AnimationClip moveClip)
        {
            var blendTree = new BlendTree
            {
                name = "Move Blend Tree",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "MoveSpeed",
                useAutomaticThresholds = false
            };
            AssetDatabase.AddObjectToAsset(blendTree, controller);
            if (idleMotion != null)
            {
                blendTree.AddChild(idleMotion, 0f);
            }
            blendTree.AddChild(moveClip, 1f);
            return blendTree;
        }

        private static AnimatorState AddState(
            AnimatorStateMachine stateMachine,
            string name,
            Motion motion,
            Vector3 position)
        {
            var state = stateMachine.AddState(name, position);
            state.motion = motion;
            state.writeDefaultValues = false;
            return state;
        }

        private static void AddIdleMoveTransitions(AnimatorState idle, AnimatorState move)
        {
            var idleToMove = idle.AddTransition(move);
            idleToMove.hasExitTime = false;
            idleToMove.duration = 0.12f;
            idleToMove.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");

            var idleToMoveBySpeed = idle.AddTransition(move);
            idleToMoveBySpeed.hasExitTime = false;
            idleToMoveBySpeed.duration = 0.12f;
            idleToMoveBySpeed.AddCondition(AnimatorConditionMode.Greater, 0.05f, "MoveSpeed");

            var moveToIdle = move.AddTransition(idle);
            moveToIdle.hasExitTime = false;
            moveToIdle.duration = 0.12f;
            moveToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");
            moveToIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "MoveSpeed");
        }

        private static void AddAnyStateTrigger(
            AnimatorStateMachine stateMachine,
            AnimatorState target,
            string trigger,
            float duration,
            bool canInterrupt)
        {
            var transition = stateMachine.AddAnyStateTransition(target);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.canTransitionToSelf = false;
            transition.orderedInterruption = true;
            transition.interruptionSource = canInterrupt
                ? TransitionInterruptionSource.SourceThenDestination
                : TransitionInterruptionSource.None;
            transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
        }

        private static void AddReturnTransitions(
            AnimatorState from,
            AnimatorState idle,
            AnimatorState move,
            float exitTime)
        {
            var toMove = from.AddTransition(move);
            toMove.hasExitTime = true;
            toMove.exitTime = exitTime;
            toMove.duration = 0.12f;
            toMove.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");

            var toIdle = from.AddTransition(idle);
            toIdle.hasExitTime = true;
            toIdle.exitTime = exitTime;
            toIdle.duration = 0.12f;
        }

        private static BossClips CreatePlaceholderClips()
        {
            return new BossClips
            {
                IdleFallback = CreateClip("Boss_Idle_Placeholder", 1f, true),
                Move = CreateClip("Boss_Move_Placeholder", 1f, true),
                Attack01 = CreateClip("Boss_Attack_01_Placeholder", 1.2f, false, ("Attack01_Hit", 0.45f)),
                Attack02 = CreateClip("Boss_Attack_02_Placeholder", 1.8f, false, ("Attack02_Hit", 0.95f)),
                Attack03 = CreateClip("Boss_Attack_03_Placeholder", 2f, false, ("AttackSignal", 0.35f), ("Attack03_Hit", 1.1f)),
                Hit = CreateClip("Boss_Hit_Placeholder", 0.45f, false),
                Stagger = CreateClip("Boss_Stagger_Placeholder", 1.2f, false),
                Enrage = CreateClip("Boss_Enrage_Placeholder", 1.6f, false, ("Enrage_Start", 0.1f)),
                Death = CreateClip("Boss_Death_Placeholder", 2f, false)
            };
        }

        private static AnimationClip CreateClip(
            string name,
            float length,
            bool loop,
            params (string functionName, float time)[] events)
        {
            var path = $"{PlaceholderFolder}/{name}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip { name = name };
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.ClearCurves();
            clip.frameRate = 30f;
            var curve = AnimationCurve.Linear(0f, 1f, Mathf.Max(0.01f, length), 1f);
            clip.SetCurve(PlaceholderAnchorName, typeof(Transform), "m_LocalScale.x", curve);

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AnimationUtility.SetAnimationEvents(
                clip,
                events.Select(item => new AnimationEvent
                {
                    functionName = item.functionName,
                    time = item.time
                }).ToArray());
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimationClip LoadPrimaryClip(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__preview__", System.StringComparison.Ordinal))
                .OrderByDescending(clip => clip.name == "Monster_Boss_1_Idle_1")
                .ThenByDescending(clip => clip.length)
                .FirstOrDefault();
        }

        private static Avatar LoadPrimaryAvatar(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Avatar>(path) ??
                AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Avatar>()
                .FirstOrDefault();
        }

        private static void ConfigureBossModelImports()
        {
            ConfigureGenericModelImporter(BossModelPath, false);
            ConfigureGenericModelImporter(IdleClipPath, true);
        }

        private static void ConfigureGenericModelImporter(string path, bool loopImportedClips)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                return;
            }

            var changed = false;
            if (importer.animationType != ModelImporterAnimationType.Generic)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                changed = true;
            }

            if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                changed = true;
            }

            if (!importer.importAnimation)
            {
                importer.importAnimation = true;
                changed = true;
            }

            if (loopImportedClips)
            {
                var clipAnimations = importer.defaultClipAnimations;
                for (var i = 0; i < clipAnimations.Length; i++)
                {
                    if (!clipAnimations[i].loopTime)
                    {
                        clipAnimations[i].loopTime = true;
                        changed = true;
                    }
                }
                importer.clipAnimations = clipAnimations;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void ApplyToBossPrefab(RuntimeAnimatorController controller, Avatar avatar)
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(BossPrefabPath);
            try
            {
                var animator = prefabRoot.GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    animator = prefabRoot.AddComponent<Animator>();
                }
                animator.runtimeAnimatorController = controller;
                animator.avatar = avatar;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                var receiver = prefabRoot.GetComponentInChildren<BossAnimationEventReceiver>();
                if (receiver == null)
                {
                    animator.gameObject.AddComponent<BossAnimationEventReceiver>();
                }

                if (animator.transform.Find(PlaceholderAnchorName) == null)
                {
                    var anchor = new GameObject(PlaceholderAnchorName);
                    anchor.hideFlags = HideFlags.HideInHierarchy;
                    anchor.transform.SetParent(animator.transform, false);
                }

                var damageable = prefabRoot.GetComponent<SeoulPlayDamageable>();
                if (damageable != null)
                {
                    var serializedDamageable = new SerializedObject(damageable);
                    serializedDamageable.FindProperty("animator").objectReferenceValue = animator;
                    serializedDamageable.FindProperty("maxHealth").floatValue = BossMaxHealth;
                    serializedDamageable.FindProperty("currentHealth").floatValue = BossMaxHealth;
                    serializedDamageable.FindProperty("playHitReaction").boolValue = false;
                    serializedDamageable.FindProperty("hitTrigger").stringValue = "Hit";
                    serializedDamageable.FindProperty("deathTrigger").stringValue = "Death";
                    serializedDamageable.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, BossPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            var parts = folderPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private struct BossClips
        {
            public AnimationClip IdleFallback;
            public AnimationClip Move;
            public AnimationClip Attack01;
            public AnimationClip Attack02;
            public AnimationClip Attack03;
            public AnimationClip Hit;
            public AnimationClip Stagger;
            public AnimationClip Enrage;
            public AnimationClip Death;
        }
    }
}
