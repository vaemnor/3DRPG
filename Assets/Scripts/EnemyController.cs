using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    [Header("Movement Speed")]
    [Tooltip("The minimum speed at which the agent will move.")]
    [SerializeField] private float agentMovementSpeedMin = 1f;
    [Tooltip("The maximum speed at which the agent will move.")]
    [SerializeField] private float agentMovementSpeedMax = 1f;

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

    private enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

    private State currentState;

    private State CurrentState
    {
        get { return currentState; }

        set
        {
            currentState = value;

            switch (currentState)
            {
                case State.Idle:
                    agent.isStopped = true;
                    StartCoroutine(Idle());
                    animator.SetTrigger("Idle");
                    break;
                case State.Patrol:
                    agent.isStopped = false;
                    StartCoroutine(StartPatrolAndSetPatrolDuration());
                    animator.SetTrigger("Walk");
                    break;
                default:
                    break;
            }
        }
    }

    private Vector3[] destinations;
    private int destinationIndex = 0;

    private Coroutine patrolCoroutine;

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
        CurrentState = State.Patrol;
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

        CurrentState = State.Patrol;
    }

    private IEnumerator StartPatrolAndSetPatrolDuration()
    {
        int patrolDuration = Random.Range(patrolDurationMin, patrolDurationMax + 1);

        patrolCoroutine = StartCoroutine(LoopThroughPatrolRoute());

        yield return new WaitForSeconds(patrolDuration);

        StopCoroutine(patrolCoroutine);
        patrolCoroutine = null;

        // After patrol duration is over, switch to Idle
        CurrentState = State.Idle;
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
}
