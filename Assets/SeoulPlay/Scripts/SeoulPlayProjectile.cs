using UnityEngine;

namespace SeoulPlay
{
    public sealed class SeoulPlayProjectile : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float speed = 45f;
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0.1f)] private float lifetime = 2f;
        [SerializeField, Min(0f)] private float gravity;
        [SerializeField, Min(0f)] private float spinDegreesPerSecond;
        [Header("Visual Offset")]
        [SerializeField] private float visualSideOffset;
        [SerializeField] private float visualDownOffset;
        [SerializeField, Min(0f)] private float visualOffsetDelay = 0.1f;
        [SerializeField, Min(0.01f)] private float visualOffsetDuration = 0.3f;

        private Vector3 direction = Vector3.forward;
        private Vector3 velocity;
        private Vector3 visualSideDirection = Vector3.zero;
        private Transform ignoredRoot;
        private Transform visualOffsetRoot;
        private float age;

        public void Launch(
            Vector3 launchDirection,
            float launchSpeed,
            float projectileDamage,
            float projectileLifetime,
            Transform owner = null)
        {
            direction = launchDirection.sqrMagnitude > 0.001f ? launchDirection.normalized : transform.forward;
            speed = launchSpeed;
            damage = projectileDamage;
            lifetime = projectileLifetime;
            ignoredRoot = owner;
            velocity = direction * speed;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        public void ConfigureMotion(float projectileGravity, float projectileSpinDegreesPerSecond = 0f)
        {
            gravity = Mathf.Max(0f, projectileGravity);
            spinDegreesPerSecond = Mathf.Max(0f, projectileSpinDegreesPerSecond);
        }

        public void ConfigureVisualOffset(
            float sideOffset,
            float downOffset,
            Vector3 sideDirection,
            float delay,
            float duration)
        {
            visualSideOffset = sideOffset;
            visualDownOffset = downOffset;
            visualSideDirection = sideDirection.sqrMagnitude > 0.001f ? sideDirection.normalized : Vector3.zero;
            visualOffsetDelay = Mathf.Max(0f, delay);
            visualOffsetDuration = Mathf.Max(0.01f, duration);

            if (HasVisualOffset())
            {
                EnsureVisualOffsetRoot();
            }
        }

        private void Update()
        {
            if (gravity > 0f)
            {
                velocity += Physics.gravity.normalized * gravity * Time.deltaTime;
                if (velocity.sqrMagnitude > 0.001f)
                {
                    direction = velocity.normalized;
                }
            }

            transform.position += velocity * Time.deltaTime;

            if (spinDegreesPerSecond > 0f)
            {
                transform.Rotate(Vector3.right, spinDegreesPerSecond * Time.deltaTime, Space.Self);
            }
            else if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }

            UpdateVisualOffset();

            age += Time.deltaTime;
            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ignoredRoot != null && other.transform.IsChildOf(ignoredRoot))
            {
                return;
            }

            var damageable = other.GetComponentInParent<SeoulPlayDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(damage, direction, ignoredRoot);
            }

            if (other.attachedRigidbody != null)
            {
                other.attachedRigidbody.AddForce(direction * damage, ForceMode.Impulse);
            }

            Destroy(gameObject);
        }

        private void EnsureVisualOffsetRoot()
        {
            if (visualOffsetRoot != null)
            {
                return;
            }

            var renderers = GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            var rootObject = new GameObject("VisualOffsetRoot");
            visualOffsetRoot = rootObject.transform;
            visualOffsetRoot.SetParent(transform, false);
            visualOffsetRoot.localPosition = Vector3.zero;
            visualOffsetRoot.localRotation = Quaternion.identity;
            visualOffsetRoot.localScale = Vector3.one;

            foreach (var sourceRenderer in renderers)
            {
                var sourceFilter = sourceRenderer.GetComponent<MeshFilter>();
                if (sourceFilter == null || sourceFilter.sharedMesh == null)
                {
                    continue;
                }

                var visualObject = new GameObject(sourceRenderer.gameObject.name + "_Visual");
                var visualTransform = visualObject.transform;
                visualTransform.SetParent(visualOffsetRoot, false);
                visualTransform.localPosition = transform.InverseTransformPoint(sourceRenderer.transform.position);
                visualTransform.localRotation = Quaternion.Inverse(transform.rotation) * sourceRenderer.transform.rotation;
                visualTransform.localScale = sourceRenderer.transform.lossyScale;

                var visualFilter = visualObject.AddComponent<MeshFilter>();
                visualFilter.sharedMesh = sourceFilter.sharedMesh;

                var visualRenderer = visualObject.AddComponent<MeshRenderer>();
                visualRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
                visualRenderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
                visualRenderer.receiveShadows = sourceRenderer.receiveShadows;

                sourceRenderer.enabled = false;
            }
        }

        private void UpdateVisualOffset()
        {
            if (visualOffsetRoot == null || !HasVisualOffset())
            {
                return;
            }

            var normalizedTime = Mathf.Clamp01((age - visualOffsetDelay) / visualOffsetDuration);
            var smoothedTime = normalizedTime * normalizedTime * (3f - 2f * normalizedTime);
            var worldOffset = (visualSideDirection * visualSideOffset + Vector3.down * visualDownOffset) * smoothedTime;
            visualOffsetRoot.localPosition = transform.InverseTransformDirection(worldOffset);
        }

        private bool HasVisualOffset()
        {
            var hasSideOffset = Mathf.Abs(visualSideOffset) > 0.001f && visualSideDirection.sqrMagnitude > 0.001f;
            var hasDownOffset = Mathf.Abs(visualDownOffset) > 0.001f;
            return hasSideOffset || hasDownOffset;
        }
    }
}
