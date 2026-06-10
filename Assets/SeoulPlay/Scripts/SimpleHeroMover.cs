using System.Collections;
using UnityEngine;
using Cinemachine;

namespace SeoulPlay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SimpleHeroMover : MonoBehaviour
    {
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveZHash = Animator.StringToHash("MoveZ");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int AimHash = Animator.StringToHash("Aim");
        private static readonly int FireHash = Animator.StringToHash("Fire");
        private static readonly int IsFiringHash = Animator.StringToHash("IsFiring");
        private static readonly int RollingHash = Animator.StringToHash("Rolling");
        private static readonly int RollForwardStateHash = Animator.StringToHash("Base Layer.Roll Forward");
        private static readonly int RollBackwardStateHash = Animator.StringToHash("Base Layer.Roll Backward");
        private static readonly int RollLeftStateHash = Animator.StringToHash("Base Layer.Roll Left");
        private static readonly int RollRightStateHash = Animator.StringToHash("Base Layer.Roll Right");
        private static readonly int LocomotionStateHash = Animator.StringToHash("Base Layer.Locomotion");
        private const string UpperBodyFireLayerName = "Upper Body Fire";
        private const string SceneCameraRigName = "Cinemachine";

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform modelRoot;
        [SerializeField] private Camera followCamera;
        [SerializeField] private Transform cameraTarget;

        [Header("Movement")]
        [SerializeField] private bool enableRollInput = true;
        [SerializeField] private bool enableFireInput = true;
        [SerializeField, Min(0f)] private float walkSpeed = 2.4f;
        [SerializeField, Min(0f)] private float runSpeed = 5.2f;
        [SerializeField, Min(0f)] private float turnSpeed = 180f;
        [SerializeField] private bool alignCameraToMovementDirection;
        [SerializeField, Min(0f)] private float movementCameraTurnSpeed = 180f;
        [SerializeField, Min(0f)] private float gamepadTurnSpeed = 150f;
        [SerializeField, Min(0f)] private float gamepadDeadZone = 0.2f;
        [SerializeField] private float gravity = -20f;

        [Header("Camera")]
        [SerializeField, Min(0f)] private float cameraDistance = 6f;
        [SerializeField, Min(0f)] private float cameraHeight = 1.65f;
        [SerializeField] private float cameraPitch = 15f;
        [SerializeField, Min(0f)] private float cameraPitchSpeed = 120f;
        [SerializeField] private float minCameraPitch = -25f;
        [SerializeField] private float maxCameraPitch = 65f;
        [SerializeField] private bool invertVerticalLook;
        [SerializeField] private bool useRightStickForCamera = true;
        [SerializeField] private bool useSceneCameraStartPose = true;
        [SerializeField, Min(0f)] private float cameraSmoothTime = 0.02f;
        [SerializeField, Min(0f)] private float cameraTargetSmoothTime = 0.04f;

        [Header("Cinemachine")]
        [SerializeField] private bool useCinemachineCamera = true;
        [SerializeField] private CinemachineVirtualCamera gameplayVirtualCamera;
        [SerializeField] private int gameplayCameraPriority = 20;
        [SerializeField] private float cinemachineCameraDistance = 6f;
        [SerializeField] private float cinemachineCameraHeight = 1.65f;
        [SerializeField] private Vector3 cinemachineFollowOffset = new Vector3(0f, 1.65f, -6f);
        [SerializeField, Min(0f)] private float gameplayCameraDamping = 0.25f;

        [Header("Roll")]
        [SerializeField, Min(0f)] private float rollSpeed = 6.8f;
        [SerializeField, Min(0f)] private float rollDuration = 0.72f;
        [SerializeField, Min(0f)] private float rollCooldown = 0.2f;
        [SerializeField] private bool useRollRootMotion;
        [SerializeField] private bool useModelOffsetRootMotion = true;
        [SerializeField] private bool useDirectionalRollAnimations;
        [SerializeField, Min(0f)] private float rollRootMotionScale = 1f;
        [SerializeField, Min(0f)] private float rollEndEarlyTime = 0.1f;
        [SerializeField, Min(0f)] private float rollExitBlendTime = 0.12f;
        [SerializeField, Min(0f)] private float rollRecoveryDuration = 0.12f;
        [SerializeField, Min(0f)] private float rollFireLockoutExtraTime = 0.15f;

        [Header("Fire")]
        [SerializeField, Min(0f)] private float upperBodyFireDuration = 0.45f;
        [SerializeField] private bool driveAnimatorAimFromAimInput;

        [Header("Stuck Recovery")]
        [SerializeField] private bool useStuckRecovery = true;
        [SerializeField, Min(0f)] private float stuckCheckDelay = 0.12f;
        [SerializeField, Min(0f)] private float stuckRecoveryPush = 0.08f;
        [SerializeField, Min(0f)] private float stuckRecoveryLift = 0.025f;
        [SerializeField, Min(0f)] private float startupStuckRecoveryDuration = 1f;

        private CharacterController characterController;
        private float verticalVelocity;
        private float cameraYaw;
        private float rollTimer;
        private float rollElapsedTime;
        private float activeRollDuration;
        private float rollCooldownTimer;
        private float rollRecoveryTimer;
        private float rollFireLockoutTimer;
        private float hitFireLockoutTimer;
        private bool deathFireBlocked;
        private float stuckTimer;
        private float startupStuckRecoveryTimer;
        private float upperBodyFireTimer;
        private int upperBodyFireLayerIndex = -1;
        private bool wasFirePressed;
        private bool isRolling;
        private bool hasSmoothedCameraTargetPosition;
        private bool hasSceneCameraStartPose;
        private bool hasForcedCinemachineScenePose;
        private bool cinemachineReady;
        private float sceneCameraStartPitch;
        private Quaternion sceneCameraStartRotation;
        private Quaternion sceneCameraLocalRotation;
        private Vector3 cameraVelocity;
        private Vector3 cameraTargetVelocity;
        private Vector3 smoothedCameraTargetPosition;
        private float movementReferenceYaw;
        private Vector3 rollDirection;
        private Vector3 rollFacingDirection;
        private Vector3 postRollFacingDirection;
        private bool hasMovementReferenceYaw;

        public bool IsRolling => isRolling;
        public bool IsRollingOrStartingRoll => IsFireBlockedByRoll;
        public bool IsFireBlockedByRoll =>
            rollFireLockoutTimer > 0f || isRolling || rollRecoveryTimer > 0f || CanStartRollThisFrame();
        public bool IsFireBlockedByHit => hitFireLockoutTimer > 0f || deathFireBlocked;
        public bool IsWeaponFirePoseActive => upperBodyFireTimer > 0f && !isRolling;
        private Vector2 lastLocomotionInput;
        private int[] animatorParameterHashes = System.Array.Empty<int>();
        private bool mouseCameraInputEnabled = true;

        public bool MouseCameraInputEnabled => mouseCameraInputEnabled;

        public void SetMouseCameraInputEnabled(bool enabled)
        {
            mouseCameraInputEnabled = enabled;
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (followCamera == null)
            {
                followCamera = Camera.main;
            }

            if (modelRoot == null && animator != null)
            {
                modelRoot = animator.transform;
            }

            cameraYaw = transform.eulerAngles.y;
            cameraPitch = Mathf.Clamp(cameraPitch, minCameraPitch, maxCameraPitch);
            EnsureCameraTarget();
            CaptureSceneCameraStartPose();
            EnsureCinemachineSetup();
            ResetCameraState();
            ResetModelRootTransform();
            CacheAnimatorParameters();
            CacheAnimatorLayers();
        }

        private IEnumerator Start()
        {
            yield return null;

            Physics.SyncTransforms();
            verticalVelocity = 0f;
            rollCooldownTimer = 0f;
            rollRecoveryTimer = 0f;
            startupStuckRecoveryTimer = startupStuckRecoveryDuration;
            cameraVelocity = Vector3.zero;
            cameraTargetVelocity = Vector3.zero;
            hasSmoothedCameraTargetPosition = false;
            ResetModelRootTransform();

            if (characterController != null)
            {
                characterController.enabled = false;
                characterController.enabled = true;
                characterController.Move(Vector3.up * Mathf.Max(0.001f, stuckRecoveryLift));
            }

            EnsureCameraTarget();
            ResetCameraState();
        }

        private void Update()
        {
            startupStuckRecoveryTimer = Mathf.Max(0f, startupStuckRecoveryTimer - Time.deltaTime);
            UpdateCameraInput();

            var input = GetMoveInput();
            input = Vector2.ClampMagnitude(input, 1f);

            var aimPressed = IsAimPressed();
            var isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetButton("LeftStickClick");
            var moveSpeed = isRunning ? runSpeed : walkSpeed;
            var firePressed = IsFirePressed();
            UpdateMovementReferenceYaw(input, aimPressed, firePressed);
            var worldMove = GetCameraRelativeMove(input, hasMovementReferenceYaw ? movementReferenceYaw : cameraYaw);

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            rollCooldownTimer = Mathf.Max(0f, rollCooldownTimer - Time.deltaTime);
            rollFireLockoutTimer = Mathf.Max(0f, rollFireLockoutTimer - Time.deltaTime);
            hitFireLockoutTimer = Mathf.Max(0f, hitFireLockoutTimer - Time.deltaTime);
            if (!isRolling && CanStartRollThisFrame())
            {
                StartRoll(input);
            }

            if (isRolling)
            {
                UpdateRoll();
            }
            else
            {
                MoveCharacter(worldMove, moveSpeed);
                UpdateFacing(worldMove, input, aimPressed, firePressed);
            }

            UpdateAnimator(input, isRunning, aimPressed, firePressed);
            if (rollRecoveryTimer > 0f)
            {
                UpdateRollRecovery();
            }
            else if (!isRolling)
            {
                ResetModelRootTransform();
            }
        }

        private void LateUpdate()
        {
            if (isRolling && useRollRootMotion)
            {
                CommitModelRootOffset(false);
            }

            UpdateCamera();
        }

        private Vector2 GetMoveInput()
        {
            var keyboardInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            var gamepadInput = new Vector2(Input.GetAxisRaw("LeftAnalogHorizontal"), Input.GetAxisRaw("LeftAnalogVertical"));

            if (gamepadInput.magnitude > gamepadDeadZone && gamepadInput.sqrMagnitude > keyboardInput.sqrMagnitude)
            {
                return gamepadInput;
            }

            return keyboardInput;
        }

        private void UpdateCameraInput()
        {
            if (isRolling)
            {
                return;
            }

            var gamepadTurn = Input.GetAxis("RightAnalogHorizontal");
            if (useRightStickForCamera && Mathf.Abs(gamepadTurn) > gamepadDeadZone)
            {
                cameraYaw += gamepadTurn * gamepadTurnSpeed * Time.deltaTime;
            }
            else if (mouseCameraInputEnabled)
            {
                cameraYaw += Input.GetAxis("Mouse X") * turnSpeed * Time.deltaTime;
            }

            var gamepadPitch = Input.GetAxis("RightAnalogVertical");
            var usesGamepadPitch = useRightStickForCamera && Mathf.Abs(gamepadPitch) > gamepadDeadZone;
            var pitchInput = usesGamepadPitch
                ? gamepadPitch
                : mouseCameraInputEnabled ? Input.GetAxis("Mouse Y") : 0f;
            var pitchDirection = invertVerticalLook ? 1f : -1f;
            cameraPitch += pitchInput * pitchDirection * cameraPitchSpeed * Time.deltaTime;
            cameraPitch = Mathf.Clamp(cameraPitch, minCameraPitch, maxCameraPitch);
        }

        private void UpdateMovementReferenceYaw(Vector2 input, bool aimPressed, bool firePressed)
        {
            if (input.sqrMagnitude <= 0.001f || aimPressed || firePressed || isRolling)
            {
                hasMovementReferenceYaw = false;
                return;
            }

            if (!hasMovementReferenceYaw)
            {
                movementReferenceYaw = cameraYaw;
                hasMovementReferenceYaw = true;
            }
        }

        private Vector3 GetCameraRelativeMove(Vector2 input)
        {
            return GetCameraRelativeMove(input, cameraYaw);
        }

        private static Vector3 GetCameraRelativeMove(Vector2 input, float referenceYaw)
        {
            var yawRotation = Quaternion.Euler(0f, referenceYaw, 0f);
            var forward = yawRotation * Vector3.forward;
            var right = yawRotation * Vector3.right;
            return Vector3.ClampMagnitude(right * input.x + forward * input.y, 1f);
        }

        private void UpdateFacing(Vector3 worldMove, Vector2 input, bool aimPressed, bool firePressed)
        {
            if (firePressed || aimPressed)
            {
                RotateToward(GetCameraForward());
                return;
            }

            if (worldMove.sqrMagnitude > 0.001f)
            {
                SnapToward(worldMove);
                RotateCameraTowardMovement(worldMove);
            }
        }

        private void RotateCameraTowardMovement(Vector3 worldMove)
        {
            if (!alignCameraToMovementDirection)
            {
                return;
            }

            RotateCameraYawToward(worldMove, movementCameraTurnSpeed);
        }

        private void RotateToward(Vector3 direction)
        {
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

        private void MoveCharacter(Vector3 worldMove, float moveSpeed)
        {
            var before = transform.position;
            var horizontalMotion = worldMove * moveSpeed * Time.deltaTime;
            if (horizontalMotion.sqrMagnitude > 0f)
            {
                characterController.Move(horizontalMotion);
            }

            characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
            RecoverIfMovementStuck(before, horizontalMotion);
        }

        private void RecoverIfMovementStuck(Vector3 before, Vector3 attemptedHorizontalMotion)
        {
            if (!useStuckRecovery || attemptedHorizontalMotion.sqrMagnitude <= 0.000001f)
            {
                stuckTimer = 0f;
                return;
            }

            var actualOffset = transform.position - before;
            actualOffset.y = 0f;
            var expectedDistance = attemptedHorizontalMotion.magnitude;
            var actualDistance = actualOffset.magnitude;
            var allowRecovery = characterController.isGrounded || startupStuckRecoveryTimer > 0f;
            if (actualDistance >= expectedDistance * 0.25f || !allowRecovery)
            {
                stuckTimer = 0f;
                return;
            }

            stuckTimer += Time.deltaTime;
            if (stuckTimer < stuckCheckDelay)
            {
                return;
            }

            var pushDirection = attemptedHorizontalMotion.normalized;
            if (HasBlockingObstacle(pushDirection, stuckRecoveryPush + characterController.skinWidth))
            {
                stuckTimer = 0f;
                return;
            }

            characterController.Move(Vector3.up * stuckRecoveryLift);
            characterController.Move(pushDirection * stuckRecoveryPush);
            stuckTimer = 0f;
        }

        private bool HasBlockingObstacle(Vector3 direction, float distance)
        {
            if (characterController == null || direction.sqrMagnitude <= 0.000001f || distance <= 0f)
            {
                return false;
            }

            var radiusScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
            var heightScale = Mathf.Abs(transform.lossyScale.y);
            var radius = characterController.radius * radiusScale;
            var height = Mathf.Max(characterController.height * heightScale, radius * 2f);
            var center = transform.TransformPoint(characterController.center);
            var halfLine = Mathf.Max(0f, height * 0.5f - radius);
            var point1 = center + Vector3.up * halfLine;
            var point2 = center - Vector3.up * halfLine;

            var hits = Physics.CapsuleCastAll(
                point1,
                point2,
                radius,
                direction.normalized,
                distance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            for (var i = 0; i < hits.Length; i++)
            {
                var hitTransform = hits[i].transform;
                if (hitTransform != null && hitTransform.root != transform.root)
                {
                    return true;
                }
            }

            return false;
        }

        public void RotateCameraYawToward(Vector3 direction, float degreesPerSecond)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            var targetYaw = Quaternion.LookRotation(direction.normalized, Vector3.up).eulerAngles.y;
            cameraYaw = Mathf.MoveTowardsAngle(
                cameraYaw,
                targetYaw,
                degreesPerSecond * Time.deltaTime);
        }

        public void ApplyHitFireLockout(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            hitFireLockoutTimer = Mathf.Max(hitFireLockoutTimer, duration);
            wasFirePressed = true;
            ResetAnimatorTrigger(FireHash);
            SetAnimatorBool(IsFiringHash, false);
            upperBodyFireTimer = 0f;
            SetUpperBodyFireLayerWeight(0f);
        }

        public void SetDeathFireBlocked(bool blocked)
        {
            deathFireBlocked = blocked;
            if (!blocked)
            {
                return;
            }

            hitFireLockoutTimer = 0f;
            wasFirePressed = true;
            ResetAnimatorTrigger(FireHash);
            SetAnimatorBool(IsFiringHash, false);
            upperBodyFireTimer = 0f;
            SetUpperBodyFireLayerWeight(0f);
        }

        private void SnapToward(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private Vector3 GetCameraForward()
        {
            return Quaternion.Euler(0f, cameraYaw, 0f) * Vector3.forward;
        }

        private bool IsAimPressed()
        {
            return Input.GetMouseButton(1)
                || Input.GetButton("LB")
                || Input.GetAxis("LT") > 0.2f;
        }

        private bool IsFirePressed()
        {
            return enableFireInput
                && !IsFireBlockedByHit
                && (Input.GetMouseButton(0)
                    || Input.GetKey(KeyCode.F)
                    || Input.GetButton("RB")
                    || Input.GetAxis("RT") > 0.2f);
        }

        private bool IsRollPressed()
        {
            return Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("B");
        }

        private bool CanStartRollThisFrame()
        {
            return enableRollInput
                && characterController != null
                && characterController.isGrounded
                && rollCooldownTimer <= 0f
                && IsRollPressed();
        }

        private void StartRoll(Vector2 input)
        {
            var rollInput = GetCardinalRollInput(ResolveRollInput(input));
            rollDirection = GetCameraRelativeMove(rollInput).normalized;
            rollFacingDirection = rollDirection;
            postRollFacingDirection = rollFacingDirection;
            SnapToward(rollFacingDirection);
            activeRollDuration = Mathf.Max(0.05f, rollDuration - rollEndEarlyTime);
            rollTimer = activeRollDuration;
            rollElapsedTime = 0f;
            rollCooldownTimer = rollCooldown;
            rollRecoveryTimer = 0f;
            rollFireLockoutTimer = Mathf.Max(
                rollFireLockoutTimer,
                rollDuration + rollRecoveryDuration + rollFireLockoutExtraTime);
            isRolling = true;
            verticalVelocity = 0f;
            cameraVelocity = Vector3.zero;
            cameraTargetVelocity = Vector3.zero;

            SetAnimatorBool(RollingHash, true);
            ResetAnimatorTrigger(FireHash);
            upperBodyFireTimer = 0f;
            SetUpperBodyFireLayerWeight(0f);
            SetAnimatorRootMotion(useRollRootMotion);
            PlayRollAnimation(rollInput);
        }

        private Vector2 ResolveRollInput(Vector2 input)
        {
            if (animator != null)
            {
                var animatedInput = new Vector2(
                    HasAnimatorParameter(MoveXHash) ? animator.GetFloat(MoveXHash) : 0f,
                    HasAnimatorParameter(MoveZHash) ? animator.GetFloat(MoveZHash) : 0f);

                if (Mathf.Abs(animatedInput.x) > 0.2f)
                {
                    return new Vector2(Mathf.Sign(animatedInput.x), 0f);
                }
            }

            if (Mathf.Abs(lastLocomotionInput.x) > 0.2f)
            {
                return new Vector2(Mathf.Sign(lastLocomotionInput.x), 0f);
            }

            return input;
        }

        private static Vector2 GetCardinalRollInput(Vector2 input)
        {
            if (input.sqrMagnitude <= 0.001f)
            {
                return Vector2.up;
            }

            return Mathf.Abs(input.x) > 0.25f
                ? new Vector2(Mathf.Sign(input.x), 0f)
                : new Vector2(0f, Mathf.Sign(input.y));
        }

        private void PlayRollAnimation(Vector2 input)
        {
            var stateHash = GetRollStateHash(input);
            if (HasAnimatorState(0, stateHash))
            {
                animator.Play(stateHash, 0, 0f);
            }
        }

        private int GetRollStateHash(Vector2 input)
        {
            if (!useDirectionalRollAnimations)
            {
                return RollForwardStateHash;
            }

            if (input.y > 0.1f)
            {
                return RollForwardStateHash;
            }

            if (input.y < -0.1f)
            {
                return RollBackwardStateHash;
            }

            return input.x < 0f ? RollLeftStateHash : RollRightStateHash;
        }

        private void UpdateRoll()
        {
            rollElapsedTime += Time.deltaTime;
            rollTimer -= Time.deltaTime;
            RotateToward(rollFacingDirection);
            if (!useRollRootMotion)
            {
                characterController.Move((rollDirection * rollSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);
            }

            if (rollTimer > 0f)
            {
                return;
            }

            isRolling = false;
            SetAnimatorBool(RollingHash, false);
            SnapToward(postRollFacingDirection);
            CrossFadeToLocomotion();
            SetAnimatorRootMotion(false);
            cameraVelocity = Vector3.zero;
            cameraTargetVelocity = Vector3.zero;
            rollRecoveryTimer = rollRecoveryDuration;
        }

        public void ApplyAnimatorRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
        {
            if (!isRolling || !useRollRootMotion || useModelOffsetRootMotion || characterController == null)
            {
                return;
            }

            deltaPosition.y = 0f;
            if (deltaPosition.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            characterController.Move(deltaPosition * rollRootMotionScale);
        }

        private void UpdateAnimator(Vector2 input, bool isRunning, bool aimPressed, bool firePressed)
        {
            if (animator == null)
            {
                return;
            }

            var speed01 = isRolling ? 0f : input.magnitude;
            var animatorMoveInput = isRolling
                ? Vector2.zero
                : GetAnimatorMoveInput(input, aimPressed, firePressed);
            SetAnimatorFloat(MoveXHash, animatorMoveInput.x, 0.1f);
            SetAnimatorFloat(MoveZHash, animatorMoveInput.y, 0.1f);
            SetAnimatorFloat(SpeedHash, isRunning ? speed01 * 2f : speed01, 0.1f);
            SetAnimatorBool(GroundedHash, characterController.isGrounded);
            SetAnimatorBool(AimHash, driveAnimatorAimFromAimInput && aimPressed);
            var fireBlocked = IsRollingOrStartingRoll || IsFireBlockedByHit;
            SetAnimatorBool(IsFiringHash, firePressed && !fireBlocked);

            if (firePressed && !wasFirePressed && !fireBlocked)
            {
                upperBodyFireTimer = upperBodyFireDuration;
                SetUpperBodyFireLayerWeight(1f);
                SetAnimatorTrigger(FireHash);
            }
            wasFirePressed = firePressed;

            UpdateUpperBodyFireLayer(firePressed);

            if (!isRolling && input.sqrMagnitude > 0.001f)
            {
                lastLocomotionInput = input;
            }
        }

        private static Vector2 GetAnimatorMoveInput(Vector2 input, bool aimPressed, bool firePressed)
        {
            if (input.sqrMagnitude <= 0.001f)
            {
                return Vector2.zero;
            }

            if (firePressed || aimPressed)
            {
                return input;
            }

            return new Vector2(0f, input.magnitude);
        }

        private void UpdateCamera()
        {
            if (followCamera == null)
            {
                return;
            }

            EnsureCameraTarget();
            UpdateCameraTarget();

            if (useCinemachineCamera && cinemachineReady)
            {
                UpdateCinemachineCamera();
                return;
            }

            if (hasSceneCameraStartPose)
            {
                UpdateScenePoseCamera();
                return;
            }

            var yawRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            var lookTarget = cameraTarget.position;
            var targetPosition = lookTarget - yawRotation * Vector3.forward * cameraDistance;
            var smoothedPosition = GetSmoothedCameraPosition(targetPosition, cameraSmoothTime);

            followCamera.transform.SetPositionAndRotation(
                smoothedPosition,
                Quaternion.LookRotation(lookTarget - smoothedPosition, Vector3.up));
        }

        private void UpdateScenePoseCamera()
        {
            var yawRotation = Quaternion.Euler(0f, cameraYaw, 0f);
            var pitchDelta = Quaternion.Euler(cameraPitch - sceneCameraStartPitch, 0f, 0f);
            var targetPosition = cameraTarget.position + yawRotation * pitchDelta * cinemachineFollowOffset;
            var targetRotation = yawRotation * pitchDelta * sceneCameraLocalRotation;
            var activeSmoothTime = IsCameraLookInputActive() ? 0f : cameraSmoothTime;
            var smoothedPosition = GetSmoothedCameraPosition(targetPosition, activeSmoothTime);

            followCamera.transform.SetPositionAndRotation(smoothedPosition, targetRotation);
        }

        private bool IsCameraLookInputActive()
        {
            var stickInput = new Vector2(
                Input.GetAxisRaw("RightAnalogHorizontal"),
                Input.GetAxisRaw("RightAnalogVertical"));
            return useRightStickForCamera && stickInput.magnitude > gamepadDeadZone
                || Mathf.Abs(Input.GetAxis("Mouse X")) > 0.001f
                || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.001f;
        }

        private void EnsureCameraTarget()
        {
            if (cameraTarget != null)
            {
                return;
            }

            cameraTarget = GetOrCreateChildTransform("CameraTarget", Vector3.up * cameraHeight);
            cameraTarget.rotation = useCinemachineCamera && cinemachineReady
                ? GetSceneRelativeCameraRotation(cameraPitch, cameraYaw)
                : Quaternion.Euler(0f, cameraYaw, 0f);
            smoothedCameraTargetPosition = cameraTarget.position;
            hasSmoothedCameraTargetPosition = true;
        }

        private void UpdateCameraTarget()
        {
            if (cameraTarget == null)
            {
                return;
            }

            var desiredPosition = transform.position + Vector3.up * cameraHeight;

            if (!hasSmoothedCameraTargetPosition)
            {
                smoothedCameraTargetPosition = desiredPosition;
                hasSmoothedCameraTargetPosition = true;
            }

            smoothedCameraTargetPosition = Vector3.SmoothDamp(
                smoothedCameraTargetPosition,
                desiredPosition,
                ref cameraTargetVelocity,
                cameraTargetSmoothTime);
            cameraTarget.position = smoothedCameraTargetPosition;
            cameraTarget.rotation = useCinemachineCamera && cinemachineReady
                ? GetSceneRelativeCameraRotation(cameraPitch, cameraYaw)
                : Quaternion.Euler(0f, cameraYaw, 0f);
        }

        private Vector3 GetSmoothedCameraPosition(Vector3 targetPosition, float activeSmoothTime)
        {
            if (activeSmoothTime <= 0.001f)
            {
                cameraVelocity = Vector3.zero;
                return targetPosition;
            }

            return Vector3.SmoothDamp(
                followCamera.transform.position,
                targetPosition,
                ref cameraVelocity,
                activeSmoothTime);
        }

        private void CaptureSceneCameraStartPose()
        {
            if (!useSceneCameraStartPose || followCamera == null || cameraTarget == null)
            {
                return;
            }

            cameraHeight = CaptureSceneCameraTargetHeight();
            var sceneTargetPosition = transform.position + Vector3.up * cameraHeight;
            cameraTarget.position = sceneTargetPosition;
            smoothedCameraTargetPosition = sceneTargetPosition;
            hasSmoothedCameraTargetPosition = true;

            sceneCameraStartRotation = followCamera.transform.rotation;

            var cameraForward = sceneCameraStartRotation * Vector3.forward;
            var flatForward = Vector3.ProjectOnPlane(cameraForward, Vector3.up);
            if (flatForward.sqrMagnitude > 0.001f)
            {
                cameraYaw = Quaternion.LookRotation(flatForward.normalized, Vector3.up).eulerAngles.y;
            }

            cameraPitch = Mathf.Clamp(NormalizePitch(sceneCameraStartRotation.eulerAngles.x), minCameraPitch, maxCameraPitch);
            sceneCameraStartPitch = cameraPitch;

            var yawRotation = Quaternion.Euler(0f, cameraYaw, 0f);
            cinemachineFollowOffset = Quaternion.Inverse(yawRotation) * (followCamera.transform.position - cameraTarget.position);
            sceneCameraLocalRotation = Quaternion.Inverse(yawRotation) * sceneCameraStartRotation;
            cinemachineCameraDistance = Mathf.Max(0.01f, -cinemachineFollowOffset.z);
            cinemachineCameraHeight = cinemachineFollowOffset.y;
            cameraDistance = cinemachineCameraDistance;
            hasSceneCameraStartPose = true;
        }

        private float CaptureSceneCameraTargetHeight()
        {
            var fallbackHeight = Mathf.Max(0.25f, cameraHeight);
            if (followCamera == null)
            {
                return fallbackHeight;
            }

            var cameraPosition = followCamera.transform.position;
            var cameraForward = followCamera.transform.forward.normalized;
            var playerPosition = transform.position;
            var verticalAxis = Vector3.up;
            var cameraToPlayer = cameraPosition - playerPosition;
            var verticalDot = Vector3.Dot(cameraForward, verticalAxis);
            var denominator = 1f - verticalDot * verticalDot;

            if (denominator <= 0.001f)
            {
                return fallbackHeight;
            }

            var rayDistance = (verticalDot * Vector3.Dot(cameraToPlayer, verticalAxis) - Vector3.Dot(cameraForward, cameraToPlayer)) / denominator;
            var targetHeight = Vector3.Dot(cameraToPlayer, verticalAxis) + rayDistance * verticalDot;
            return Mathf.Clamp(targetHeight, 0.25f, 3f);
        }

        private static float NormalizePitch(float pitch)
        {
            return pitch > 180f ? pitch - 360f : pitch;
        }

        private void EnsureCinemachineSetup()
        {
            if (!useCinemachineCamera && followCamera != null)
            {
                var existingBrain = followCamera.GetComponent<CinemachineBrain>();
                if (existingBrain != null)
                {
                    existingBrain.enabled = false;
                }

                cinemachineReady = false;
                return;
            }

            if (!useCinemachineCamera || followCamera == null)
            {
                return;
            }

            var brain = followCamera.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = followCamera.gameObject.AddComponent<CinemachineBrain>();
            }
            brain.enabled = true;
            brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            brain.m_DefaultBlend.m_Time = 0f;

            gameplayVirtualCamera = EnsureVirtualCamera(
                gameplayVirtualCamera,
                "CM Gameplay Camera",
                cameraTarget,
                null,
                gameplayCameraPriority,
                gameplayCameraDamping);

            cinemachineReady = gameplayVirtualCamera != null;
            UpdateCinemachinePriority();
        }

        private CinemachineVirtualCamera EnsureVirtualCamera(
            CinemachineVirtualCamera virtualCamera,
            string cameraName,
            Transform followTarget,
            Transform lookAtTarget,
            int priority,
            float damping)
        {
            if (virtualCamera == null)
            {
                var cameraObject = GetOrCreateSceneCameraObject(cameraName);
                virtualCamera = cameraObject.GetComponent<CinemachineVirtualCamera>();
                if (virtualCamera == null)
                {
                    virtualCamera = cameraObject.AddComponent<CinemachineVirtualCamera>();
                }
            }
            else
            {
                MoveVirtualCameraToSceneRig(virtualCamera, cameraName);
            }

            virtualCamera.Priority = priority;
            virtualCamera.Follow = followTarget;
            virtualCamera.LookAt = null;
            virtualCamera.m_Lens.FieldOfView = followCamera.fieldOfView;
            virtualCamera.transform.rotation = GetSceneRelativeCameraRotation(cameraPitch, cameraYaw);
            virtualCamera.PreviousStateIsValid = false;

            var thirdPersonFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if (thirdPersonFollow == null)
            {
                thirdPersonFollow = virtualCamera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
            }

            if (thirdPersonFollow != null)
            {
                var distance = Mathf.Max(0.01f, -cinemachineFollowOffset.z);
                thirdPersonFollow.Damping = new Vector3(damping, damping, damping);
                thirdPersonFollow.ShoulderOffset = new Vector3(Mathf.Abs(cinemachineFollowOffset.x), 0f, 0f);
                thirdPersonFollow.VerticalArmLength = 0f;
                thirdPersonFollow.CameraSide = cinemachineFollowOffset.x < 0f ? 0f : 1f;
                thirdPersonFollow.CameraDistance = distance;
                cinemachineCameraDistance = distance;
                cinemachineCameraHeight = cinemachineFollowOffset.y;
            }

            var composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
            if (composer != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(composer);
                }
                else
                {
                    DestroyImmediate(composer);
                }
            }

            return virtualCamera;
        }

        private GameObject GetOrCreateSceneCameraObject(string cameraName)
        {
            var rig = GetOrCreateSceneCameraRig();
            var existing = rig.Find(cameraName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            var cameraObject = new GameObject(cameraName);
            cameraObject.transform.SetParent(rig, false);
            cameraObject.transform.localPosition = Vector3.zero;
            cameraObject.transform.localRotation = Quaternion.identity;
            cameraObject.transform.localScale = Vector3.one;
            return cameraObject;
        }

        private void MoveVirtualCameraToSceneRig(CinemachineVirtualCamera virtualCamera, string cameraName)
        {
            if (virtualCamera == null)
            {
                return;
            }

            var rig = GetOrCreateSceneCameraRig();
            if (virtualCamera.transform.parent == rig)
            {
                return;
            }

            virtualCamera.gameObject.name = cameraName;
            virtualCamera.transform.SetParent(rig, true);
        }

        private Transform GetOrCreateSceneCameraRig()
        {
            var rigObject = GameObject.Find(SceneCameraRigName);
            if (rigObject == null)
            {
                rigObject = new GameObject(SceneCameraRigName);
                rigObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            return rigObject.transform;
        }

        private void ResetCameraState()
        {
            if (cameraTarget != null)
            {
                var targetPosition = transform.position + Vector3.up * cameraHeight;
                cameraTarget.position = targetPosition;
                cameraTarget.rotation = useCinemachineCamera && cinemachineReady
                    ? GetSceneRelativeCameraRotation(cameraPitch, cameraYaw)
                    : Quaternion.Euler(0f, cameraYaw, 0f);
                smoothedCameraTargetPosition = targetPosition;
                hasSmoothedCameraTargetPosition = true;
            }

            var cameraRotation = hasSceneCameraStartPose
                ? sceneCameraStartRotation
                : Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            if (gameplayVirtualCamera != null)
            {
                gameplayVirtualCamera.transform.rotation = cameraRotation;
                gameplayVirtualCamera.PreviousStateIsValid = false;
            }

            if (followCamera != null)
            {
                var lookTarget = cameraTarget != null
                    ? cameraTarget.position
                    : transform.position + Vector3.up * cameraHeight;
                var pitchDelta = Quaternion.Euler(cameraPitch - sceneCameraStartPitch, 0f, 0f);
                var cameraPosition = hasSceneCameraStartPose
                    ? lookTarget + Quaternion.Euler(0f, cameraYaw, 0f) * pitchDelta * cinemachineFollowOffset
                    : lookTarget - cameraRotation * Vector3.forward * cameraDistance;
                followCamera.transform.SetPositionAndRotation(
                    cameraPosition,
                    hasSceneCameraStartPose
                        ? Quaternion.Euler(0f, cameraYaw, 0f) * pitchDelta * sceneCameraLocalRotation
                        : Quaternion.LookRotation(lookTarget - cameraPosition, Vector3.up));
            }
        }

        private Transform GetOrCreateChildTransform(string childName, Vector3 localPosition)
        {
            var child = transform.Find(childName);
            if (child == null)
            {
                child = new GameObject(childName).transform;
                child.SetParent(transform, false);
            }

            child.localPosition = localPosition;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
            return child;
        }

        private void UpdateCinemachineCamera()
        {
            UpdateCinemachinePriority();

            if (gameplayVirtualCamera != null)
            {
                gameplayVirtualCamera.transform.rotation = GetSceneRelativeCameraRotation(cameraPitch, cameraYaw);
            }

            ForceCinemachineToSceneCameraPoseOnce();
        }

        private void ForceCinemachineToSceneCameraPoseOnce()
        {
            if (!hasSceneCameraStartPose || hasForcedCinemachineScenePose || followCamera == null)
            {
                return;
            }

            var cameraPosition = followCamera.transform.position;
            var cameraRotation = followCamera.transform.rotation;

            if (gameplayVirtualCamera != null)
            {
                gameplayVirtualCamera.ForceCameraPosition(cameraPosition, cameraRotation);
            }

            hasForcedCinemachineScenePose = true;
        }

        private Quaternion GetSceneRelativeCameraRotation(float pitch, float yaw)
        {
            if (!hasSceneCameraStartPose)
            {
                return Quaternion.Euler(pitch, yaw, 0f);
            }

            var yawRotation = Quaternion.Euler(0f, yaw, 0f);
            var pitchDelta = Quaternion.Euler(pitch - sceneCameraStartPitch, 0f, 0f);
            return yawRotation * pitchDelta * sceneCameraLocalRotation;
        }

        private void UpdateCinemachinePriority()
        {
            if (gameplayVirtualCamera != null)
            {
                gameplayVirtualCamera.Priority = gameplayCameraPriority;
            }
        }

        private void CacheAnimatorParameters()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                animatorParameterHashes = System.Array.Empty<int>();
                return;
            }

            var parameters = animator.parameters;
            animatorParameterHashes = new int[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                animatorParameterHashes[i] = parameters[i].nameHash;
            }
        }

        private void CacheAnimatorLayers()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                upperBodyFireLayerIndex = -1;
                return;
            }

            upperBodyFireLayerIndex = animator.GetLayerIndex(UpperBodyFireLayerName);
            SetUpperBodyFireLayerWeight(0f);
            SetAnimatorRootMotion(false);
        }

        private void UpdateUpperBodyFireLayer(bool fireHeld)
        {
            if (upperBodyFireLayerIndex < 0)
            {
                return;
            }

            if (isRolling)
            {
                upperBodyFireTimer = 0f;
                SetUpperBodyFireLayerWeight(0f);
                return;
            }

            if (fireHeld)
            {
                upperBodyFireTimer = upperBodyFireDuration;
                SetUpperBodyFireLayerWeight(1f);
                return;
            }

            upperBodyFireTimer = Mathf.Max(0f, upperBodyFireTimer - Time.deltaTime);
            SetUpperBodyFireLayerWeight(upperBodyFireTimer > 0f ? 1f : 0f);
        }

        private void SetUpperBodyFireLayerWeight(float weight)
        {
            if (animator != null && upperBodyFireLayerIndex >= 0)
            {
                animator.SetLayerWeight(upperBodyFireLayerIndex, weight);
            }
        }

        private void SetAnimatorRootMotion(bool enabled)
        {
            if (animator != null)
            {
                animator.applyRootMotion = enabled;
            }
        }

        private void CrossFadeToLocomotion()
        {
            if (HasAnimatorState(0, LocomotionStateHash))
            {
                animator.CrossFade(LocomotionStateHash, rollExitBlendTime, 0);
            }
        }

        private bool HasAnimatorState(int layerIndex, int stateHash)
        {
            return animator != null &&
                animator.runtimeAnimatorController != null &&
                layerIndex >= 0 &&
                layerIndex < animator.layerCount &&
                animator.HasState(layerIndex, stateHash);
        }

        private void ResetModelRootTransform()
        {
            if (modelRoot == null)
            {
                return;
            }

            modelRoot.localPosition = Vector3.zero;
            modelRoot.localRotation = Quaternion.identity;
        }

        private void UpdateRollRecovery()
        {
            if (modelRoot == null)
            {
                rollRecoveryTimer = 0f;
                return;
            }

            rollRecoveryTimer = Mathf.Max(0f, rollRecoveryTimer - Time.deltaTime);
            SnapToward(postRollFacingDirection);
            var blend = rollRecoveryDuration <= 0f ? 1f : 1f - rollRecoveryTimer / rollRecoveryDuration;
            var smooth = 1f - Mathf.Pow(1f - blend, 2f);

            modelRoot.localPosition = Vector3.zero;
            modelRoot.localRotation = Quaternion.Slerp(modelRoot.localRotation, Quaternion.identity, smooth);

            if (rollRecoveryTimer <= 0f)
            {
                ResetModelRootTransform();
            }
        }

        private void CommitModelRootOffset(bool resetRotation)
        {
            if (modelRoot == null || characterController == null)
            {
                return;
            }

            var visualPosition = modelRoot.position;
            modelRoot.localPosition = Vector3.zero;
            if (resetRotation)
            {
                modelRoot.localRotation = Quaternion.identity;
            }

            var correction = visualPosition - modelRoot.position;
            correction.y = 0f;
            if (correction.sqrMagnitude > 0.000001f)
            {
                characterController.Move(correction * rollRootMotionScale);
            }
        }

        private bool HasAnimatorParameter(int hash)
        {
            for (var i = 0; i < animatorParameterHashes.Length; i++)
            {
                if (animatorParameterHashes[i] == hash)
                {
                    return true;
                }
            }

            return false;
        }

        private void SetAnimatorFloat(int hash, float value, float dampTime)
        {
            if (animator != null && HasAnimatorParameter(hash))
            {
                animator.SetFloat(hash, value, dampTime, Time.deltaTime);
            }
        }

        private void SetAnimatorBool(int hash, bool value)
        {
            if (animator != null && HasAnimatorParameter(hash))
            {
                animator.SetBool(hash, value);
            }
        }

        private void SetAnimatorTrigger(int hash)
        {
            if (animator != null && HasAnimatorParameter(hash))
            {
                animator.SetTrigger(hash);
            }
        }

        private void ResetAnimatorTrigger(int hash)
        {
            if (animator != null && HasAnimatorParameter(hash))
            {
                animator.ResetTrigger(hash);
            }
        }
    }

}
