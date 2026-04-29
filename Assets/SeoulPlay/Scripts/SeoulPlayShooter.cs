using UnityEngine;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class SeoulPlayShooter : MonoBehaviour
    {
        [SerializeField] private SeoulPlayWeaponHolder weaponHolder;
        [SerializeField] private SeoulPlayCrosshairUI crosshair;
        [SerializeField] private SimpleHeroMover heroMover;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField, Min(1f)] private float projectileSpeed = 55f;
        [SerializeField, Min(0.1f)] private float projectileLifetime = 2f;
        [SerializeField, Min(0f)] private float muzzleForwardOffset = 0.35f;
        [SerializeField] private bool rotateBodyToAim = true;
        [SerializeField] private bool rotateCameraToAim;
        [SerializeField, Min(0f)] private float aimTurnSpeed = 540f;
        [SerializeField, Min(0f)] private float aimCameraTurnSpeed = 540f;
        [SerializeField, Min(0f)] private float aimFacingHeight = 1.2f;

        private readonly RaycastHit[] aimHits = new RaycastHit[16];
        private float nextFireTime;

        private void Awake()
        {
            if (weaponHolder == null)
            {
                weaponHolder = GetComponent<SeoulPlayWeaponHolder>();
            }

            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }

            if (crosshair == null)
            {
                crosshair = GetComponent<SeoulPlayCrosshairUI>();
            }

            if (heroMover == null)
            {
                heroMover = GetComponent<SimpleHeroMover>();
            }
        }

        private void Update()
        {
            UpdateAimFacing();

            if (!IsFireHeld() || Time.time < nextFireTime)
            {
                return;
            }

            Fire();
        }

        private void UpdateAimFacing()
        {
            if (aimCamera == null || !IsAimFacingHeld())
            {
                return;
            }

            var origin = transform.position + Vector3.up * aimFacingHeight;
            var direction = GetAimDirection(origin);
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            if (rotateBodyToAim)
            {
                var targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    aimTurnSpeed * Time.deltaTime);
            }

            if (rotateCameraToAim && heroMover != null)
            {
                heroMover.RotateCameraYawToward(direction, aimCameraTurnSpeed);
            }
        }

        private void Fire()
        {
            var weapon = weaponHolder != null ? weaponHolder.EquippedWeapon : null;
            var fireRate = weapon != null ? weapon.FireRate : 8f;
            nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);

            var muzzle = weapon != null ? weapon.Muzzle : transform;
            var direction = GetAimDirection(muzzle.position);
            var spawnPosition = muzzle.position + direction * muzzleForwardOffset;
            var projectileObject = projectilePrefab != null
                ? Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction, Vector3.up))
                : CreateDefaultProjectile(spawnPosition, direction);

            var projectile = projectileObject.GetComponent<SeoulPlayProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<SeoulPlayProjectile>();
            }

            var damage = weapon != null ? weapon.Damage : 10f;
            projectile.Launch(direction, projectileSpeed, damage, projectileLifetime, transform);
        }

        private Vector3 GetAimDirection(Vector3 fromPosition)
        {
            if (aimCamera == null)
            {
                return transform.forward;
            }

            var viewportPoint = crosshair != null
                ? crosshair.ViewportPoint
                : new Vector3(0.5f, 0.5f, 0f);
            var ray = aimCamera.ViewportPointToRay(viewportPoint);
            var targetPoint = GetAimTargetPoint(ray);
            var direction = targetPoint - fromPosition;
            return direction.sqrMagnitude > 0.001f ? direction.normalized : ray.direction;
        }

        private Vector3 GetAimTargetPoint(Ray ray)
        {
            var hitCount = Physics.RaycastNonAlloc(
                ray,
                aimHits,
                200f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            var bestDistance = float.PositiveInfinity;
            var targetPoint = ray.origin + ray.direction * 80f;
            for (var i = 0; i < hitCount; i++)
            {
                var hit = aimHits[i];
                if (hit.collider == null || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    targetPoint = hit.point;
                }
            }

            return targetPoint;
        }

        private static bool IsFireHeld()
        {
            return Input.GetMouseButton(0) || Input.GetButton("RB") || Input.GetAxis("RT") > 0.2f;
        }

        private static bool IsAimFacingHeld()
        {
            var aimStick = new Vector2(
                Input.GetAxisRaw("RightAnalogHorizontal"),
                Input.GetAxisRaw("RightAnalogVertical"));
            return aimStick.magnitude > 0.2f
                || Input.GetMouseButton(0)
                || Input.GetMouseButton(1)
                || Input.GetButton("LB")
                || Input.GetButton("RB")
                || Input.GetAxis("LT") > 0.2f
                || Input.GetAxis("RT") > 0.2f;
        }

        private static GameObject CreateDefaultProjectile(Vector3 position, Vector3 direction)
        {
            var projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "SeoulPlay Bullet";
            projectileObject.transform.SetPositionAndRotation(
                position,
                Quaternion.LookRotation(direction, Vector3.up));
            projectileObject.transform.localScale = Vector3.one * 0.08f;

            var collider = projectileObject.GetComponent<SphereCollider>();
            collider.isTrigger = true;

            return projectileObject;
        }
    }
}
