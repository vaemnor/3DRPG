using System.Collections;
using UnityEngine;

public enum HitboxOriginator { Player, Enemy }

public class Hitbox : MonoBehaviour
{
    [Tooltip("The type of actor the hit box belongs to.")]
    [SerializeField] private HitboxOriginator hitboxOriginator = HitboxOriginator.Player;
    [Tooltip("The duration in seconds after which the hitbox will be destroyed.")]
    [SerializeField] private float hitboxLifetime = 1.0f;
    [Tooltip("The amount of damage inflicted to the recipient.")]
    [SerializeField] private int damage = 0;

    [Tooltip("The tag of the player game object.")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("The tag of the enemy game object.")]
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
                    // hurt the enemy
                }
                break;
            case HitboxOriginator.Enemy:
                if (other.CompareTag(playerTag))
                {
                    // hurt the player
                }
                break;
            default:
                break;
        }
    }
}
