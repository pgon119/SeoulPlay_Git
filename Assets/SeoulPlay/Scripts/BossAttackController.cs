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
        [SerializeField, Min(0f), Tooltip("보스가 플레이어를 발견하고 추적을 시작하는 거리입니다. 이 값 밖으로 나가면 보스가 대기 상태로 돌아갑니다.")]
        private float detectionRange = 30f;

        [SerializeField, Min(0f), Tooltip("보스가 공격할 수 있다고 판단하는 최대 거리입니다. Detection Range보다 크게 잡아도 OnValidate에서 Detection Range 안으로 제한됩니다.")]
        private float attackRange = 12f;

        [SerializeField, Min(0f), Tooltip("보스가 플레이어에게 접근하다가 멈추는 거리입니다. 현재 공격 로직상 실제 투척 거리는 이 값에 가까워집니다.")]
        private float stopDistance = 10f;

        [Header("Attack 1 - Rock Throw")]
        [SerializeField, Min(0.1f), Tooltip("공격이 끝난 뒤 다음 공격을 시작하기까지 기다리는 시간입니다. 값이 작을수록 보스가 더 자주 공격합니다.")]
        private float attackCooldown = 2.5f;

        [SerializeField, Min(0.1f), Tooltip("공격 애니메이션 중 보스가 다른 행동으로 넘어가지 못하게 잠그는 시간입니다. 공격 모션 길이에 맞춰 조정합니다.")]
        private float attackLockDuration = 2.2f;

        [SerializeField, Min(0f), Tooltip("플레이어를 향해 다가갈 때의 이동 속도입니다. 값이 클수록 보스가 빠르게 추적합니다.")]
        private float chaseMoveSpeed = 1.6f;

        [SerializeField, Min(0f), Tooltip("공격 거리 근처에서 멈출 때 사용하는 여유 거리입니다. 값이 크면 보스가 조금 더 멀리서 멈춥니다.")]
        private float chaseStopBuffer = 0.25f;

        [SerializeField, Min(0f), Tooltip("추적 속도가 목표 속도까지 올라가는 가속도입니다. 값이 클수록 보스가 즉시 속도를 냅니다.")]
        private float chaseAcceleration = 6f;

        [SerializeField, Min(0f), Tooltip("플레이어를 바라보도록 회전하는 속도입니다. 값이 클수록 방향 전환이 빠릅니다.")]
        private float rotateSpeed = 360f;

        [SerializeField, Range(0f, 180f), Tooltip("플레이어 방향과 보스 정면의 각도 차이가 이 값보다 크면 이동 애니메이션을 멈추고 회전에 집중합니다.")]
        private float moveAngleThreshold = 15f;

        [SerializeField, Range(0f, 180f), Tooltip("공격을 시작하기 전에 플레이어를 바라봐야 하는 각도 기준입니다. 값이 작을수록 더 정확히 바라본 뒤 공격합니다.")]
        private float attackFacingAngle = 12f;

        [SerializeField, Min(0f), Tooltip("제자리에서 방향만 돌 때 애니메이터에 전달할 이동 속도입니다. 보통 0이면 제자리 회전처럼 보입니다.")]
        private float turnInPlaceMoveSpeed = 0f;

        [SerializeField, Tooltip("켜면 공격 후 쿨다운 중에도 플레이어 방향으로 몸을 돌립니다.")]
        private bool allowRotateDuringCooldown = true;

        [SerializeField, Tooltip("켜면 추적 중 이동 여부, 거리, 각도, 속도 로그를 콘솔에 출력합니다. 디버깅할 때만 켜는 것을 권장합니다.")]
        private bool logChaseMovement;

        [SerializeField, Min(0f), Tooltip("돌 투사체가 플레이어에게 맞았을 때 주는 피해량입니다.")]
        private float projectileDamage = 12f;

        [SerializeField, Min(0f), Tooltip("돌 투사체의 초기 발사 속도입니다. 값이 클수록 빠르고 낮은 궤적으로 날아갑니다.")]
        private float projectileSpeed = 16f;

        [SerializeField, Min(0f), Tooltip("돌 투사체에 적용되는 아래 방향 중력입니다. 값이 클수록 포물선이 빨리 떨어집니다.")]
        private float projectileGravity = 9.5f;

        [SerializeField, Min(0f), Tooltip("날아가는 돌의 회전 속도입니다. 시각 효과용 값이라 명중 판정에는 거의 영향이 없습니다.")]
        private float projectileSpin = 360f;

        [SerializeField, Min(0.1f), Tooltip("발사된 돌이 자동으로 사라지기까지의 시간입니다. 너무 작으면 도착 전에 사라질 수 있습니다.")]
        private float projectileLifetime = 4f;

        [SerializeField, Min(0f), Tooltip("보스가 조준할 플레이어 위치의 높이 보정입니다. 값이 클수록 몸통이나 머리 쪽을 향해 던집니다.")]
        private float targetAimHeight = 1.1f;

        [SerializeField, Min(0f), Tooltip("조준 위치에 랜덤으로 더하는 반경입니다. 값이 클수록 돌이 빗나갈 가능성이 커집니다.")]
        private float aimRandomRadius = 1.25f;

        [SerializeField, Tooltip("발사 직후 돌의 보이는 위치를 좌우로 보정하는 값입니다. 양수/음수로 좌우 방향을 바꿀 수 있습니다.")]
        private float projectileVisualSideOffset = -0.75f;

        [SerializeField, Min(0f), Tooltip("발사 직후 돌의 보이는 위치를 아래로 내리는 값입니다. 손 위치와 실제 궤적이 어색할 때 조정합니다.")]
        private float projectileVisualDownOffset = 0f;

        [SerializeField, Min(0f), Tooltip("돌의 시각 위치 보정을 시작하기 전 기다리는 시간입니다.")]
        private float projectileVisualOffsetDelay = 0.1f;

        [SerializeField, Min(0.01f), Tooltip("돌의 시각 위치 보정이 원래 궤적으로 섞여 들어가는 시간입니다. 값이 클수록 부드럽지만 늦게 따라갑니다.")]
        private float projectileVisualOffsetDuration = 0.35f;

        [SerializeField, Min(0f), Tooltip("발사 위치를 보스 정면으로 밀어내는 거리입니다. 값이 크면 돌이 보스 몸 안에서 나오지 않습니다.")]
        private float spawnForwardOffset = 0.8f;

        [SerializeField, Min(0.05f), Tooltip("돌 프리팹에 별도 스케일이 없을 때 사용하는 기본 크기입니다.")]
        private float defaultRockScale = 0.45f;

        [SerializeField, Tooltip("켜면 보스가 자동으로 플레이어를 추적하고 공격합니다. 끄면 대기 상태에 머뭅니다.")]
        private bool autoAttack = true;

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

            if (targetAimHeight <= 0f)
            {
                var footTarget = FindChildTransform(target, FootTargetName);
                if (footTarget != null)
                {
                    return footTarget.position;
                }
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
