using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class CompanyLogoSceneController : MonoBehaviour
    {
        [Header("Logo")]
        [SerializeField] private Sprite logoSprite;
        [SerializeField, Min(1f)] private float logoWidth = 560f;

        [Header("Timing")]
        [SerializeField] private bool autoAdvance = true;
        [SerializeField, Min(0f)] private float displayDuration = 1.8f;
        [SerializeField, Min(0f)] private float fadeInDuration = 0.45f;
        [SerializeField, Min(0f)] private float fadeOutDuration = 0.45f;
        [SerializeField] private bool allowSkip = true;

        [Header("Scene Flow")]
        [SerializeField] private string nextSceneName = "SeoulPlay_VideoScene_1";

        [Header("Style")]
        [SerializeField] private Color backgroundColor = Color.black;

        private CanvasGroup canvasGroup;
        private bool isLoading;

        private IEnumerator Start()
        {
            BuildLogoView();

            yield return FadeCanvas(0f, 1f, fadeInDuration);

            if (!autoAdvance)
            {
                yield break;
            }

            if (displayDuration > 0f)
            {
                yield return new WaitForSeconds(displayDuration);
            }

            LoadNextScene();
        }

        private void Update()
        {
            if (!allowSkip || isLoading || !AnyInputDown())
            {
                return;
            }

            LoadNextScene();
        }

        private void BuildLogoView()
        {
            var canvasObject = new GameObject("Company Logo Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGroup = canvasObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            var backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            backgroundObject.transform.SetParent(canvasObject.transform, false);

            var backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            var backgroundImage = backgroundObject.GetComponent<Image>();
            backgroundImage.color = backgroundColor;

            if (logoSprite != null)
            {
                BuildLogoImage(canvasObject.transform);
            }
            else
            {
                BuildPlaceholderText(canvasObject.transform);
            }
        }

        private void BuildLogoImage(Transform parent)
        {
            var logoObject = new GameObject("Company Logo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            logoObject.transform.SetParent(parent, false);

            var logoImage = logoObject.GetComponent<Image>();
            logoImage.sprite = logoSprite;
            logoImage.preserveAspect = true;
            logoImage.color = Color.white;

            var logoRect = logoObject.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.pivot = new Vector2(0.5f, 0.5f);
            logoRect.anchoredPosition = Vector2.zero;

            var spriteSize = logoSprite.rect.size;
            var aspect = spriteSize.y > 0f ? spriteSize.x / spriteSize.y : 1f;
            logoRect.sizeDelta = new Vector2(logoWidth, logoWidth / Mathf.Max(0.01f, aspect));
        }

        private static void BuildPlaceholderText(Transform parent)
        {
            var textObject = new GameObject("Company Logo Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);

            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(720f, 160f);

            var text = textObject.GetComponent<Text>();
            text.text = "Company Logo";
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 64;
            text.color = Color.white;
            text.font = GetBuiltInFont();
        }

        private void LoadNextScene()
        {
            if (isLoading || string.IsNullOrWhiteSpace(nextSceneName))
            {
                return;
            }

            isLoading = true;
            StartCoroutine(LoadNextSceneRoutine());
        }

        private IEnumerator LoadNextSceneRoutine()
        {
            yield return FadeCanvas(canvasGroup.alpha, 0f, fadeOutDuration);
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }

        private IEnumerator FadeCanvas(float from, float to, float duration)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                canvasGroup.alpha = to;
                yield break;
            }

            for (var elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = to;
        }

        private static bool AnyInputDown()
        {
            if (Input.anyKeyDown)
            {
                return true;
            }

            for (var i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    return true;
                }
            }

            return false;
        }

        private static Font GetBuiltInFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
