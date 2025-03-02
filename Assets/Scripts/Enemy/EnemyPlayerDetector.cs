using UnityEngine;

public class EnemyPlayerDetector : MonoBehaviour
{
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
            enemyController.ChangeState(EnemyController.EnemyState.Chase);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            enemyController.ChangeState(EnemyController.EnemyState.Idle);
        }
    }
}
