using UnityEngine;
using UnityEngine.SceneManagement;

// this prob could've been a base persistence, but it was complicated due to there being multiple distinct players across scenes
public class PlayerPersistence : MonoBehaviour
{
    [SerializeField] protected PlayerInventory inventory;
    [SerializeField] protected HealthTracker playerHealth;

    private string lastSceneKey = "LastPlayerScene";
    private string lastXLocationKey = "SpawnX";
    private string lastYLocationKey = "SpawnY";


    public void SavePlayer()
    {
        SaveLocation();
        SaveInventory();
        SaveHealth();
    }

    public void SaveLocation()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"location: {sceneName}");
        PlayerPrefs.SetString(lastSceneKey, sceneName);
        PlayerPrefs.SetFloat(lastXLocationKey, transform.position.x);
        PlayerPrefs.SetFloat(lastYLocationKey, transform.position.y);
    }

    public void SaveInventory()
    {
        inventory.HardSave();
    }

    public void SaveHealth()
    {
        PlayerPrefs.SetInt("PlayerHealth", playerHealth.currentHealth);
    }

    public void SaveUpgradeData()
    {
        // save upgrade stuff
    }

    [ContextMenu("Reset Player Save Data")]
    private void ResetPlayerData()
    {
        PlayerPrefs.DeleteKey(lastSceneKey);
        PlayerPrefs.DeleteKey(lastXLocationKey);
        PlayerPrefs.DeleteKey(lastYLocationKey);
    }
}
