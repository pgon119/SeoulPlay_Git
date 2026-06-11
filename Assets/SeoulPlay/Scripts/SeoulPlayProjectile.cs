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
        [SerializeField, Min(0.01f)] private float castRadius = 0.18f;
        [SerializeField] private bool collisionEnabled = true;
        [SerializeField] private bool hitTriggerColliders;
        [SerializeField] private bool showHitDebug;
        [Header("Impact Feedback")]
        [SerializeField] private GameObject impactVfxPrefab;
        [SerializeField, Min(0.05f)] private float impactVfxLifetime = 3f;
        [SerializeField] private bool alignImpactVfxToTravelDirection = true;
        [SerializeField, Range(-1, 31)] private int foregroundVfxLayer = -1;
        [Header("Visual Offset")]
        [SerializeField] private float visualSideOffset;
        [SerializeField] private float visualDownOffset;
        [SerializeField, Min(0f)] private float visualOffsetDelay = 0.1f;
        [SerializeField, Min(0.01f)] private float visualOffsetDuration = 0.3f;

        private readonly RaycastHit[] travelHits = new RaycastHit[8];
        private Vector3 direction = Vector3.forward;
        private Vector3 velocity;
        private Vector3 visualSideDirection = Vector3.zero;
        private Transform ignoredRoot;
        private Transform visualOffsetRoot;
        private Collider projectileCollider;
        private float age;

        private void Awake()
        {
            projectileCollider = GetComponent<Collider>();
        }

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

            if (projectileCollider == null)
            {
                projectileCollider = GetComponent<Collider>();
            }
        }

        public void ConfigureMotion(float projectileGravity, float projectileSpinDegreesPerSecond = 0f)
        {
            gravity = Mathf.Max(0f, projectileGravity);
            spinDegreesPerSecond = Mathf.Max(0f, projectileSpinDegreesPerSecond);
        }

        public void ConfigureCollision(bool enabled)
        {
            collisionEnabled = enabled;

            if (projectileCollider == null)
            {
                projectileCollider = GetComponent<Collider>();
            }

            if (projectileCollider != null)
            {
                projectileCollider.enabled = enabled;
            }
        }

        public void ConfigureTriggerHits(bool enabled)
        {
            hitTriggerColliders = enabled;
        }

        public void ConfigureDebug(bool enabled)
        {
            showHitDebug = enabled;
        }

        public void ConfigureImpactVfx(GameObject prefab, float vfxLifetime = 3f, bool alignToTravelDirection = true)
        {
            impactVfxPrefab = prefab;
            impactVfxLifetime = Mathf.Max(0.05f, vfxLifetime);
            alignImpactVfxToTravelDirection = alignToTravelDirection;
        }

        public void ConfigureForegroundVfxLayer(int layer)
        {
            foregroundVfxLayer = layer;
            ApplyRendererLayerRecursive(transform, layer);
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

            var currentPosition = transform.position;
            var nextPosition = currentPosition + velocity * Time.deltaTime;
            if (collisionEnabled && CheckTravelHit(currentPosition, nextPosition))
            {
                return;
            }

            transform.position = nextPosition;

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
            if (!collisionEnabled)
            {
                return;
            }

            HandleHit(other, direction);
        }

        private bool CheckTravelHit(Vector3 fromPosition, Vector3 toPosition)
        {
            var travel = toPosition - fromPosition;
            var distance = travel.magnitude;
            if (distance <= 0.001f)
            {
                return false;
            }

            var radius = GetCastRadius();
            var hitCount = Physics.SphereCastNonAlloc(
                fromPosition,
                radius,
                travel / distance,
                travelHits,
                distance,
                Physics.DefaultRaycastLayers,
                hitTriggerColliders ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);

            var bestDistance = float.PositiveInfinity;
            Collider bestCollider = null;
            for (var i = 0; i < hitCount; i++)
            {
                var hit = travelHits[i];
                if (hit.collider == null || ShouldIgnoreCollider(hit.collider))
                {
                    continue;
                }

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestCollider = hit.collider;
                }
            }

            if (bestCollider == null)
            {
                return false;
            }

            transform.position = fromPosition + travel.normalized * Mathf.Max(0f, bestDistance);
            HandleHit(bestCollider, travel.normalized);
            return true;
        }

        private void HandleHit(Collider other, Vector3 hitDirection)
        {
            if (ShouldIgnoreCollider(other))
            {
                return;
            }

            var damageable = ResolveDamageable(other);
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(damage, hitDirection, ignoredRoot);
                if (showHitDebug)
                {
                    Debug.Log($"SeoulPlayProjectile hit {damageable.name} for {damage:0.##}", damageable);
                }
            }
            else if (showHitDebug)
            {
                Debug.Log($"SeoulPlayProjectile hit {other.name}, but no live SeoulPlayDamageable was found.", other);
            }

            if (other.attachedRigidbody != null)
            {
                other.attachedRigidbody.AddForce(hitDirection * damage, ForceMode.Impulse);
            }

            SpawnImpactVfx(hitDirection);
            Destroy(gameObject);
        }

        private void SpawnImpactVfx(Vector3 hitDirection)
        {
            if (impactVfxPrefab == null)
            {
                return;
            }

            var rotation = Quaternion.identity;
            if (alignImpactVfxToTravelDirection && hitDirection.sqrMagnitude > 0.001f)
            {
                rotation = Quaternion.LookRotation(hitDirection.normalized, Vector3.up);
            }

            var vfxObject = Instantiate(impactVfxPrefab, transform.position, rotation);
            ApplyLayerRecursive(vfxObject.transform, foregroundVfxLayer);
            Destroy(vfxObject, impactVfxLifetime);
        }

        private bool ShouldIgnoreCollider(Collider other)
        {
            return other == null
                || (!hitTriggerColliders && other.isTrigger)
                || other.transform.IsChildOf(transform)
                || (ignoredRoot != null && other.transform.IsChildOf(ignoredRoot))
                || other.GetComponentInParent<SeoulPlayProjectile>() != null;
        }

        private static SeoulPlayDamageable ResolveDamageable(Collider targetCollider)
        {
            if (targetCollider == null)
            {
                return null;
            }

            var damageable = targetCollider.GetComponentInParent<SeoulPlayDamageable>();
            if (damageable != null)
            {
                return damageable;
            }

            var parent = targetCollider.transform.parent;
            while (parent != null)
            {
                damageable = parent.GetComponentInChildren<SeoulPlayDamageable>();
                if (damageable != null)
                {
                    return damageable;
                }

                parent = parent.parent;
            }

            var root = targetCollider.transform.root;
            return root != null ? root.GetComponentInChildren<SeoulPlayDamageable>() : null;
        }

        private float GetCastRadius()
        {
            if (projectileCollider is SphereCollider sphereCollider)
            {
                var scale = transform.lossyScale;
                var largestAxis = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                return Mathf.Max(castRadius, sphereCollider.radius * largestAxis);
            }

            return castRadius;
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
            rootObject.layer = gameObject.layer;
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
                visualObject.layer = gameObject.layer;
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

        private static void ApplyLayerRecursive(Transform target, int layer)
        {
            if (target == null || layer < 0 || layer > 31)
            {
                return;
            }

            target.gameObject.layer = layer;
            for (var i = 0; i < target.childCount; i++)
            {
                ApplyLayerRecursive(target.GetChild(i), layer);
            }
        }

        private static void ApplyRendererLayerRecursive(Transform target, int layer)
        {
            if (target == null || layer < 0 || layer > 31)
            {
                return;
            }

            if (target.GetComponent<Renderer>() != null)
            {
                target.gameObject.layer = layer;
            }

            for (var i = 0; i < target.childCount; i++)
            {
                ApplyRendererLayerRecursive(target.GetChild(i), layer);
            }
        }
    }
}
