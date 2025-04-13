using System.Collections;
using System.Collections.Generic;
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

    // Tracks what this hitbox has already hit
    private HashSet<GameObject> alreadyHitObjects = new();

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
        if (alreadyHitObjects.Contains(other.gameObject))
            return; // Skip if already hit

        switch (hitboxOriginator)
        {
            case HitboxOriginator.Player:
                if (other.CompareTag(enemyTag))
                {
                    if (other.TryGetComponent(out EnemyController enemy))
                    {
                        alreadyHitObjects.Add(other.gameObject);
                        enemy.TakeHit(PlayerController.Instance.transform.position, knockbackDuration, knockbackSpeed, damage);
                    }
                }
                break;

            case HitboxOriginator.Enemy:
                if (other.CompareTag(playerTag))
                {
                    alreadyHitObjects.Add(other.gameObject);
                    PlayerController.Instance.TakeHit(transform.position, knockbackDuration, knockbackSpeed, damage);
                }
                break;
        }
    }
}
