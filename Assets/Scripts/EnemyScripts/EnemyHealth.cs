using UnityEngine;

public class EnemyHealth : MonoBehaviour

{
    [SerializeField] private HealthBar healthBar;
    public int currentHealth;
    public int maxHealth;
    [SerializeField] private AudioClip deathSound;

    private void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

    }

        public void ChangeHealth(int amount)
        {
            currentHealth += amount;

            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
            else if (currentHealth <= 0)
            {
                currentHealth = 0;
            }

            if (healthBar != null)
            {
                healthBar.SetHealth(currentHealth);
            }

            if (currentHealth <= 0)
            {
                if (deathSound) 
                    AudioSource.PlayClipAtPoint(deathSound, transform.position);

                // Save enemy death; health needn't be tracked
                var isPersistantEnemy = GetComponent<EnemyPersistence>();
                if (isPersistantEnemy) isPersistantEnemy.MarkAsDead();

                Destroy(gameObject);
            }
        }


}
