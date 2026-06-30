using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField, Min(0.01f)] private float projectileVisualScale = 1f;
        [SerializeField, Min(1f)] private float projectileSpeed = 55f;
        [SerializeField, Min(0.1f)] private float projectileLifetime = 2f;
        [SerializeField, Min(0f)] private float muzzleForwardOffset;
        [SerializeField] private bool rotateBodyToAim = true;
        [SerializeField] private bool rotateCameraToAim;
        [SerializeField, Min(0f)] private float aimTurnSpeed = 540f;
        [SerializeField, Min(0f)] private float aimCameraTurnSpeed = 540f;
        [SerializeField, Min(0f)] private float aimFacingHeight = 1.2f;
        [SerializeField, Min(0f)] private float floorAimIgnoreDrop = 0.2f;
        [SerializeField, Min(0f)] private float fallbackShotOriginHeight = 1.1f;
        [SerializeField, Min(0f)] private float fallbackShotOriginForwardOffset = 0.45f;
        [SerializeField, Min(0f)] private float maxMuzzleOriginDistance = 2f;
        [Header("Roll Fire Lockout")]
        [SerializeField] private bool blockFireWhileRolling = true;
        [SerializeField, Min(0f)] private float rollInputFireLockoutDuration = 1f;
        [SerializeField] private bool useInstantHitDamage = true;
        [SerializeField] private LayerMask damageHitMask = 1 << 20;
        [SerializeField, Min(0.01f)] private float instantHitRadius = 0.6f;
        [SerializeField, Min(0.1f)] private float minimumVisibleHitDistance = 3f;
        [SerializeField, Min(0f)] private float impactFrontOffset = 0.35f;
        [SerializeField, Min(0.01f)] private float minimumVisibleProjectileFlightTime = 0.16f;
        [SerializeField, Min(0f)] private float instantHitVfxDelayPadding = 0.02f;
        [SerializeField] private bool showHitDebug = true;
        [Header("Shot Feedback")]
        [SerializeField] private GameObject muzzleVfxPrefab;
        [SerializeField] private GameObject impactVfxPrefab;
        [SerializeField, Min(0.05f)] private float shotVfxLifetime = 3f;
        [SerializeField] private bool alignShotVfxToShotDirection = true;
        [SerializeField] private bool renderShotVfxAfterNoFogBoss = true;
        [SerializeField, Range(0, 31)] private int shotVfxLayer = 23;
        [SerializeField] private bool addProjectileTrail = true;
        [SerializeField] private bool skipTrailWhenProjectileHasParticles = true;
        [SerializeField, Min(0.01f)] private float projectileTrailDuration = 0.18f;
        [SerializeField, Min(0.001f)] private float projectileTrailWidth = 0.035f;
        [SerializeField] private Color projectileTrailColor = new(1f, 0.78f, 0.18f, 1f);
        [Header("Shot Audio")]
        [SerializeField] private AudioClip rifleShotSound;
        [SerializeField, Range(0f, 1f)] private float rifleShotVolume = 1f;
        [SerializeField] private AudioSource shotAudioSource;
        [Header("Ammo")]
        [SerializeField, Min(1)] private int maxBulletCount = 30;
        [SerializeField, Min(0f)] private float reloadDuration = 3f;
        [SerializeField] private bool autoReloadWhenEmpty = true;
        [SerializeField] private KeyCode reloadKey = KeyCode.R;
        [SerializeField] private TextMeshProUGUI useBulletText;
        [SerializeField] private TextMeshProUGUI reloadCountText;
        [SerializeField] private Image reloadGaugeImage;
        [SerializeField] private GameObject reloadGaugeRoot;
        [SerializeField] private Color normalBulletTextColor = Color.white;
        [SerializeField] private Color lowBulletTextColor = new Color32(0xF8, 0x3E, 0x3C, 0xFF);
        [SerializeField, Min(0)] private int lowBulletWarningThreshold = 5;

        private readonly RaycastHit[] aimHits = new RaycastHit[16];
        private float nextFireTime;
        private float rollInputFireLockoutTimer;
        private float hitFireLockoutTimer;
        private bool deathFireBlocked;
        private int currentBulletCount;
        private float reloadTimer;

        private readonly struct ShotHitResult
        {
            public ShotHitResult(string status, bool hasHit, Vector3 point, Vector3 direction)
            {
                Status = status;
                HasHit = hasHit;
                Point = point;
                Direction = direction;
            }

            public string Status { get; }
            public bool HasHit { get; }
            public Vector3 Point { get; }
            public Vector3 Direction { get; }
        }

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
                ResolveHeroMover();
            }

            currentBulletCount = Mathf.Clamp(currentBulletCount <= 0 ? maxBulletCount : currentBulletCount, 0, maxBulletCount);
            EnsureShotAudioSource();
            ResolveAmmoUI();
            UpdateAmmoUI();
        }

        private void OnEnable()
        {
            if (currentBulletCount <= 0)
            {
                currentBulletCount = maxBulletCount;
            }

            ResolveAmmoUI();
            EnsureShotAudioSource();
            UpdateAmmoUI();
        }

        private void Update()
        {
            UpdateReload();
            UpdateRollInputFireLockout();
            hitFireLockoutTimer = Mathf.Max(0f, hitFireLockoutTimer - Time.deltaTime);

            if (deathFireBlocked || hitFireLockoutTimer > 0f || ShouldBlockFireForRoll())
            {
                return;
            }

            UpdateAimFacing();

            if (Input.GetKeyDown(reloadKey) && currentBulletCount < maxBulletCount)
            {
                BeginReload();
            }

            if (IsReloading)
            {
                return;
            }

            if (!IsFireHeld() || Time.time < nextFireTime)
            {
                return;
            }

            if (currentBulletCount <= 0)
            {
                if (autoReloadWhenEmpty)
                {
                    BeginReload();
                }

                return;
            }

            Fire();
            currentBulletCount = Mathf.Max(0, currentBulletCount - 1);
            UpdateAmmoUI();

            if (currentBulletCount <= 0 && autoReloadWhenEmpty)
            {
                BeginReload();
            }
        }

        private bool IsReloading => reloadTimer > 0f;

        public void ApplyHitFireLockout(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            hitFireLockoutTimer = Mathf.Max(hitFireLockoutTimer, duration);
            nextFireTime = Mathf.Max(nextFireTime, Time.time + duration);
        }

        public void SetDeathFireBlocked(bool blocked)
        {
            deathFireBlocked = blocked;
            if (blocked)
            {
                hitFireLockoutTimer = 0f;
                nextFireTime = float.PositiveInfinity;
            }
            else if (float.IsPositiveInfinity(nextFireTime))
            {
                nextFireTime = 0f;
            }
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

            SnapBodyToAim();

            var weapon = weaponHolder != null ? weaponHolder.EquippedWeapon : null;
            var fireRate = weapon != null ? weapon.FireRate : 8f;
            nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);

            PlayRifleShotSound();
            weaponHolder?.SnapMountToFirePose();

            var muzzle = weapon != null ? weapon.Muzzle : transform;
            var range = weapon != null ? weapon.Range : 80f;
            var muzzlePosition = ResolveMuzzleWorldPosition(muzzle);
            var direction = GetShotDirection(muzzlePosition, range);
            var spawnPosition = muzzlePosition;
            var damage = weapon != null ? weapon.Damage : 10f;
            var hitResult = useInstantHitDamage
                ? ApplyInstantHitDamage(spawnPosition, direction, damage, range)
                : new ShotHitResult("projectile damage", false, Vector3.zero, direction);
            var visualDistance = hitResult.HasHit ? Vector3.Distance(spawnPosition, hitResult.Point) : 0f;
            var visualLifetime = hitResult.HasHit
                ? Mathf.Clamp(visualDistance / Mathf.Max(1f, projectileSpeed), minimumVisibleProjectileFlightTime, projectileLifetime)
                : projectileLifetime;
            var launchSpeed = hitResult.HasHit && visualLifetime > 0.001f
                ? visualDistance / visualLifetime
                : projectileSpeed;
            var projectileObject = CreateProjectileObject(muzzle, spawnPosition, direction);
            spawnPosition = projectileObject.transform.position;
            ApplyShotVfxLayer(projectileObject, false);

            var projectile = projectileObject.GetComponent<SeoulPlayProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<SeoulPlayProjectile>();
            }

            projectile.Launch(direction, launchSpeed, useInstantHitDamage ? 0f : damage, visualLifetime, transform);
            projectile.ConfigureMotion(0f);
            projectile.ConfigureCollision(!hitResult.HasHit);
            projectile.ConfigureImpactVfx(hitResult.HasHit ? null : impactVfxPrefab, shotVfxLifetime, alignShotVfxToShotDirection);
            projectile.ConfigureForegroundVfxLayer(renderShotVfxAfterNoFogBoss ? shotVfxLayer : -1);
            SpawnMuzzleVfx(muzzle, muzzlePosition, direction);
            if (hitResult.HasHit)
            {
                InvokeImpactVfx(hitResult.Point, hitResult.Direction, visualLifetime + instantHitVfxDelayPadding);
            }

            AddProjectileTrail(projectileObject);
        }

        private void BeginReload()
        {
            if (IsReloading || currentBulletCount >= maxBulletCount)
            {
                return;
            }

            reloadTimer = Mathf.Max(0.01f, reloadDuration);
            UpdateAmmoUI();
        }

        private void UpdateReload()
        {
            if (!IsReloading)
            {
                return;
            }

            reloadTimer = Mathf.Max(0f, reloadTimer - Time.deltaTime);
            if (!IsReloading)
            {
                currentBulletCount = maxBulletCount;
            }

            UpdateAmmoUI();
        }

        private void UpdateAmmoUI()
        {
            if (useBulletText != null)
            {
                useBulletText.text = currentBulletCount.ToString();
                useBulletText.color = currentBulletCount <= lowBulletWarningThreshold
                    ? lowBulletTextColor
                    : normalBulletTextColor;
            }

            var reloadProgress = IsReloading
                ? 1f - Mathf.Clamp01(reloadTimer / Mathf.Max(0.01f, reloadDuration))
                : 0f;

            if (reloadGaugeRoot != null && reloadGaugeRoot.activeSelf != IsReloading)
            {
                reloadGaugeRoot.SetActive(IsReloading);
            }

            if (reloadGaugeImage != null)
            {
                reloadGaugeImage.fillAmount = reloadProgress;
            }

            if (reloadCountText != null)
            {
                reloadCountText.text = Mathf.CeilToInt(Mathf.Clamp(reloadTimer, 0f, reloadDuration)).ToString();
                if (reloadCountText.gameObject.activeSelf != IsReloading)
                {
                    reloadCountText.gameObject.SetActive(IsReloading);
                }
            }
        }

        private void PlayRifleShotSound()
        {
            if (rifleShotSound == null || rifleShotVolume <= 0f)
            {
                return;
            }

            var audioSource = EnsureShotAudioSource();
            if (audioSource == null)
            {
                return;
            }

            audioSource.PlayOneShot(rifleShotSound, rifleShotVolume);
        }

        private AudioSource EnsureShotAudioSource()
        {
            if (shotAudioSource == null)
            {
                shotAudioSource = GetComponent<AudioSource>();
            }

            if (shotAudioSource == null)
            {
                shotAudioSource = gameObject.AddComponent<AudioSource>();
            }

            shotAudioSource.playOnAwake = false;
            shotAudioSource.loop = false;
            shotAudioSource.spatialBlend = 0f;
            shotAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            return shotAudioSource;
        }

        private void ResolveAmmoUI()
        {
            if (useBulletText == null)
            {
                var textObject = FindSceneObjectByName("Text (TMP)_UseBullet");
                if (textObject != null)
                {
                    useBulletText = textObject.GetComponent<TextMeshProUGUI>();
                }
            }

            if (reloadCountText == null)
            {
                var countObject = FindSceneObjectByName("Text (TMP)_Count");
                if (countObject != null)
                {
                    reloadCountText = countObject.GetComponent<TextMeshProUGUI>();
                }
            }

            if (reloadGaugeImage == null)
            {
                var gaugeObject = FindSceneObjectByName("Image_Guage");
                if (gaugeObject != null)
                {
                    reloadGaugeImage = gaugeObject.GetComponent<Image>();
                }
            }

            if (reloadGaugeRoot == null)
            {
                reloadGaugeRoot = FindSceneObjectByName("Bullet_Reload");
            }
        }

        private static GameObject FindSceneObjectByName(string objectName)
        {
            var activeObject = GameObject.Find(objectName);
            if (activeObject != null)
            {
                return activeObject;
            }

            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var candidate in allObjects)
            {
                if (candidate != null && candidate.scene.IsValid() && candidate.name == objectName)
                {
                    return candidate;
                }
            }

            return null;
        }

        private void SnapBodyToAim()
        {
            if (!rotateBodyToAim || aimCamera == null)
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

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
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

        private Vector3 ResolveMuzzleWorldPosition(Transform muzzle)
        {
            if (muzzle == null)
            {
                return GetFallbackShotOrigin(transform.forward);
            }

            return muzzle.parent != null
                ? muzzle.parent.TransformPoint(muzzle.localPosition)
                : muzzle.position;
        }

        private ShotHitResult ApplyInstantHitDamage(Vector3 origin, Vector3 direction, float damage, float range)
        {
            var hitCount = Physics.SphereCastNonAlloc(
                origin,
                instantHitRadius,
                direction,
                aimHits,
                Mathf.Max(1f, range),
                GetDamageHitMask(),
                QueryTriggerInteraction.Collide);

            var bestDistance = float.PositiveInfinity;
            SeoulPlayDamageable bestDamageable = null;
            Vector3 bestDirection = direction;
            Vector3 bestPoint = origin + direction * Mathf.Max(1f, range);
            Collider bestCollider = null;
            for (var i = 0; i < hitCount; i++)
            {
                var hit = aimHits[i];
                if (hit.collider == null || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (!IsDamageableHitCollider(hit.collider))
                {
                    continue;
                }

                var damageable = ResolveDamageable(hit.collider);
                if (damageable == null || !damageable.IsAlive)
                {
                    continue;
                }

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestDamageable = damageable;
                    bestDirection = hit.normal.sqrMagnitude > 0.001f ? -hit.normal : direction;
                    bestPoint = GetVisibleImpactPoint(origin, direction, hit);
                    bestCollider = hit.collider;
                }
            }

            if (bestDamageable == null)
            {
                if (showHitDebug)
                {
                    Debug.Log($"SeoulPlayShooter instant hit missed from {origin} dir {direction} range {range:0.##}");
                }

                return new ShotHitResult("miss", false, Vector3.zero, direction);
            }

            bestDamageable.TakeDamage(damage, bestDirection, transform);
            if (showHitDebug)
            {
                Debug.Log($"SeoulPlayShooter instant hit {bestDamageable.name} via {bestCollider.name} for {damage:0.##}", bestDamageable);
            }

            return new ShotHitResult($"hit {bestDamageable.name}", true, bestPoint, bestDirection);
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

        private bool ShouldBlockFireForRoll()
        {
            if (!blockFireWhileRolling)
            {
                return false;
            }

            if (rollInputFireLockoutTimer > 0f)
            {
                return true;
            }

            if (heroMover == null)
            {
                ResolveHeroMover();
            }

            if (heroMover == null)
            {
                return false;
            }

            return heroMover.IsFireBlockedByRoll;
        }

        private void UpdateRollInputFireLockout()
        {
            rollInputFireLockoutTimer = Mathf.Max(0f, rollInputFireLockoutTimer - Time.deltaTime);
            if (IsRollInputPressed())
            {
                rollInputFireLockoutTimer = Mathf.Max(rollInputFireLockoutTimer, rollInputFireLockoutDuration);
            }
        }

        private static bool IsRollInputPressed()
        {
            return Input.GetKeyDown(KeyCode.C) || GetButtonDownSafe("B");
        }

        private void ResolveHeroMover()
        {
            heroMover = GetComponent<SimpleHeroMover>();
            if (heroMover != null)
            {
                return;
            }

            heroMover = GetComponentInParent<SimpleHeroMover>();
            if (heroMover != null)
            {
                return;
            }

            heroMover = GetComponentInChildren<SimpleHeroMover>();
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

        private void AddProjectileTrail(GameObject projectileObject)
        {
            if (!addProjectileTrail
                || projectileObject == null
                || projectileObject.GetComponent<TrailRenderer>() != null
                || (skipTrailWhenProjectileHasParticles && projectileObject.GetComponentInChildren<ParticleSystem>() != null))
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

        private void SpawnMuzzleVfx(Transform muzzle, Vector3 position, Vector3 direction)
        {
            if (muzzleVfxPrefab == null)
            {
                return;
            }

            var rotation = muzzle != null ? muzzle.rotation : Quaternion.identity;
            if (alignShotVfxToShotDirection && direction.sqrMagnitude > 0.001f)
            {
                rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }

            var vfxObject = muzzle != null
                ? Instantiate(muzzleVfxPrefab, muzzle)
                : Instantiate(muzzleVfxPrefab, position, rotation);
            vfxObject.transform.position = position;
            vfxObject.transform.rotation = rotation;
            RestartParticlesAtTransform(vfxObject);
            ApplyShotVfxLayer(vfxObject, false);
            Destroy(vfxObject, shotVfxLifetime);
        }

        private void SpawnImpactVfx(Vector3 position, Vector3 direction)
        {
            if (impactVfxPrefab == null)
            {
                return;
            }

            var rotation = Quaternion.identity;
            if (alignShotVfxToShotDirection && direction.sqrMagnitude > 0.001f)
            {
                rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }

            var vfxObject = Instantiate(impactVfxPrefab, position, rotation);
            ApplyShotVfxLayer(vfxObject, false);
            Destroy(vfxObject, shotVfxLifetime);
        }

        private static void RestartParticlesAtTransform(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            var particles = target.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var particle in particles)
            {
                var main = particle.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;

                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particle.Clear(true);
                particle.Play(true);
            }
        }

        private void ApplyShotVfxLayer(GameObject target, bool includeWholeObject)
        {
            if (!renderShotVfxAfterNoFogBoss || target == null)
            {
                return;
            }

            ApplyLayerRecursive(target.transform, shotVfxLayer, includeWholeObject);
        }

        private static void ApplyLayerRecursive(Transform target, int layer, bool includeWholeObject)
        {
            if (target == null || layer < 0 || layer > 31)
            {
                return;
            }

            if (includeWholeObject || target.GetComponent<Renderer>() != null)
            {
                target.gameObject.layer = layer;
            }

            for (var i = 0; i < target.childCount; i++)
            {
                ApplyLayerRecursive(target.GetChild(i), layer, includeWholeObject);
            }
        }

        private void InvokeImpactVfx(Vector3 position, Vector3 direction, float delay)
        {
            if (!isActiveAndEnabled)
            {
                SpawnImpactVfx(position, direction);
                return;
            }

            StartCoroutine(SpawnImpactVfxAfterDelay(position, direction, delay));
        }

        private System.Collections.IEnumerator SpawnImpactVfxAfterDelay(Vector3 position, Vector3 direction, float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            SpawnImpactVfx(position, direction);
        }

        private Vector3 GetVisibleImpactPoint(Vector3 origin, Vector3 direction, RaycastHit hit)
        {
            if (hit.collider == null)
            {
                var fallbackDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
                return origin + fallbackDirection * minimumVisibleHitDistance;
            }

            var normal = hit.normal.sqrMagnitude > 0.001f
                ? hit.normal.normalized
                : direction.sqrMagnitude > 0.001f
                    ? -direction.normalized
                    : -transform.forward;
            return hit.point + normal * impactFrontOffset;
        }

        private static bool IsDamageableHitCollider(Collider targetCollider)
        {
            return targetCollider != null;
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

            return null;
        }

        private int GetDamageHitMask()
        {
            return damageHitMask.value != 0 ? damageHitMask.value : Physics.DefaultRaycastLayers;
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

        private static bool GetButtonDownSafe(string buttonName)
        {
            try
            {
                return Input.GetButtonDown(buttonName);
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

        private GameObject CreateProjectileObject(Transform muzzle, Vector3 fallbackPosition, Vector3 direction)
        {
            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            var projectileObject = new GameObject(projectilePrefab != null ? projectilePrefab.name + "_ShotRoot" : "SeoulPlay Bullet");
            if (muzzle != null)
            {
                projectileObject.transform.SetParent(muzzle, false);
                projectileObject.transform.localPosition = Vector3.zero;
                projectileObject.transform.localRotation = Quaternion.identity;
                projectileObject.transform.localScale = Vector3.one;

                var spawnPosition = projectileObject.transform.position;
                projectileObject.transform.SetParent(null, true);
                projectileObject.transform.SetPositionAndRotation(spawnPosition, rotation);
            }
            else
            {
                projectileObject.transform.SetPositionAndRotation(fallbackPosition, rotation);
            }

            if (projectilePrefab == null)
            {
                var defaultPosition = projectileObject.transform.position;
                Destroy(projectileObject);
                return CreateDefaultProjectile(defaultPosition, direction);
            }

            var visualObject = Instantiate(projectilePrefab, projectileObject.transform);
            visualObject.name = projectilePrefab.name + "_Visual";
            visualObject.transform.localPosition = Vector3.zero;
            visualObject.transform.localRotation = Quaternion.identity;
            visualObject.transform.localScale = Vector3.one * projectileVisualScale;

            var prefabProjectile = visualObject.GetComponent<SeoulPlayProjectile>();
            if (prefabProjectile != null)
            {
                Destroy(prefabProjectile);
            }

            var prefabCollider = visualObject.GetComponent<Collider>();
            if (prefabCollider != null)
            {
                prefabCollider.enabled = false;
            }

            projectileObject.AddComponent<SeoulPlayProjectile>();
            RestartParticlesAtTransform(visualObject);
            return projectileObject;
        }
    }
}
