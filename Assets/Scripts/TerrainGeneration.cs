using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TerrainGeneration : MonoBehaviour
{
    public GameObject goblin_house;
    public Tilemap groundTilemap;
    public Tilemap decorationTilemap;

    public TileBase[] grassTile;
    public TileBase[] decorationTiles;

    [Header("Player")]
    public Transform player;

    [Header("Map Size")]
    [Min(1)] public int mapWidth = 25;
    [Min(1)] public int mapHeight = 25;

    [Header("Generation")]
    public int seed = 0;
    public bool randomSeed = true;
    [Range(0f, 1f)] public float decorationDensity = 0.025f;

    [Header("Goblin Houses")]
    public int goblinHouseCount = 4;
    public int edgePadding = 2;

    private System.Random rnd;

    public int StartX => -(mapWidth / 2);
    public int StartY => -(mapHeight / 2);

    public float MinWorldX => StartX;
    public float MinWorldY => StartY;
    public float MaxWorldX => StartX + mapWidth;
    public float MaxWorldY => StartY + mapHeight;

    private void Start()
    {
        if (randomSeed)
            seed = Random.Range(0, 100000);

        rnd = new System.Random(seed);

        GenerateMap();
        PlaceGoblinHouses();
        PlacePlayerAtCenter();
    }

    private void GenerateMap()
    {
        if (groundTilemap == null) return;

        groundTilemap.ClearAllTiles();

        if (decorationTilemap != null)
            decorationTilemap.ClearAllTiles();

        int startX = StartX;
        int startY = StartY;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                int tileX = startX + x;
                int tileY = startY + y;

                Vector3Int tilePos = new Vector3Int(tileX, tileY, 0);

                if (grassTile != null && grassTile.Length > 0)
                {
                    int grassIndex = PickGrassIndex();
                    groundTilemap.SetTile(tilePos, grassTile[grassIndex]);
                }

                if (decorationTilemap != null && decorationTiles != null && decorationTiles.Length > 0)
                {
                    var tileRnd = new System.Random(seed ^ (tileX * 73856093) ^ (tileY * 19349663));

                    if (tileRnd.NextDouble() < decorationDensity)
                    {
                        int decoIndex = tileRnd.Next(0, decorationTiles.Length);
                        decorationTilemap.SetTile(tilePos, decorationTiles[decoIndex]);
                    }
                }
            }
        }

        groundTilemap.CompressBounds();
        if (decorationTilemap != null)
            decorationTilemap.CompressBounds();
    }

    private void PlaceGoblinHouses()
    {
        if (goblin_house == null || groundTilemap == null) return;

        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

        int minX = StartX + edgePadding;
        int maxX = StartX + mapWidth - edgePadding;
        int minY = StartY + edgePadding;
        int maxY = StartY + mapHeight - edgePadding;

        for (int i = 0; i < goblinHouseCount; i++)
        {
            Vector2Int tilePos2D;
            int safety = 0;

            do
            {
                int x = rnd.Next(minX, maxX);
                int y = rnd.Next(minY, maxY);
                tilePos2D = new Vector2Int(x, y);
                safety++;
            }
            while (
                (usedPositions.Contains(tilePos2D) || tilePos2D == Vector2Int.zero) &&
                safety < 100
            );

            usedPositions.Add(tilePos2D);

            Vector3Int cellPos = new Vector3Int(tilePos2D.x, tilePos2D.y, 0);
            Vector3 worldPos = groundTilemap.GetCellCenterWorld(cellPos);

            Instantiate(goblin_house, worldPos, Quaternion.identity);
        }
    }

    private void PlacePlayerAtCenter()
    {
        if (player == null || groundTilemap == null) return;

        Vector3 centerPos = groundTilemap.GetCellCenterWorld(Vector3Int.zero);
        player.position = new Vector3(centerPos.x, centerPos.y, player.position.z);
    }

    private int PickGrassIndex()
    {
        if (grassTile == null || grassTile.Length == 0) return 0;
        if (grassTile.Length == 1) return 0;

        double roll = rnd.NextDouble();

        if (roll < 0.9 && grassTile.Length > 1)
            return 1;

        int pick = rnd.Next(0, grassTile.Length - 1);
        if (pick >= 1) pick++;
        return pick;
    }
}