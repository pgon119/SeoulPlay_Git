using Cinemachine;
using Invector.vCamera;
using Invector.vCharacterController;
using SeoulPlay.Cutscenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SeoulPlay.Editor
{
    public static class CutsceneSetupMenu
    {
        private const string RootName = "Cutscene System";
        private const string HandoffCameraName = "CM Handoff Camera";
        private const string ExampleCameraName = "CM Boss Cutscene Camera Example";
        private const string TimelinePath = "Assets/SeoulPlay/Cutscenes/Timelines/TPS_BossCutscene_Template.playable";

        [MenuItem("SeoulPlay/Cutscenes/Setup Current Scene")]
        public static void SetupCurrentScene()
        {
            var gameplayCamera = Object.FindObjectOfType<vThirdPersonCamera>(true);
            var mainCamera = FindSceneCamera(gameplayCamera);
            if (mainCamera == null)
            {
                Debug.LogError("Cutscene setup failed: no Camera component or vThirdPersonCamera was found in the open scene.");
                return;
            }

            if (!mainCamera.CompareTag("MainCamera") && TagExists("MainCamera"))
            {
                Undo.RecordObject(mainCamera.gameObject, "Tag gameplay camera as MainCamera");
                mainCamera.gameObject.tag = "MainCamera";
            }

            if (gameplayCamera == null)
            {
                gameplayCamera = mainCamera.GetComponent<vThirdPersonCamera>();
            }

            var playerInput = Object.FindObjectOfType<vThirdPersonInput>(true);

            var brain = mainCamera.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = Undo.AddComponent<CinemachineBrain>(mainCamera.gameObject);
            }
            brain.enabled = false;

            var root = GameObject.Find(RootName);
            if (root == null)
            {
                root = new GameObject(RootName);
                Undo.RegisterCreatedObjectUndo(root, "Create Cutscene System");
            }

            var bridge = root.GetComponent<CutsceneCameraBridge>();
            if (bridge == null)
            {
                bridge = Undo.AddComponent<CutsceneCameraBridge>(root);
            }

            var director = root.GetComponent<PlayableDirector>();
            if (director == null)
            {
                director = Undo.AddComponent<PlayableDirector>(root);
            }
            director.playOnAwake = false;
            director.extrapolationMode = DirectorWrapMode.None;

            var handoffCamera = GetOrCreateVirtualCamera(root.transform, HandoffCameraName, mainCamera, 0);
            var exampleCamera = GetOrCreateVirtualCamera(root.transform, ExampleCameraName, mainCamera, 10);
            exampleCamera.transform.position = mainCamera.transform.position + mainCamera.transform.forward * 5f + Vector3.up * 1.5f;
            exampleCamera.transform.rotation = Quaternion.LookRotation(mainCamera.transform.position - exampleCamera.transform.position, Vector3.up);

            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (timeline == null)
            {
                EnsureFolder("Assets/SeoulPlay/Cutscenes/Timelines");
                timeline = ScriptableObject.CreateInstance<TimelineAsset>();
                AssetDatabase.CreateAsset(timeline, TimelinePath);
                AssetDatabase.SaveAssets();
            }
            director.playableAsset = timeline;

            AssignBridgeReferences(bridge, mainCamera, gameplayCamera, playerInput, brain, handoffCamera, director);

            Selection.activeGameObject = root;
            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(mainCamera.gameObject);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("Cutscene setup complete. Add Cinemachine shots to the template Timeline, then call CutsceneCameraBridge.Play() from your boss/event trigger.");
        }

        [MenuItem("SeoulPlay/Cutscenes/Setup TPS_Invector_Test Scene")]
        public static void SetupTpsInvectorTestScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/TPS_Invector_Test.unity");
            SetupCurrentScene();
            EditorSceneManager.SaveOpenScenes();
        }

        private static CinemachineVirtualCamera GetOrCreateVirtualCamera(Transform parent, string name, Camera sourceCamera, int priority)
        {
            var child = parent.Find(name);
            GameObject cameraObject;
            if (child == null)
            {
                cameraObject = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(cameraObject, "Create Cinemachine Camera");
                cameraObject.transform.SetParent(parent);
            }
            else
            {
                cameraObject = child.gameObject;
            }

            cameraObject.transform.SetPositionAndRotation(sourceCamera.transform.position, sourceCamera.transform.rotation);

            var virtualCamera = cameraObject.GetComponent<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                virtualCamera = Undo.AddComponent<CinemachineVirtualCamera>(cameraObject);
            }

            virtualCamera.Priority = priority;
            virtualCamera.m_Lens.FieldOfView = sourceCamera.fieldOfView;
            virtualCamera.m_Lens.NearClipPlane = sourceCamera.nearClipPlane;
            virtualCamera.m_Lens.FarClipPlane = sourceCamera.farClipPlane;
            return virtualCamera;
        }

        private static Camera FindSceneCamera(vThirdPersonCamera gameplayCamera)
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                return mainCamera;
            }

            if (gameplayCamera != null)
            {
                var cameraOnGameplayObject = gameplayCamera.GetComponent<Camera>();
                if (cameraOnGameplayObject != null)
                {
                    return cameraOnGameplayObject;
                }
            }

            var cameras = Object.FindObjectsOfType<Camera>(true);
            if (cameras.Length == 1)
            {
                return cameras[0];
            }

            foreach (var camera in cameras)
            {
                if (camera.GetComponent<vThirdPersonCamera>() != null)
                {
                    return camera;
                }
            }

            foreach (var camera in cameras)
            {
                if (camera.cameraType == CameraType.Game)
                {
                    return camera;
                }
            }

            return cameras.Length > 0 ? cameras[0] : null;
        }

        private static bool TagExists(string tagName)
        {
            foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (tag == tagName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AssignBridgeReferences(
            CutsceneCameraBridge bridge,
            Camera mainCamera,
            vThirdPersonCamera gameplayCamera,
            vThirdPersonInput playerInput,
            CinemachineBrain brain,
            CinemachineVirtualCameraBase handoffCamera,
            PlayableDirector director)
        {
            var serializedBridge = new SerializedObject(bridge);
            serializedBridge.FindProperty("mainCamera").objectReferenceValue = mainCamera;
            serializedBridge.FindProperty("gameplayCamera").objectReferenceValue = gameplayCamera;
            serializedBridge.FindProperty("playerInput").objectReferenceValue = playerInput;
            serializedBridge.FindProperty("cinemachineBrain").objectReferenceValue = brain;
            serializedBridge.FindProperty("handoffCamera").objectReferenceValue = handoffCamera;
            serializedBridge.FindProperty("timelineDirector").objectReferenceValue = director;
            serializedBridge.ApplyModifiedProperties();
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
    }
}
