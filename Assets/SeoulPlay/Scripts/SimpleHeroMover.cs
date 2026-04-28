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
        private static readonly int RollingHash = Animator.StringToHash("Rolling");
        private static readonly int RollForwardStateHash = Animator.StringToHash("Base Layer.Roll Forward");
        private static readonly int RollBackwardStateHash = Animator.StringToHash("Base Layer.Roll Backward");
        private static readonly int RollLeftStateHash = Animator.StringToHash("Base Layer.Roll Left");
        private static readonly int RollRightStateHash = Animator.StringToHash("Base Layer.Roll Right");
        private static readonly int LocomotionStateHash = Animator.StringToHash("Base Layer.Locomotion");
        private const string UpperBodyFireLayerName = "Upper Body Fire";

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform modelRoot;
        [SerializeField] private Camera followCamera;
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private Transform rollCameraTarget;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float walkSpeed = 2.4f;
        [SerializeField, Min(0f)] private float runSpeed = 5.2f;
        [SerializeField, Min(0f)] private float turnSpeed = 180f;
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
        [SerializeField, Min(0f)] private float rollCameraSmoothTime = 0.12f;
        [SerializeField, Min(0f)] private float cameraTargetSmoothTime = 0.04f;
        [SerializeField, Min(0f)] private float rollCameraTargetSmoothTime = 0.14f;

        [Header("Cinemachine")]
        [SerializeField] private bool useCinemachineCamera = true;
        [SerializeField] private CinemachineVirtualCamera gameplayVirtualCamera;
        [SerializeField] private CinemachineVirtualCamera rollVirtualCamera;
        [SerializeField] private int gameplayCameraPriority = 20;
        [SerializeField] private int inactiveCameraPriority = 5;
        [SerializeField] private int rollCameraPriority = 30;
        [SerializeField] private float cinemachineCameraDistance = 6f;
        [SerializeField] private float cinemachineCameraHeight = 1.65f;
        [SerializeField] private Vector3 cinemachineFollowOffset = new Vector3(0f, 1.65f, -6f);
        [SerializeField, Range(0f, 1f)] private float rollCameraFollowStartNormalized = 0f;
        [SerializeField, Min(0f)] private float rollCameraGroundHeight = 0.15f;
        [SerializeField] private bool lockRollCameraYaw = true;
        [SerializeField, Min(0f)] private float gameplayCameraDamping = 0.25f;
        [SerializeField, Min(0f)] private float rollCameraDamping = 0.1f;

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

        [Header("Fire")]
        [SerializeField, Min(0f)] private float upperBodyFireDuration = 0.45f;
        [SerializeField] private bool driveAnimatorAimFromAimInput;

        private CharacterController characterController;
        private float verticalVelocity;
        private float cameraYaw;
        private float lockedRollCameraYaw;
        private float lockedRollCameraPitch;
        private float rollTimer;
        private float rollElapsedTime;
        private float activeRollDuration;
        private float rollCooldownTimer;
        private float rollRecoveryTimer;
        private float upperBodyFireTimer;
        private int upperBodyFireLayerIndex = -1;
        private bool wasFirePressed;
        private bool isRolling;
        private bool hasSmoothedCameraTargetPosition;
        private bool hasSceneCameraStartPose;
        private bool cinemachineReady;
        private bool wasCinemachineRolling;
        private float sceneCameraStartPitch;
        private Quaternion sceneCameraStartRotation;
        private Quaternion sceneCameraLocalRotation;
        private Vector3 cameraVelocity;
        private Vector3 cameraTargetVelocity;
        private Vector3 smoothedCameraTargetPosition;
        private Vector3 rollDirection;
        private Vector3 rollFacingDirection;
        private Vector3 postRollFacingDirection;
        private Vector3 lockedRollCameraTargetPosition;
        private Vector3 rollCameraTargetStartPosition;
        private Vector3 rollCameraTargetEndPosition;
        private Vector2 lastLocomotionInput;
        private int[] animatorParameterHashes = System.Array.Empty<int>();

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

        private void Update()
        {
            UpdateCameraInput();

            var input = GetMoveInput();
            input = Vector2.ClampMagnitude(input, 1f);

            var aimPressed = IsAimPressed();
            var isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetButton("LeftStickClick");
            var moveSpeed = isRunning ? runSpeed : walkSpeed;
            var worldMove = GetCameraRelativeMove(input);

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            rollCooldownTimer = Mathf.Max(0f, rollCooldownTimer - Time.deltaTime);
            if (!isRolling && characterController.isGrounded && rollCooldownTimer <= 0f && IsRollPressed())
            {
                StartRoll(input);
            }

            if (isRolling)
            {
                UpdateRoll();
            }
            else
            {
                characterController.Move((worldMove * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);
                UpdateFacing(worldMove, input, aimPressed);
            }

            UpdateAnimator(input, isRunning, aimPressed);
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
            else
            {
                cameraYaw += Input.GetAxis("Mouse X") * turnSpeed * Time.deltaTime;
            }

            var gamepadPitch = Input.GetAxis("RightAnalogVertical");
            var pitchInput = useRightStickForCamera && Mathf.Abs(gamepadPitch) > gamepadDeadZone
                ? gamepadPitch
                : Input.GetAxis("Mouse Y");
            var pitchDirection = invertVerticalLook ? 1f : -1f;
            cameraPitch += pitchInput * pitchDirection * cameraPitchSpeed * Time.deltaTime;
            cameraPitch = Mathf.Clamp(cameraPitch, minCameraPitch, maxCameraPitch);
        }

        private Vector3 GetCameraRelativeMove(Vector2 input)
        {
            var yawRotation = Quaternion.Euler(0f, cameraYaw, 0f);
            var forward = yawRotation * Vector3.forward;
            var right = yawRotation * Vector3.right;
            return Vector3.ClampMagnitude(right * input.x + forward * input.y, 1f);
        }

        private void UpdateFacing(Vector3 worldMove, Vector2 input, bool aimPressed)
        {
            if (aimPressed || (input.sqrMagnitude > 0.001f && input.y <= 0.1f))
            {
                var cameraForward = Quaternion.Euler(0f, cameraYaw, 0f) * Vector3.forward;
                RotateToward(cameraForward);
                return;
            }

            if (worldMove.sqrMagnitude > 0.001f)
            {
                RotateToward(worldMove);
            }
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

        private void SnapToward(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private bool IsAimPressed()
        {
            return Input.GetMouseButton(1)
                || Input.GetButton("LB")
                || Input.GetAxis("LT") > 0.2f;
        }

        private bool IsRollPressed()
        {
            return Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("B");
        }

        private void StartRoll(Vector2 input)
        {
            var rollInput = GetCardinalRollInput(ResolveRollInput(input));
            rollDirection = GetCameraRelativeMove(rollInput).normalized;
            rollFacingDirection = rollDirection;
            postRollFacingDirection = rollInput.y < -0.1f
                ? Quaternion.Euler(0f, cameraYaw, 0f) * Vector3.forward
                : rollFacingDirection;
            SnapToward(rollFacingDirection);
            activeRollDuration = Mathf.Max(0.05f, rollDuration - rollEndEarlyTime);
            rollTimer = activeRollDuration;
            rollElapsedTime = 0f;
            lockedRollCameraYaw = cameraYaw;
            lockedRollCameraPitch = cameraPitch;
            lockedRollCameraTargetPosition = cameraTarget != null
                ? cameraTarget.position
                : transform.position + Vector3.up * cameraHeight;
            rollCameraTargetStartPosition = lockedRollCameraTargetPosition;
            rollCameraTargetEndPosition = rollCameraTargetStartPosition + rollDirection * rollSpeed * activeRollDuration;
            rollCameraTargetEndPosition.y = rollCameraTargetStartPosition.y;
            rollCooldownTimer = rollCooldown;
            rollRecoveryTimer = 0f;
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
            if (animator != null)
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

        private void UpdateAnimator(Vector2 input, bool isRunning, bool aimPressed)
        {
            if (animator == null)
            {
                return;
            }

            var speed01 = isRolling ? 0f : input.magnitude;
            SetAnimatorFloat(MoveXHash, isRolling ? 0f : input.x, 0.1f);
            SetAnimatorFloat(MoveZHash, isRolling ? 0f : input.y, 0.1f);
            SetAnimatorFloat(SpeedHash, isRunning ? speed01 * 2f : speed01, 0.1f);
            SetAnimatorBool(GroundedHash, characterController.isGrounded);
            SetAnimatorBool(AimHash, driveAnimatorAimFromAimInput && aimPressed);

            var firePressed = Input.GetMouseButton(0) || Input.GetButton("RB") || Input.GetAxis("RT") > 0.2f;
            if (firePressed && !wasFirePressed && !isRolling)
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

        private void UpdateCamera()
        {
            if (followCamera == null)
            {
                return;
            }

            EnsureCameraTarget();
            UpdateCameraTarget();

            if (hasSceneCameraStartPose)
            {
                UpdateScenePoseCamera();
                return;
            }

            if (useCinemachineCamera && cinemachineReady)
            {
                UpdateCinemachineCamera();
                return;
            }

            var yawRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            var lookTarget = cameraTarget.position;
            var targetPosition = lookTarget - yawRotation * Vector3.forward * cameraDistance;
            var activeSmoothTime = isRolling || rollRecoveryTimer > 0f
                ? rollCameraSmoothTime
                : cameraSmoothTime;
            var smoothedPosition = Vector3.SmoothDamp(
                followCamera.transform.position,
                targetPosition,
                ref cameraVelocity,
                activeSmoothTime);

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
            var activeSmoothTime = isRolling || rollRecoveryTimer > 0f
                ? rollCameraSmoothTime
                : IsCameraLookInputActive() ? 0f : cameraSmoothTime;
            var smoothedPosition = activeSmoothTime <= 0.001f
                ? targetPosition
                : Vector3.SmoothDamp(
                    followCamera.transform.position,
                    targetPosition,
                    ref cameraVelocity,
                    activeSmoothTime);

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
            cameraTarget.rotation = Quaternion.Euler(0f, cameraYaw, 0f);
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
            var activeSmoothTime = isRolling || rollRecoveryTimer > 0f
                ? rollCameraTargetSmoothTime
                : cameraTargetSmoothTime;

            if (!hasSmoothedCameraTargetPosition)
            {
                smoothedCameraTargetPosition = desiredPosition;
                hasSmoothedCameraTargetPosition = true;
            }

            smoothedCameraTargetPosition = Vector3.SmoothDamp(
                smoothedCameraTargetPosition,
                desiredPosition,
                ref cameraTargetVelocity,
                activeSmoothTime);
            cameraTarget.position = smoothedCameraTargetPosition;
            cameraTarget.rotation = Quaternion.Euler(0f, cameraYaw, 0f);
        }

        private void CaptureSceneCameraStartPose()
        {
            if (!useSceneCameraStartPose || followCamera == null || cameraTarget == null)
            {
                return;
            }

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

        private static float NormalizePitch(float pitch)
        {
            return pitch > 180f ? pitch - 360f : pitch;
        }

        private void EnsureCinemachineSetup()
        {
            if (useSceneCameraStartPose && followCamera != null)
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
            brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            brain.m_DefaultBlend.m_Time = 0f;

            if (rollCameraTarget == null)
            {
                rollCameraTarget = GetOrCreateChildTransform("RollCameraTarget", Vector3.up * rollCameraGroundHeight);
            }

            gameplayVirtualCamera = EnsureVirtualCamera(
                gameplayVirtualCamera,
                "CM Gameplay Camera",
                cameraTarget,
                null,
                gameplayCameraPriority,
                gameplayCameraDamping);

            rollVirtualCamera = EnsureVirtualCamera(
                rollVirtualCamera,
                "CM Roll Camera",
                rollCameraTarget,
                null,
                inactiveCameraPriority,
                rollCameraDamping);

            cinemachineReady = gameplayVirtualCamera != null && rollVirtualCamera != null;
            UpdateCinemachinePriorities(false);
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
                var cameraObject = GetOrCreateChildTransform(cameraName, Vector3.zero).gameObject;
                virtualCamera = cameraObject.GetComponent<CinemachineVirtualCamera>();
                if (virtualCamera == null)
                {
                    virtualCamera = cameraObject.AddComponent<CinemachineVirtualCamera>();
                }
            }

            virtualCamera.Priority = priority;
            virtualCamera.Follow = followTarget;
            virtualCamera.LookAt = lookAtTarget;
            virtualCamera.m_Lens.FieldOfView = followCamera.fieldOfView;
            virtualCamera.transform.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            virtualCamera.PreviousStateIsValid = false;

            var transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer == null)
            {
                transposer = virtualCamera.AddCinemachineComponent<CinemachineTransposer>();
            }

            transposer.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;
            transposer.m_FollowOffset = cinemachineFollowOffset;
            transposer.m_XDamping = damping;
            transposer.m_YDamping = damping;
            transposer.m_ZDamping = damping;

            if (lookAtTarget != null)
            {
                var composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
                if (composer == null)
                {
                    composer = virtualCamera.AddCinemachineComponent<CinemachineComposer>();
                }

                composer.m_TrackedObjectOffset = Vector3.zero;
                composer.m_HorizontalDamping = damping;
                composer.m_VerticalDamping = damping;
            }
            else
            {
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
            }

            return virtualCamera;
        }

        private void ResetCameraState()
        {
            if (cameraTarget != null)
            {
                var targetPosition = transform.position + Vector3.up * cameraHeight;
                cameraTarget.position = targetPosition;
                cameraTarget.rotation = Quaternion.Euler(0f, cameraYaw, 0f);
                smoothedCameraTargetPosition = targetPosition;
                hasSmoothedCameraTargetPosition = true;
            }

            if (rollCameraTarget != null)
            {
                rollCameraTarget.position = cameraTarget != null
                    ? cameraTarget.position
                    : transform.position + Vector3.up * rollCameraGroundHeight;
                rollCameraTarget.rotation = Quaternion.Euler(0f, cameraYaw, 0f);
            }

            var cameraRotation = hasSceneCameraStartPose
                ? sceneCameraStartRotation
                : Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            if (gameplayVirtualCamera != null)
            {
                gameplayVirtualCamera.transform.rotation = cameraRotation;
                gameplayVirtualCamera.PreviousStateIsValid = false;
            }

            if (rollVirtualCamera != null)
            {
                rollVirtualCamera.transform.rotation = cameraRotation;
                rollVirtualCamera.PreviousStateIsValid = false;
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
            var rollCameraActive = isRolling && IsRollCameraFollowActive();
            UpdateCinemachinePriorities(rollCameraActive);
            UpdateRollCameraTarget();

            if (gameplayVirtualCamera != null)
            {
                gameplayVirtualCamera.transform.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            }

            if (rollVirtualCamera != null)
            {
                rollVirtualCamera.transform.rotation = Quaternion.Euler(GetActiveCameraPitch(), GetActiveCameraYaw(), 0f);
                if (isRolling)
                {
                    rollVirtualCamera.PreviousStateIsValid = false;
                }
            }
        }

        private void UpdateCinemachinePriorities(bool rolling)
        {
            if (wasCinemachineRolling == rolling && Application.isPlaying)
            {
                return;
            }

            wasCinemachineRolling = rolling;

            if (gameplayVirtualCamera != null)
            {
                gameplayVirtualCamera.Priority = rolling ? inactiveCameraPriority : gameplayCameraPriority;
            }

            if (rollVirtualCamera != null)
            {
                rollVirtualCamera.Priority = rolling ? rollCameraPriority : inactiveCameraPriority;
            }
        }

        private void UpdateRollCameraTarget()
        {
            if (rollCameraTarget == null)
            {
                return;
            }

            var activeYaw = GetActiveCameraYaw();
            var followActive = isRolling && IsRollCameraFollowActive();
            if (followActive)
            {
                rollCameraTarget.position = Vector3.Lerp(
                    rollCameraTargetStartPosition,
                    rollCameraTargetEndPosition,
                    GetRollCameraProgress01());
            }
            else
            {
                rollCameraTarget.position = cameraTarget.position;
            }
            rollCameraTarget.rotation = Quaternion.Euler(0f, activeYaw, 0f);
        }

        private bool IsRollCameraFollowActive()
        {
            if (!isRolling)
            {
                return false;
            }

            var duration = activeRollDuration > 0f ? activeRollDuration : Mathf.Max(0.05f, rollDuration - rollEndEarlyTime);
            return rollElapsedTime >= duration * rollCameraFollowStartNormalized;
        }

        private float GetRollCameraProgress01()
        {
            var duration = activeRollDuration > 0f ? activeRollDuration : Mathf.Max(0.05f, rollDuration - rollEndEarlyTime);
            var startTime = duration * rollCameraFollowStartNormalized;
            var remainingDuration = Mathf.Max(0.001f, duration - startTime);
            var progress = Mathf.Clamp01((rollElapsedTime - startTime) / remainingDuration);
            return progress * progress * (3f - 2f * progress);
        }

        private float GetActiveCameraYaw()
        {
            return isRolling && lockRollCameraYaw ? lockedRollCameraYaw : cameraYaw;
        }

        private float GetActiveCameraPitch()
        {
            return isRolling && lockRollCameraYaw ? lockedRollCameraPitch : cameraPitch;
        }

        private void CacheAnimatorParameters()
        {
            if (animator == null)
            {
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
            if (animator == null)
            {
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
            if (animator != null)
            {
                animator.CrossFade(LocomotionStateHash, rollExitBlendTime, 0);
            }
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
