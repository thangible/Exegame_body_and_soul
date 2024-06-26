using UnityEngine;

public class DestroyableObject : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 1;
    private int currentHealth;

    [Header("Effects")]
    public GameObject hitEffect;
    public GameObject destructionEffect;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        // play hit effect if it exists
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            DestroyObject();
        }
    }

    private void DestroyObject()
    {
        // play destruction effect if it exists
        if (destructionEffect != null)
        {
            Instantiate(destructionEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
