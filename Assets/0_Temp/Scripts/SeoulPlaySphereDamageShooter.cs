using UnityEngine;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class SeoulPlaySphereDamageShooter : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private bool aimAtTarget = true;
        [SerializeField, Min(0.05f)] private float fireInterval = 1f;
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0f)] private float heavyDamage = 24f;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 8f;
        [SerializeField, Min(0.1f)] private float projectileLifetime = 4f;
        [SerializeField, Min(0.02f)] private float sphereRadius = 0.22f;
        [SerializeField, Min(1f)] private float heavySphereRadiusMultiplier = 2f;
        [SerializeField, Min(1)] private int smallShotsBeforeHeavy = 2;
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.6f, 0.75f);
        [SerializeField] private Color sphereColor = new Color(1f, 0.25f, 0.12f, 1f);
        [SerializeField] private Color heavySphereColor = new Color(0.8f, 0.1f, 1f, 1f);
        [SerializeField] private bool showHitDebug = true;

        private float fireTimer;
        private Material sphereMaterial;
        private Material heavySphereMaterial;
        private int shotIndex;

        private void Reset()
        {
            var hero = FindObjectOfType<SimpleHeroMover>();
            if (hero != null)
            {
                target = hero.transform;
            }
        }

        private void OnDisable()
        {
            fireTimer = 0f;
        }

        private void Update()
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer > 0f)
            {
                return;
            }

            fireTimer = fireInterval;
            Fire();
        }

        public void SetTarget(Transform value)
        {
            target = value;
        }

        private void Fire()
        {
            var isHeavyShot = shotIndex >= smallShotsBeforeHeavy;
            shotIndex = isHeavyShot ? 0 : shotIndex + 1;

            var direction = GetFireDirection();
            var spawnPosition = transform.TransformPoint(spawnOffset);
            var projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = isHeavyShot ? "Heavy Damage Test Sphere" : "Damage Test Sphere";
            projectileObject.transform.SetPositionAndRotation(spawnPosition, Quaternion.LookRotation(direction, Vector3.up));
            var activeRadius = isHeavyShot ? sphereRadius * heavySphereRadiusMultiplier : sphereRadius;
            projectileObject.transform.localScale = Vector3.one * (activeRadius * 2f);

            var renderer = projectileObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetSphereMaterial(isHeavyShot);
            }

            var projectile = projectileObject.AddComponent<SeoulPlayProjectile>();
            projectile.ConfigureDebug(showHitDebug);
            projectile.ConfigureTriggerHits(true);
            projectile.Launch(direction, projectileSpeed, isHeavyShot ? heavyDamage : damage, projectileLifetime, transform);
        }

        private Vector3 GetFireDirection()
        {
            if (aimAtTarget && target != null)
            {
                var targetPoint = target.position + Vector3.up;
                var directionToTarget = targetPoint - transform.TransformPoint(spawnOffset);
                if (directionToTarget.sqrMagnitude > 0.001f)
                {
                    return directionToTarget.normalized;
                }
            }

            return transform.forward.sqrMagnitude > 0.001f ? transform.forward.normalized : Vector3.forward;
        }

        private Material GetSphereMaterial(bool heavy)
        {
            if (heavy && heavySphereMaterial != null)
            {
                return heavySphereMaterial;
            }

            if (!heavy && sphereMaterial != null)
            {
                return sphereMaterial;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader)
            {
                name = heavy ? "Heavy Damage Test Sphere Material" : "Damage Test Sphere Material",
                color = heavy ? heavySphereColor : sphereColor
            };

            if (heavy)
            {
                heavySphereMaterial = material;
            }
            else
            {
                sphereMaterial = material;
            }

            return material;
        }
    }
}
