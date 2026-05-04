using UnityEngine;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class MainCameraMouseLookToggle : MonoBehaviour
    {
        [SerializeField] private bool mouseLookEnabled;
        [SerializeField] private SimpleHeroMover heroMover;

        private void Awake()
        {
            Apply();
        }

        private void Start()
        {
            Apply();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Apply();
        }

        public void SetMouseLookEnabled(bool enabled)
        {
            mouseLookEnabled = enabled;
            Apply();
        }

        private void Apply()
        {
            if (heroMover == null)
            {
                heroMover = FindObjectOfType<SimpleHeroMover>();
            }

            if (heroMover != null)
            {
                heroMover.SetMouseCameraInputEnabled(mouseLookEnabled);
            }
        }
    }
}
