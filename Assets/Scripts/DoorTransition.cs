using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public string sceneToLoad;
    public Vector3 spawnPosition;

    public bool isDungeon1Exit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Door triggered on {gameObject.name}, loading: {sceneToLoad}");
            if (string.IsNullOrEmpty(sceneToLoad)) return;

            // Dungeon exits are always allowed. All other doors are blocked during a wave.
            if (!isDungeon1Exit && Timer.IsWaveActive)
            {
                Debug.Log($"[Door] Scene change blocked on \"{gameObject.name}\" — wave is active!");
                return;
            }

            if (isDungeon1Exit)
            {
                PlayerPrefs.SetInt("Dungeon1Complete", 1);
            }

            PlayerPrefs.SetFloat("SpawnX", spawnPosition.x);
            PlayerPrefs.SetFloat("SpawnY", spawnPosition.y);
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}