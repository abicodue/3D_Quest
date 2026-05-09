using UnityEngine;

public class PlayerController3D_Old : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float rotationSpeed = 1000f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraPivot;

    // 카메라 관련 Input
    private float mouseX; 
    private float mouseY;
    private float pitch = 0f;

    // 이동 관련 Input
    private float moveX;
    private float moveZ;

    private bool isGrounded;
    private bool isRunning;



    private void Awake()
    {
        // TODO:
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        Debug.Log($"Animator found on: {animator.gameObject.name}");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // TODO:
        HandleInput();
        HandleRotation();
        CheckGround();
        HandleJump();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        // TODO:
        HandleMovement();
    }

    private void HandleInput()
    {       
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");
        
        isRunning = Input.GetKey(KeyCode.LeftShift) && (moveX != 0f || moveZ != 0f);
        
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
    }

    private void HandleRotation()
    {
        // 플레이어 마우스 X로 좌우 회전
        transform.Rotate(0f, mouseX * rotationSpeed, 0f);
        // cameraPivot 마우스 Y로 상하 회전
        pitch -= mouseY * rotationSpeed;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
      
    }

    private void HandleMovement()
    {
        // 카메라 기준 forward 방향 벡터 이용
        Vector3 cameraForward = cameraPivot.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        // 카메라 기준 right 벡터 이용
        Vector3 cameraRight = cameraPivot.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        // 입력값(moveX, moveZ) 같이 이용하여, 움직이는 direction 구하기
        Vector3 direction = cameraForward * moveZ + cameraRight * moveX;
        if (direction != Vector3.zero) direction.Normalize();

        float speed = isRunning ? runSpeed : walkSpeed;

        // velocity.y 값은 rb.linearVelocity.y 값을 이용하고, x,z축 방향 값만 바꿔서 넣어준다.
        Vector3 velocity = rb.linearVelocity;
        velocity.x = direction.x * speed;
        velocity.z = direction.z * speed;
      
        rb.linearVelocity = velocity;

        Debug.Log($"moveX={moveX}, moveZ={moveZ}, dir={direction}, vel={rb.linearVelocity}");
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                Vector3 velocity = rb.linearVelocity;
                velocity.y = jumpForce;
                rb.linearVelocity = velocity;
            }
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
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

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}