using System.Collections;
using UnityEngine;

public enum HitboxOriginator { Player, Enemy }

public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Settings")]
    [Tooltip("Type of actor the hit box belongs to.")]
    [SerializeField] private HitboxOriginator hitboxOriginator = HitboxOriginator.Player;
    [Tooltip("Duration in seconds after which the hitbox will be destroyed.")]
    [SerializeField] private float hitboxLifetime = 1.0f;
    [Tooltip("Amount of damage inflicted to the recipient.")]
    [SerializeField] private int damage = 0;
    [Tooltip("Duration of the knockback inflicted to the recipient.")]
    [SerializeField] private float knockbackDuration = 1.0f;
    [Tooltip("Speed of the knockback inflicted to the recipient.")]
    [SerializeField] private float knockbackSpeed = 1.0f;

    [Header("Tag Settings")]
    [Tooltip("Tag of the player game object.")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Tag of the enemy game object.")]
    [SerializeField] private string enemyTag = "Enemy";

    private void Awake()
    {
        StartCoroutine(DestroyHitboxAfterDelay());
    }

    private IEnumerator DestroyHitboxAfterDelay()
    {
        yield return new WaitForSeconds(hitboxLifetime);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (hitboxOriginator)
        {
            case HitboxOriginator.Player:
                if (other.CompareTag(enemyTag))
                {
                    other.TryGetComponent(out EnemyController enemy);
                    enemy.TakeHit(PlayerController.Instance.transform.position, knockbackDuration, knockbackSpeed, damage);
                }
                break;
            case HitboxOriginator.Enemy:
                if (other.CompareTag(playerTag))
                {
                    PlayerController.Instance.TakeHit(transform.position, knockbackDuration, knockbackSpeed, damage);
                }
                break;
            default:
                break;
        }
    }
}
