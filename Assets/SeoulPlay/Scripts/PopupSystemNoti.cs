using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class PopupSystemNoti : MonoBehaviour
    {
        [SerializeField] private Button confirmButton;

        private GameObject previousSelection;

        public void Configure(Button button)
        {
            confirmButton = button;
        }

        private void Awake()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(Hide);
            }
        }

        private void Update()
        {
            if (Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.Escape))
            {
                Hide();
            }
        }

        public void Show()
        {
            if (EventSystem.current != null)
            {
                previousSelection = EventSystem.current.currentSelectedGameObject;
            }

            gameObject.SetActive(true);

            if (EventSystem.current != null && confirmButton != null)
            {
                EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            if (EventSystem.current != null && previousSelection != null)
            {
                EventSystem.current.SetSelectedGameObject(previousSelection);
            }
        }
    }
}
