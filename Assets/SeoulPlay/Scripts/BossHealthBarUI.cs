using UnityEngine;
using UnityEngine.UI;

namespace SeoulPlay
{
    public sealed class BossHealthBarUI : MonoBehaviour
    {
        [SerializeField] private SeoulPlayDamageable target;
        [SerializeField] private Image fillImage;
        [SerializeField, Min(0.01f)] private float fillFollowSpeed = 2.5f;

        private RectTransform fillRect;
        private float displayedFill = 1f;

        private void Awake()
        {
            if (fillImage != null)
            {
                fillRect = fillImage.rectTransform;
            }

            displayedFill = GetTargetFill();
            ApplyFill(displayedFill);
        }

        private void Update()
        {
            var targetFill = GetTargetFill();
            displayedFill = Mathf.MoveTowards(
                displayedFill,
                targetFill,
                fillFollowSpeed * Time.deltaTime);
            ApplyFill(displayedFill);
        }

        public void SetTarget(SeoulPlayDamageable value)
        {
            target = value;
            displayedFill = GetTargetFill();
            ApplyFill(displayedFill);
        }

        private float GetTargetFill()
        {
            if (target == null || target.MaxHealth <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(target.CurrentHealth / target.MaxHealth);
        }

        private void ApplyFill(float value)
        {
            if (fillRect == null)
            {
                return;
            }

            var localScale = fillRect.localScale;
            localScale.x = Mathf.Clamp01(value);
            fillRect.localScale = localScale;
        }
    }
}
