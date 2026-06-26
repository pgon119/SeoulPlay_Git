using System.Collections;
using System.Collections.Generic;
using PixPlays.ElementalVFX;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Playables;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class BossAttackController : MonoBehaviour
    {
        private const string FootTargetName = "ProjectileTarget_Foot";
        private const string EarthBlastPillarUnitName = "EarthBlast_PillarUnit_BossSkill";
        private static readonly int Attack01Hash = Animator.StringToHash("Attack01");
        private static readonly int Attack02Hash = Animator.StringToHash("Attack02");
        private static readonly int Attack03Hash = Animator.StringToHash("Attack03");
        private static readonly int Attack03StateHash = Animator.StringToHash("Attack_03");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");

        public enum BossState
        {
            Idle,
            Chase,
            AttackSelect,
            Attack,
            Cooldown,
            Dead
        }

        public enum BossAttackType
        {
            None,
            Attack1RockThrow,
            Attack2EarthBlast,
            Attack3JumpSlam
        }

        private struct AttackCandidate
        {
            public BossAttackType AttackType;
            public float Weight;

            public AttackCandidate(BossAttackType attackType, float weight)
            {
                AttackType = attackType;
                Weight = weight;
            }
        }

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform target;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private GameObject rockProjectilePrefab;
        [SerializeField] private GameObject heldRockObject;
        [SerializeField] private GameObject attack2VfxPrefab;
        [SerializeField] private GameObject attack2PillarVfxPrefab;
        [SerializeField] private Transform attack2Origin;

        [Header("AI Ranges")]
        [SerializeField, Min(0f), Tooltip("보스가 플레이어를 발견하고 추적을 시작하는 거리입니다. 이 값 밖으로 나가면 보스가 대기 상태로 돌아갑니다.")]
        private float detectionRange = 30f;

        [SerializeField, Min(0f), Tooltip("보스가 공격할 수 있다고 판단하는 최대 거리입니다. Detection Range보다 크게 잡아도 OnValidate에서 Detection Range 안으로 제한됩니다.")]
        private float attackRange = 12f;

        [SerializeField, Min(0f), Tooltip("보스가 플레이어에게 접근하다가 멈추는 거리입니다. 현재 공격 로직상 실제 투척 거리는 이 값에 가까워집니다.")]
        private float stopDistance = 10f;

        [SerializeField, Min(0f)] private float closeRangeDistance = 5f;
        [SerializeField, Min(0f)] private float midRangeDistance = 8f;
        [SerializeField, Min(0f)] private float longRangeDistance = 12f;

        [Header("AI Cooldowns")]
        [SerializeField, Min(0.1f), FormerlySerializedAs("attackCooldown")]
        private float globalAttackCooldown = 2.5f;

        [SerializeField, Min(0.1f)] private float attack1Cooldown = 2f;
        [SerializeField, Min(0.1f)] private float attack2Cooldown = 4f;
        [SerializeField, Min(0.1f)] private float attack3Cooldown = 7f;
        [SerializeField, Range(0f, 1f)] private float repeatAttackWeightMultiplier = 0.3f;

        [Header("AI Weights")]
        [SerializeField, Min(0f)] private float longRangeAttack1Weight = 80f;
        [SerializeField, Min(0f)] private float longRangeAttack3Weight = 20f;
        [SerializeField, Min(0f)] private float midRangeAttack2Weight = 70f;
        [SerializeField, Min(0f)] private float midRangeAttack1Weight = 30f;
        [SerializeField, Min(0f)] private float closeRangeAttack3Weight = 70f;
        [SerializeField, Min(0f)] private float closeRangeAttack2Weight = 20f;
        [SerializeField, Min(0f)] private float closeRangeAttack1Weight = 10f;

        [Header("Attack 1 - Rock Throw")]
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

        [Header("Attack 1 - Bullet Fan")]
        [SerializeField] private GameObject attack1BulletPrefab;
        [SerializeField, Min(1)] private int attack1BulletCount = 10;
        [SerializeField, Range(0f, 180f)] private float attack1FanAngle = 45f;
        [SerializeField, Min(0f)] private float attack1BulletInterval = 0.08f;
        [SerializeField, Min(0f)] private float attack1BulletDamage = 10f;
        [SerializeField, Min(0.1f)] private float attack1BulletSpeed = 8f;
        [SerializeField, Min(0.1f)] private float attack1BulletLifetime = 4f;
        [SerializeField, Min(0.02f)] private float attack1BulletRadius = 0.22f;
        [SerializeField, Min(0f)] private float attack1BulletSpawnHeight = 1.1f;
        [SerializeField, Min(0f)] private float attack1BulletSpawnForwardOffset = 1f;
        [SerializeField] private Vector3 attack1BulletCenterOffset = new Vector3(0f, 0f, 0.76f);
        [SerializeField] private Color attack1BulletColor = new Color(1f, 0.25f, 0.12f, 1f);

        [Header("Attack 2 - Earth Blast")]
        [SerializeField, Min(0f)] private float attack2Damage = 18f;
        [SerializeField, FormerlySerializedAs("attack2PillarBaseRadius"), Min(0.05f)] private float attack2StartRadius = 0.7f;
        [SerializeField, FormerlySerializedAs("attack2Range"), Min(0.1f)] private float attack2BoxLength = 8f;
        [SerializeField, FormerlySerializedAs("attack2Radius"), Min(0.05f)] private float attack2BoxWidth = 1.5f;
        [SerializeField, Min(0.05f)] private float attack2BoxHeight = 2.5f;
        [SerializeField, Min(0f)] private float attack2ForwardOffset = 1.2f;
        [SerializeField, Range(0f, 90f)] private float attack2SideAngle = 35f;
        [SerializeField, Min(1)] private int attack2PillarCount = 6;
        [SerializeField, Min(0.05f)] private float attack2PillarSpacing = 1.2f;
        [SerializeField, Min(0f)] private float attack2PillarDelayStep = 0.07f;
        [SerializeField, Min(0.05f)] private float attack2PillarVfxBaseScale = 0.7f;
        [SerializeField, Min(0f)] private float attack2PillarVfxScaleStep = 0.25f;
        [SerializeField, Min(0.1f)] private float attack2VfxDuration = 2f;
        [SerializeField, Min(0f)] private float attack2VfxDestroyDelay = 3f;
        [SerializeField] private float attack2SupportVfxZOffset = 0f;
        [SerializeField] private LayerMask attack2HitMask = ~0;
        [SerializeField] private bool attack2HitTriggers = true;
        [SerializeField] private bool attack2ParticlesOnly = true;
        [SerializeField] private bool useAttack2InAutoAttack = true;
        [SerializeField, Min(0f)] private float attack2KnockbackDistance = 1.6f;
        [SerializeField, Min(0f)] private float attack2KnockbackDuration = 0.24f;
        [SerializeField] private bool showAttack2DamageArea;
        [SerializeField, Min(0.05f)] private float attack2DamageAreaDuration = 0.45f;
        [SerializeField] private Color attack2DamageAreaColor = new Color(1f, 0.2f, 0.05f, 0.24f);

        [Header("Attack 3 - Jump Slam")]
        [SerializeField, Min(0f)] private float attack3Damage = 24f;
        [SerializeField, Min(0.05f)] private float attack3DamageRadius = 3f;
        [SerializeField, Min(0f)] private float attack3MinRange = 6f;
        [SerializeField, Min(0f)] private float attack3MaxRange = 12f;
        [SerializeField, Min(0f)] private float attack3LandingOffset = 0f;
        [SerializeField, Min(0.05f)] private float attack3JumpMoveDuration = 0.45f;
        [SerializeField] private GameObject attack3ImpactVfxPrefab;
        [SerializeField, Min(0.1f)] private float attack3VfxDuration = 2f;
        [SerializeField, Min(0f)] private float attack3VfxDestroyDelay = 3f;
        [SerializeField] private LayerMask attack3HitMask = ~0;
        [SerializeField] private bool attack3HitTriggers = true;
        [SerializeField] private bool attack3ParticlesOnly = true;
        [SerializeField] private bool useAttack3InAutoAttack = true;

        [SerializeField, Tooltip("켜면 보스가 자동으로 플레이어를 추적하고 공격합니다. 끄면 대기 상태에 머뭅니다.")]
        private bool autoAttack = true;

        private SeoulPlayDamageable damageable;
        [SerializeField] private BossState currentState = BossState.Idle;
        private float attackLockedUntil;
        private float cooldownEndsAt;
        private float attack1ReadyAt;
        private float attack2ReadyAt;
        private float attack3ReadyAt;
        private float currentChaseSpeed;
        private BossAttackType currentAttackType = BossAttackType.None;
        private BossAttackType lastAttackType = BossAttackType.None;
        private Vector3 lockedAttackDirection = Vector3.forward;
        private Vector3 lockedTargetPosition;
        private bool hasLockedAttackTarget;
        private bool attack1RockFired;
        private GameObject attack1RockClone;
        private bool attack2EarthBlastFired;
        private bool attack3SlamFired;
        private bool attack3ImpactVfxFired;
        private Vector3 lockedAttack3ImpactPosition;
        private bool hasLockedAttack3ImpactPosition;
        private Coroutine attack1BulletFanRoutine;
        private Coroutine attack3JumpRoutine;
        private Material attack1BulletMaterial;
        private Material attack2DamageAreaMaterial;

        public BossState CurrentState => currentState;
        public bool AutoAttackEnabled => autoAttack;

        private void Awake()
        {
            damageable = GetComponent<SeoulPlayDamageable>();

            ResolveAnimatorReference();
            ConfigureAnimatorForRuntime();

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

        private void OnEnable()
        {
            ResolveAnimatorReference();
            ConfigureAnimatorForRuntime();
        }

        private void OnDestroy()
        {
            if (attack1BulletMaterial != null)
            {
                DestroyRuntimeMaterial(attack1BulletMaterial);
                attack1BulletMaterial = null;
            }

            if (attack2DamageAreaMaterial != null)
            {
                DestroyRuntimeMaterial(attack2DamageAreaMaterial);
                attack2DamageAreaMaterial = null;
            }
        }

        private static void DestroyRuntimeMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(material);
            }
            else
            {
                DestroyImmediate(material);
            }
        }

        private void OnValidate()
        {
            detectionRange = Mathf.Max(0f, detectionRange);
            attackRange = Mathf.Clamp(attackRange, 0f, detectionRange);
            stopDistance = Mathf.Clamp(stopDistance, 0f, attackRange);
            closeRangeDistance = Mathf.Max(0f, closeRangeDistance);
            midRangeDistance = Mathf.Max(closeRangeDistance, midRangeDistance);
            longRangeDistance = Mathf.Max(midRangeDistance, longRangeDistance);
            globalAttackCooldown = Mathf.Max(0.1f, globalAttackCooldown);
            attack1Cooldown = Mathf.Max(0.1f, attack1Cooldown);
            attack2Cooldown = Mathf.Max(0.1f, attack2Cooldown);
            attack3Cooldown = Mathf.Max(0.1f, attack3Cooldown);
            attackLockDuration = Mathf.Max(0.1f, attackLockDuration);
            chaseMoveSpeed = Mathf.Max(0f, chaseMoveSpeed);
            chaseAcceleration = Mathf.Max(0f, chaseAcceleration);
            rotateSpeed = Mathf.Max(0f, rotateSpeed);
            turnInPlaceMoveSpeed = Mathf.Max(0f, turnInPlaceMoveSpeed);
            attack1BulletCount = Mathf.Max(1, attack1BulletCount);
            attack1FanAngle = Mathf.Clamp(attack1FanAngle, 0f, 180f);
            attack1BulletInterval = Mathf.Max(0f, attack1BulletInterval);
            attack1BulletDamage = Mathf.Max(0f, attack1BulletDamage);
            attack1BulletSpeed = Mathf.Max(0.1f, attack1BulletSpeed);
            attack1BulletLifetime = Mathf.Max(0.1f, attack1BulletLifetime);
            attack1BulletRadius = Mathf.Max(0.02f, attack1BulletRadius);
            attack1BulletSpawnHeight = Mathf.Max(0f, attack1BulletSpawnHeight);
            attack1BulletSpawnForwardOffset = Mathf.Max(0f, attack1BulletSpawnForwardOffset);
            attack2Damage = Mathf.Max(0f, attack2Damage);
            attack2StartRadius = Mathf.Max(0.05f, attack2StartRadius);
            attack2BoxLength = Mathf.Max(0.1f, attack2BoxLength);
            attack2BoxWidth = Mathf.Max(0.05f, attack2BoxWidth);
            attack2BoxHeight = Mathf.Max(0.05f, attack2BoxHeight);
            attack2ForwardOffset = Mathf.Max(0f, attack2ForwardOffset);
            attack2PillarCount = Mathf.Max(1, attack2PillarCount);
            attack2PillarSpacing = Mathf.Max(0.05f, attack2PillarSpacing);
            attack2PillarDelayStep = Mathf.Max(0f, attack2PillarDelayStep);
            attack2PillarVfxBaseScale = Mathf.Max(0.05f, attack2PillarVfxBaseScale);
            attack2PillarVfxScaleStep = Mathf.Max(0f, attack2PillarVfxScaleStep);
            attack2VfxDuration = Mathf.Max(0.1f, attack2VfxDuration);
            attack2VfxDestroyDelay = Mathf.Max(0f, attack2VfxDestroyDelay);
            attack2KnockbackDistance = Mathf.Max(0f, attack2KnockbackDistance);
            attack2KnockbackDuration = Mathf.Max(0f, attack2KnockbackDuration);
            attack2DamageAreaDuration = Mathf.Max(0.05f, attack2DamageAreaDuration);
            attack3Damage = Mathf.Max(0f, attack3Damage);
            attack3DamageRadius = Mathf.Max(0.05f, attack3DamageRadius);
            attack3MinRange = Mathf.Max(0f, attack3MinRange);
            attack3MaxRange = Mathf.Max(attack3MinRange, attack3MaxRange);
            attack3LandingOffset = Mathf.Max(0f, attack3LandingOffset);
            attack3JumpMoveDuration = Mathf.Max(0.05f, attack3JumpMoveDuration);
            attack3VfxDuration = Mathf.Max(0.1f, attack3VfxDuration);
            attack3VfxDestroyDelay = Mathf.Max(0f, attack3VfxDestroyDelay);
        }

        private void Update()
        {
            if (!CanAct())
            {
                ChangeState(BossState.Dead);
            }

            if (!autoAttack && currentState != BossState.Dead && currentState != BossState.Attack)
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
                case BossState.AttackSelect:
                    UpdateAttackSelectState();
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
            currentAttackType = BossAttackType.None;
            hasLockedAttackTarget = false;
            hasLockedAttack3ImpactPosition = false;
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

            if (distance <= GetAttackSelectRange())
            {
                currentChaseSpeed = 0f;
                var isFacingTarget = RotateTowardTarget(attackFacingAngle);
                SetAnimatorMovement(false, 0f);

                if (!isFacingTarget)
                {
                    return;
                }

                if (Time.time >= cooldownEndsAt)
                {
                    ChangeState(BossState.AttackSelect);
                }
                else if (Time.time < cooldownEndsAt)
                {
                    ChangeState(BossState.Cooldown);
                }

                return;
            }

            ChaseTarget();
        }

        private void UpdateAttackSelectState()
        {
            currentChaseSpeed = 0f;
            SetAnimatorMovement(false, 0f);

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

            if (Time.time < cooldownEndsAt)
            {
                ChangeState(BossState.Cooldown);
                return;
            }

            if (distance > GetAttackSelectRange())
            {
                ChangeState(BossState.Chase);
                return;
            }

            if (!RotateTowardTarget(attackFacingAngle))
            {
                return;
            }

            if (!TrySelectAutoAttack(distance, out var selectedAttack))
            {
                var nextReadyAt = GetNextReadyTime();
                if (!float.IsPositiveInfinity(nextReadyAt) && nextReadyAt > Time.time)
                {
                    cooldownEndsAt = Mathf.Max(cooldownEndsAt, nextReadyAt);
                    ChangeState(BossState.Cooldown);
                    return;
                }

                ChangeState(BossState.Chase);
                return;
            }

            StartSelectedAttack(selectedAttack);
        }

        private void UpdateAttackState()
        {
            currentChaseSpeed = 0f;
            SetAnimatorMovement(false, 0f);

            if (Time.time >= attackLockedUntil)
            {
                cooldownEndsAt = Time.time + globalAttackCooldown;
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

            ChangeState(GetFlatDistanceToTarget() <= GetAttackSelectRange() ? BossState.AttackSelect : BossState.Chase);
        }

        private void UpdateDeadState()
        {
            currentChaseSpeed = 0f;
            currentAttackType = BossAttackType.None;
            hasLockedAttackTarget = false;
            hasLockedAttack3ImpactPosition = false;
            SetAnimatorMovement(false, 0f);
            StopAttack3JumpMove();
            StopAttack1BulletFan();
            DestroyAttack1RockClone();
            HideHeldRock();
        }

        public void StartAttack1()
        {
            if (!CanAct() || currentState == BossState.Attack)
            {
                return;
            }

            BeginAttack(BossAttackType.Attack1RockThrow);
            ChangeState(BossState.Attack);
            attackLockedUntil = Time.time + attackLockDuration;
            attack1RockFired = false;
            attack2EarthBlastFired = true;
            attack3SlamFired = true;
            attack3ImpactVfxFired = true;
            hasLockedAttack3ImpactPosition = false;
            StopAttack1BulletFan();
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

            BeginAttack(BossAttackType.Attack2EarthBlast);
            ChangeState(BossState.Attack);
            attackLockedUntil = Time.time + attackLockDuration;
            attack2EarthBlastFired = false;
            attack3SlamFired = true;
            attack3ImpactVfxFired = true;
            hasLockedAttack3ImpactPosition = false;
            DestroyAttack1RockClone();
            HideHeldRock();
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

            BeginAttack(BossAttackType.Attack3JumpSlam);
            ChangeState(BossState.Attack);
            attack2EarthBlastFired = true;
            attack3SlamFired = false;
            attack3ImpactVfxFired = false;
            hasLockedAttack3ImpactPosition = false;
            DestroyAttack1RockClone();
            HideHeldRock();
            currentChaseSpeed = 0f;
            SetAnimatorMovement(false, 0f);
            attackLockedUntil = Time.time + GetAttack3LockDuration();
            PlayAttack3AnimationFromStart();

        }

        public void FireAttack1Rock()
        {
            FireAttack1BulletFan();
        }

        public void FireAttack1BulletFan()
        {
            if (!CanAct() || attack1RockFired)
            {
                return;
            }

            attack1RockFired = true;
            HideHeldRock();
            StopAttack1BulletFan();
            attack1BulletFanRoutine = StartCoroutine(PlayAttack1BulletFan(GetLockedAttackDirection()));
        }

        public void FinishAttack()
        {
            if (currentState != BossState.Attack)
            {
                return;
            }

            attackLockedUntil = Mathf.Min(attackLockedUntil, Time.time);
            if (currentAttackType == BossAttackType.Attack1RockThrow)
            {
                StopAttack1BulletFan();
            }
        }

        public void Attack01_End()
        {
            FinishAttack();
        }

        public void Attack01_Hit()
        {
            FireAttack1BulletFan();
        }

        public void Attack01_ThrowRock()
        {
            FireAttack1Rock();
        }

        public void FireAttack2EarthBlast()
        {
            if (!CanAct() || attack2EarthBlastFired)
            {
                return;
            }

            attack2EarthBlastFired = true;

            var forward = GetLockedAttackDirection();
            var source = GetAttack2Source(forward);
            var damagedTargets = new HashSet<SeoulPlayDamageable>();

            SpawnAttack2SupportVfx();
            SpawnAttack2StartAreaPreview(source);
            DamageAttack2StartArea(source, forward, damagedTargets);
            FireAttack2EarthBlastLine(source, forward, damagedTargets);

            if (attack2SideAngle <= 0f)
            {
                return;
            }

            FireAttack2EarthBlastLine(source, Quaternion.AngleAxis(attack2SideAngle, Vector3.up) * forward, damagedTargets);
            FireAttack2EarthBlastLine(source, Quaternion.AngleAxis(-attack2SideAngle, Vector3.up) * forward, damagedTargets);
        }

        public void FireAttack3JumpSlam()
        {
            if (!CanAct() || !IsCurrentAttack(BossAttackType.Attack3JumpSlam))
            {
                return;
            }

            if (attack3JumpRoutine != null)
            {
                return;
            }

            if (!hasLockedAttack3ImpactPosition)
            {
                return;
            }

            PerformAttack3Impact();
        }

        public void Attack02_Hit()
        {
            FireAttack2EarthBlast();
        }

        public void StartAttack3JumpSlamMove()
        {
            if (!CanAct() || !IsCurrentAttack(BossAttackType.Attack3JumpSlam))
            {
                return;
            }

            if (attack3JumpRoutine != null || hasLockedAttack3ImpactPosition)
            {
                return;
            }

            StartAttack3JumpMove();
        }

        public void Attack03_Jump()
        {
            StartAttack3JumpSlamMove();
        }

        public void Attack03_Hit()
        {
            FireAttack3JumpSlam();
        }

        public void Attack03_Effect()
        {
            FireAttack3ImpactVfx();
        }

        public void Attack03_Damage()
        {
            DamageAttack3Impact();
        }

        public void AttackSignal()
        {
        }

        public void Enrage_Start()
        {
        }

        private void PerformAttack3Impact()
        {
            FireAttack3ImpactVfx();
            DamageAttack3Impact();
        }

        public void FireAttack3ImpactVfx()
        {
            if (!CanAct() || attack3ImpactVfxFired || !IsCurrentAttack(BossAttackType.Attack3JumpSlam))
            {
                return;
            }

            attack3ImpactVfxFired = true;

            var forward = GetLockedAttackDirection();
            var impactPosition = GetAttack3ImpactPosition(forward);
            SpawnAttack3ImpactVfx(impactPosition, forward);
        }

        public void DamageAttack3Impact()
        {
            if (!CanAct() || attack3SlamFired || !IsCurrentAttack(BossAttackType.Attack3JumpSlam))
            {
                return;
            }

            attack3SlamFired = true;

            var forward = GetLockedAttackDirection();
            var impactPosition = GetAttack3ImpactPosition(forward);
            DamageAttack3Impact(impactPosition, forward);
        }

        private bool TrySelectAutoAttack(float distance, out BossAttackType selectedAttack)
        {
            var candidates = new List<AttackCandidate>(3);
            if (distance >= longRangeDistance)
            {
                AddAttackCandidate(candidates, BossAttackType.Attack1RockThrow, longRangeAttack1Weight);
                AddAttackCandidate(candidates, BossAttackType.Attack3JumpSlam, longRangeAttack3Weight, distance);
            }
            else if (distance >= midRangeDistance)
            {
                AddAttackCandidate(candidates, BossAttackType.Attack2EarthBlast, midRangeAttack2Weight);
                AddAttackCandidate(candidates, BossAttackType.Attack1RockThrow, midRangeAttack1Weight);
            }
            else
            {
                AddAttackCandidate(candidates, BossAttackType.Attack3JumpSlam, closeRangeAttack3Weight, distance);
                AddAttackCandidate(candidates, BossAttackType.Attack2EarthBlast, closeRangeAttack2Weight);
                AddAttackCandidate(candidates, BossAttackType.Attack1RockThrow, closeRangeAttack1Weight);
            }

            selectedAttack = PickWeightedAttack(candidates);
            return selectedAttack != BossAttackType.None;
        }

        private void AddAttackCandidate(List<AttackCandidate> candidates, BossAttackType attackType, float baseWeight)
        {
            AddAttackCandidate(candidates, attackType, baseWeight, -1f);
        }

        private void AddAttackCandidate(List<AttackCandidate> candidates, BossAttackType attackType, float baseWeight, float distance)
        {
            if (baseWeight <= 0f || !IsAttackReady(attackType) || !IsAttackAnimationAvailable(attackType))
            {
                return;
            }

            if (distance >= 0f && !CanUseAttackAtDistance(attackType, distance))
            {
                return;
            }

            var weight = attackType == lastAttackType ? baseWeight * repeatAttackWeightMultiplier : baseWeight;
            if (weight <= 0f)
            {
                return;
            }

            candidates.Add(new AttackCandidate(attackType, weight));
        }

        private bool CanUseAttackAtDistance(BossAttackType attackType, float distance)
        {
            if (attackType != BossAttackType.Attack3JumpSlam)
            {
                return true;
            }

            return useAttack3InAutoAttack && distance >= attack3MinRange && distance <= attack3MaxRange;
        }

        private BossAttackType PickWeightedAttack(List<AttackCandidate> candidates)
        {
            var totalWeight = 0f;
            for (var index = 0; index < candidates.Count; index++)
            {
                totalWeight += candidates[index].Weight;
            }

            if (totalWeight <= 0f)
            {
                return BossAttackType.None;
            }

            var roll = Random.Range(0f, totalWeight);
            for (var index = 0; index < candidates.Count; index++)
            {
                roll -= candidates[index].Weight;
                if (roll <= 0f)
                {
                    return candidates[index].AttackType;
                }
            }

            return candidates[candidates.Count - 1].AttackType;
        }

        private void StartSelectedAttack(BossAttackType selectedAttack)
        {
            switch (selectedAttack)
            {
                case BossAttackType.Attack1RockThrow:
                    StartAttack1();
                    break;
                case BossAttackType.Attack2EarthBlast:
                    StartAttack2();
                    break;
                case BossAttackType.Attack3JumpSlam:
                    StartAttack3();
                    break;
            }
        }

        public void SetTarget(Transform value)
        {
            target = value;
            if (currentState == BossState.Idle && target != null && CanAct())
            {
                ChangeState(BossState.Chase);
            }
        }

        public void SetAutoAttack(bool value)
        {
            autoAttack = value;
            if (!autoAttack && currentState != BossState.Attack && currentState != BossState.Dead)
            {
                currentChaseSpeed = 0f;
                ChangeState(BossState.Idle);
                SetAnimatorMovement(false, 0f);
            }
        }

        public void ResetCooldowns()
        {
            cooldownEndsAt = 0f;
            attack1ReadyAt = 0f;
            attack2ReadyAt = 0f;
            attack3ReadyAt = 0f;
        }

        public void ForceFinishAttack()
        {
            attackLockedUntil = 0f;
            cooldownEndsAt = 0f;
            currentAttackType = BossAttackType.None;
            hasLockedAttackTarget = false;
            hasLockedAttack3ImpactPosition = false;
            attack1RockFired = true;
            attack2EarthBlastFired = true;
            attack3SlamFired = true;
            attack3ImpactVfxFired = true;
            currentChaseSpeed = 0f;

            StopAttack3JumpMove();
            DestroyAttack1RockClone();
            HideHeldRock();
            ResetAttackTriggers();
            SetAnimatorMovement(false, 0f);

            if (CanAct())
            {
                ChangeState(autoAttack && target != null ? BossState.Chase : BossState.Idle);
            }
        }

        public void ShowHeldRock()
        {
            HideHeldRock();
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
            DestroyAttack1RockClone();
            HideHeldRock();
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

        private void BeginAttack(BossAttackType attackType)
        {
            currentAttackType = attackType;
            lastAttackType = attackType;
            LockAttackTarget();
            MarkAttackCooldown(attackType);
        }

        private void LockAttackTarget()
        {
            lockedTargetPosition = target != null ? target.position : transform.position + GetFlatForward();
            var direction = lockedTargetPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = GetFlatForward();
            }

            lockedAttackDirection = direction.normalized;
            hasLockedAttackTarget = true;
            transform.rotation = GetLookRotation(lockedAttackDirection);
        }

        private Vector3 GetLockedAttackDirection()
        {
            if (!hasLockedAttackTarget || lockedAttackDirection.sqrMagnitude <= 0.001f)
            {
                return GetFlatForward();
            }

            return lockedAttackDirection.normalized;
        }

        private bool IsCurrentAttack(BossAttackType attackType)
        {
            return currentState == BossState.Attack && currentAttackType == attackType;
        }

        private void MarkAttackCooldown(BossAttackType attackType)
        {
            var readyAt = Time.time + GetAttackCooldown(attackType);
            switch (attackType)
            {
                case BossAttackType.Attack1RockThrow:
                    attack1ReadyAt = readyAt;
                    break;
                case BossAttackType.Attack2EarthBlast:
                    attack2ReadyAt = readyAt;
                    break;
                case BossAttackType.Attack3JumpSlam:
                    attack3ReadyAt = readyAt;
                    break;
            }
        }

        private bool IsAttackReady(BossAttackType attackType)
        {
            switch (attackType)
            {
                case BossAttackType.Attack1RockThrow:
                    return Time.time >= attack1ReadyAt;
                case BossAttackType.Attack2EarthBlast:
                    return Time.time >= attack2ReadyAt;
                case BossAttackType.Attack3JumpSlam:
                    return Time.time >= attack3ReadyAt;
                default:
                    return false;
            }
        }

        private float GetAttackCooldown(BossAttackType attackType)
        {
            switch (attackType)
            {
                case BossAttackType.Attack1RockThrow:
                    return attack1Cooldown;
                case BossAttackType.Attack2EarthBlast:
                    return attack2Cooldown;
                case BossAttackType.Attack3JumpSlam:
                    return attack3Cooldown;
                default:
                    return 0f;
            }
        }

        private bool IsAttackAnimationAvailable(BossAttackType attackType)
        {
            switch (attackType)
            {
                case BossAttackType.Attack1RockThrow:
                    return animator == null || HasAnimatorParameter(Attack01Hash, AnimatorControllerParameterType.Trigger);
                case BossAttackType.Attack2EarthBlast:
                    return useAttack2InAutoAttack && animator != null && HasAnimatorParameter(Attack02Hash, AnimatorControllerParameterType.Trigger);
                case BossAttackType.Attack3JumpSlam:
                    return useAttack3InAutoAttack && animator != null &&
                        (HasAnimatorParameter(Attack03Hash, AnimatorControllerParameterType.Trigger) ||
                        animator.HasState(0, Attack03StateHash));
                default:
                    return false;
            }
        }

        private float GetNextReadyTime()
        {
            var nextReadyAt = float.PositiveInfinity;
            if (IsAttackAnimationAvailable(BossAttackType.Attack1RockThrow))
            {
                nextReadyAt = Mathf.Min(nextReadyAt, attack1ReadyAt);
            }

            if (IsAttackAnimationAvailable(BossAttackType.Attack2EarthBlast))
            {
                nextReadyAt = Mathf.Min(nextReadyAt, attack2ReadyAt);
            }

            if (IsAttackAnimationAvailable(BossAttackType.Attack3JumpSlam))
            {
                nextReadyAt = Mathf.Min(nextReadyAt, attack3ReadyAt);
            }

            return nextReadyAt;
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
            var targetPosition = currentAttackType == BossAttackType.Attack1RockThrow && hasLockedAttackTarget
                ? lockedTargetPosition + Vector3.up * targetAimHeight
                : GetBaseProjectileTargetPosition();
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

        private Vector3 GetFlatForward()
        {
            var forward = transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
        }

        private Vector3 GetAttack2Source(Vector3 forward)
        {
            var origin = attack2Origin != null ? attack2Origin.position : transform.position;
            return origin + forward * attack2ForwardOffset;
        }

        private Vector3 GetAttack3ImpactPosition(Vector3 forward)
        {
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.001f)
            {
                forward = GetFlatForward();
            }

            if (hasLockedAttack3ImpactPosition)
            {
                return lockedAttack3ImpactPosition;
            }

            return GetAttack3LockedLandingPosition(transform.position, forward.normalized);
        }

        private IEnumerator PlayAttack1BulletFan(Vector3 forward)
        {
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.001f)
            {
                forward = GetFlatForward();
            }

            forward.Normalize();
            var count = Mathf.Max(1, attack1BulletCount);
            var angleStep = count > 1 ? -attack1FanAngle / (count - 1) : 0f;
            var startAngle = count > 1 ? attack1FanAngle * 0.5f : 0f;

            for (var i = 0; i < count; i++)
            {
                if (!CanAct())
                {
                    attack1BulletFanRoutine = null;
                    yield break;
                }

                var angle = startAngle + angleStep * i;
                var direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                var spawnPosition = GetAttack1BulletSpawnPosition(direction);
                SpawnAttack1Bullet(spawnPosition, direction.normalized);

                if (attack1BulletInterval > 0f && i < count - 1)
                {
                    yield return new WaitForSeconds(attack1BulletInterval);
                }
            }

            attack1BulletFanRoutine = null;
        }

        private void StopAttack1BulletFan()
        {
            if (attack1BulletFanRoutine == null)
            {
                return;
            }

            StopCoroutine(attack1BulletFanRoutine);
            attack1BulletFanRoutine = null;
        }

        private void StopAttack3JumpMove()
        {
            if (attack3JumpRoutine == null)
            {
                return;
            }

            StopCoroutine(attack3JumpRoutine);
            attack3JumpRoutine = null;
        }

        private void SpawnAttack1Bullet(Vector3 spawnPosition, Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = GetFlatForward();
            }

            direction.Normalize();
            var spawnRotation = Quaternion.LookRotation(direction, Vector3.up);
            var projectileObject = attack1BulletPrefab != null
                ? Instantiate(attack1BulletPrefab, spawnPosition, spawnRotation)
                : CreateDefaultAttack1Bullet(spawnPosition, spawnRotation);

            projectileObject.name = "Boss Attack 1 Bullet";
            projectileObject.transform.localScale = Vector3.one * (attack1BulletRadius * 2f);
            EnsureProjectilePhysics(projectileObject);

            var projectile = projectileObject.GetComponent<SeoulPlayProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<SeoulPlayProjectile>();
            }

            projectile.ConfigureMotion(0f);
            projectile.ConfigureTriggerHits(true);
            projectile.Launch(direction, attack1BulletSpeed, attack1BulletDamage, attack1BulletLifetime, transform);
        }

        private Vector3 GetAttack1BulletSpawnPosition(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = GetFlatForward();
            }

            return transform.position
                + transform.TransformDirection(attack1BulletCenterOffset)
                + Vector3.up * attack1BulletSpawnHeight
                + direction.normalized * attack1BulletSpawnForwardOffset;
        }

        private GameObject CreateDefaultAttack1Bullet(Vector3 position, Quaternion rotation)
        {
            var bulletObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulletObject.transform.SetPositionAndRotation(position, rotation);

            var collider = bulletObject.GetComponent<SphereCollider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            var renderer = bulletObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetAttack1BulletMaterial();
            }

            var rigidbody = bulletObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            return bulletObject;
        }

        private Material GetAttack1BulletMaterial()
        {
            if (attack1BulletMaterial != null)
            {
                return attack1BulletMaterial;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            attack1BulletMaterial = new Material(shader)
            {
                name = "Boss Attack 1 Bullet Material",
                color = attack1BulletColor
            };

            return attack1BulletMaterial;
        }

        private void StartAttack3JumpMove()
        {
            StopAttack3JumpMove();

            attack3JumpRoutine = StartCoroutine(PlayAttack3JumpMove(GetLockedAttackDirection()));
        }

        private void PlayAttack3AnimationFromStart()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return;
            }

            ResetAttackTriggers();

            if (animator.HasState(0, Attack03StateHash))
            {
                animator.Play(Attack03StateHash, 0, 0f);
                animator.Update(0f);
                return;
            }

            if (HasAnimatorParameter(Attack03Hash, AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger(Attack03Hash);
            }
        }

        private void ResetAttackTriggers()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return;
            }

            if (HasAnimatorParameter(Attack01Hash, AnimatorControllerParameterType.Trigger))
            {
                animator.ResetTrigger(Attack01Hash);
            }

            if (HasAnimatorParameter(Attack02Hash, AnimatorControllerParameterType.Trigger))
            {
                animator.ResetTrigger(Attack02Hash);
            }

            if (HasAnimatorParameter(Attack03Hash, AnimatorControllerParameterType.Trigger))
            {
                animator.ResetTrigger(Attack03Hash);
            }
        }

        private IEnumerator PlayAttack3JumpMove(Vector3 forward)
        {
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.001f)
            {
                forward = GetFlatForward();
            }

            forward.Normalize();
            var start = transform.position;
            var end = GetAttack3LockedLandingPosition(start, forward);
            end.y = start.y;

            lockedAttack3ImpactPosition = end;
            hasLockedAttack3ImpactPosition = true;

            var duration = GetAttack3JumpMoveDuration();
            var elapsed = 0f;

            while (elapsed < duration)
            {
                if (!CanAct())
                {
                    attack3JumpRoutine = null;
                    yield break;
                }

                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
                transform.position = position;
                yield return null;
            }

            transform.position = end;
            attack3JumpRoutine = null;
            if (IsCurrentAttack(BossAttackType.Attack3JumpSlam))
            {
                PerformAttack3Impact();
            }
        }

        private Vector3 GetAttack3LockedLandingPosition(Vector3 fallbackPosition, Vector3 forward)
        {
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.001f)
            {
                forward = GetFlatForward();
            }

            forward.Normalize();
            var landingPosition = hasLockedAttackTarget ? lockedTargetPosition : fallbackPosition;
            landingPosition.y = fallbackPosition.y;

            if (attack3LandingOffset > 0f)
            {
                landingPosition -= forward * attack3LandingOffset;
            }

            return landingPosition;
        }

        private float GetAttack3JumpMoveDuration()
        {
            return Mathf.Max(0.05f, attack3JumpMoveDuration);
        }

        private float GetAttack3LockDuration()
        {
            var clipLength = GetCurrentAttack3ClipLength();
            return clipLength > 0f ? Mathf.Max(attackLockDuration, clipLength) : attackLockDuration;
        }

        private float GetCurrentAttack3ClipLength()
        {
            var clip = GetCurrentAttack3Clip();
            return clip != null ? clip.length : 0f;
        }

        private AnimationClip GetCurrentAttack3Clip()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return null;
            }

            var currentClip = GetFirstClip(animator.GetCurrentAnimatorClipInfo(0));
            if (currentClip != null)
            {
                return currentClip;
            }

            if (animator.IsInTransition(0))
            {
                var nextClip = GetFirstClip(animator.GetNextAnimatorClipInfo(0));
                if (nextClip != null)
                {
                    return nextClip;
                }
            }

            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip != null && (clip.name == "root_Boss_Attack_JumpSlam" || clip.name == "Attack_03"))
                {
                    return clip;
                }
            }

            return null;
        }

        private static AnimationClip GetFirstClip(AnimatorClipInfo[] clipInfos)
        {
            for (var i = 0; i < clipInfos.Length; i++)
            {
                var clip = clipInfos[i].clip;
                if (clip != null && clip.length > 0f)
                {
                    return clip;
                }
            }

            return null;
        }

        private void FireAttack2EarthBlastLine(
            Vector3 source,
            Vector3 direction,
            HashSet<SeoulPlayDamageable> damagedTargets)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = GetFlatForward();
            }

            direction.Normalize();
            SpawnAttack2BoxAreaPreview(source, direction);
            DamageAttack2BoxArea(source, direction, damagedTargets);
            StartCoroutine(PlayAttack2PillarLine(source, direction));
        }

        private void SpawnAttack2SupportVfx()
        {
            if (attack2VfxPrefab == null)
            {
                return;
            }

            var vfxPosition = transform.position;
            vfxPosition += GetLockedAttackDirection() * attack2SupportVfxZOffset;
            var vfxRotation = Quaternion.LookRotation(GetLockedAttackDirection(), Vector3.up);
            var vfxObject = Instantiate(attack2VfxPrefab, vfxPosition, vfxRotation);

            var embeddedPillarUnit = FindChildTransform(vfxObject.transform, EarthBlastPillarUnitName);
            if (embeddedPillarUnit != null)
            {
                embeddedPillarUnit.gameObject.SetActive(false);
            }

            foreach (var director in vfxObject.GetComponentsInChildren<PlayableDirector>(true))
            {
                director.Stop();
                director.enabled = false;
            }

            foreach (var animatorComponent in vfxObject.GetComponentsInChildren<Animator>(true))
            {
                animatorComponent.enabled = false;
            }

            foreach (var meshRenderer in vfxObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                meshRenderer.enabled = false;
            }

            foreach (var particleSystem in vfxObject.GetComponentsInChildren<ParticleSystem>(true))
            {
                particleSystem.gameObject.SetActive(true);
                particleSystem.Clear(true);
                particleSystem.Play(true);
            }

            Destroy(vfxObject, attack2VfxDuration + attack2VfxDestroyDelay);
        }

        private void SpawnAttack3ImpactVfx(Vector3 position, Vector3 direction)
        {
            var useAttack3ImpactPrefab = attack3ImpactVfxPrefab != null;
            var prefab = useAttack3ImpactPrefab ? attack3ImpactVfxPrefab : attack2PillarVfxPrefab != null ? attack2PillarVfxPrefab : attack2VfxPrefab;
            if (prefab == null)
            {
                return;
            }

            GameObject vfxObject;
            var rotation = direction.sqrMagnitude > 0.001f ? Quaternion.LookRotation(direction, Vector3.up) : transform.rotation;
            if (useAttack3ImpactPrefab)
            {
                vfxObject = Instantiate(prefab, position, rotation);
                vfxObject.transform.localScale = Vector3.one;
            }
            else
            {
                vfxObject = Instantiate(prefab, position, rotation);
                vfxObject.transform.localScale = Vector3.one * attack3DamageRadius;
            }

            if (!attack3ParticlesOnly)
            {
                var baseVfx = vfxObject.GetComponent<BaseVfx>();
                if (baseVfx != null)
                {
                    baseVfx.Play(new VfxData(position, position + direction, attack3VfxDuration, attack3DamageRadius));
                    return;
                }
            }

            foreach (var director in vfxObject.GetComponentsInChildren<PlayableDirector>(true))
            {
                director.Stop();
                director.enabled = false;
            }

            foreach (var animatorComponent in vfxObject.GetComponentsInChildren<Animator>(true))
            {
                animatorComponent.enabled = false;
            }

            foreach (var meshRenderer in vfxObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                meshRenderer.enabled = false;
            }

            foreach (var particleSystem in vfxObject.GetComponentsInChildren<ParticleSystem>(true))
            {
                particleSystem.gameObject.SetActive(true);
                particleSystem.Clear(true);
                particleSystem.Play(true);
            }

            Destroy(vfxObject, attack3VfxDuration + attack3VfxDestroyDelay);
        }

        private void DamageAttack3Impact(Vector3 position, Vector3 direction)
        {
            if (attack3Damage <= 0f)
            {
                return;
            }

            var query = attack3HitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
            var hits = Physics.OverlapSphere(position, attack3DamageRadius, attack3HitMask, query);
            var damagedTargets = new HashSet<SeoulPlayDamageable>();
            foreach (var hit in hits)
            {
                var targetDamageable = hit.GetComponentInParent<SeoulPlayDamageable>();
                if (targetDamageable == null || targetDamageable == damageable || !damagedTargets.Add(targetDamageable))
                {
                    continue;
                }

                targetDamageable.TakeDamage(attack3Damage, direction, transform);
            }

            DamageAttack3TargetFallback(position, direction, damagedTargets);
        }

        private void DamageAttack3TargetFallback(
            Vector3 position,
            Vector3 direction,
            HashSet<SeoulPlayDamageable> damagedTargets)
        {
            if (target == null)
            {
                return;
            }

            var targetDamageable = target.GetComponentInParent<SeoulPlayDamageable>();
            if (targetDamageable == null || targetDamageable == damageable || !damagedTargets.Add(targetDamageable))
            {
                return;
            }

            var offset = targetDamageable.transform.position - position;
            offset.y = 0f;
            if (offset.sqrMagnitude > attack3DamageRadius * attack3DamageRadius)
            {
                return;
            }

            targetDamageable.TakeDamage(attack3Damage, direction, transform);
        }

        private IEnumerator PlayAttack2PillarLine(
            Vector3 source,
            Vector3 direction)
        {
            for (var index = 0; index < attack2PillarCount; index++)
            {
                if (index > 0 && attack2PillarDelayStep > 0f)
                {
                    yield return new WaitForSeconds(attack2PillarDelayStep);
                }

                if (!CanAct())
                {
                    yield break;
                }

                var radius = attack2PillarVfxBaseScale + attack2PillarVfxScaleStep * index;
                var position = source + direction * attack2PillarSpacing * index;
                SpawnAttack2PillarVfx(position, direction, radius);
            }
        }

        private void SpawnAttack2PillarVfx(Vector3 position, Vector3 direction, float radius)
        {
            var prefab = attack2PillarVfxPrefab != null ? attack2PillarVfxPrefab : attack2VfxPrefab;
            if (prefab == null)
            {
                return;
            }

            var rotation = direction.sqrMagnitude > 0.001f ? Quaternion.LookRotation(direction, Vector3.up) : transform.rotation;
            var vfxObject = Instantiate(prefab, position, rotation);
            vfxObject.transform.localScale = Vector3.one * radius;

            if (!attack2ParticlesOnly)
            {
                var baseVfx = vfxObject.GetComponent<BaseVfx>();
                if (baseVfx != null)
                {
                    baseVfx.Play(new VfxData(position, position + direction, attack2VfxDuration, radius));
                    return;
                }
            }

            foreach (var director in vfxObject.GetComponentsInChildren<PlayableDirector>(true))
            {
                director.Stop();
                director.enabled = false;
            }

            foreach (var animatorComponent in vfxObject.GetComponentsInChildren<Animator>(true))
            {
                animatorComponent.enabled = false;
            }

            foreach (var meshRenderer in vfxObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                meshRenderer.enabled = false;
            }

            foreach (var particleSystem in vfxObject.GetComponentsInChildren<ParticleSystem>(true))
            {
                particleSystem.gameObject.SetActive(true);
                particleSystem.Clear(true);
                particleSystem.Play(true);
            }

            Destroy(vfxObject, attack2VfxDuration + attack2VfxDestroyDelay);
        }

        private void SpawnAttack2StartAreaPreview(Vector3 position)
        {
            if (!showAttack2DamageArea || attack2StartRadius <= 0f)
            {
                return;
            }

            var previewObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            previewObject.name = "Attack 2 Start Damage Area Preview";
            previewObject.transform.position = position;
            previewObject.transform.localScale = Vector3.one * attack2StartRadius * 2f;

            DisableAndDestroyPreviewCollider(previewObject);
            ConfigureAttack2DamageAreaRenderer(previewObject);
            Destroy(previewObject, attack2DamageAreaDuration);
        }

        private void SpawnAttack2BoxAreaPreview(Vector3 source, Vector3 direction)
        {
            if (!showAttack2DamageArea || attack2BoxLength <= 0f || attack2BoxWidth <= 0f || attack2BoxHeight <= 0f)
            {
                return;
            }

            var boxCenter = GetAttack2BoxCenter(source, direction);
            var rotation = GetAttack2BoxRotation(direction);
            var previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            previewObject.name = "Attack 2 Box Damage Area Preview";
            previewObject.transform.SetPositionAndRotation(boxCenter, rotation);
            previewObject.transform.localScale = new Vector3(attack2BoxWidth, attack2BoxHeight, attack2BoxLength);

            DisableAndDestroyPreviewCollider(previewObject);
            ConfigureAttack2DamageAreaRenderer(previewObject);
            Destroy(previewObject, attack2DamageAreaDuration);
        }

        private void DisableAndDestroyPreviewCollider(GameObject previewObject)
        {
            var collider = previewObject != null ? previewObject.GetComponent<Collider>() : null;
            if (collider == null)
            {
                return;
            }

            collider.enabled = false;
            Destroy(collider);
        }

        private void ConfigureAttack2DamageAreaRenderer(GameObject previewObject)
        {
            var renderer = previewObject != null ? previewObject.GetComponent<MeshRenderer>() : null;
            if (renderer == null)
            {
                return;
            }

            var material = GetAttack2DamageAreaMaterial();
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private Material GetAttack2DamageAreaMaterial()
        {
            if (attack2DamageAreaMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Unlit/Transparent")
                    ?? Shader.Find("Sprites/Default")
                    ?? Shader.Find("Standard")
                    ?? Shader.Find("Diffuse");
                if (shader == null)
                {
                    return null;
                }

                attack2DamageAreaMaterial = new Material(shader)
                {
                    name = "Attack 2 Damage Area Preview",
                    hideFlags = HideFlags.HideAndDontSave,
                    renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent,
                };
            }

            if (attack2DamageAreaMaterial.HasProperty("_BaseColor"))
            {
                attack2DamageAreaMaterial.SetColor("_BaseColor", attack2DamageAreaColor);
            }

            if (attack2DamageAreaMaterial.HasProperty("_Color"))
            {
                attack2DamageAreaMaterial.SetColor("_Color", attack2DamageAreaColor);
            }

            if (attack2DamageAreaMaterial.HasProperty("_Surface"))
            {
                attack2DamageAreaMaterial.SetFloat("_Surface", 1f);
            }

            if (attack2DamageAreaMaterial.HasProperty("_SrcBlend"))
            {
                attack2DamageAreaMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            }

            if (attack2DamageAreaMaterial.HasProperty("_DstBlend"))
            {
                attack2DamageAreaMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            if (attack2DamageAreaMaterial.HasProperty("_ZWrite"))
            {
                attack2DamageAreaMaterial.SetInt("_ZWrite", 0);
            }

            attack2DamageAreaMaterial.EnableKeyword("_ALPHABLEND_ON");

            return attack2DamageAreaMaterial;
        }

        private void DamageAttack2StartArea(
            Vector3 position,
            Vector3 direction,
            HashSet<SeoulPlayDamageable> damagedTargets)
        {
            if (attack2Damage <= 0f)
            {
                return;
            }

            var query = attack2HitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
            var hits = Physics.OverlapSphere(position, attack2StartRadius, attack2HitMask, query);
            DamageAttack2Hits(hits, position, direction, damagedTargets);
        }

        private void DamageAttack2BoxArea(
            Vector3 source,
            Vector3 direction,
            HashSet<SeoulPlayDamageable> damagedTargets)
        {
            if (attack2Damage <= 0f)
            {
                return;
            }

            var query = attack2HitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
            var boxCenter = GetAttack2BoxCenter(source, direction);
            var halfExtents = new Vector3(attack2BoxWidth * 0.5f, attack2BoxHeight * 0.5f, attack2BoxLength * 0.5f);
            var hits = Physics.OverlapBox(boxCenter, halfExtents, GetAttack2BoxRotation(direction), attack2HitMask, query);
            DamageAttack2Hits(hits, source, direction, damagedTargets);
        }

        private void DamageAttack2Hits(
            Collider[] hits,
            Vector3 source,
            Vector3 direction,
            HashSet<SeoulPlayDamageable> damagedTargets)
        {
            foreach (var hit in hits)
            {
                var targetDamageable = hit.GetComponentInParent<SeoulPlayDamageable>();
                if (targetDamageable == null || targetDamageable == damageable || !damagedTargets.Add(targetDamageable))
                {
                    continue;
                }

                targetDamageable.TakeDamage(attack2Damage, direction, transform);
                ApplyAttack2Knockback(targetDamageable, source, direction);
            }
        }

        private void ApplyAttack2Knockback(SeoulPlayDamageable targetDamageable, Vector3 source, Vector3 attackDirection)
        {
            if (targetDamageable == null || attack2KnockbackDistance <= 0f || attack2KnockbackDuration <= 0f)
            {
                return;
            }

            var heroMover = targetDamageable.GetComponentInParent<SimpleHeroMover>();
            if (heroMover == null)
            {
                heroMover = targetDamageable.GetComponentInChildren<SimpleHeroMover>();
            }

            if (heroMover == null)
            {
                return;
            }

            attackDirection.y = 0f;
            if (attackDirection.sqrMagnitude <= 0.001f)
            {
                attackDirection = GetFlatForward();
            }

            attackDirection.Normalize();
            var right = Vector3.Cross(Vector3.up, attackDirection).normalized;
            if (right.sqrMagnitude <= 0.001f)
            {
                return;
            }

            var offsetFromLine = targetDamageable.transform.position - source;
            offsetFromLine.y = 0f;
            var sideSign = Mathf.Sign(Vector3.Dot(offsetFromLine, right));
            if (Mathf.Approximately(sideSign, 0f))
            {
                sideSign = Random.value < 0.5f ? -1f : 1f;
            }

            heroMover.ApplyKnockback(right * sideSign, attack2KnockbackDistance, attack2KnockbackDuration);
        }

        private Vector3 GetAttack2BoxCenter(Vector3 source, Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = GetFlatForward();
            }

            direction.Normalize();
            var startOffset = attack2StartRadius + attack2BoxLength * 0.5f;
            return source + direction * startOffset + Vector3.up * (attack2BoxHeight * 0.5f);
        }

        private Quaternion GetAttack2BoxRotation(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(direction.normalized, Vector3.up)
                : transform.rotation;
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

        private float GetAttackSelectRange()
        {
            return Mathf.Max(attackRange, longRangeDistance, useAttack3InAutoAttack ? attack3MaxRange : 0f);
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

        private void ConfigureAnimatorForRuntime()
        {
            if (animator == null)
            {
                return;
            }

            animator.enabled = true;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.applyRootMotion = false;

            if (animator.speed <= 0f)
            {
                animator.speed = 1f;
            }

            foreach (var skinnedMeshRenderer in animator.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                skinnedMeshRenderer.updateWhenOffscreen = true;
            }

            var eventRelay = animator.GetComponent<BossAnimationEventRelay>();
            if (eventRelay == null)
            {
                eventRelay = animator.gameObject.AddComponent<BossAnimationEventRelay>();
            }

            eventRelay.Initialize(this);
        }

        private void ResolveAnimatorReference()
        {
            var animators = GetComponentsInChildren<Animator>(true);
            if (animators == null || animators.Length == 0)
            {
                animator = null;
                return;
            }

            var preferredAnimator = FindModelAnimator(animators);
            if (preferredAnimator == null)
            {
                preferredAnimator = animator != null ? animator : animators[0];
            }

            animator = preferredAnimator;

            foreach (var candidate in animators)
            {
                if (candidate != null && candidate != animator && candidate.gameObject == gameObject)
                {
                    candidate.enabled = false;
                }
            }
        }

        private Animator FindModelAnimator(Animator[] animators)
        {
            foreach (var candidate in animators)
            {
                if (candidate == null || candidate.gameObject == gameObject)
                {
                    continue;
                }

                if (candidate.runtimeAnimatorController != null &&
                    candidate.GetComponentInChildren<SkinnedMeshRenderer>(true) != null)
                {
                    return candidate;
                }
            }

            return null;
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

        private void DrawAttack2GizmoLine(Vector3 source, Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            direction.Normalize();
            var previous = source;
            for (var index = 0; index < attack2PillarCount; index++)
            {
                var position = source + direction * attack2PillarSpacing * index;
                if (index > 0)
                {
                    Gizmos.DrawLine(previous, position);
                }

                previous = position;
            }

            var previousMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(GetAttack2BoxCenter(source, direction), GetAttack2BoxRotation(direction), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(attack2BoxWidth, attack2BoxHeight, attack2BoxLength));
            Gizmos.matrix = previousMatrix;
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

            var attack2Forward = GetFlatForward();
            var attack2Source = GetAttack2Source(attack2Forward);
            Gizmos.color = new Color(0.55f, 0.35f, 0.1f, 0.75f);
            Gizmos.DrawWireSphere(attack2Source, attack2StartRadius);
            DrawAttack2GizmoLine(attack2Source, attack2Forward);
            if (attack2SideAngle > 0f)
            {
                DrawAttack2GizmoLine(attack2Source, Quaternion.AngleAxis(attack2SideAngle, Vector3.up) * attack2Forward);
                DrawAttack2GizmoLine(attack2Source, Quaternion.AngleAxis(-attack2SideAngle, Vector3.up) * attack2Forward);
            }

            var attack3Forward = GetFlatForward();
            var attack3Landing = GetAttack3LockedLandingPosition(transform.position, attack3Forward);
            Gizmos.color = new Color(1f, 0.15f, 0.05f, 0.75f);
            Gizmos.DrawLine(transform.position, attack3Landing);
            Gizmos.DrawWireSphere(attack3Landing, attack3DamageRadius);
            Gizmos.color = new Color(1f, 0.15f, 0.05f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, attack3MinRange);
            Gizmos.DrawWireSphere(transform.position, attack3MaxRange);

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
