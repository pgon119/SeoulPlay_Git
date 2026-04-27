using UnityEngine;

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

        [Header("Movement")]
        [SerializeField, Min(0f)] private float walkSpeed = 2.4f;
        [SerializeField, Min(0f)] private float runSpeed = 5.2f;
        [SerializeField, Min(0f)] private float turnSpeed = 180f;
        [SerializeField, Min(0f)] private float gamepadTurnSpeed = 150f;
        [SerializeField, Min(0f)] private float gamepadDeadZone = 0.2f;
        [SerializeField] private float gravity = -20f;

        [Header("Camera")]
        [SerializeField, Min(0f)] private float cameraDistance = 5f;
        [SerializeField, Min(0f)] private float cameraHeight = 1.45f;
        [SerializeField] private float cameraPitch = 15f;
        [SerializeField, Min(0f)] private float cameraPitchSpeed = 120f;
        [SerializeField] private float minCameraPitch = -25f;
        [SerializeField] private float maxCameraPitch = 65f;
        [SerializeField] private bool invertVerticalLook;
        [SerializeField, Min(0f)] private float cameraSmoothTime = 0.08f;
        [SerializeField, Min(0f)] private float rollCameraSmoothTime = 0.02f;

        [Header("Roll")]
        [SerializeField, Min(0f)] private float rollSpeed = 6.8f;
        [SerializeField, Min(0f)] private float rollDuration = 0.72f;
        [SerializeField, Min(0f)] private float rollCooldown = 0.2f;
        [SerializeField] private bool useRollRootMotion = true;
        [SerializeField] private bool useModelOffsetRootMotion = true;
        [SerializeField, Min(0f)] private float rollRootMotionScale = 1f;
        [SerializeField, Min(0f)] private float rollEndEarlyTime = 0.1f;
        [SerializeField, Min(0f)] private float rollExitBlendTime = 0.12f;
        [SerializeField, Min(0f)] private float rollRecoveryDuration = 0.12f;

        [Header("Fire")]
        [SerializeField, Min(0f)] private float upperBodyFireDuration = 0.45f;

        private CharacterController characterController;
        private float verticalVelocity;
        private float cameraYaw;
        private float rollTimer;
        private float rollCooldownTimer;
        private float rollRecoveryTimer;
        private float upperBodyFireTimer;
        private int upperBodyFireLayerIndex = -1;
        private bool wasFirePressed;
        private bool isRolling;
        private Vector3 cameraVelocity;
        private Vector3 rollDirection;
        private Vector3 rollFacingDirection;
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

            ResetModelRootTransform();
            cameraYaw = transform.eulerAngles.y;
            cameraPitch = Mathf.Clamp(cameraPitch, minCameraPitch, maxCameraPitch);
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
            var gamepadTurn = Input.GetAxis("RightAnalogHorizontal");
            if (Mathf.Abs(gamepadTurn) > gamepadDeadZone)
            {
                cameraYaw += gamepadTurn * gamepadTurnSpeed * Time.deltaTime;
            }
            else
            {
                cameraYaw += Input.GetAxis("Mouse X") * turnSpeed * Time.deltaTime;
            }

            var gamepadPitch = Input.GetAxis("RightAnalogVertical");
            var pitchInput = Mathf.Abs(gamepadPitch) > gamepadDeadZone ? gamepadPitch : Input.GetAxis("Mouse Y");
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

        private bool IsAimPressed()
        {
            return Input.GetMouseButton(1) || Input.GetButton("LB") || Input.GetAxis("LT") > 0.2f;
        }

        private bool IsRollPressed()
        {
            return Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("B");
        }

        private void StartRoll(Vector2 input)
        {
            var rollInput = GetCardinalRollInput(ResolveRollInput(input));
            rollDirection = GetCameraRelativeMove(rollInput).normalized;
            rollFacingDirection = useRollRootMotion
                ? Quaternion.Euler(0f, cameraYaw, 0f) * Vector3.forward
                : rollDirection;
            rollTimer = Mathf.Max(0.05f, rollDuration - rollEndEarlyTime);
            rollCooldownTimer = rollCooldown;
            rollRecoveryTimer = 0f;
            isRolling = true;
            verticalVelocity = 0f;
            cameraVelocity = Vector3.zero;

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

        private static int GetRollStateHash(Vector2 input)
        {
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
            CrossFadeToLocomotion();
            SetAnimatorRootMotion(false);
            cameraVelocity = Vector3.zero;
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
            SetAnimatorBool(AimHash, aimPressed);

            var firePressed = Input.GetMouseButton(0) || Input.GetButton("RB") || Input.GetAxis("RT") > 0.2f;
            if (firePressed && !wasFirePressed && !isRolling)
            {
                upperBodyFireTimer = upperBodyFireDuration;
                SetUpperBodyFireLayerWeight(1f);
                SetAnimatorTrigger(FireHash);
            }
            wasFirePressed = firePressed;

            UpdateUpperBodyFireLayer();

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

            var yawRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            var lookTarget = transform.position + Vector3.up * cameraHeight;
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

        private void UpdateUpperBodyFireLayer()
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

    [RequireComponent(typeof(Animator))]
    public sealed class AnimatorRootMotionRelay : MonoBehaviour
    {
        [SerializeField] private SimpleHeroMover mover;
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (mover == null)
            {
                mover = GetComponentInParent<SimpleHeroMover>();
            }
        }

        private void OnAnimatorMove()
        {
            if (animator != null && mover != null)
            {
                mover.ApplyAnimatorRootMotion(animator.deltaPosition, animator.deltaRotation);
            }
        }
    }
}
