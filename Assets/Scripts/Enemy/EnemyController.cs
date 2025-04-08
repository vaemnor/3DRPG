using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Patrol, Chase, Attack, CoolDown }

public class EnemyController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    [Header("Movement Speed")]
    [Tooltip("The minimum speed at which the agent will move.")]
    [SerializeField] private float agentMovementSpeedMin = 1.0f;
    [Tooltip("The maximum speed at which the agent will move.")]
    [SerializeField] private float agentMovementSpeedMax = 1.0f;

    [Header("Patrol Route")]
    [Tooltip("Parent of the navigation objects.")]
    [SerializeField] private Transform patrolRoute;
    [Tooltip("Whether the agent patrols in a circle or picks a random destination each time.")]
    [SerializeField] private bool isRandomPatrol = false;

    [Header("Idle Duration")]
    [Tooltip("The minimum duration in seconds at which the agent will idle.")]
    [SerializeField] private float idleDurationMin = 1.0f;
    [Tooltip("The maximum duration in seconds at which the agent will idle.")]
    [SerializeField] private float idleDurationMax = 1.0f;

    [Header("Patrol Duration")]
    [Tooltip("The minimum duration in seconds at which the agent will patrol.")]
    [SerializeField] private float patrolDurationMin = 1.0f;
    [Tooltip("The maximum duration in seconds at which the agent will patrol.")]
    [SerializeField] private float patrolDurationMax = 1.0f;

    [Header("Attack Settings")]
    [Tooltip("The attack's animation.")]
    [SerializeField] private AnimationClip attackAnimation;
    [Tooltip("The hitbox that is instantiated upon attacking.")]
    [SerializeField] private GameObject hitbox;
    [Tooltip("The speed at which the enemy will rotate toward the player during the attack.")]
    [SerializeField] private float rotationSpeed = 1.0f;
    [Tooltip("The minimum duration in seconds at which the agent will wait after an attack to resume patrol.")]
    [SerializeField] private float cooldownMin = 1.0f;
    [Tooltip("The maximum duration in seconds at which the agent will wait after an attack to resume patrol.")]
    [SerializeField] private float cooldownMax = 1.0f;

    [Header("Miscellaneous")]
    [Tooltip("The detector that will trigger chasing when the player is within the radius.")]
    [SerializeField] private EnemyPlayerDetector chaseDetector;
    [Tooltip("The detector that will trigger an attack when the player is within the radius.")]
    [SerializeField] private EnemyPlayerDetector attackDetector;

    [HideInInspector] public EnemyState currentState;
    private Coroutine currentStateCoroutine;

    private Vector3[] destinations;
    private int destinationIndex = 0;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

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
        if (currentState == newState) return;

        if (currentStateCoroutine != null)
            StopCoroutine(currentStateCoroutine);

        currentState = newState;

        switch (currentState)
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
                currentStateCoroutine = StartCoroutine(Cooldown());
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
        while (currentState == EnemyState.Patrol)
        {
            agent.SetDestination(destinations[destinationIndex]);

            // Wait until the agent has finished reaching the current destination
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
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

        while (currentState == EnemyState.Chase)
        {
            agent.SetDestination(PlayerController.Instance.transform.position);
            yield return null;
        }
    }

    private IEnumerator AttackPlayer()
    {
        agent.isStopped = true;

        float elapsedTime = 0.0f;
        while (elapsedTime < attackAnimation.length)
        {
            RotateTowardPlayer();
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ChangeState(EnemyState.CoolDown);
    }

    public void RotateTowardPlayer()
    {
        Vector3 direction = PlayerController.Instance.transform.position - transform.position;
        direction.y = 0.0f;
        direction.Normalize();

        if (direction.sqrMagnitude > 0.0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private IEnumerator Cooldown()
    {
        agent.isStopped = true;

        float cooldown = Random.Range(cooldownMin, cooldownMax);
        yield return new WaitForSeconds(cooldown);

        if (currentState != EnemyState.Attack && attackDetector.IsPlayerInAttackRange)
            ChangeState(EnemyState.Attack);
        else if (currentState != EnemyState.Chase && chaseDetector.IsPlayerInChaseRange)
            ChangeState(EnemyState.Chase);
        else
            ChangeState(EnemyState.Patrol);
    }

    // Called in the attack's animation as an animation event
    public void InstantiateHitbox()
    {
        Instantiate(hitbox, transform);
    }

    private void HandleAnimations()
    {
        animator.SetBool("isIdling", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isChasing", false);

        switch (currentState)
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
        }
    }
}
