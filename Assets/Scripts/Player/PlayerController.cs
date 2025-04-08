using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private Camera mainCamera;
    private CharacterController characterController;
    private Animator animator;
    private InputSystem_Actions playerInput;
    private StateMachine stateMachine;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 1.0f;
    [SerializeField] private float runSpeed = 1.0f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float airHorizontalMovementSpeed = 1.0f;
    [SerializeField] private float landingDuration = 1.0f;

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeDuration = 1.0f;
    [SerializeField] private float dodgeDistance = 1.0f;

    [Header("Sit Settings")]
    [SerializeField] private float sitDuration = 1.0f;

    [Header("Ground Raycast Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 1.0f;

    [Header("Miscellaneous")]
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float rotationSpeed = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool isDebugEnabled = false;
    [SerializeField] private TextMeshProUGUI currentStateText;

    public Camera MainCamera => mainCamera;
    public CharacterController CharacterController => characterController;
    public Animator Animator => animator;
    public Vector2 MovementInput { get; private set; }
    public bool IsRunPressed { get; private set; }
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float JumpHeight => jumpHeight;
    public float AirHorizontalMovementSpeed => airHorizontalMovementSpeed;
    public float LandingDuration => landingDuration;
    public float DodgeDistance => dodgeDistance;
    public float DodgeDuration => dodgeDuration;
    public float SitDuration => sitDuration;
    public float Gravity => gravity;
    public float RotationSpeed => rotationSpeed;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        mainCamera = Camera.main;
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerInput = new InputSystem_Actions();
        stateMachine = new StateMachine();
    }

    private void Start()
    {
        if (isDebugEnabled)
            currentStateText.gameObject.SetActive(true);

        stateMachine.Initialize(new IdleState(this, stateMachine));
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
        playerInput.Player.Move.performed += context => MovementInput = context.ReadValue<Vector2>();
        playerInput.Player.Move.canceled += context => MovementInput = Vector2.zero;
        playerInput.Player.Run.started += context => IsRunPressed = true;
        playerInput.Player.Run.canceled += context => IsRunPressed = false;

        playerInput.Player.Jump.started += context => TryJump();
        playerInput.Player.Dodge.started += context => TryDodge();
        playerInput.Player.Sit.started += context => TrySit();
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
    }

    private void Update()
    {
        stateMachine.CurrentState.Update();

        if (isDebugEnabled)
            currentStateText.text = $"Current State: {stateMachine.CurrentState}";
    }

    public void Move(Vector3 movementDirection, float movementSpeed)
    {
        movementDirection.x *= movementSpeed;
        movementDirection.z *= movementSpeed;

        characterController.Move(movementDirection * Time.deltaTime);
        RotateToward(new Vector3(movementDirection.x, 0.0f, movementDirection.z));
    }

    public Vector3 CalculateMovementDirection()
    {
        // Convert input to world space relative to camera direction
        Vector3 camForward = MainCamera.transform.forward;
        Vector3 camRight = MainCamera.transform.right;
        camForward.y = 0.0f;
        camRight.y = 0.0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 movementDirection = camForward * MovementInput.y + camRight * MovementInput.x;
        return movementDirection.normalized;
    }

    public void TryJump()
    {
        if (stateMachine.CurrentState is not JumpState && stateMachine.CurrentState is not FallState && stateMachine.CurrentState is not DodgeState && stateMachine.CurrentState is not SitState && IsGrounded())
        {
            stateMachine.ChangeState(new JumpState(this, stateMachine));
        }
    }

    public void TryDodge()
    {
        if (stateMachine.CurrentState is not DodgeState && MovementInput.magnitude > 0.0f)
        {
            stateMachine.ChangeState(new DodgeState(this, stateMachine));
        }
    }

    public void TrySit()
    {
        if (stateMachine.CurrentState is IdleState || stateMachine.CurrentState is WalkState || stateMachine.CurrentState is RunState)
        {
            stateMachine.ChangeState(new SitState(this, stateMachine));
        }
    }

    public void OnSitEndComplete()
    {
        stateMachine.ChangeState(new IdleState(this, stateMachine));
    }

    public void RotateToward(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public bool IsGrounded()
    {
        Vector3 raycastOrigin = transform.position + new Vector3(0.0f, groundCheckDistance * 0.9f, 0.0f);
        return Physics.Raycast(raycastOrigin, Vector3.down, groundCheckDistance, groundLayer);
    }
}
