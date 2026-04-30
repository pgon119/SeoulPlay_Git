using UnityEngine;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class BossAttackController : MonoBehaviour
    {
        private static readonly int Attack01Hash = Animator.StringToHash("Attack01");

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform target;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private GameObject rockProjectilePrefab;
        [SerializeField] private GameObject heldRockObject;

        [Header("Attack 1 - Rock Throw")]
        [SerializeField, Min(0f)] private float attackRange = 24f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 2.5f;
        [SerializeField, Min(0.1f)] private float attackLockDuration = 1.2f;
        [SerializeField] private bool rotateTowardTarget;
        [SerializeField, Min(0f)] private float turnSpeed = 360f;
        [SerializeField, Min(0f)] private float projectileDamage = 12f;
        [SerializeField, Min(0f)] private float projectileSpeed = 18f;
        [SerializeField, Min(0f)] private float projectileGravity = 7f;
        [SerializeField, Min(0f)] private float projectileSpin = 360f;
        [SerializeField, Min(0.1f)] private float projectileLifetime = 4f;
        [SerializeField, Min(0f)] private float targetAimHeight = 1.1f;
        [SerializeField, Min(0f)] private float spawnForwardOffset = 0.8f;
        [SerializeField, Min(0.05f)] private float defaultRockScale = 0.45f;
        [SerializeField] private bool autoAttack = true;

        private SeoulPlayDamageable damageable;
        private float nextAttackTime;
        private float attackLockedUntil;
        private bool attack1RockFired;
        private GameObject attack1RockClone;

        private void Awake()
        {
            damageable = GetComponent<SeoulPlayDamageable>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (target == null)
            {
                target = FindDefaultTarget();
            }

            if (heldRockObject == null)
            {
                heldRockObject = FindChildGameObject("Boss_1_Attack _1_RockProjectile");
            }

            if (projectileSpawnPoint == null)
            {
                var holdPoint = FindChildTransform("Attack_1_RockHoldPoint");
                projectileSpawnPoint = holdPoint != null ? holdPoint : heldRockObject != null ? heldRockObject.transform : null;
            }

            HideHeldRock();
        }

        private void Update()
        {
            if (!autoAttack || !CanAct())
            {
                return;
            }

            if (target == null)
            {
                target = FindDefaultTarget();
                if (target == null)
                {
                    return;
                }
            }

            if (rotateTowardTarget)
            {
                RotateTowardTarget();
            }

            if (Time.time < nextAttackTime || Time.time < attackLockedUntil)
            {
                return;
            }

            if (GetFlatDistanceToTarget() <= attackRange)
            {
                StartAttack1();
            }
        }

        public void StartAttack1()
        {
            if (!CanAct())
            {
                return;
            }

            nextAttackTime = Time.time + attackCooldown;
            attackLockedUntil = Time.time + attackLockDuration;
            attack1RockFired = false;
            DestroyAttack1RockClone();
            HideHeldRock();

            if (animator != null && HasAnimatorParameter(Attack01Hash))
            {
                animator.SetTrigger(Attack01Hash);
            }
        }

        public void FireAttack1Rock()
        {
            if (!CanAct() || attack1RockFired)
            {
                return;
            }

            if (attack1RockClone == null)
            {
                CreateAttack1RockClone();
            }

            if (attack1RockClone == null)
            {
                return;
            }

            attack1RockFired = true;
            var projectileObject = attack1RockClone;
            attack1RockClone = null;

            var spawnPosition = projectileObject.transform.position;
            var direction = GetProjectileDirection(spawnPosition);

            projectileObject.transform.SetParent(null, true);
            projectileObject.SetActive(true);
            HideHeldRock();
            EnsureProjectilePhysics(projectileObject);

            var projectile = projectileObject.GetComponent<SeoulPlayProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<SeoulPlayProjectile>();
            }

            projectile.Launch(direction, projectileSpeed, projectileDamage, projectileLifetime, transform);
            projectile.ConfigureMotion(projectileGravity, projectileSpin);
        }

        public void SetTarget(Transform value)
        {
            target = value;
        }

        public void ShowHeldRock()
        {
            CreateAttack1RockClone();
        }

        public void HideHeldRock()
        {
            if (heldRockObject != null)
            {
                heldRockObject.SetActive(false);
            }
        }

        public void CreateAttack1RockClone()
        {
            if (!CanAct() || attack1RockFired)
            {
                return;
            }

            DestroyAttack1RockClone();
            HideHeldRock();

            var guideTransform = heldRockObject != null ? heldRockObject.transform : projectileSpawnPoint;
            if (heldRockObject != null)
            {
                attack1RockClone = Instantiate(heldRockObject, guideTransform.parent);
                attack1RockClone.transform.localPosition = guideTransform.localPosition;
                attack1RockClone.transform.localRotation = guideTransform.localRotation;
                attack1RockClone.transform.localScale = guideTransform.localScale;
            }
            else if (rockProjectilePrefab != null)
            {
                var spawnPosition = GetProjectileSpawnPosition(guideTransform);
                var spawnRotation = guideTransform != null ? guideTransform.rotation : transform.rotation;
                attack1RockClone = Instantiate(rockProjectilePrefab, spawnPosition, spawnRotation, guideTransform);
            }
            else
            {
                var spawnPosition = GetProjectileSpawnPosition(guideTransform);
                var spawnRotation = guideTransform != null ? guideTransform.rotation : transform.rotation;
                attack1RockClone = CreateDefaultRock(spawnPosition, spawnRotation);
                if (guideTransform != null)
                {
                    attack1RockClone.transform.SetParent(guideTransform, true);
                }
            }

            attack1RockClone.name = "Boss Attack 1 Rock Clone";
            attack1RockClone.SetActive(true);
        }

        private void DestroyAttack1RockClone()
        {
            if (attack1RockClone == null)
            {
                return;
            }

            Destroy(attack1RockClone);
            attack1RockClone = null;
        }

        private bool CanAct()
        {
            return damageable == null || damageable.IsAlive;
        }

        private Vector3 GetProjectileSpawnPosition(Transform spawnTransform)
        {
            if (spawnTransform != null)
            {
                return spawnTransform.position;
            }

            return transform.position + Vector3.up * targetAimHeight + transform.forward * spawnForwardOffset;
        }

        private void EnsureProjectilePhysics(GameObject projectileObject)
        {
            if (projectileObject.GetComponentInChildren<Collider>() == null)
            {
                var collider = projectileObject.AddComponent<SphereCollider>();
                collider.isTrigger = true;
            }

            foreach (var targetCollider in projectileObject.GetComponentsInChildren<Collider>())
            {
                targetCollider.isTrigger = true;
            }

            var rigidbody = projectileObject.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = projectileObject.AddComponent<Rigidbody>();
            }

            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        private Vector3 GetProjectileDirection(Vector3 spawnPosition)
        {
            if (target == null)
            {
                return transform.forward;
            }

            var targetPosition = target.position + Vector3.up * targetAimHeight;
            if (projectileGravity > 0f && TryGetBallisticDirection(spawnPosition, targetPosition, out var ballisticDirection))
            {
                return ballisticDirection;
            }

            var direction = targetPosition - spawnPosition;
            return direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
        }

        private bool TryGetBallisticDirection(Vector3 origin, Vector3 targetPosition, out Vector3 launchDirection)
        {
            launchDirection = transform.forward;

            var displacement = targetPosition - origin;
            var horizontal = new Vector3(displacement.x, 0f, displacement.z);
            var horizontalDistance = horizontal.magnitude;
            if (horizontalDistance <= 0.001f || projectileSpeed <= 0.001f || projectileGravity <= 0.001f)
            {
                return false;
            }

            var speedSquared = projectileSpeed * projectileSpeed;
            var gravityValue = projectileGravity;
            var discriminant = speedSquared * speedSquared -
                gravityValue * (gravityValue * horizontalDistance * horizontalDistance + 2f * displacement.y * speedSquared);
            if (discriminant < 0f)
            {
                return false;
            }

            var angle = Mathf.Atan((speedSquared - Mathf.Sqrt(discriminant)) / (gravityValue * horizontalDistance));
            var horizontalDirection = horizontal / horizontalDistance;
            launchDirection = horizontalDirection * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle);
            return launchDirection.sqrMagnitude > 0.001f;
        }

        private void RotateTowardTarget()
        {
            if (target == null)
            {
                return;
            }

            var direction = target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime);
        }

        private float GetFlatDistanceToTarget()
        {
            if (target == null)
            {
                return float.PositiveInfinity;
            }

            var offset = target.position - transform.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        private Transform FindDefaultTarget()
        {
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                return playerObject.transform;
            }

            var hero = FindObjectOfType<SimpleHeroMover>();
            return hero != null ? hero.transform : null;
        }

        private GameObject FindChildGameObject(string childName)
        {
            var child = FindChildTransform(childName);
            return child != null ? child.gameObject : null;
        }

        private Transform FindChildTransform(string childName)
        {
            if (string.IsNullOrEmpty(childName))
            {
                return null;
            }

            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }

        private GameObject CreateDefaultRock(Vector3 position, Quaternion rotation)
        {
            var rockObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rockObject.name = "Boss Rock Projectile";
            rockObject.transform.SetPositionAndRotation(position, rotation);
            rockObject.transform.localScale = Vector3.one * defaultRockScale;

            var collider = rockObject.GetComponent<SphereCollider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            var rigidbody = rockObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            return rockObject;
        }

        private bool HasAnimatorParameter(int hash)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return false;
            }

            foreach (var parameter in animator.parameters)
            {
                if (parameter.nameHash == hash && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.45f, 0.05f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, attackRange);

            var spawnTransform = heldRockObject != null ? heldRockObject.transform : projectileSpawnPoint;
            var spawnPosition = spawnTransform != null
                ? spawnTransform.position
                : transform.position + Vector3.up * targetAimHeight + transform.forward * spawnForwardOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(spawnPosition, 0.15f);
            Gizmos.DrawRay(spawnPosition, transform.forward * 1.5f);
        }
    }
}
