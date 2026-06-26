using UnityEngine;

namespace SeoulPlay
{
    public sealed class SeoulPlayCrosshairUI : MonoBehaviour
    {
        [SerializeField] private bool visible = true;
        [SerializeField, Min(1f)] private float length = 9f;
        [SerializeField, Min(1f)] private float gap = 5f;
        [SerializeField, Min(1f)] private float thickness = 2f;
        [SerializeField] private Color color = Color.white;
        [SerializeField, Range(0f, 0.45f)] private float maxViewportOffset = 0.12f;
        [SerializeField, Min(0f)] private float gamepadAimSpeed = 0.35f;
        [SerializeField, Min(0f)] private float recenterSpeed = 1.8f;
        [SerializeField, Min(0f)] private float gamepadDeadZone = 0.2f;
        [SerializeField] private bool gamepadControlsCrosshair = true;
        [SerializeField] private bool mouseControlsCrosshair;

        private Texture2D pixel;
        private Vector2 viewportOffset;

        public Vector3 ViewportPoint => new(0.5f + viewportOffset.x, 0.5f + viewportOffset.y, 0f);

        private void Awake()
        {
            pixel = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            pixel.SetPixel(0, 0, Color.white);
            pixel.Apply();
        }

        private void Update()
        {
            if (mouseControlsCrosshair && Input.mousePresent)
            {
                viewportOffset = new Vector2(
                    Mathf.Clamp01(Input.mousePosition.x / Mathf.Max(1f, Screen.width)) - 0.5f,
                    Mathf.Clamp01(Input.mousePosition.y / Mathf.Max(1f, Screen.height)) - 0.5f);
                viewportOffset = Vector2.ClampMagnitude(viewportOffset, maxViewportOffset);
                return;
            }

            if (!gamepadControlsCrosshair)
            {
                viewportOffset = Vector2.zero;
                return;
            }

            var lookInput = new Vector2(
                Input.GetAxisRaw("RightAnalogHorizontal"),
                Input.GetAxisRaw("RightAnalogVertical"));

            if (lookInput.magnitude <= gamepadDeadZone)
            {
                viewportOffset = Vector2.MoveTowards(
                    viewportOffset,
                    Vector2.zero,
                    recenterSpeed * Time.deltaTime);
                return;
            }

            viewportOffset += lookInput * gamepadAimSpeed * Time.deltaTime;
            viewportOffset = Vector2.MoveTowards(
                viewportOffset,
                Vector2.zero,
                recenterSpeed * Time.deltaTime);
            viewportOffset = Vector2.ClampMagnitude(viewportOffset, maxViewportOffset);
        }

        private void OnGUI()
        {
            if (!visible || pixel == null)
            {
                return;
            }

            var viewportPoint = ViewportPoint;
            var center = new Vector2(Screen.width * viewportPoint.x, Screen.height * (1f - viewportPoint.y));
            var previousColor = GUI.color;
            GUI.color = color;

            DrawRect(center.x - gap - length, center.y - thickness * 0.5f, length, thickness);
            DrawRect(center.x + gap, center.y - thickness * 0.5f, length, thickness);
            DrawRect(center.x - thickness * 0.5f, center.y - gap - length, thickness, length);
            DrawRect(center.x - thickness * 0.5f, center.y + gap, thickness, length);

            GUI.color = previousColor;
        }

        private void DrawRect(float x, float y, float width, float height)
        {
            GUI.DrawTexture(new Rect(x, y, width, height), pixel);
        }
    }
}
