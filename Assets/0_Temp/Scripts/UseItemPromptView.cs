using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UseItemPromptView : MonoBehaviour
{
    private void Awake()
    {
        RectTransform root = GetComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 0.16f);
        root.anchorMax = new Vector2(0.5f, 0.16f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = new Vector2(360f, 82f);

        Image background = GetComponent<Image>();
        if (background == null)
        {
            background = gameObject.AddComponent<Image>();
        }

        background.color = new Color(0.04f, 0.04f, 0.04f, 0.88f);
        background.raycastTarget = false;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObject.transform.SetParent(transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(16f, 8f);
        labelRect.offsetMax = new Vector2(-16f, -8f);

        Text label = labelObject.GetComponent<Text>();
        label.text = "A로 사용하기";
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = 31;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
    }
}
