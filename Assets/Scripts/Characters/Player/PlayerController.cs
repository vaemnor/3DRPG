using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private Camera mainCamera;
    private CharacterController characterController;
    private Animator animator;
    private InputSystem_Actions playerInput;
    private StateMachine stateMachine;
    private HealthBar healthBar;

    [Header("Movement Settings")]
    [Tooltip("Speed at which the character walks.")]
    [SerializeField] private float walkSpeed = 1.0f;
    [Tooltip("Speed at which the character runs.")]
    [SerializeField] private float runSpeed = 1.0f;

    [Header("Jump Settings")]
    [Tooltip("Maximum height the character can reach when jumping.")]
    [SerializeField] private float jumpHeight = 1.0f;
    [Tooltip("Speed at which the character can move horizontally while in the air.")]
    [SerializeField] private float airHorizontalMovementSpeed = 1.0f;

    [Header("Dodge Settings")]
    [Tooltip("Duration of the dodge movement in seconds.")]
    [SerializeField] private float dodgeDuration = 1.0f;
    [Tooltip("Distance the character covers when dodging.")]
    [SerializeField] private float dodgeDistance = 1.0f;

    [Header("Attack Settings")]
    [Tooltip("The hitbox that is instantiated upon attacking.")]
    [SerializeField] private GameObject hitbox;

    [Header("Sit Settings")]
    [Tooltip("Duration the character remains in the sitting state.")]
    [SerializeField] private float sitDuration = 1.0f;

    [Header("Death Settings")]
    [Tooltip("Duration in seconds at which the character will remain dead.")]
    [SerializeField] private float deathDuration = 1.0f;

    [Header("Ground Raycast Settings")]
    [Tooltip("Layer(s) considered as ground for raycasting purposes.")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Distance below the character to check for ground.")]
    [SerializeField] private float groundCheckDistance = 1.0f;

    [Header("Miscellaneous Settings")]
    [Tooltip("Gravity force applied to the character.")]
    [SerializeField] private float gravity = 9.81f;
    [Tooltip("Speed at which the character rotates to face movement direction.")]
    [SerializeField] private float rotationSpeed = 1.0f;

    [Header("Debug Settings")]
    [Tooltip("Toggle to enable or disable debug output in the editor.")]
    [SerializeField] private bool isDebugEnabled = false;

    public Camera MainCamera => mainCamera;
    public CharacterController CharacterController => characterController;
    public Animator Animator => animator;
    public Vector2 MovementInput { get; private set; }
    public bool IsRunPressed { get; private set; }
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float JumpHeight => jumpHeight;
    public float AirHorizontalMovementSpeed => airHorizontalMovementSpeed;
    public float DodgeDistance => dodgeDistance;
    public float DodgeDuration => dodgeDuration;
    public float SitDuration => sitDuration;
    public float DeathDuration => deathDuration;
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
        healthBar = GetComponentInChildren<HealthBar>();
    }

    private void Start()
    {
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
        playerInput.Player.Attack.started += context => TryAttack();
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
            Debug.Log($"Current State: {stateMachine.CurrentState}");
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
        if
        (
            stateMachine.CurrentState is not HitGroundState &&
            stateMachine.CurrentState is not HitAirState &&
            stateMachine.CurrentState is not DieGroundState &&
            stateMachine.CurrentState is not DieAirState &&
            stateMachine.CurrentState is not ResurrectState &&
            stateMachine.CurrentState is not JumpState &&
            stateMachine.CurrentState is not FallState &&
            stateMachine.CurrentState is not DodgeState &&
            IsGrounded()
        )
        {
            stateMachine.ChangeState(new JumpState(this, stateMachine));
        }
    }

    public void TryDodge()
    {
        if
        (
            stateMachine.CurrentState is not HitGroundState &&
            stateMachine.CurrentState is not HitAirState &&
            stateMachine.CurrentState is not DieGroundState &&
            stateMachine.CurrentState is not DieAirState &&
            stateMachine.CurrentState is not ResurrectState &&
            stateMachine.CurrentState is not DodgeState &&
            MovementInput.magnitude > 0.0f
        )
        {
            stateMachine.ChangeState(new DodgeState(this, stateMachine));
        }
    }

    public void TryAttack()
    {
        if ((stateMachine.CurrentState is IdleState || stateMachine.CurrentState is WalkState || stateMachine.CurrentState is RunState) && stateMachine.CurrentState is not AttackState)
        {
            stateMachine.ChangeState(new AttackState(this, stateMachine));
        }
    }

    public void TrySit()
    {
        if ((stateMachine.CurrentState is IdleState || stateMachine.CurrentState is WalkState || stateMachine.CurrentState is RunState) && stateMachine.CurrentState is not SitState)
        {
            stateMachine.ChangeState(new SitState(this, stateMachine));
        }
    }

    // Called by animation events to signal the end of a state animation and trigger transition to Idle.
    public void OnStateAnimationComplete()
    {
        stateMachine.ChangeState(new IdleState(this, stateMachine));
    }

    // Called in the attack's animation as an animation event
    public void InstantiateHitbox()
    {
        Instantiate(hitbox, transform);
    }

    public void TakeHit(Vector3 attackerPosition, float knockbackDuration, float knockbackSpeed, int damage)
    {
        Vector3 knockbackDirection = (transform.position - attackerPosition).normalized;
        knockbackDirection.y = 0.0f;

        if (stateMachine.CurrentState is not DieGroundState && stateMachine.CurrentState is not DieAirState && stateMachine.CurrentState is not ResurrectState)
        {
            healthBar.DecreaseHealth(damage);

            if (healthBar.CurrentHealth > 0)
            {
                if (IsGrounded())
                    stateMachine.ChangeState(new HitGroundState(this, stateMachine, knockbackDirection, knockbackDuration, knockbackSpeed));
                else
                    stateMachine.ChangeState(new HitAirState(this, stateMachine, knockbackDirection, knockbackDuration, knockbackSpeed));
            }
            else
            {
                if (IsGrounded())
                    stateMachine.ChangeState(new DieGroundState(this, stateMachine, knockbackDirection, knockbackDuration, knockbackSpeed));
                else
                    stateMachine.ChangeState(new DieAirState(this, stateMachine, knockbackDirection, knockbackDuration, knockbackSpeed));
            }
        }
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
