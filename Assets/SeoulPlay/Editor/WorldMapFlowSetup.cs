#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SeoulPlay.Editor
{
    public static class WorldMapFlowSetup
    {
        private const string VideoScenePath = "Assets/SeoulPlay/Scenes/SeoulPlay_VideoScene/SeoulPlay_VideoScene_1.unity";
        private const string TitleScenePath = "Assets/SeoulPlay/Scenes/SeoulPlay_UI/SeoulPlay_Title.unity";
        private const string WorldMapScenePath = "Assets/SeoulPlay/Scenes/SeoulPlay_UI/SeoulPlay_WorldMap.unity";
        private const string UiPrefabFolder = "Assets/SeoulPlay/Prefab/UI";

        [MenuItem("SeoulPlay/Setup World Map Flow")]
        public static void Setup()
        {
            ConfigureVideoScene();
            ConfigureTitleScene();
            ConfigureWorldMapScene();
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("SeoulPlay world map flow setup completed.");
        }

        private static void ConfigureVideoScene()
        {
            var scene = EditorSceneManager.OpenScene(VideoScenePath, OpenSceneMode.Single);
            var controller = Object.FindObjectOfType<VideoCutsceneSceneController>();
            SetString(controller, "nextSceneName", "SeoulPlay_Title");
            EditorSceneManager.SaveScene(scene);
        }

        private static void ConfigureTitleScene()
        {
            var scene = EditorSceneManager.OpenScene(TitleScenePath, OpenSceneMode.Single);
            var loader = Object.FindObjectOfType<TimelineSceneLoader>();
            SetString(loader, "targetSceneName", "SeoulPlay_WorldMap");

            var director = loader != null ? loader.GetComponent<PlayableDirector>() : null;
            if (director != null)
            {
                director.enabled = false;
                EditorUtility.SetDirty(director);
            }

            var loaderData = new SerializedObject(loader);
            loaderData.FindProperty("evaluateFirstFrameOnStart").boolValue = false;
            loaderData.ApplyModifiedPropertiesWithoutUndo();

            var startButtonObject = GameObject.Find("Button_Start");
            var startButton = startButtonObject != null ? startButtonObject.GetComponent<Button>() : null;
            if (startButton == null)
            {
                throw new MissingReferenceException("Title Start button was not found.");
            }

            ClearPersistentCalls(startButton);
            UnityEventTools.AddPersistentListener(startButton.onClick, loader.LoadTargetScene);
            EditorUtility.SetDirty(startButton);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ConfigureWorldMapScene()
        {
            var scene = EditorSceneManager.OpenScene(WorldMapScenePath, OpenSceneMode.Single);
            var streetObject = GameObject.Find("Button_WorldMap_Icon_1");
            var comingSoonObject = GameObject.Find("Button_WorldMap_Icon_2") ?? GameObject.Find("Button_WorldMap_Icon_ComingSoon");
            if (streetObject == null || comingSoonObject == null)
            {
                throw new MissingReferenceException("World map icon buttons were not found.");
            }

            comingSoonObject.name = "Button_WorldMap_Icon_2";
            var streetButton = streetObject.GetComponent<Button>();
            var comingSoonButton = comingSoonObject.GetComponent<Button>();
            ClearPersistentCalls(streetButton);
            ClearPersistentCalls(comingSoonButton);

            ConfigureSelection(streetObject);
            ConfigureSelection(comingSoonObject);
            ConfigureExplicitNavigation(streetButton, comingSoonButton);

            Directory.CreateDirectory(UiPrefabFolder);
            streetObject = SaveAndConnect(streetObject, $"{UiPrefabFolder}/Button_WorldMap_Icon_1.prefab");
            comingSoonObject = SaveAndConnect(comingSoonObject, $"{UiPrefabFolder}/Button_WorldMap_Icon_2.prefab");
            streetButton = streetObject.GetComponent<Button>();
            comingSoonButton = comingSoonObject.GetComponent<Button>();
            ConfigureExplicitNavigation(streetButton, comingSoonButton);

            var canvas = streetObject.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                throw new MissingReferenceException("World map Canvas was not found.");
            }

            var popup = CreatePopupPrefab();
            var existingPopup = GameObject.Find("Popup_SystemNoti");
            if (existingPopup != null)
            {
                Object.DestroyImmediate(existingPopup);
            }

            var popupInstance = (GameObject)PrefabUtility.InstantiatePrefab(popup, canvas.transform);
            popupInstance.name = "Popup_SystemNoti";
            popupInstance.SetActive(false);

            var existingController = Object.FindObjectOfType<WorldMapController>();
            if (existingController == null)
            {
                existingController = new GameObject("WorldMapController").AddComponent<WorldMapController>();
            }

            existingController.Configure(streetButton, comingSoonButton, popupInstance.GetComponent<PopupSystemNoti>());
            EditorUtility.SetDirty(existingController);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ConfigureSelection(GameObject buttonObject)
        {
            var image = buttonObject.transform.Find("Image_Select");
            if (image == null)
            {
                throw new MissingReferenceException($"{buttonObject.name}/Image_Select was not found.");
            }

            var selection = buttonObject.GetComponent<WorldMapIconSelection>() ?? buttonObject.AddComponent<WorldMapIconSelection>();
            selection.Configure(image.gameObject);
            EditorUtility.SetDirty(selection);
        }

        private static void ConfigureExplicitNavigation(Button left, Button right)
        {
            var leftNavigation = left.navigation;
            leftNavigation.mode = Navigation.Mode.Explicit;
            leftNavigation.selectOnLeft = right;
            leftNavigation.selectOnRight = right;
            left.navigation = leftNavigation;

            var rightNavigation = right.navigation;
            rightNavigation.mode = Navigation.Mode.Explicit;
            rightNavigation.selectOnLeft = left;
            rightNavigation.selectOnRight = left;
            right.navigation = rightNavigation;
        }

        private static GameObject SaveAndConnect(GameObject instance, string path)
        {
            var prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(instance);
            if (prefabRoot != null)
            {
                PrefabUtility.UnpackPrefabInstance(prefabRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }

            PrefabUtility.SaveAsPrefabAssetAndConnect(instance, path, InteractionMode.AutomatedAction);
            return instance;
        }

        private static GameObject CreatePopupPrefab()
        {
            var root = CreateUiObject("Popup_SystemNoti", null);
            var rootRect = root.GetComponent<RectTransform>();
            Stretch(rootRect);
            var blocker = root.AddComponent<Image>();
            blocker.color = new Color(0f, 0f, 0f, 0.65f);

            var window = CreateUiObject("Panel_Window", root.transform);
            var windowRect = window.GetComponent<RectTransform>();
            windowRect.anchorMin = windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.sizeDelta = new Vector2(760f, 340f);
            window.GetComponent<CanvasRenderer>();
            var windowImage = window.AddComponent<Image>();
            windowImage.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);

            var messageObject = CreateUiObject("Text_Message", window.transform);
            var messageRect = messageObject.GetComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0.08f, 0.38f);
            messageRect.anchorMax = new Vector2(0.92f, 0.9f);
            messageRect.offsetMin = messageRect.offsetMax = Vector2.zero;
            var message = messageObject.AddComponent<TextMeshProUGUI>();
            message.text = "곧 업데이트될 예정입니다.";
            message.fontSize = 42f;
            message.alignment = TextAlignmentOptions.Center;
            message.color = Color.white;

            var buttonObject = CreateUiObject("Button_Confirm", window.transform);
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.2f);
            buttonRect.sizeDelta = new Vector2(240f, 80f);
            var buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.16f, 0.52f, 0.86f, 1f);
            var button = buttonObject.AddComponent<Button>();

            var labelObject = CreateUiObject("Text_Label", buttonObject.transform);
            Stretch(labelObject.GetComponent<RectTransform>());
            var label = labelObject.AddComponent<TextMeshProUGUI>();
            label.text = "확인";
            label.fontSize = 32f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;

            var popup = root.AddComponent<PopupSystemNoti>();
            popup.Configure(button);
            var path = $"{UiPrefabFolder}/Popup_SystemNoti.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            gameObject.layer = LayerMask.NameToLayer("UI");
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ClearPersistentCalls(Button button)
        {
            while (button.onClick.GetPersistentEventCount() > 0)
            {
                UnityEventTools.RemovePersistentListener(button.onClick, 0);
            }

            EditorUtility.SetDirty(button);
        }

        private static void SetString(Object target, string propertyName, string value)
        {
            if (target == null)
            {
                throw new MissingReferenceException($"Target for {propertyName} was not found.");
            }

            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureBuildSettings()
        {
            var desiredOrder = new[]
            {
                VideoScenePath,
                TitleScenePath,
                WorldMapScenePath,
                "Assets/SeoulPlay/Scenes/SeoulPlay_UI/SeoulPlay_Loading.unity",
                "Assets/SeoulPlay/Scenes/SeoulPlay_Street1/SeoulPlay_Street1.unity",
                "Assets/SeoulPlay/Scenes/SeoulPlay_BossBattle/SeoulPlay_BossBattle.unity"
            };

            var existing = EditorBuildSettings.scenes.ToDictionary(scene => scene.path, scene => scene);
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (var path in desiredOrder)
            {
                scenes.Add(existing.TryGetValue(path, out var scene)
                    ? new EditorBuildSettingsScene(path, scene.enabled)
                    : new EditorBuildSettingsScene(path, true));
            }

            scenes.AddRange(EditorBuildSettings.scenes.Where(scene => !desiredOrder.Contains(scene.path)));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
