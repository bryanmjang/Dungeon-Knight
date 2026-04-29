using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthTracker : MonoBehaviour
{

    [SerializeField] public int currentHealth = 10;
    [SerializeField] private HealthBar healthBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var isPersistant = GetComponent<PlayerPersistence>();
        if (isPersistant && PlayerPrefs.HasKey("PlayerHealth")) currentHealth = PlayerPrefs.GetInt("PlayerHealth");
        else currentHealth = Statsmanager.instance.maxHealth;

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(Statsmanager.instance.maxHealth);
            healthBar.SetHealth(currentHealth);
        }
    }

    // returns true if creature dies, false otherwise.
    public bool GiveDamage(int dmgReceived)
    {
        currentHealth -= dmgReceived;
        
        if (healthBar != null) healthBar.SetHealth(currentHealth);

        // if you run out of health, you die.
        if (currentHealth <= 0) {
            Die();
            return true;
        } else return false;
    }

    public void GiveHealth(int hpReceived)
    {
        currentHealth += hpReceived;

        // can't heal beyond max
        if (currentHealth > Statsmanager.instance.maxHealth)
        {
            currentHealth = Statsmanager.instance.maxHealth;
        }

        if (healthBar != null) healthBar.SetHealth(currentHealth);
    }

    // logic to resolve on death
    private void Die() {
        GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(ReturnToTitle());
    }

    private IEnumerator ReturnToTitle()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("Title");
    }

    public bool isMax()
    {
        return (currentHealth == Statsmanager.instance.maxHealth);
    }

}
