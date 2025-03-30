using UnityEngine;

public enum EnemyPlayerDetectorType { Chase, Attack }

public class EnemyPlayerDetector : MonoBehaviour
{
    private EnemyController enemyController;

    [SerializeField] private EnemyPlayerDetectorType detectorType = EnemyPlayerDetectorType.Chase;
    [SerializeField] private string playerTag = "Player";

    public bool IsPlayerInsideChaseRadius { get; private set; } = false;
    public bool IsPlayerInsideAttackRadius { get; private set; } = false;

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
                    IsPlayerInsideChaseRadius = true;
                    enemyController.ChangeState(EnemyState.Chase);
                    break;
                case EnemyPlayerDetectorType.Attack:
                    IsPlayerInsideAttackRadius = true;
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
                    IsPlayerInsideChaseRadius = false;
                    enemyController.ChangeState(EnemyState.Idle);
                    break;
                case EnemyPlayerDetectorType.Attack:
                    IsPlayerInsideAttackRadius = false;
                    break;
            }
        }
    }
}
