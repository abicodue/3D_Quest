using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController3D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualTransform;
    [SerializeField] private StaminaManager staminaManager;
    [SerializeField] private MeleeHitBoxAttack meleeHitBoxAttack;

    [Header("InputAction")]
    // Camera Input
    [SerializeField]
    private InputAction lookInputAction;
    private Vector2 lookInput;
    
    [SerializeField]
    private InputAction zoomInInputAction;
    private bool zoomInInput;

    [SerializeField]
    private InputAction zoomOutInputAction;

    // Jump Input
    [SerializeField]
    private InputAction jumpInputAction;
    private bool jumpInput;

    // Run Input
    [SerializeField]
    private InputAction runInputAction;
    private bool runInput;

    // Move Input
    [SerializeField]
    private InputAction moveInputAction;
    private Vector2 moveInput;   

    [Header("Camera")]
    [SerializeField] private float cameraRotationSpeed = 50f;
    //[SerializeField] private float playerRotationSpeed = 500f;
    [SerializeField] private float visualRotationSpeedFromIdle = 250f;
    [SerializeField] private float visualRotationSpeedWhileMoving = 500f;
    private bool wasMoving;
    private float pitch = 0f;
    private float cameraPivotYaw = 0f;
    private bool zoomOutInput;
    private float zoomStep = 2f;
    private float minZoomDistance = 0f;
    private float maxZoomDistance = 8f;
    private float currentZoomDistance = 4f;
    private bool isCameraRotationLocked = false;

    [Header("Jump")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpStaminaDrain = 3f;

    [Header("Run")]
    [SerializeField] private float runStaminaDrain = 5f;
    private bool runBlocked;    
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private Vector3 moveDirection;

    private bool isGrounded;
    private bool isRunning;
    private bool isVisualRotationLocked;
    private Vector3 visualFacingDirection;
    private bool isAttacking;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        if (cameraPivot == null) Debug.LogWarning("cameraPivot is missing");
        if (mainCamera == null) Debug.LogWarning("mainCamera is missing");
        if (groundCheckPoint == null) Debug.LogWarning("groundCheckPoint is missing");
        if (visualTransform == null) Debug.LogWarning("visualTransform is missing");

        if (staminaManager == null)
        {
            staminaManager = GetComponent<StaminaManager>();
        }

        if (meleeHitBoxAttack == null)
        {
            meleeHitBoxAttack = GetComponent<MeleeHitBoxAttack>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;        

        if (mainCamera != null)
        {
            currentZoomDistance = -mainCamera.localPosition.z;
        }
    }

    private void OnEnable()
    {
        lookInputAction.Enable();
        zoomInInputAction.Enable();
        zoomOutInputAction.Enable();
        moveInputAction.Enable();
        runInputAction.Enable();
        jumpInputAction.Enable();

        if (meleeHitBoxAttack != null)
        {
            meleeHitBoxAttack.OnAttackStarted += HandleAttackStarted;
            meleeHitBoxAttack.OnAttackEnded += HandleAttackEnded;
        }
    }

    private void OnDisable()
    {
        lookInputAction.Disable();
        zoomInInputAction.Disable();
        zoomOutInputAction.Disable();
        moveInputAction.Disable();
        runInputAction.Disable();
        jumpInputAction.Disable();

        if (meleeHitBoxAttack != null)
        {
            meleeHitBoxAttack.OnAttackStarted -= HandleAttackStarted;
            meleeHitBoxAttack.OnAttackEnded -= HandleAttackEnded;
        }
    }

    private void Update()
    {
        HandleInput();        
        HandleRotation();
        HandleZoom();
        HandleMoveDirection();
        HandleVisualRotation();
        CheckGround();
        HandleJump();
        HandleRun();                       
        UpdateAnimator();
    }

    private void FixedUpdate()
    {        
        HandleMovement();
    }

    private void HandleInput()
    {
        lookInput = lookInputAction.ReadValue<Vector2>();
        zoomInInput = zoomInInputAction.triggered;
        zoomOutInput = zoomOutInputAction.triggered;

        if (isAttacking)
        {
            moveInput = Vector2.zero;
            runInput = false;
            jumpInput = false;
            return;
        }
        moveInput = moveInputAction.ReadValue<Vector2>();
        runInput = runInputAction.IsPressed();
        jumpInput = jumpInputAction.WasPressedThisFrame();
        /*
        runInput = runInputAction.ReadValue<float>() > 0f;
        jumpInput = jumpInputAction.triggered;
        */
    }

    private void HandleRotation()
    {
        if (cameraPivot == null)
        {
            return;
        }

        if (isCameraRotationLocked)
        {
            return;
        }

        // Pitch
        pitch -= lookInput.y * cameraRotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        // Yaw
        float deltaYaw = lookInput.x * cameraRotationSpeed * Time.deltaTime;
        cameraPivotYaw += deltaYaw;

        /*
        if (moveInput.sqrMagnitude >= 0.0001f)
        {
            Vector3 playerForward = transform.forward;
            playerForward.y = 0f;
            playerForward.Normalize();

            Vector3 cameraForward = cameraPivot.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            float angleDiff = Vector3.SignedAngle(playerForward, cameraForward, Vector3.up);
            float maxTurn = playerRotationSpeed * Time.deltaTime;
            float turnAmount = Mathf.Clamp(angleDiff, -maxTurn, maxTurn);

            transform.Rotate(0f, turnAmount, 0f);

            cameraPivotYaw -= turnAmount;
            while (cameraPivotYaw > 180f) cameraPivotYaw -= 360f;
            while (cameraPivotYaw < -180f) cameraPivotYaw += 360f;
        }
        */
               
        cameraPivot.localRotation = Quaternion.Euler(pitch, cameraPivotYaw, 0f);
              
    }

    private void HandleZoom()
    {
        if (mainCamera == null)
        {
            return;
        }

        if (zoomInInput)
        {
            currentZoomDistance -= zoomStep;
        }

        if (zoomOutInput)
        {
            currentZoomDistance += zoomStep;
        }

        currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);

        Vector3 mainCameraPosition = mainCamera.localPosition;
        mainCameraPosition.z = -currentZoomDistance;
        mainCamera.localPosition = mainCameraPosition;
    }  

    private void HandleMoveDirection()
    {
        // CameraPivot 기준 forward 방향 벡터 이용
        Vector3 cameraPivotForward = cameraPivot.forward;
        cameraPivotForward.y = 0f;
        cameraPivotForward.Normalize();

        // CameraPivot 기준 right 벡터 이용
        Vector3 cameraPivotRight = cameraPivot.right;
        cameraPivotRight.y = 0f;
        cameraPivotRight.Normalize();

        // 입력값(moveInput) 같이 이용하여, 움직이는 direction 구하기
        moveDirection = cameraPivotForward * moveInput.y + cameraPivotRight * moveInput.x;
        if (moveDirection.sqrMagnitude > 0.0001f) moveDirection.Normalize();
    }

    private void HandleVisualRotation()
    {
        if (visualTransform == null)
        {
            return;
        }

        if (isVisualRotationLocked)
        {
            if (visualFacingDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion attackRotation = Quaternion.LookRotation(visualFacingDirection, Vector3.up);
            visualTransform.rotation = attackRotation;
            return;
        }

        if (moveDirection.sqrMagnitude < 0.0001f)
        {
            wasMoving = false;
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        float visualRotationSpeed = wasMoving ? visualRotationSpeedWhileMoving : visualRotationSpeedFromIdle;

        visualTransform.rotation = Quaternion.RotateTowards(visualTransform.rotation, targetRotation, visualRotationSpeed * Time.deltaTime);

        wasMoving = true;
    }

    private void CheckGround()
    {
        if (groundCheckPoint == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
        // Debug.Log($"grounded:{isGrounded}, point:{groundCheckPoint.position}, radius:{groundCheckRadius}, mask:{groundLayer.value}");
    }

    private void HandleJump()
    {
        if (isAttacking)
        {
            return;
        }

        if (jumpInput)
        {
            /*
            Vector3 velocity = rb.linearVelocity;
            */

            if (isGrounded && staminaManager.UseStamina(jumpStaminaDrain))
            {
                /*
                velocity.y = jumpForce;
                rb.linearVelocity = velocity;
                */

                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }            
        }
    }

    private void HandleRun()
    {
        if (isAttacking)
        {
            isRunning = false;
            return;
        }

        if (!runInput)
        {
            runBlocked = false;
        }
        
        if (runInput && !runBlocked && moveInput.sqrMagnitude > 0f)
        {
            if (staminaManager.UseStamina(runStaminaDrain * Time.deltaTime))
            {
                isRunning = true;
            }
            else
            {
                isRunning = false;
                runBlocked = true;
            }           
        }
        else
        {
            isRunning = false;
            if (isGrounded)
            {
                staminaManager.RecoverStamina(staminaManager.StaminaRecovery * Time.deltaTime);
            }        
        }
    }    

    private void UpdateAnimator()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        Vector3 horizontalVelocity = velocity;

        float speed = horizontalVelocity.magnitude;
        
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    private void HandleMovement()
    {
        float speed = isRunning ? runSpeed : walkSpeed;

        // velocity.y 값은 rb.linearVelocity.y 값을 이용하고, x,z축 방향 값만 바꿔서 넣어준다.
        Vector3 velocity = rb.linearVelocity;

        // 지상에선 공격중에 이동정지
        if (isGrounded && isAttacking)
        {
            velocity.x = 0f;
            velocity.z = 0f;
            rb.linearVelocity = velocity;
            return;
        }

        // 점프 후에 이동입력 없어도 이동 유지
        if (isGrounded)
        {
            velocity.x = moveDirection.x * speed;
            velocity.z = moveDirection.z * speed;
        }
        else
        {
            // 공중에서는 기존 속도 유지 = 즉, 아무것도 안 건드림
            // 코드를 적지 않는다! 구현을 하지 않는다! 논리적인 구조만으로 해결! = 최고의 성능/ 최고의 개발법
        }       

        rb.linearVelocity = velocity;
    }

    private void HandleAttackStarted()
    {
        if (cameraPivot == null)
        {
            return;
        }

        Vector3 forward = cameraPivot.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f)
        {
            return;
        }
        forward.Normalize();

        visualFacingDirection = forward;
        isVisualRotationLocked = true;
        isAttacking = true;

        if (isGrounded)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x = 0f;
            velocity.z = 0f;
            rb.linearVelocity = velocity;
        }
    }

    private void HandleAttackEnded()
    {
        isVisualRotationLocked = false;
        isAttacking = false;
    }

    private void OnDrawGizmos()
    {
        if (groundCheckPoint == null) return;

        Gizmos.color = isGrounded ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }

    public void LockCameraRotation()
    {
        isCameraRotationLocked = true;
    }

    public void UnlockCameraRotation()
    {
        isCameraRotationLocked = false;
    }
}