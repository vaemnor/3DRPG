using UnityEngine;

public enum EnemyPlayerDetectorType { Chase, Attack }

public class EnemyPlayerDetector : MonoBehaviour
{
    private EnemyController enemyController;

    [Tooltip("The type of the enemy player detector. If the player is inside the radius of the chase detector type, the enemy will start chasing the player. If the player inside the radius of the attack detector type, the enemy will start an attack.")]
    [SerializeField] private EnemyPlayerDetectorType detectorType = EnemyPlayerDetectorType.Chase;
    [Tooltip("The tag of the player game object.")]
    [SerializeField] private string playerTag = "Player";

    public bool IsPlayerInChaseRange { get; private set; } = false;
    public bool IsPlayerInAttackRange { get; private set; } = false;

    private void Awake()
    {
        enemyController = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            switch (detectorType)
            {
                case EnemyPlayerDetectorType.Chase:
                    IsPlayerInChaseRange = true;

                    if (!(enemyController.currentState == EnemyState.Chase || enemyController.currentState == EnemyState.CoolDown))
                        enemyController.ChangeState(EnemyState.Chase);
                    break;
                case EnemyPlayerDetectorType.Attack:
                    IsPlayerInAttackRange = true;

                    if (!(enemyController.currentState == EnemyState.Attack || enemyController.currentState == EnemyState.CoolDown))
                        enemyController.ChangeState(EnemyState.Attack);
                    break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            switch (detectorType)
            {
                case EnemyPlayerDetectorType.Chase:
                    IsPlayerInChaseRange = false;

                    if (enemyController.currentState == EnemyState.Chase)
                        enemyController.ChangeState(EnemyState.Idle);
                    break;
                case EnemyPlayerDetectorType.Attack:
                    IsPlayerInAttackRange = false;
                    break;
            }
        }
    }
}
