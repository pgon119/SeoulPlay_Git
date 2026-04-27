using System.Collections.Generic;
using System.IO;
using System.Linq;
using SeoulPlay;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeoulPlay.Editor
{
    public static class SeoulPlayHeroSetup
    {
        private const string HeroModelPath = "Assets/SeoulPlay/Modeling/Hero/Hero_1.fbx";
        private const string BuildingPrefabPath = "Assets/SeoulPlay/Modeling/Background/Building.prefab";
        private const string AnimationFolder = "Assets/SeoulPlay/Animation";
        private const string Animation2Folder = "Assets/SeoulPlay/Animation2";
        private const string RollAnimationFolder = Animation2Folder + "/Roll";
        private const string GeneratedFolder = "Assets/SeoulPlay/Generated";
        private const string ControllerPath = GeneratedFolder + "/SeoulPlay_Hero.controller";
        private const string UpperBodyMaskPath = GeneratedFolder + "/SeoulPlay_UpperBody.mask";
        private const string PrefabPath = GeneratedFolder + "/SeoulPlay_Hero_Player.prefab";
        private const string ScenePath = "Assets/Scenes/SeoulPlay_Test.unity";
        private const float RollClipEndTrimFrames = 6f;

        private static readonly Dictionary<string, string> AnimationPaths = new()
        {
            ["IdleAim"] = Animation2Folder + "/idle aiming.fbx",
            ["WalkForward"] = Animation2Folder + "/walk forward.fbx",
            ["WalkBackward"] = Animation2Folder + "/walk backward.fbx",
            ["WalkForwardRight"] = Animation2Folder + "/walk forward right.fbx",
            ["WalkForwardLeft"] = Animation2Folder + "/walk forward left.fbx",
            ["WalkBackwardRight"] = Animation2Folder + "/walk backward right.fbx",
            ["WalkBackwardLeft"] = Animation2Folder + "/walk backward left.fbx",
            ["RunForward"] = Animation2Folder + "/run forward.fbx",
            ["RunBackward"] = Animation2Folder + "/run backward.fbx",
            ["Strafe"] = Animation2Folder + "/walk right.fbx",
            ["StrafeLeft"] = Animation2Folder + "/walk left.fbx",
            ["JumpForward"] = Animation2Folder + "/jump up.fbx",
            ["JumpBackward"] = Animation2Folder + "/jump down.fbx",
            ["RollForward"] = RollAnimationFolder + "/A_Roll_IdleFwd.fbx",
            ["RollBackward"] = RollAnimationFolder + "/A_Roll_IdleBwd.fbx",
            ["RollLeft"] = RollAnimationFolder + "/A_Roll_IdleFwdLt_90.fbx",
            ["RollRight"] = RollAnimationFolder + "/A_Roll_IdleFwdRt_90.fbx",
            ["Fire"] = AnimationFolder + "/firing rifle.fbx",
            ["Die"] = Animation2Folder + "/death from the front.fbx",
        };

        [MenuItem("SeoulPlay/Character/Rebuild Hero Test Setup")]
        public static void RebuildHeroTestSetup()
        {
            EnsureFolder(GeneratedFolder);
            var avatar = ConfigureHeroModel();
            ConfigureAnimationImports(avatar);

            var clips = LoadClips();
            var upperBodyMask = CreateUpperBodyMask();
            var controller = CreateAnimatorController(clips, upperBodyMask);
            CreateHeroPrefab(controller);
            CreateHeroTestScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SeoulPlay hero test setup rebuilt.");
        }

        private static Avatar ConfigureHeroModel()
        {
            var importer = AssetImporter.GetAtPath(HeroModelPath) as ModelImporter;
            if (importer == null)
            {
                throw new FileNotFoundException($"Hero model was not found: {HeroModelPath}");
            }

            var changed = false;
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.autoGenerateAvatarMappingIfUnspecified = true;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }

            var avatar = AssetDatabase.LoadAllAssetsAtPath(HeroModelPath).OfType<Avatar>().FirstOrDefault();
            if (avatar == null)
            {
                throw new FileNotFoundException($"Hero avatar was not generated: {HeroModelPath}");
            }

            return avatar;
        }

        private static void ConfigureAnimationImports(Avatar avatar)
        {
            foreach (var pair in AnimationPaths)
            {
                var importer = AssetImporter.GetAtPath(pair.Value) as ModelImporter;
                if (importer == null)
                {
                    Debug.LogWarning($"Animation model was not found: {pair.Value}");
                    continue;
                }

                importer.importAnimation = true;
                importer.animationType = ModelImporterAnimationType.Human;
                if (UsesOwnAvatar(pair.Key))
                {
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    importer.sourceAvatar = null;
                    importer.autoGenerateAvatarMappingIfUnspecified = true;
                }
                else
                {
                    importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                    importer.sourceAvatar = avatar;
                }

                var clips = importer.defaultClipAnimations;
                var clip = SelectPrimaryClip(pair.Key, clips);
                if (clip != null)
                {
                    clip.name = pair.Key;
                    clip.loopTime = IsLoopingClip(pair.Key);
                    clip.keepOriginalOrientation = !UsesOwnAvatar(pair.Key);
                    clip.keepOriginalPositionY = true;
                    clip.keepOriginalPositionXZ = !UsesOwnAvatar(pair.Key);
                    if (UsesOwnAvatar(pair.Key))
                    {
                        TrimRollClipEnd(clip);
                    }
                    importer.clipAnimations = new[] { clip };
                }

                importer.SaveAndReimport();
            }
        }

        private static Dictionary<string, AnimationClip> LoadClips()
        {
            var clips = new Dictionary<string, AnimationClip>();
            foreach (var pair in AnimationPaths)
            {
                var clip = AssetDatabase.LoadAllAssetsAtPath(pair.Value)
                    .OfType<AnimationClip>()
                    .Where(candidate => !candidate.name.StartsWith("__preview__", System.StringComparison.Ordinal))
                    .OrderByDescending(candidate => candidate.name == pair.Key)
                    .ThenByDescending(candidate => candidate.length)
                    .FirstOrDefault() ??
                    AssetDatabase.LoadAllAssetsAtPath(pair.Value)
                        .OfType<AnimationClip>()
                        .FirstOrDefault(candidate => !candidate.name.StartsWith("__preview__", System.StringComparison.Ordinal));

                if (clip == null)
                {
                    Debug.LogWarning($"Animation clip was not found: {pair.Value}");
                    continue;
                }

                clips[pair.Key] = clip;
            }

            return clips;
        }

        private static ModelImporterClipAnimation SelectPrimaryClip(
            string clipName,
            IReadOnlyList<ModelImporterClipAnimation> clips)
        {
            if (clips.Count == 0)
            {
                return null;
            }

            return clips
                .OrderByDescending(clip => IsPreferredRollClip(clipName, clip))
                .ThenBy(clip => IsTPoseClip(clip))
                .ThenByDescending(clip => clip.lastFrame - clip.firstFrame)
                .First();
        }

        private static bool IsPreferredRollClip(string clipName, ModelImporterClipAnimation clip)
        {
            if (!UsesOwnAvatar(clipName))
            {
                return false;
            }

            return clip.name.Contains("human_male", System.StringComparison.OrdinalIgnoreCase) ||
                clip.takeName.Contains("human_male", System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTPoseClip(ModelImporterClipAnimation clip)
        {
            return clip.name.Contains("tpose", System.StringComparison.OrdinalIgnoreCase) ||
                clip.takeName.Contains("tpose", System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool UsesOwnAvatar(string clipName)
        {
            return clipName is "RollForward" or "RollBackward" or "RollLeft" or "RollRight";
        }

        private static void TrimRollClipEnd(ModelImporterClipAnimation clip)
        {
            var clipFrames = clip.lastFrame - clip.firstFrame;
            if (clipFrames > RollClipEndTrimFrames + 1f)
            {
                clip.lastFrame -= RollClipEndTrimFrames;
            }
        }

        private static AvatarMask CreateUpperBodyMask()
        {
            var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(UpperBodyMaskPath);
            if (mask == null)
            {
                mask = new AvatarMask
                {
                    name = "SeoulPlay_UpperBody"
                };
                AssetDatabase.CreateAsset(mask, UpperBodyMaskPath);
            }

            foreach (AvatarMaskBodyPart part in System.Enum.GetValues(typeof(AvatarMaskBodyPart)))
            {
                mask.SetHumanoidBodyPartActive(part, false);
            }

            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Head, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftHandIK, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightHandIK, true);
            EditorUtility.SetDirty(mask);
            return mask;
        }

        private static AnimatorController CreateAnimatorController(
            IReadOnlyDictionary<string, AnimationClip> clips,
            AvatarMask upperBodyMask)
        {
            AssetDatabase.DeleteAsset(ControllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveZ", AnimatorControllerParameterType.Float);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Aim", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Fire", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Rolling", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

            var stateMachine = controller.layers[0].stateMachine;
            stateMachine.states = System.Array.Empty<ChildAnimatorState>();

            var locomotion = stateMachine.AddState("Locomotion");
            locomotion.motion = CreateLocomotionBlendTree(controller, clips);
            stateMachine.defaultState = locomotion;

            var rollForward = AddStateIfClipExists(stateMachine, "Roll Forward", clips, "RollForward", new Vector3(300f, -160f, 0f));
            var rollBackward = AddStateIfClipExists(stateMachine, "Roll Backward", clips, "RollBackward", new Vector3(300f, -80f, 0f));
            var rollLeft = AddStateIfClipExists(stateMachine, "Roll Left", clips, "RollLeft", new Vector3(300f, -240f, 0f));
            var rollRight = AddStateIfClipExists(stateMachine, "Roll Right", clips, "RollRight", new Vector3(300f, -320f, 0f));
            var die = AddStateIfClipExists(stateMachine, "Die", clips, "Die", new Vector3(300f, 80f, 0f));

            AddRollExit(locomotion, rollForward);
            AddRollExit(locomotion, rollBackward);
            AddRollExit(locomotion, rollLeft);
            AddRollExit(locomotion, rollRight);

            if (die != null)
            {
                var transition = stateMachine.AddAnyStateTransition(die);
                transition.AddCondition(AnimatorConditionMode.If, 0f, "Die");
                transition.duration = 0.1f;
                transition.canTransitionToSelf = false;
            }

            AddUpperBodyFireLayer(controller, clips, upperBodyMask);
            return controller;
        }

        private static void AddUpperBodyFireLayer(
            AnimatorController controller,
            IReadOnlyDictionary<string, AnimationClip> clips,
            AvatarMask upperBodyMask)
        {
            if (!clips.TryGetValue("Fire", out var fireClip))
            {
                Debug.LogWarning("Fire animation clip was not found. Upper Body Fire layer was not created.");
                return;
            }

            var stateMachine = new AnimatorStateMachine
            {
                name = "Upper Body Fire"
            };
            AssetDatabase.AddObjectToAsset(stateMachine, controller);

            var idle = stateMachine.AddState("Upper Body Empty", new Vector3(200f, 0f, 0f));
            stateMachine.defaultState = idle;

            var fire = stateMachine.AddState("Fire", new Vector3(200f, 100f, 0f));
            fire.motion = fireClip;
            fire.writeDefaultValues = false;

            var fireTransition = stateMachine.AddAnyStateTransition(fire);
            fireTransition.AddCondition(AnimatorConditionMode.If, 0f, "Fire");
            fireTransition.duration = 0.04f;
            fireTransition.canTransitionToSelf = false;

            var exitTransition = fire.AddTransition(idle);
            exitTransition.hasExitTime = true;
            exitTransition.exitTime = 0.9f;
            exitTransition.duration = 0.08f;

            controller.AddLayer(new AnimatorControllerLayer
            {
                name = "Upper Body Fire",
                stateMachine = stateMachine,
                avatarMask = upperBodyMask,
                defaultWeight = 0f,
                blendingMode = AnimatorLayerBlendingMode.Override,
                iKPass = false
            });
        }

        private static BlendTree CreateLocomotionBlendTree(
            AnimatorController controller,
            IReadOnlyDictionary<string, AnimationClip> clips)
        {
            var blendTree = new BlendTree
            {
                name = "Locomotion Blend Tree",
                blendType = BlendTreeType.FreeformDirectional2D,
                blendParameter = "MoveX",
                blendParameterY = "MoveZ",
                useAutomaticThresholds = false
            };

            AssetDatabase.AddObjectToAsset(blendTree, controller);
            AddChild(blendTree, clips, "IdleAim", Vector2.zero);
            AddChild(blendTree, clips, "WalkForward", new Vector2(0f, 0.5f));
            AddChild(blendTree, clips, "WalkBackward", new Vector2(0f, -0.5f));
            AddChild(blendTree, clips, "WalkForwardRight", new Vector2(0.5f, 0.5f));
            AddChild(blendTree, clips, "WalkForwardLeft", new Vector2(-0.5f, 0.5f));
            AddChild(blendTree, clips, "WalkBackwardRight", new Vector2(0.5f, -0.5f));
            AddChild(blendTree, clips, "WalkBackwardLeft", new Vector2(-0.5f, -0.5f));
            AddChild(blendTree, clips, "RunForward", new Vector2(0f, 1f));
            AddChild(blendTree, clips, "RunBackward", new Vector2(0f, -1f));
            AddChild(blendTree, clips, "Strafe", new Vector2(1f, 0f));
            AddChild(blendTree, clips, "StrafeLeft", new Vector2(-1f, 0f));
            return blendTree;
        }

        private static AnimatorState AddStateIfClipExists(
            AnimatorStateMachine stateMachine,
            string stateName,
            IReadOnlyDictionary<string, AnimationClip> clips,
            string clipName,
            Vector3 position)
        {
            if (!clips.TryGetValue(clipName, out var clip))
            {
                return null;
            }

            var state = stateMachine.AddState(stateName, position);
            state.motion = clip;
            state.writeDefaultValues = false;
            return state;
        }

        private static void AddRollExit(
            AnimatorState locomotion,
            AnimatorState rollState)
        {
            if (rollState == null)
            {
                return;
            }

            var exit = rollState.AddTransition(locomotion);
            exit.hasExitTime = true;
            exit.exitTime = 0.82f;
            exit.duration = 0.12f;
        }

        private static void AddChild(
            BlendTree blendTree,
            IReadOnlyDictionary<string, AnimationClip> clips,
            string clipName,
            Vector2 position,
            bool mirror = false)
        {
            if (clips.TryGetValue(clipName, out var clip))
            {
                blendTree.AddChild(clip, position);
                if (mirror)
                {
                    var children = blendTree.children;
                    children[^1].mirror = true;
                    blendTree.children = children;
                }
            }
        }

        private static void CreateHeroPrefab(RuntimeAnimatorController controller)
        {
            var heroModel = AssetDatabase.LoadAssetAtPath<GameObject>(HeroModelPath);
            if (heroModel == null)
            {
                throw new FileNotFoundException($"Hero model was not found: {HeroModelPath}");
            }

            var root = new GameObject("SeoulPlay_Hero_Player");
            root.tag = TagExists("Player") ? "Player" : "Untagged";
            var characterController = root.AddComponent<CharacterController>();
            characterController.center = new Vector3(0f, 0.95f, 0f);
            characterController.height = 1.9f;
            characterController.radius = 0.3f;

            var mover = root.AddComponent<SimpleHeroMover>();
            var model = (GameObject)PrefabUtility.InstantiatePrefab(heroModel, root.transform);
            model.name = "Hero_1_Model";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            var animator = model.GetComponent<Animator>();
            if (animator == null)
            {
                animator = model.AddComponent<Animator>();
            }
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            var rootMotionRelay = model.GetComponent<AnimatorRootMotionRelay>();
            if (rootMotionRelay == null)
            {
                rootMotionRelay = model.AddComponent<AnimatorRootMotionRelay>();
            }

            var serializedMover = new SerializedObject(mover);
            serializedMover.FindProperty("animator").objectReferenceValue = animator;
            serializedMover.FindProperty("modelRoot").objectReferenceValue = model.transform;
            serializedMover.ApplyModifiedPropertiesWithoutUndo();

            var serializedRelay = new SerializedObject(rootMotionRelay);
            serializedRelay.FindProperty("mover").objectReferenceValue = mover;
            serializedRelay.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void CreateHeroTestScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "SeoulPlay_Test";

            CreatePlane();
            CreateBuilding();
            CreateLight();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            player.transform.position = Vector3.zero;

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = TagExists("MainCamera") ? "MainCamera" : "Untagged";
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            camera.transform.SetPositionAndRotation(new Vector3(0f, 2.2f, -5f), Quaternion.Euler(15f, 0f, 0f));

            var mover = player.GetComponent<SimpleHeroMover>();
            var serializedMover = new SerializedObject(mover);
            serializedMover.FindProperty("followCamera").objectReferenceValue = camera;
            serializedMover.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void CreatePlane()
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Test Plane";
            plane.transform.localScale = new Vector3(5f, 1f, 5f);
        }

        private static void CreateBuilding()
        {
            var buildingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BuildingPrefabPath);
            if (buildingPrefab == null)
            {
                Debug.LogWarning($"Building prefab was not found: {BuildingPrefabPath}");
                return;
            }

            var building = (GameObject)PrefabUtility.InstantiatePrefab(buildingPrefab);
            building.name = "Building";
            building.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            building.transform.localScale = Vector3.one;
        }

        private static void CreateLight()
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static bool IsLoopingClip(string name)
        {
            return name is "IdleAim" or "WalkForward" or "WalkBackward" or "WalkForwardRight" or "WalkForwardLeft"
                or "WalkBackwardRight" or "WalkBackwardLeft" or "RunForward" or "RunBackward" or "Strafe" or "StrafeLeft";
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

        private static bool TagExists(string tagName)
        {
            return UnityEditorInternal.InternalEditorUtility.tags.Contains(tagName);
        }
    }
}
