using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    bool isGameRunning = true; // in the event of state needing be checked;
    PlayerInput playerInput;
    [SerializeField] GameObject pauseScreen;

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        playerInput = player.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.Log("Couldn't find a player");
        }
        else 
        {
            playerInput.SwitchCurrentActionMap("Player");

            Time.timeScale = 1.0f;

            // should work, doesn't 
            //            playerInput.actions.FindActionMap("Player").FindAction("PauseGame").performed += PauseGame;
            //          playerInput.actions.FindActionMap("UI").FindAction("UnpauseGame").performed += ResumeGame;
        }

    }

    // stops game world from running
    void StopWorldSimulation()
    {
        isGameRunning = false;
        Time.timeScale = 0;
        playerInput.SwitchCurrentActionMap("UI");
    }

    // stops game world from running
    void StartWorldSimulation()
    {
        isGameRunning = true;
        Time.timeScale = 1;
        playerInput.SwitchCurrentActionMap("Player");
    }

    public void PauseGame(InputAction.CallbackContext context)
    {
        StopWorldSimulation();
        pauseScreen.SetActive(true);
    }
    public void PauseGame()
    {
        StopWorldSimulation();
        pauseScreen.SetActive(true);
    }

    public void ResumeGame(InputAction.CallbackContext context)
    {
        StartWorldSimulation();
        pauseScreen.SetActive(false);
    }

    public void ResumeGame()
    {
        StartWorldSimulation();
        pauseScreen.SetActive(false);
    }

    public void SaveGame()
    {
        Debug.Log("Logic for Save TDB");
        var saver = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerPersistence>();
        Debug.Log(saver != null);
        saver.SavePlayer();
    }

    public void QuitGameToMenu()
    {
        SaveGame();
        SceneManager.LoadScene("Title");
    }

}
