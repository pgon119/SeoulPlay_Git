using UnityEngine;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class CanvasParticleEffect : MonoBehaviour
    {
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] private RectTransform targetRect;
        [SerializeField] private Camera renderCamera;
        [SerializeField, Min(0.01f)] private float distanceFromCamera = 5f;
        [SerializeField] private Vector3 positionOffset;
        [SerializeField] private Vector3 rotationEuler;
        [SerializeField, Min(0.001f)] private float worldScale = 1f;
        [SerializeField] private int sortingOrder = 100;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool stopOnDisable = true;

        private Canvas canvas;
        private GameObject particleInstance;
        private ParticleSystem[] particleSystems;
        private bool ownsParticleInstance;

        private void Awake()
        {
            if (targetRect == null)
            {
                targetRect = transform as RectTransform;
            }

            canvas = GetComponentInParent<Canvas>();

            if (renderCamera == null)
            {
                renderCamera = ResolveRenderCamera();
            }

            CreateParticleInstance();
            ApplyRendererSorting();
            UpdateParticleTransform();
        }

        private void OnEnable()
        {
            if (particleInstance != null)
            {
                particleInstance.SetActive(true);
            }

            if (playOnEnable)
            {
                Play();
            }
        }

        private void LateUpdate()
        {
            UpdateParticleTransform();
        }

        private void OnDisable()
        {
            if (stopOnDisable)
            {
                Stop();
            }

            if (particleInstance != null)
            {
                particleInstance.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (ownsParticleInstance && particleInstance != null)
            {
                Destroy(particleInstance);
            }
        }

        public void Play()
        {
            if (particleSystems == null)
            {
                return;
            }

            foreach (var particleSystem in particleSystems)
            {
                particleSystem.Play(true);
            }
        }

        public void Stop()
        {
            if (particleSystems == null)
            {
                return;
            }

            foreach (var particleSystem in particleSystems)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void CreateParticleInstance()
        {
            if (particlePrefab == null || particleInstance != null)
            {
                return;
            }

            if (particlePrefab == gameObject)
            {
                particleInstance = gameObject;
                ownsParticleInstance = false;
                particleSystems = particleInstance.GetComponentsInChildren<ParticleSystem>(true);
                return;
            }

            if (particlePrefab.GetComponentInChildren<CanvasParticleEffect>(true) != null)
            {
                Debug.LogWarning(
                    $"CanvasParticleEffect on {name} skipped particle prefab '{particlePrefab.name}' because it contains another CanvasParticleEffect.",
                    this);
                return;
            }

            particleInstance = Instantiate(particlePrefab);
            ownsParticleInstance = true;
            particleInstance.name = $"{particlePrefab.name} UI Particle";
            particleSystems = particleInstance.GetComponentsInChildren<ParticleSystem>(true);
        }

        private void UpdateParticleTransform()
        {
            if (particleInstance == null || targetRect == null || renderCamera == null)
            {
                return;
            }

            var screenPosition = RectTransformUtility.WorldToScreenPoint(GetCanvasCamera(), targetRect.position);
            var worldPosition = renderCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, distanceFromCamera));

            particleInstance.transform.SetPositionAndRotation(
                worldPosition + positionOffset,
                renderCamera.transform.rotation * Quaternion.Euler(rotationEuler));

            particleInstance.transform.localScale = Vector3.one * worldScale;
        }

        private Camera GetCanvasCamera()
        {
            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return canvas.worldCamera != null ? canvas.worldCamera : renderCamera;
        }

        private Camera ResolveRenderCamera()
        {
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera != null)
            {
                return canvas.worldCamera;
            }

            return Camera.main;
        }

        private void ApplyRendererSorting()
        {
            if (particleInstance == null)
            {
                return;
            }

            foreach (var particleRenderer in particleInstance.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                particleRenderer.sortingOrder = sortingOrder;
            }
        }
    }
}
