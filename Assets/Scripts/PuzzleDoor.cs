using UnityEngine;
using UnityEngine.SceneManagement;

// Used exclusively for terrain-generated puzzle doors.
// Hand-placed dungeon entrances use the Door component instead.
public class PuzzleDoor : MonoBehaviour
{
    public string puzzleScene = "RandomPuzzleBuild";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"PuzzleDoor triggered on {gameObject.name}, loading: {puzzleScene}");
            SceneManager.LoadScene(puzzleScene);
        }
    }
}
