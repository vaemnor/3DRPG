using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Patrol, Chase, Attack, CoolDown, Hit, Die, Resurrect }

public class EnemyController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    private HealthBar healthBar;

    [Header("Movement Speed Settings")]
    [Tooltip("Minimum speed at which the agent will move.")]
    [SerializeField] private float agentMovementSpeedMin = 1.0f;
    [Tooltip("Maximum speed at which the agent will move.")]
    [SerializeField] private float agentMovementSpeedMax = 1.0f;

    [Header("Patrol Route Settings")]
    [Tooltip("Parent of the navigation objects.")]
    [SerializeField] private Transform patrolRoute;
    [Tooltip("Whether the agent patrols in a circle or picks a random destination each time.")]
    [SerializeField] private bool isRandomPatrol = false;

    [Header("Idle Duration Settings")]
    [Tooltip("Minimum duration in seconds at which the agent will idle.")]
    [SerializeField] private float idleDurationMin = 1.0f;
    [Tooltip("Maximum duration in seconds at which the agent will idle.")]
    [SerializeField] private float idleDurationMax = 1.0f;

    [Header("Patrol Duration Settings")]
    [Tooltip("Minimum duration in seconds at which the agent will patrol.")]
    [SerializeField] private float patrolDurationMin = 1.0f;
    [Tooltip("Maximum duration in seconds at which the agent will patrol.")]
    [SerializeField] private float patrolDurationMax = 1.0f;

    [Header("Hit State Settings")]
    [Tooltip("The hit animation.")]
    [SerializeField] private AnimationClip hitAnimation;
    [Tooltip("Speed of the hit animation set in the animator.")]
    [SerializeField] private float hitAnimationSpeed = 1.0f;

    [Header("Death Settings")]
    [Tooltip("Minimum duration in seconds at which the enemy will remain dead.")]
    [SerializeField] private float deathDurationMin = 1.0f;
    [Tooltip("Maximum duration in seconds at which the enemy will remain dead.")]
    [SerializeField] private float deathDurationMax = 1.0f;
    [Tooltip("The resurrect animation.")]
    [SerializeField] private AnimationClip resurrectAnimation;
    [Tooltip("Speed of the resurrect animation set in the animator.")]
    [SerializeField] private float resurrectAnimationSpeed = 1.0f;

    [Header("Attack Settings")]
    [Tooltip("The attack animation.")]
    [SerializeField] private AnimationClip attackAnimation;
    [Tooltip("Speed of the attack animation set in the animator.")]
    [SerializeField] private float attackAnimationSpeed = 1.0f;
    [Tooltip("The hitbox that is instantiated upon attacking.")]
    [SerializeField] private GameObject hitbox;
    [Tooltip("Speed at which the enemy will rotate toward the player during the attack.")]
    [SerializeField] private float rotationSpeed = 1.0f;
    [Tooltip("Minimum duration in seconds at which the agent will wait after an attack to resume patrol.")]
    [SerializeField] private float cooldownMin = 1.0f;
    [Tooltip("Maximum duration in seconds at which the agent will wait after an attack to resume patrol.")]
    [SerializeField] private float cooldownMax = 1.0f;

    [Header("Detector Settings")]
    [Tooltip("Detector that will trigger chasing when the player is within the radius.")]
    [SerializeField] private EnemyPlayerDetector chaseDetector;
    [Tooltip("Detector that will trigger an attack when the player is within the radius.")]
    [SerializeField] private EnemyPlayerDetector attackDetector;

    public EnemyState CurrentState { get; private set; }
    private Coroutine currentStateCoroutine;

    private Vector3[] destinations;
    private int destinationIndex = 0;

    private Vector3 knockbackDirection = Vector3.zero;
    private float knockbackDuration = 0.0f;
    private float knockbackSpeed = 0.0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        healthBar = GetComponentInChildren<HealthBar>();

        if (patrolRoute != null)
        {
            InitializePatrolRoute();
        }

        // Set random movement speed
        agent.speed = Random.Range(agentMovementSpeedMin, agentMovementSpeedMax);
    }

    private void Start()
    {
        ChangeState(EnemyState.Patrol);
    }

    public void ChangeState(EnemyState newState)
    {
        if (currentStateCoroutine != null)
            StopCoroutine(currentStateCoroutine);

        CurrentState = newState;

        switch (CurrentState)
        {
            case EnemyState.Idle:
                currentStateCoroutine = StartCoroutine(Idle());
                break;
            case EnemyState.Patrol:
                currentStateCoroutine = StartCoroutine(StartPatrolAndSetPatrolDuration());
                break;
            case EnemyState.Chase:
                currentStateCoroutine = StartCoroutine(ChasePlayer());
                break;
            case EnemyState.Attack:
                currentStateCoroutine = StartCoroutine(AttackPlayer());
                break;
            case EnemyState.CoolDown:
                currentStateCoroutine = StartCoroutine(CoolDown());
                break;
            case EnemyState.Hit:
                currentStateCoroutine = StartCoroutine(Hit());
                break;
            case EnemyState.Die:
                currentStateCoroutine = StartCoroutine(Die());
                break;
            case EnemyState.Resurrect:
                currentStateCoroutine = StartCoroutine(Resurrect());
                break;
        }

        HandleAnimations();
    }

    private void InitializePatrolRoute()
    {
        destinations = new Vector3[patrolRoute.childCount];

        for (int i = 0; i < destinations.Length; i++)
        {
            destinations[i] = patrolRoute.GetChild(i).position;
        }

        // Set random first destination
        destinationIndex = Random.Range(0, destinations.Length);
    }

    private IEnumerator Idle()
    {
        agent.isStopped = true;

        float idleDuration = Random.Range(idleDurationMin, idleDurationMax);
        yield return new WaitForSeconds(idleDuration);

        ChangeState(EnemyState.Patrol);
    }


    private IEnumerator StartPatrolAndSetPatrolDuration()
    {
        agent.isStopped = false;

        float patrolDuration = Random.Range(patrolDurationMin, patrolDurationMax);

        StartCoroutine(LoopThroughPatrolRoute());

        yield return new WaitForSeconds(patrolDuration);

        // After patrol duration is over, switch to Idle
        ChangeState(EnemyState.Idle);
    }

    private IEnumerator LoopThroughPatrolRoute()
    {
        while (CurrentState == EnemyState.Patrol)
        {
            agent.SetDestination(destinations[destinationIndex]);

            // Wait until the agent has finished reaching the current destination
            while (CurrentState == EnemyState.Patrol && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
            {
                yield return null;
            }

            // Move to the next destination
            if (!isRandomPatrol)
            {
                destinationIndex = (destinationIndex + 1) % destinations.Length;
            }
            else if (isRandomPatrol)
            {
                destinationIndex = Random.Range(0, destinations.Length);
            }
        }
    }

    private IEnumerator ChasePlayer()
    {
        agent.isStopped = false;

        while (CurrentState == EnemyState.Chase)
        {
            agent.SetDestination(PlayerController.Instance.transform.position);
            yield return null;
        }
    }

    private IEnumerator AttackPlayer()
    {
        agent.isStopped = true;

        float elapsedTime = 0.0f;
        while (CurrentState == EnemyState.Attack && elapsedTime < (attackAnimation.length / attackAnimationSpeed))
        {
            RotateToward(PlayerController.Instance.transform.position - transform.position);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ChangeState(EnemyState.CoolDown);
    }

    private IEnumerator CoolDown()
    {
        agent.isStopped = true;

        float cooldown = Random.Range(cooldownMin, cooldownMax);
        yield return new WaitForSeconds(cooldown);

        if (attackDetector.IsPlayerInAttackRange)
            ChangeState(EnemyState.Attack);
        else if (chaseDetector.IsPlayerInChaseRange)
            ChangeState(EnemyState.Chase);
        else
            ChangeState(EnemyState.Patrol);
    }

    private IEnumerator Hit()
    {
        agent.enabled = false; // disable agent to allow knockback

        float elapsedTime = 0.0f;
        while (CurrentState == EnemyState.Hit && elapsedTime < knockbackDuration)
        {
            elapsedTime += Time.deltaTime;

            transform.position += (knockbackDirection * knockbackSpeed) * Time.deltaTime;
            RotateToward(-knockbackDirection);

            yield return null;
        }

        agent.enabled = true; // re-enable agent after knockback

        yield return new WaitForSeconds((hitAnimation.length / hitAnimationSpeed) - elapsedTime);

        if (attackDetector.IsPlayerInAttackRange)
            ChangeState(EnemyState.Attack);
        else if (chaseDetector.IsPlayerInChaseRange)
            ChangeState(EnemyState.Chase);
        else
            ChangeState(EnemyState.Patrol);
    }


    private IEnumerator Die()
    {
        agent.enabled = false; // disable agent to allow knockback

        float elapsedTime = 0.0f;
        while (CurrentState == EnemyState.Die && elapsedTime < knockbackDuration)
        {
            elapsedTime += Time.deltaTime;

            transform.position += (knockbackDirection * knockbackSpeed) * Time.deltaTime;
            RotateToward(-knockbackDirection);

            yield return null;
        }

        agent.enabled = true; // re-enable agent after knockback

        float deathDuration = Random.Range(deathDurationMin, deathDurationMax);
        yield return new WaitForSeconds(deathDuration - elapsedTime);

        ChangeState(EnemyState.Resurrect);
    }

    private IEnumerator Resurrect()
    {
        agent.isStopped = true;

        yield return new WaitForSeconds(resurrectAnimation.length / resurrectAnimationSpeed);

        healthBar.InitializeHealth();

        if (attackDetector.IsPlayerInAttackRange)
            ChangeState(EnemyState.Attack);
        else if (chaseDetector.IsPlayerInChaseRange)
            ChangeState(EnemyState.Chase);
        else
            ChangeState(EnemyState.Patrol);
    }

    // Called in the attack's animation as an animation event
    public void InstantiateHitbox()
    {
        Instantiate(hitbox, transform);
    }

    public void TakeHit(Vector3 attackerPosition, float _knockbackDuration, float _knockbackSpeed, int damage)
    {
        Vector3 _knockbackDirection = (transform.position - attackerPosition).normalized;
        _knockbackDirection.y = 0.0f;

        knockbackDirection = _knockbackDirection;
        knockbackDuration = _knockbackDuration;
        knockbackSpeed = _knockbackSpeed;

        if (CurrentState != EnemyState.Die && CurrentState != EnemyState.Resurrect)
        {
            healthBar.DecreaseHealth(damage);

            if (healthBar.CurrentHealth > 0)
                ChangeState(EnemyState.Hit);
            else
                ChangeState(EnemyState.Die);
        }
    }

    public void RotateToward(Vector3 direction)
    {
        direction.y = 0.0f;
        direction.Normalize();

        if (direction.sqrMagnitude > 0.0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleAnimations()
    {
        animator.SetBool("isIdling", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isChasing", false);
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Hit");
        animator.ResetTrigger("Die");
        animator.ResetTrigger("Resurrect");

        switch (CurrentState)
        {
            case EnemyState.Idle:
                animator.SetBool("isIdling", true);
                break;
            case EnemyState.Patrol:
                animator.SetBool("isWalking", true);
                break;
            case EnemyState.Chase:
                animator.SetBool("isChasing", true);
                break;
            case EnemyState.Attack:
                animator.SetTrigger("Attack");
                break;
            case EnemyState.CoolDown:
                animator.SetBool("isIdling", true);
                break;
            case EnemyState.Hit:
                animator.SetTrigger("Hit");
                break;
            case EnemyState.Die:
                animator.SetTrigger("Die");
                break;
            case EnemyState.Resurrect:
                animator.SetTrigger("Resurrect");
                break;
        }
    }
}
