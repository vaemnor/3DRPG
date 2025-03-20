using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Patrol, Chase, Attack }

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
    [SerializeField] private int idleDurationMin = 1;
    [Tooltip("The maximum duration in seconds at which the agent will idle.")]
    [SerializeField] private int idleDurationMax = 1;

    [Header("Patrol Duration")]
    [Tooltip("The minimum duration in seconds at which the agent will patrol.")]
    [SerializeField] private int patrolDurationMin = 1;
    [Tooltip("The maximum duration in seconds at which the agent will patrol.")]
    [SerializeField] private int patrolDurationMax = 1;

    [Header("Attack Settings")]
    [Tooltip("The duration in seconds between consecutive attacks.")]
    [SerializeField] private float attackCooldown = 1.0f;
    [Tooltip("The attack's animation.")]
    [SerializeField] private AnimationClip attackAnimation;
    [Tooltip("The detector that will trigger attacks when the player is within the radius.")]
    [SerializeField] private EnemyPlayerDetector attackDetector;

    private EnemyState currentState;

    public EnemyState CurrentState
    {
        get { return currentState; }

        set
        {
            animator.SetBool("isIdling", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isChasing", false);

            currentState = value;

            switch (currentState)
            {
                case EnemyState.Idle:
                    agent.isStopped = true;
                    StartCoroutine(Idle());
                    animator.SetBool("isIdling", true);
                    break;
                case EnemyState.Patrol:
                    agent.isStopped = false;
                    StartCoroutine(StartPatrolAndSetPatrolDuration());
                    animator.SetBool("isWalking", true);
                    break;
                case EnemyState.Chase:
                    agent.isStopped = false;
                    StartCoroutine(ChasePlayer());
                    animator.SetBool("isChasing", true);
                    break;
                case EnemyState.Attack:
                    agent.isStopped = true;
                    StartCoroutine(AttackPlayer());
                    animator.SetTrigger("Attack");
                    break;
                default:
                    break;
            }
        }
    }

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

    public void ChangeState(EnemyState enemyState)
    {
        StopAllCoroutines();

        CurrentState = enemyState;
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
        int idleDuration = Random.Range(idleDurationMin, idleDurationMax + 1);

        yield return new WaitForSeconds(idleDuration);

        ChangeState(EnemyState.Patrol);
    }

    private IEnumerator StartPatrolAndSetPatrolDuration()
    {
        int patrolDuration = Random.Range(patrolDurationMin, patrolDurationMax + 1);

        StartCoroutine(LoopThroughPatrolRoute());

        yield return new WaitForSeconds(patrolDuration);

        // After patrol duration is over, switch to Idle
        ChangeState(EnemyState.Idle);
    }

    private IEnumerator LoopThroughPatrolRoute()
    {
        while (true)
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
        while (true)
        {
            agent.SetDestination(PlayerController.Instance.transform.position);
            yield return null;
        }
    }

    private IEnumerator AttackPlayer()
    {
        yield return new WaitForSeconds(attackAnimation.length);
        yield return new WaitForSeconds(attackCooldown);

        // TODO: Play some kinda animation while waiting

        if (!attackDetector.IsPlayerInsideAttackRadius)
            ChangeState(EnemyState.Chase);
        else if (attackDetector.IsPlayerInsideAttackRadius)
            ChangeState(EnemyState.Attack);
    }
}
