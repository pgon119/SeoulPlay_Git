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
        [SerializeField, Min(0f)] private float floorAimIgnoreDrop = 0.2f;
        [SerializeField, Min(0f)] private float fallbackShotOriginHeight = 1.1f;
        [SerializeField, Min(0f)] private float fallbackShotOriginForwardOffset = 0.45f;
        [SerializeField, Min(0f)] private float maxMuzzleOriginDistance = 2f;
        [SerializeField] private bool useInstantHitDamage = true;
        [SerializeField, Min(0.01f)] private float instantHitRadius = 0.6f;
        [SerializeField] private bool showHitDebug = true;
        [Header("Shot Feedback")]
        [SerializeField] private bool addProjectileTrail = true;
        [SerializeField, Min(0.01f)] private float projectileTrailDuration = 0.18f;
        [SerializeField, Min(0.001f)] private float projectileTrailWidth = 0.035f;
        [SerializeField] private Color projectileTrailColor = new(1f, 0.78f, 0.18f, 1f);
        [SerializeField] private bool showDebugOverlay = true;

        private readonly RaycastHit[] aimHits = new RaycastHit[16];
        private float nextFireTime;
        private float lastFireTime = -999f;
        private string lastFireStatus = "No shots yet";

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
            if (weaponHolder != null && weaponHolder.EquippedWeapon == null)
            {
                weaponHolder.EquipDefaultWeapon();
            }

            var weapon = weaponHolder != null ? weaponHolder.EquippedWeapon : null;
            var fireRate = weapon != null ? weapon.FireRate : 8f;
            nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);

            var muzzle = weapon != null ? weapon.Muzzle : transform;
            var range = weapon != null ? weapon.Range : 80f;
            var stableOrigin = GetFallbackShotOrigin(transform.forward);
            var muzzlePosition = GetSafeMuzzlePosition(muzzle, stableOrigin);
            var direction = GetShotDirection(muzzlePosition, range);
            var spawnPosition = GetSafeMuzzlePosition(muzzle, GetFallbackShotOrigin(direction)) + direction * muzzleForwardOffset;
            var damage = weapon != null ? weapon.Damage : 10f;
            var hitStatus = useInstantHitDamage
                ? ApplyInstantHitDamage(spawnPosition, direction, damage, range)
                : "projectile damage";
            var projectileObject = projectilePrefab != null
                ? Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction, Vector3.up))
                : CreateDefaultProjectile(spawnPosition, direction);

            var projectile = projectileObject.GetComponent<SeoulPlayProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<SeoulPlayProjectile>();
            }

            projectile.Launch(direction, projectileSpeed, useInstantHitDamage ? 0f : damage, projectileLifetime, transform);
            projectile.ConfigureMotion(0f);
            AddProjectileTrail(projectileObject);
            lastFireTime = Time.time;
            lastFireStatus = $"Fired {projectileObject.name} damage {damage:0.##} {hitStatus} dir {direction.x:0.00},{direction.y:0.00},{direction.z:0.00} y {spawnPosition.y:0.00}";
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

        private Vector3 GetShotDirection(Vector3 muzzlePosition, float range)
        {
            if (aimCamera == null)
            {
                return transform.forward;
            }

            var viewportPoint = crosshair != null
                ? crosshair.ViewportPoint
                : new Vector3(0.5f, 0.5f, 0f);
            var ray = aimCamera.ViewportPointToRay(viewportPoint);
            var targetPoint = ray.origin + ray.direction * Mathf.Max(1f, range);
            var hasAimTarget = false;
            var hitCount = Physics.RaycastNonAlloc(
                ray,
                aimHits,
                200f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            var bestDistance = float.PositiveInfinity;
            for (var i = 0; i < hitCount; i++)
            {
                var hit = aimHits[i];
                if (!IsValidShotAimHit(hit, muzzlePosition))
                {
                    continue;
                }

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    targetPoint = hit.point;
                    hasAimTarget = true;
                }
            }

            if (!hasAimTarget)
            {
                return GetFallbackShotDirection(ray.direction);
            }

            var direction = targetPoint - muzzlePosition;
            return direction.sqrMagnitude > 0.001f ? direction.normalized : GetFallbackShotDirection(ray.direction);
        }

        private bool IsValidShotAimHit(RaycastHit hit, Vector3 muzzlePosition)
        {
            if (hit.collider == null || hit.transform.IsChildOf(transform))
            {
                return false;
            }

            var damageable = hit.collider.GetComponentInParent<SeoulPlayDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                return true;
            }

            var isFloorLikeHit = Vector3.Dot(hit.normal, Vector3.up) > 0.45f;
            var isBelowMuzzle = hit.point.y < muzzlePosition.y - floorAimIgnoreDrop;
            return !isFloorLikeHit || !isBelowMuzzle;
        }

        private Vector3 GetFallbackShotDirection(Vector3 cameraDirection)
        {
            var direction = cameraDirection.sqrMagnitude > 0.001f
                ? cameraDirection.normalized
                : transform.forward;

            if (direction.y < 0f)
            {
                direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            }

            return direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
        }

        private Vector3 GetSafeMuzzlePosition(Transform muzzle, Vector3 fallbackOrigin)
        {
            if (muzzle == null)
            {
                return fallbackOrigin;
            }

            var muzzlePosition = muzzle.position;
            var footPosition = transform.position;
            var tooLow = muzzlePosition.y < footPosition.y + 0.25f;
            var tooFar = Vector3.Distance(muzzlePosition, footPosition) > maxMuzzleOriginDistance;
            return tooLow || tooFar ? fallbackOrigin : muzzlePosition;
        }

        private Vector3 GetFallbackShotOrigin(Vector3 shotDirection)
        {
            var flatDirection = Vector3.ProjectOnPlane(shotDirection, Vector3.up);
            if (flatDirection.sqrMagnitude <= 0.001f)
            {
                flatDirection = transform.forward;
            }

            return transform.position
                + Vector3.up * fallbackShotOriginHeight
                + flatDirection.normalized * fallbackShotOriginForwardOffset;
        }

        private string ApplyInstantHitDamage(Vector3 origin, Vector3 direction, float damage, float range)
        {
            var hitCount = Physics.SphereCastNonAlloc(
                origin,
                instantHitRadius,
                direction,
                aimHits,
                Mathf.Max(1f, range),
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide);

            var bestDistance = float.PositiveInfinity;
            SeoulPlayDamageable bestDamageable = null;
            Vector3 bestDirection = direction;
            Collider bestCollider = null;
            for (var i = 0; i < hitCount; i++)
            {
                var hit = aimHits[i];
                if (hit.collider == null || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                var damageable = hit.collider.GetComponentInParent<SeoulPlayDamageable>();
                if (damageable == null || !damageable.IsAlive)
                {
                    continue;
                }

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestDamageable = damageable;
                    bestDirection = hit.normal.sqrMagnitude > 0.001f ? -hit.normal : direction;
                    bestCollider = hit.collider;
                }
            }

            if (bestDamageable == null)
            {
                if (showHitDebug)
                {
                    Debug.Log($"SeoulPlayShooter instant hit missed from {origin} dir {direction} range {range:0.##}");
                }

                return "miss";
            }

            bestDamageable.TakeDamage(damage, bestDirection, transform);
            if (showHitDebug)
            {
                Debug.Log($"SeoulPlayShooter instant hit {bestDamageable.name} via {bestCollider.name} for {damage:0.##}", bestDamageable);
            }

            return $"hit {bestDamageable.name}";
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
            return Input.GetMouseButton(0)
                || Input.GetKey(KeyCode.F)
                || GetButtonSafe("RB")
                || GetAxisSafe("RT") > 0.2f;
        }

        private static bool IsAimFacingHeld()
        {
            var aimStick = new Vector2(
                Input.GetAxisRaw("RightAnalogHorizontal"),
                Input.GetAxisRaw("RightAnalogVertical"));
            return aimStick.magnitude > 0.2f
                || Input.GetMouseButton(0)
                || Input.GetKey(KeyCode.F)
                || Input.GetMouseButton(1)
                || GetButtonSafe("LB")
                || GetButtonSafe("RB")
                || GetAxisSafe("LT") > 0.2f
                || GetAxisSafe("RT") > 0.2f;
        }

        private void OnGUI()
        {
            if (!showDebugOverlay)
            {
                return;
            }

            var inputHeld = IsFireHeld();
            var weapon = weaponHolder != null ? weaponHolder.EquippedWeapon : null;
            GUI.color = Color.yellow;
            GUI.Label(
                new Rect(16f, 96f, 520f, 86f),
                $"Shooter active | Fire input: {inputHeld}\nWeapon: {(weapon != null ? weapon.name : "none")} | Projectile: {(projectilePrefab != null ? projectilePrefab.name : "default")}\nLast shot: {lastFireStatus} ({Time.time - lastFireTime:0.0}s ago)");
            GUI.color = Color.white;
        }

        private void AddProjectileTrail(GameObject projectileObject)
        {
            if (!addProjectileTrail || projectileObject == null || projectileObject.GetComponent<TrailRenderer>() != null)
            {
                return;
            }

            var trail = projectileObject.AddComponent<TrailRenderer>();
            trail.time = projectileTrailDuration;
            trail.startWidth = projectileTrailWidth;
            trail.endWidth = 0f;
            trail.minVertexDistance = 0.02f;
            trail.numCornerVertices = 2;
            trail.numCapVertices = 2;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = projectileTrailColor;
            trail.endColor = new Color(projectileTrailColor.r, projectileTrailColor.g, projectileTrailColor.b, 0f);
        }

        private static bool GetButtonSafe(string buttonName)
        {
            try
            {
                return Input.GetButton(buttonName);
            }
            catch (UnityException)
            {
                return false;
            }
        }

        private static float GetAxisSafe(string axisName)
        {
            try
            {
                return Input.GetAxis(axisName);
            }
            catch (UnityException)
            {
                return 0f;
            }
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
