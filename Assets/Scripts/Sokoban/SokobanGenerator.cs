using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SokobanGenerator : MonoBehaviour
{
    private sealed class SolverState
    {
        public Vector2Int Player;
        public Vector2Int[] Boxes;
    }

    [Header("Grid")]
    public int width = 10;
    public int height = 10;
    public int boxCount = 1;
    public int reverseMoves = 60;
    public int interiorWallCount = 25;
    public int seed = 0;
    public bool randomSeed = true;
    public int maxGenerationAttempts = 50;
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public Tilemap objectTilemap;
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase boxTile;
    public TileBase targetTile;
    public TileBase boxOnTargetTile;
    public GameObject playerObject; 

    public enum Cell { Wall, Floor, Target }

    private Cell[,] grid;
    private Vector2Int[] boxPositions;
    private Vector2Int[] targetPositions;
    private Vector2Int playerPos;
    private System.Random rnd;

    public Cell[,] Grid => grid;
    public Vector2Int[] BoxPositions => boxPositions;
    public Vector2Int[] TargetPositions => targetPositions;
    public Vector2Int PlayerPos { get => playerPos; set => playerPos = value; }

    private static readonly Vector2Int[] Directions = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private void Start() => Generate();

    public void Generate()
    {
        int baseSeed = randomSeed ? UnityEngine.Random.Range(0, 100000) : seed;
        bool generated = false;

        for (int attempt = 0; attempt < Mathf.Max(1, maxGenerationAttempts); attempt++)
        {
            seed = baseSeed + attempt;
            rnd = new System.Random(seed);

            if (!TryGenerateLayout())
            {
                continue;
            }

            generated = true;
            break;
        }

        if (!generated)
        {
            Debug.LogWarning("SokobanGenerator could not build a solvable puzzle. Falling back to the latest attempt.", this);
        }

        Render();
    }

    private bool TryGenerateLayout()
    {
        BuildRoom();
        if (!PlaceTargetsAndBoxes())
        {
            return false;
        }

        RunBackwardsGeneration();

        if (IsAlreadySolved())
        {
            return false;
        }

        return IsPuzzleSolvable();
    }

    private void BuildRoom()
    {
        grid = new Cell[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    ? Cell.Wall : Cell.Floor;

        int wallCount = Mathf.Max(0, interiorWallCount);
        for (int i = 0; i < wallCount; i++)
        {
            int wx = rnd.Next(1, width - 1);
            int wy = rnd.Next(1, height - 1);
            grid[wx, wy] = Cell.Wall;
        }

        EnsureConnectivity();
    }

    private void EnsureConnectivity()
    {
        Vector2Int start = new Vector2Int(-1, -1);

        for (int x = 1; x < width - 1 && start.x < 0; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (grid[x, y] != Cell.Wall)
                {
                    start = new Vector2Int(x, y);
                    break;
                }
            }
        }

        if (start.x < 0)
        {
            return;
        }

        var reachable = FloodFill(start, null);

        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (grid[x, y] == Cell.Floor && !reachable.Contains(new Vector2Int(x, y)))
                    grid[x, y] = Cell.Wall;
    }

    private bool PlaceTargetsAndBoxes()
    {
        var floors = GetFloorTiles();
        if (floors.Count < boxCount + 1)
        {
            return false;
        }

        Shuffle(floors);

        targetPositions = new Vector2Int[boxCount];
        boxPositions = new Vector2Int[boxCount];

        for (int i = 0; i < boxCount; i++)
        {
            targetPositions[i] = floors[i];
            grid[floors[i].x, floors[i].y] = Cell.Target;
        }

        for (int i = 0; i < boxCount; i++)
            boxPositions[i] = targetPositions[i];

        for (int i = boxCount; i < floors.Count; i++)
        {
            playerPos = floors[i];
            return true;
        }

        return false;
    }

    private void RunBackwardsGeneration()
    {
        int attempts = reverseMoves * 100;
        int moves = 0;

        while (moves < reverseMoves && attempts-- > 0)
        {
            int bi = rnd.Next(0, boxCount);
            Vector2Int box = boxPositions[bi];
            Vector2Int dir = Directions[rnd.Next(0, 4)];

            Vector2Int newBoxPos = box - dir;
            Vector2Int playerMustReach = box + dir;

            if (!InBounds(newBoxPos) || !IsWalkable(newBoxPos, bi)) continue;
            if (!InBounds(playerMustReach) || !IsWalkable(playerMustReach, bi)) continue;
            if (IsDeadCell(newBoxPos)) continue;

            var reachable = FloodFill(playerPos, boxPositions);
            if (!reachable.Contains(playerMustReach)) continue;

            boxPositions[bi] = newBoxPos;
            playerPos = box;
            moves++;
        }
    }

    private bool IsDeadCell(Vector2Int pos)
    {
        if (grid[pos.x, pos.y] == Cell.Target) return false;

        bool wallLeft  = grid[pos.x - 1, pos.y] == Cell.Wall;
        bool wallRight = grid[pos.x + 1, pos.y] == Cell.Wall;
        bool wallUp    = grid[pos.x, pos.y + 1] == Cell.Wall;
        bool wallDown  = grid[pos.x, pos.y - 1] == Cell.Wall;

        if ((wallLeft || wallRight) && (wallUp || wallDown)) return true;

        if (wallUp || wallDown)
        {
            bool targetInRow = false;
            for (int x = pos.x; x >= 0 && grid[x, pos.y] != Cell.Wall; x--)
                if (grid[x, pos.y] == Cell.Target) { targetInRow = true; break; }
            if (!targetInRow)
                for (int x = pos.x; x < width && grid[x, pos.y] != Cell.Wall; x++)
                    if (grid[x, pos.y] == Cell.Target) { targetInRow = true; break; }
            if (!targetInRow) return true;
        }

        if (wallLeft || wallRight)
        {
            bool targetInCol = false;
            for (int y = pos.y; y >= 0 && grid[pos.x, y] != Cell.Wall; y--)
                if (grid[pos.x, y] == Cell.Target) { targetInCol = true; break; }
            if (!targetInCol)
                for (int y = pos.y; y < height && grid[pos.x, y] != Cell.Wall; y++)
                    if (grid[pos.x, y] == Cell.Target) { targetInCol = true; break; }
            if (!targetInCol) return true;
        }

        return false;
    }

    private HashSet<Vector2Int> FloodFill(Vector2Int start, Vector2Int[] boxes)
    {
        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();

        if (!IsWalkableForPlayer(start, boxes)) return visited;

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var curr = queue.Dequeue();
            foreach (var dir in Directions)
            {
                var next = curr + dir;
                if (!visited.Contains(next) && InBounds(next) && IsWalkableForPlayer(next, boxes))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        return visited;
    }

    private bool IsWalkableForPlayer(Vector2Int pos, Vector2Int[] boxes)
    {
        if (grid[pos.x, pos.y] == Cell.Wall) return false;
        if (boxes != null)
            foreach (var b in boxes)
                if (b == pos) return false;
        return true;
    }

    private bool IsWalkable(Vector2Int pos, int excludeBox)
    {
        if (grid[pos.x, pos.y] == Cell.Wall) return false;
        for (int i = 0; i < boxCount; i++)
            if (i != excludeBox && boxPositions[i] == pos) return false;
        return true;
    }

    public void Render()
    {
        floorTilemap?.ClearAllTiles();
        wallTilemap?.ClearAllTiles();
        objectTilemap?.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                if (grid[x, y] == Cell.Wall)
                    wallTilemap.SetTile(pos, wallTile);
                else
                {
                    floorTilemap.SetTile(pos, floorTile);
                    if (grid[x, y] == Cell.Target)
                        objectTilemap.SetTile(pos, targetTile);
                }
            }
        }

        foreach (var box in boxPositions)
        {
            var pos = new Vector3Int(box.x, box.y, 0);
            bool onTarget = grid[box.x, box.y] == Cell.Target;
            objectTilemap.SetTile(pos, onTarget ? boxOnTargetTile : boxTile);
        }

        if (playerObject == null)
            playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            playerObject.transform.position = new Vector3(playerPos.x + 0.5f, playerPos.y + 0.5f, 0);
    }

    private bool InBounds(Vector2Int pos) =>
        pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;

    private List<Vector2Int> GetFloorTiles()
    {
        var list = new List<Vector2Int>();
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (grid[x, y] == Cell.Floor)
                    list.Add(new Vector2Int(x, y));
        return list;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private bool IsAlreadySolved()
    {
        foreach (var box in boxPositions)
        {
            if (grid[box.x, box.y] != Cell.Target)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsPuzzleSolvable()
    {
        var initialBoxes = CopyAndSortBoxes(boxPositions);
        var initialState = new SolverState
        {
            Player = playerPos,
            Boxes = initialBoxes
        };

        var queue = new Queue<SolverState>();
        var visited = new HashSet<string>();
        string initialKey = BuildStateKey(initialState.Player, initialState.Boxes);
        visited.Add(initialKey);
        queue.Enqueue(initialState);

        while (queue.Count > 0)
        {
            SolverState current = queue.Dequeue();
            if (AreAllBoxesOnTargets(current.Boxes))
            {
                return true;
            }

            var reachable = FloodFill(current.Player, current.Boxes);

            for (int i = 0; i < current.Boxes.Length; i++)
            {
                Vector2Int box = current.Boxes[i];

                foreach (var dir in Directions)
                {
                    Vector2Int playerPushPosition = box - dir;
                    Vector2Int pushedBoxPosition = box + dir;

                    if (!reachable.Contains(playerPushPosition))
                    {
                        continue;
                    }

                    if (!InBounds(pushedBoxPosition) || grid[pushedBoxPosition.x, pushedBoxPosition.y] == Cell.Wall)
                    {
                        continue;
                    }

                    if (ContainsBox(current.Boxes, pushedBoxPosition, i))
                    {
                        continue;
                    }

                    Vector2Int[] nextBoxes = (Vector2Int[])current.Boxes.Clone();
                    nextBoxes[i] = pushedBoxPosition;
                    nextBoxes = CopyAndSortBoxes(nextBoxes);

                    string key = BuildStateKey(box, nextBoxes);
                    if (visited.Add(key))
                    {
                        queue.Enqueue(new SolverState
                        {
                            Player = box,
                            Boxes = nextBoxes
                        });
                    }
                }
            }
        }

        return false;
    }

    private bool AreAllBoxesOnTargets(Vector2Int[] boxes)
    {
        foreach (var box in boxes)
        {
            if (grid[box.x, box.y] != Cell.Target)
            {
                return false;
            }
        }

        return true;
    }

    private bool ContainsBox(Vector2Int[] boxes, Vector2Int pos, int ignoreIndex = -1)
    {
        for (int i = 0; i < boxes.Length; i++)
        {
            if (i != ignoreIndex && boxes[i] == pos)
            {
                return true;
            }
        }

        return false;
    }

    private Vector2Int[] CopyAndSortBoxes(Vector2Int[] boxes)
    {
        Vector2Int[] copy = (Vector2Int[])boxes.Clone();
        System.Array.Sort(copy, (a, b) =>
        {
            int xCompare = a.x.CompareTo(b.x);
            return xCompare != 0 ? xCompare : a.y.CompareTo(b.y);
        });
        return copy;
    }

    private string BuildStateKey(Vector2Int player, Vector2Int[] boxes)
    {
        var reachable = FloodFill(player, boxes);
        Vector2Int canonicalPlayer = player;

        foreach (Vector2Int pos in reachable)
        {
            if (pos.x < canonicalPlayer.x || (pos.x == canonicalPlayer.x && pos.y < canonicalPlayer.y))
            {
                canonicalPlayer = pos;
            }
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.Append(canonicalPlayer.x).Append(',').Append(canonicalPlayer.y).Append('|');

        for (int i = 0; i < boxes.Length; i++)
        {
            builder.Append(boxes[i].x).Append(',').Append(boxes[i].y).Append(';');
        }

        return builder.ToString();
    }
}
