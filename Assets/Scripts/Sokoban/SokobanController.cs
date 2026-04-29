using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SokobanGenerator))]
public class SokobanController : MonoBehaviour
{
    private struct MoveState
    {
        public Vector2Int PlayerPos;
        public Vector2Int[] BoxPositions;
    }

    public string overworldSceneName = "Overworld";

    private SokobanGenerator gen;
    private PlayerMovement playerMovement;
    private Rigidbody2D playerRb;
    private readonly System.Collections.Generic.Stack<MoveState> undoStack = new System.Collections.Generic.Stack<MoveState>();

    private void Awake()
    {
        gen = GetComponent<SokobanGenerator>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            playerRb = player.GetComponent<Rigidbody2D>();
            if (playerMovement != null) playerMovement.enabled = false;
            if (playerRb != null) playerRb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void Update()
    {
        Vector2Int dir = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))    dir = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))  dir = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))  dir = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector2Int.right;

        if (Input.GetKeyDown(KeyCode.R)) { gen.Generate(); undoStack.Clear(); return; }
        if (Input.GetKeyDown(KeyCode.U)) { UndoMove(); return; }
        if (Input.GetKeyDown(KeyCode.Escape)) { SceneManager.LoadScene(overworldSceneName); return; }

        if (dir != Vector2Int.zero)
            TryMove(dir);
    }

    private void TryMove(Vector2Int dir)
    {
        Vector2Int newPos = gen.PlayerPos + dir;

        if (!InBounds(newPos) || gen.Grid[newPos.x, newPos.y] == SokobanGenerator.Cell.Wall)
            return;

        MoveState previousState = CaptureState();

        int boxIndex = GetBoxAt(newPos);
        if (boxIndex >= 0)
        {
            Vector2Int newBoxPos = newPos + dir;

            if (!InBounds(newBoxPos) || gen.Grid[newBoxPos.x, newBoxPos.y] == SokobanGenerator.Cell.Wall)
                return;
            if (GetBoxAt(newBoxPos) >= 0) return; // box blocked by another box

            gen.BoxPositions[boxIndex] = newBoxPos;
        }

        gen.PlayerPos = newPos;
        undoStack.Push(previousState);
        gen.Render();
        // Player GameObject is moved inside Render()

        if (CheckWin())
            SceneManager.LoadScene(overworldSceneName);
    }

    private int GetBoxAt(Vector2Int pos)
    {
        for (int i = 0; i < gen.BoxPositions.Length; i++)
            if (gen.BoxPositions[i] == pos) return i;
        return -1;
    }

    private bool CheckWin()
    {
        foreach (var box in gen.BoxPositions)
            if (gen.Grid[box.x, box.y] != SokobanGenerator.Cell.Target)
                return false;
        return true;
    }

    private bool InBounds(Vector2Int pos) =>
        pos.x >= 0 && pos.x < gen.Grid.GetLength(0) &&
        pos.y >= 0 && pos.y < gen.Grid.GetLength(1);

    private MoveState CaptureState()
    {
        return new MoveState
        {
            PlayerPos = gen.PlayerPos,
            BoxPositions = (Vector2Int[])gen.BoxPositions.Clone()
        };
    }

    private void UndoMove()
    {
        if (undoStack.Count == 0)
            return;

        MoveState previousState = undoStack.Pop();
        gen.PlayerPos = previousState.PlayerPos;

        for (int i = 0; i < gen.BoxPositions.Length; i++)
            gen.BoxPositions[i] = previousState.BoxPositions[i];

        gen.Render();
    }
}
