using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 2f;
    public int damage = 1;

    void Start()
    {
        // Destroy the projectile after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the projectile hit an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Apply damage logic here if the enemy has a health component
            // collision.gameObject.GetComponent<EnemyHealth>().TakeDamage(damage);
        }

        // Destroy the projectile on impact
        Destroy(gameObject);
    }
}
