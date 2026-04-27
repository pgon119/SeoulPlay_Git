using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SeoulPlay.Editor
{
    public static class InvectorBootstrap
    {
        private const string ProjectSettingsPackage =
            "Assets/Invector-3rdPersonController/Basic Locomotion/Editor/Resources/vProjectSettings.unitypackage";

        private const string PlayerPrefabPath =
            "Assets/Invector-3rdPersonController/Shooter/Prefabs/Player/vShooterMelee_NoInventory.prefab";

        private const string ScenePath = "Assets/Scenes/TPS_Invector_Test.unity";

        [MenuItem("SeoulPlay/Invector/Create TPS Test Scene")]
        public static void Run()
        {
            ImportInvectorProjectSettings();
            EnsureDefines("INVECTOR_BASIC", "INVECTOR_MELEE", "INVECTOR_SHOOTER");
            UrpMaterialConverter.FixInvectorMaterials();
            CreateTestScene();
        }

        private static void ImportInvectorProjectSettings()
        {
            AssetDatabase.ImportPackage(ProjectSettingsPackage, false);
            AssetDatabase.Refresh();
        }

        private static void EnsureDefines(params string[] requiredDefines)
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group)
                .Split(';')
                .Where(symbol => !string.IsNullOrWhiteSpace(symbol))
                .ToHashSet();

            foreach (var define in requiredDefines)
            {
                defines.Add(define);
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.OrderBy(symbol => symbol)));
        }

        private static void CreateTestScene()
        {
            CreateTestScene("TPS_Invector_Test", ScenePath);
        }

        private static void CreateTestScene(string sceneName, string scenePath)
        {
            DeleteIfExists("Assets/Scenes/TPS_Test_Ground.mat");
            for (var i = 0; i < 5; i++)
            {
                DeleteIfExists($"Assets/Scenes/TPS_Test_Target_{i + 1}.mat");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = sceneName;

            CreateLighting();
            CreateGround();
            CreateTargets();
            CreatePlayer();
            EnsureEventSystem();

            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void DeleteIfExists(string path)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

        private static void CreateLighting()
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.65f, 0.72f, 0.8f);
            RenderSettings.ambientEquatorColor = new Color(0.45f, 0.48f, 0.5f);
            RenderSettings.ambientGroundColor = new Color(0.24f, 0.25f, 0.24f);
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Test Ground";
            ground.transform.localScale = new Vector3(5f, 1f, 5f);

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
            {
                name = "TPS Test Ground"
            };
            material.color = new Color(0.28f, 0.34f, 0.3f);
            ground.GetComponent<Renderer>().sharedMaterial = material;

            AssetDatabase.CreateAsset(material, "Assets/Scenes/TPS_Test_Ground.mat");
        }

        private static void CreateTargets()
        {
            for (var i = 0; i < 5; i++)
            {
                var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                target.name = $"Target Dummy {i + 1}";
                target.transform.position = new Vector3(-8f + i * 4f, 1f, 12f);
                target.transform.localScale = new Vector3(0.9f, 1.4f, 0.9f);

                var renderer = target.GetComponent<Renderer>();
                renderer.sharedMaterial = CreateTargetMaterial(i);
            }
        }

        private static Material CreateTargetMaterial(int index)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
            {
                name = $"TPS Test Target {index + 1}"
            };
            material.color = index % 2 == 0 ? new Color(0.55f, 0.16f, 0.14f) : new Color(0.16f, 0.28f, 0.52f);
            AssetDatabase.CreateAsset(material, $"Assets/Scenes/TPS_Test_Target_{index + 1}.mat");
            return material;
        }

        private static void CreatePlayer()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                throw new System.IO.FileNotFoundException($"Invector player prefab was not found: {PlayerPrefabPath}");
            }

            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            player.name = "Player_Invector_TPS";
            player.transform.position = Vector3.zero;
            player.transform.rotation = Quaternion.identity;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
}
