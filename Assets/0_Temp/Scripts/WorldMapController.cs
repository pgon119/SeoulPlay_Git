using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
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
        [SerializeField] private AnimationClip street1IntroClip;

        private const float NavigationPressThreshold = 0.6f;
        private const float NavigationReleaseThreshold = 0.35f;
        private bool joystickAxisReleased = true;
        private bool isTransitioning;
        private PlayableGraph introGraph;

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

            if (street1IntroAnimator != null && street1IntroClip != null)
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

            introGraph = PlayableGraph.Create("WorldMapStreet1Intro");
            var output = AnimationPlayableOutput.Create(introGraph, "Animation", street1IntroAnimator);
            var clipPlayable = AnimationClipPlayable.Create(introGraph, street1IntroClip);
            output.SetSourcePlayable(clipPlayable);

            introGraph.Play();

            var duration = street1IntroClip.length;
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }

            DestroyIntroGraph();
            LoadStreet1Scene();
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

        private void OnDestroy()
        {
            DestroyIntroGraph();
        }

        private void DestroyIntroGraph()
        {
            if (introGraph.IsValid())
            {
                introGraph.Destroy();
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
