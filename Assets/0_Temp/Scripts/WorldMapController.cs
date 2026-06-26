using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class WorldMapController : MonoBehaviour
    {
        [SerializeField] private Button street1Button;
        [SerializeField] private Button comingSoonButton;
        [SerializeField] private PopupSystemNoti systemNotiPopup;
        [SerializeField] private string street1LoadingSceneName = "SeoulPlay_Loading";
        [SerializeField] private Animator street1IntroAnimator;
        [SerializeField] private string street1IntroTriggerName = "StartIntro";
        [SerializeField] private AnimationClip street1IntroClip;
        [SerializeField] private AnimationClip[] street1IntroSequenceClips;

        private const float NavigationPressThreshold = 0.6f;
        private const float NavigationReleaseThreshold = 0.35f;
        private bool joystickAxisReleased = true;
        private bool isTransitioning;

        public void Configure(Button streetButton, Button soonButton, PopupSystemNoti popup)
        {
            street1Button = streetButton;
            comingSoonButton = soonButton;
            systemNotiPopup = popup;
        }

        private void Awake()
        {
            if (street1Button != null)
            {
                street1Button.onClick.AddListener(LoadStreet1);
            }

            if (comingSoonButton != null)
            {
                comingSoonButton.onClick.AddListener(ShowComingSoonPopup);
            }
        }

        private void Start()
        {
            SelectButton(street1Button);
        }

        private void Update()
        {
            if (isTransitioning || (systemNotiPopup != null && systemNotiPopup.gameObject.activeInHierarchy))
            {
                return;
            }

            var horizontal = Input.GetAxisRaw("LeftAnalogHorizontal");
            if (Mathf.Abs(horizontal) <= NavigationReleaseThreshold)
            {
                joystickAxisReleased = true;
                return;
            }

            if (!joystickAxisReleased || Mathf.Abs(horizontal) < NavigationPressThreshold)
            {
                return;
            }

            joystickAxisReleased = false;
            var current = EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject
                : null;

            SelectButton(current == street1Button.gameObject ? comingSoonButton : street1Button);
        }

        private void LoadStreet1()
        {
            if (isTransitioning)
            {
                return;
            }

            if (street1IntroAnimator != null && HasStreet1IntroClips())
            {
                StartCoroutine(PlayStreet1IntroAndLoad());
                return;
            }

            LoadStreet1Scene();
        }

        private IEnumerator PlayStreet1IntroAndLoad()
        {
            isTransitioning = true;
            SetButtonsInteractable(false);

            var remainingLoopTime = GetCurrentAnimatorLoopRemainingTime();
            street1IntroAnimator.SetTrigger(street1IntroTriggerName);

            if (remainingLoopTime > 0f)
            {
                yield return new WaitForSeconds(remainingLoopTime);
            }

            var introDuration = GetStreet1IntroClipsDuration();
            if (introDuration > 0f)
            {
                yield return new WaitForSeconds(introDuration);
            }

            LoadStreet1Scene();
        }

        private float GetCurrentAnimatorLoopRemainingTime()
        {
            if (street1IntroAnimator == null || !street1IntroAnimator.isActiveAndEnabled)
            {
                return 0f;
            }

            var stateInfo = street1IntroAnimator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.loop)
            {
                return 0f;
            }

            var length = stateInfo.length;
            if (length <= 0f)
            {
                return 0f;
            }

            var elapsedNormalized = Mathf.Repeat(stateInfo.normalizedTime, 1f);
            var remaining = 1f - elapsedNormalized;
            return remaining > 0.001f ? remaining * length : 0f;
        }

        private float GetStreet1IntroClipsDuration()
        {
            var duration = 0f;
            foreach (var clip in GetStreet1IntroClips())
            {
                if (clip != null)
                {
                    duration += clip.length;
                }
            }

            return duration;
        }

        private bool HasStreet1IntroClips()
        {
            return HasStreet1IntroSequenceClips() || street1IntroClip != null;
        }

        private System.Collections.Generic.IEnumerable<AnimationClip> GetStreet1IntroClips()
        {
            if (HasStreet1IntroSequenceClips())
            {
                return street1IntroSequenceClips;
            }

            return new[] { street1IntroClip };
        }

        private bool HasStreet1IntroSequenceClips()
        {
            if (street1IntroSequenceClips == null)
            {
                return false;
            }

            foreach (var clip in street1IntroSequenceClips)
            {
                if (clip != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void LoadStreet1Scene()
        {
            if (!string.IsNullOrWhiteSpace(street1LoadingSceneName))
            {
                SceneManager.LoadScene(street1LoadingSceneName, LoadSceneMode.Single);
            }
        }

        private void ShowComingSoonPopup()
        {
            if (systemNotiPopup != null)
            {
                systemNotiPopup.Show();
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (street1Button != null)
            {
                street1Button.interactable = interactable;
            }

            if (comingSoonButton != null)
            {
                comingSoonButton.interactable = interactable;
            }
        }

        private static void SelectButton(Button button)
        {
            if (EventSystem.current != null && button != null)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject);
            }
        }
    }
}
