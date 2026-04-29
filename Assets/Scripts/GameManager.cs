using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static void LoadScene(string sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public static void QuitGame() { 
        Application.Quit();
    }

    public static void clearSaveData()
    {
        PlayerPrefs.DeleteAll();
        PlayerInventory.dumpInventory();
    }

    public static void LoadProgress(string defaultScene)
    {
        // Load the target scene
        if (PlayerPrefs.HasKey("LastPlayerScene"))
        {
            LoadScene(PlayerPrefs.GetString("LastPlayerScene"));
        }

        else
        {
            LoadScene(defaultScene);
        }

    }

    public static void UnloadScene(string sceneToUnload)
    {
        SceneManager.UnloadSceneAsync(sceneToUnload);
    }

}
