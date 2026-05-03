using UnityEngine;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class BossAttackController : MonoBehaviour
    {
        private const string FootTargetName = "ProjectileTarget_Foot";
        private static readonly int Attack01Hash = Animator.StringToHash("Attack01");
        private static readonly int Attack02Hash = Animator.StringToHash("Attack02");
        private static readonly int Attack03Hash = Animator.StringToHash("Attack03");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");

        public enum BossState
        {
            Idle,
            Chase,
            Attack,
            Cooldown,
            Dead
        }

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform target;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private GameObject rockProjectilePrefab;
        [SerializeField] private GameObject heldRockObject;

        [Header("AI Ranges")]
        [SerializeField, Min(0f)] private float detectionRange = 30f;
        [SerializeField, Min(0f)] private float attackRange = 12f;
        [SerializeField, Min(0f)] private float stopDistance = 10f;

        [Header("Attack 1 - Rock Throw")]
        [SerializeField, Min(0.1f)] private float attackCooldown = 2.5f;
        [SerializeField, Min(0.1f)] private float attackLockDuration = 2.2f;
        [SerializeField, Min(0f)] private float chaseMoveSpeed = 1.6f;
        [SerializeField, Min(0f)] private float chaseStopBuffer = 0.25f;
        [SerializeField, Min(0f)] private float chaseAcceleration = 6f;
        [SerializeField, Min(0f)] private float rotateSpeed = 360f;
        [SerializeField, Range(0f, 180f)] private float moveAngleThreshold = 15f;
        [SerializeField, Range(0f, 180f)] private float attackFacingAngle = 12f;
        [SerializeField, Min(0f)] private float turnInPlaceMoveSpeed = 0f;
        [SerializeField] private bool allowRotateDuringCooldown = true;
        [SerializeField] private bool logChaseMovement;
        [SerializeField, Min(0f)] private float projectileDamage = 12f;
        [SerializeField, Min(0f), Tooltip("Initial launch speed for the boss rock projectile.")]
        private float projectileSpeed = 16f;
        [SerializeField, Min(0f), Tooltip("Custom downward gravity applied to the boss rock projectile. Higher values make the arc drop faster.")]
        private float projectileGravity = 9.5f;
        [SerializeField, Min(0f)] private float projectileSpin = 360f;
        [SerializeField, Min(0.1f)] private float projectileLifetime = 4f;
        [SerializeField, Min(0f)] private float targetAimHeight = 1.1f;
        [SerializeField, Min(0f)] private float aimRandomRadius = 1.25f;
        [SerializeField] private float projectileVisualSideOffset = -0.75f;
        [SerializeField, Min(0f)] private float projectileVisualDownOffset = 0f;
        [SerializeField, Min(0f)] private float projectileVisualOffsetDelay = 0.1f;
        [SerializeField, Min(0.01f)] private float projectileVisualOffsetDuration = 0.35f;
        [SerializeField, Min(0f)] private float spawnForwardOffset = 0.8f;
        [SerializeField, Min(0.05f)] private float defaultRockScale = 0.45f;
        [SerializeField] private bool autoAttack = true;

        private SeoulPlayDamageable damageable;
        [SerializeField] private BossState currentState = BossState.Idle;
        private float attackLockedUntil;
        private float cooldownEndsAt;
        private float currentChaseSpeed;
        private bool attack1RockFired;
        private GameObject attack1RockClone;

        public BossState CurrentState => currentState;

        private void Awake()
        {
            damageable = GetComponent<SeoulPlayDamageable>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator != null && animator.applyRootMotion)
            {
                animator.applyRootMotion = false;
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
            SetAnimatorMovement(false, 0f);
        }

        private void OnValidate()
        {
            detectionRange = Mathf.Max(0f, detectionRange);
            attackRange = Mathf.Clamp(attackRange, 0f, detectionRange);
            stopDistance = Mathf.Clamp(stopDistance, 0f, attackRange);
            attackCooldown = Mathf.Max(0.1f, attackCooldown);
            attackLockDuration = Mathf.Max(0.1f, attackLockDuration);
            chaseMoveSpeed = Mathf.Max(0f, chaseMoveSpeed);
            chaseAcceleration = Mathf.Max(0f, chaseAcceleration);
            rotateSpeed = Mathf.Max(0f, rotateSpeed);
            turnInPlaceMoveSpeed = Mathf.Max(0f, turnInPlaceMoveSpeed);
        }

        private void Update()
        {
            if (!CanAct())
            {
                ChangeState(BossState.Dead);
            }

            if (!autoAttack && currentState != BossState.Dead)
            {
                ChangeState(BossState.Idle);
                SetAnimatorMovement(false, 0f);
                return;
            }

            switch (currentState)
            {
                case BossState.Idle:
                    UpdateIdleState();
                    break;
                case BossState.Chase:
                    UpdateChaseState();
                    break;
                case BossState.Attack:
                    UpdateAttackState();
                    break;
                case BossState.Cooldown:
                    UpdateCooldownState();
                    break;
                case BossState.Dead:
                    UpdateDeadState();
                    break;
            }
        }

        private void UpdateIdleState()
        {
            currentChaseSpeed = 0f;
            SetAnimatorMovement(false, 0f);

            if (target == null)
            {
                target = FindDefaultTarget();
                if (target == null)
                {
                    return;
                }
            }

            if (GetFlatDistanceToTarget() <= detectionRange)
            {
                ChangeState(BossState.Chase);
            }
        }

        private void UpdateChaseState()
        {
            if (target == null)
            {
                ChangeState(BossState.Idle);
                return;
            }

            var distance = GetFlatDistanceToTarget();
            if (distance > detectionRange)
            {
                ChangeState(BossState.Idle);
                return;
            }

            if (distance <= GetStopDistance())
            {
                currentChaseSpeed = 0f;
                var isFacingTarget = RotateTowardTarget(attackFacingAngle);
                SetAnimatorMovement(false, 0f);

                if (!isFacingTarget)
                {
                    return;
                }

                if (distance <= attackRange && Time.time >= cooldownEndsAt)
                {
                    StartAttack1();
                }
                else if (Time.time < cooldownEndsAt)
                {
                    ChangeState(BossState.Cooldown);
                }

                return;
            }

            ChaseTarget();
        }

        private void UpdateAttackState()
        {
            currentChaseSpeed = 0f;
            SetAnimatorMovement(false, 0f);

            if (Time.time >= attackLockedUntil)
            {
                cooldownEndsAt = Time.time + attackCooldown;
                ChangeState(BossState.Cooldown);
            }
        }

        private void UpdateCooldownState()
        {
            currentChaseSpeed = 0f;
            if (allowRotateDuringCooldown && target != null && GetFlatDistanceToTarget() <= detectionRange)
            {
                RotateTowardTarget(attackFacingAngle);
                SetAnimatorMovement(false, 0f);
            }
            else
            {
                SetAnimatorMovement(false, 0f);
            }

            if (Time.time < cooldownEndsAt)
            {
                return;
            }

            if (target == null || GetFlatDistanceToTarget() > detectionRange)
            {
                ChangeState(BossState.Idle);
                return;
            }

            ChangeState(BossState.Chase);
        }

        private void UpdateDeadState()
        {
            currentChaseSpeed = 0f;
            SetAnimatorMovement(false, 0f);
            DestroyAttack1RockClone();
            HideHeldRock();
        }

        public void StartAttack1()
        {
            if (!CanAct() || currentState == BossState.Attack)
            {
                return;
            }

            ChangeState(BossState.Attack);
            attackLockedUntil = Time.time + attackLockDuration;
            attack1RockFired = false;
            DestroyAttack1RockClone();
            HideHeldRock();
            currentChaseSpeed = 0f;
            SetAnimatorMovement(false, 0f);

            if (animator != null && HasAnimatorParameter(Attack01Hash, AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger(Attack01Hash);
            }
        }

        public void StartAttack2()
        {
            if (!CanAct() || currentState == BossState.Attack)
            {
                return;
            }

            ChangeState(BossState.Attack);
            attackLockedUntil = Time.time + attackLockDuration;
            currentChaseSpeed = 0f;
            SetAnimatorMovement(false, 0f);

            if (animator != null && HasAnimatorParameter(Attack02Hash, AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger(Attack02Hash);
            }
        }

        public void StartAttack3()
        {
            if (!CanAct() || currentState == BossState.Attack)
            {
                return;
            }

            ChangeState(BossState.Attack);
            attackLockedUntil = Time.time + attackLockDuration;
            currentChaseSpeed = 0f;
            SetAnimatorMovement(false, 0f);

            if (animator != null && HasAnimatorParameter(Attack03Hash, AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger(Attack03Hash);
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
            projectile.ConfigureVisualOffset(
                projectileVisualSideOffset,
                projectileVisualDownOffset,
                transform.right,
                projectileVisualOffsetDelay,
                projectileVisualOffsetDuration);
        }

        public void FinishAttack()
        {
            if (currentState != BossState.Attack)
            {
                return;
            }

            attackLockedUntil = Mathf.Min(attackLockedUntil, Time.time);
        }

        public void Attack01_End()
        {
            FinishAttack();
        }

        public void SetTarget(Transform value)
        {
            target = value;
            if (currentState == BossState.Idle && target != null && CanAct())
            {
                ChangeState(BossState.Chase);
            }
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

        private void ChangeState(BossState nextState)
        {
            if (currentState == nextState)
            {
                return;
            }

            currentState = nextState;
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

            var targetPosition = GetProjectileTargetPosition(true);
            if (projectileGravity > 0f && TryGetBallisticDirection(spawnPosition, targetPosition, out var ballisticDirection))
            {
                return ballisticDirection;
            }

            var direction = targetPosition - spawnPosition;
            return direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
        }

        private Vector3 GetProjectileTargetPosition(bool includeRandomOffset)
        {
            var targetPosition = GetBaseProjectileTargetPosition();
            if (includeRandomOffset && aimRandomRadius > 0f)
            {
                var randomOffset = Random.insideUnitCircle * aimRandomRadius;
                targetPosition += new Vector3(randomOffset.x, 0f, randomOffset.y);
            }

            return targetPosition;
        }

        private Vector3 GetBaseProjectileTargetPosition()
        {
            if (target == null)
            {
                return transform.position + transform.forward * spawnForwardOffset;
            }

            var footTarget = FindChildTransform(target, FootTargetName);
            if (footTarget != null)
            {
                return footTarget.position;
            }

            return target.position + Vector3.up * targetAimHeight;
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

        private bool RotateTowardTarget(float facingThreshold)
        {
            if (target == null)
            {
                return true;
            }

            var direction = target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return true;
            }

            var targetRotation = GetLookRotation(direction.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime);
            return Quaternion.Angle(transform.rotation, targetRotation) <= facingThreshold;
        }

        private void ChaseTarget()
        {
            if (target == null || chaseMoveSpeed <= 0f)
            {
                SetAnimatorMovement(false, 0f);
                return;
            }

            var offset = target.position - transform.position;
            offset.y = 0f;
            var distance = offset.magnitude;
            var targetDirection = offset.normalized;
            var targetRotation = GetLookRotation(targetDirection);
            var angleBeforeRotation = GetHorizontalForwardAngleTo(targetDirection);

            if (distance <= GetStopDistance() || angleBeforeRotation > moveAngleThreshold)
            {
                currentChaseSpeed = Mathf.MoveTowards(currentChaseSpeed, 0f, chaseAcceleration * Time.deltaTime);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.deltaTime);
                var angleAfterTurnOnly = GetHorizontalForwardAngleTo(targetDirection);
                LogChaseMovement(distance, angleBeforeRotation, angleAfterTurnOnly, false);
                SetAnimatorMovement(false, 0f);
                return;
            }

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime);
            var angleAfterRotation = GetHorizontalForwardAngleTo(targetDirection);

            currentChaseSpeed = Mathf.MoveTowards(
                currentChaseSpeed,
                chaseMoveSpeed,
                chaseAcceleration * Time.deltaTime);
            var forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.001f)
            {
                SetAnimatorMovement(false, 0f);
                return;
            }

            var remainingDistance = distance - GetStopDistance() + chaseStopBuffer;
            var moveDistance = Mathf.Min(currentChaseSpeed * Time.deltaTime, remainingDistance);
            transform.position += forward.normalized * moveDistance;
            LogChaseMovement(distance, angleBeforeRotation, angleAfterRotation, true);
            SetAnimatorMovement(true, currentChaseSpeed);
        }

        private void LogChaseMovement(float distance, float angleBeforeRotation, float angleAfterRotation, bool didMove)
        {
            if (!logChaseMovement)
            {
                return;
            }

            Debug.Log(
                $"Boss Chase | moved={didMove} distance={distance:F2} angleBefore={angleBeforeRotation:F1} angleAfter={angleAfterRotation:F1} speed={currentChaseSpeed:F2}",
                this);
        }

        private Quaternion GetLookRotation(Vector3 direction)
        {
            return Quaternion.LookRotation(direction, Vector3.up);
        }

        private float GetHorizontalForwardAngleTo(Vector3 targetDirection)
        {
            var forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.001f || targetDirection.sqrMagnitude <= 0.001f)
            {
                return 0f;
            }

            return Vector3.Angle(forward.normalized, targetDirection.normalized);
        }

        private float GetStopDistance()
        {
            if (attackRange <= 0f)
            {
                return Mathf.Max(0f, stopDistance);
            }

            return Mathf.Clamp(stopDistance, 0f, attackRange);
        }

        private void SetAnimatorMovement(bool isMoving, float moveSpeed)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return;
            }

            if (HasAnimatorParameter(IsMovingHash, AnimatorControllerParameterType.Bool))
            {
                animator.SetBool(IsMovingHash, isMoving);
            }

            if (HasAnimatorParameter(MoveSpeedHash, AnimatorControllerParameterType.Float))
            {
                animator.SetFloat(MoveSpeedHash, isMoving ? moveSpeed : 0f);
            }
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

        private static Transform FindChildTransform(Transform root, string childName)
        {
            if (root == null || string.IsNullOrEmpty(childName))
            {
                return null;
            }

            foreach (var child in root.GetComponentsInChildren<Transform>(true))
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

        private bool HasAnimatorParameter(int hash, AnimatorControllerParameterType parameterType)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return false;
            }

            foreach (var parameter in animator.parameters)
            {
                if (parameter.nameHash == hash && parameter.type == parameterType)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.45f, 0.65f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.color = new Color(1f, 0.45f, 0.05f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
            Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.45f);
            Gizmos.DrawWireSphere(transform.position, GetStopDistance());

            var spawnTransform = heldRockObject != null ? heldRockObject.transform : projectileSpawnPoint;
            var spawnPosition = spawnTransform != null
                ? spawnTransform.position
                : transform.position + Vector3.up * targetAimHeight + transform.forward * spawnForwardOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(spawnPosition, 0.15f);
            Gizmos.DrawRay(spawnPosition, transform.forward * 1.5f);

            if (target == null)
            {
                return;
            }

            var targetPosition = GetProjectileTargetPosition(false);
            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.8f);
            Gizmos.DrawWireSphere(targetPosition, Mathf.Max(0.1f, aimRandomRadius));

            if (!TryGetBallisticDirection(spawnPosition, targetPosition, out var launchDirection))
            {
                launchDirection = (targetPosition - spawnPosition).normalized;
            }

            var velocity = launchDirection * projectileSpeed;
            var previous = spawnPosition;
            var step = 0.08f;
            var maxTime = Mathf.Min(projectileLifetime, 3f);

            for (var time = step; time <= maxTime; time += step)
            {
                var next = spawnPosition + velocity * time;
                if (projectileGravity > 0f)
                {
                    next += 0.5f * Physics.gravity.normalized * projectileGravity * time * time;
                }

                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
