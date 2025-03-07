using UnityEngine;

public class EnemyPlayerDetector : MonoBehaviour
{
    private enum DetectorType
    {
        Chase,
        Attack
    }

    [SerializeField] private DetectorType detectorType;

    private EnemyController enemyController;

    [SerializeField] private string playerTag = "Player";

    private void Awake()
    {
        enemyController = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (detectorType == DetectorType.Chase)
            {
                enemyController.ChangeState(EnemyController.EnemyState.Chase);
            }
            else if (detectorType == DetectorType.Attack)
            {
                enemyController.ChangeState(EnemyController.EnemyState.Attack);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (detectorType == DetectorType.Chase)
            {
                enemyController.ChangeState(EnemyController.EnemyState.Idle);
            }
            else if (detectorType == DetectorType.Attack)
            {
                enemyController.ChangeState(EnemyController.EnemyState.Chase);
            }
        }
    }
}
