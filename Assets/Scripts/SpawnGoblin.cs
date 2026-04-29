using System.Collections.Generic;
using UnityEngine;

public class SpawnGoblin : MonoBehaviour
{
    public GameObject goblin;
    public GameObject goblin_builing;

    // Assign map-corner transforms here. Goblins spawn at a random one.
    public Transform[] spawnPoints;

    // Called by Timer to spawn a player-attacking goblin from this spawner.
    public GameObject SpawnPlayer()
    {
        if (goblin == null) return null;
        GameObject go = Instantiate(goblin);
        go.transform.position = PickSpawnPosition();
        go.transform.localScale = Vector3.one;
        return go;
    }

    // Called by Timer to spawn a building-attacking goblin from this spawner.
    public GameObject SpawnBuilding()
    {
        if (goblin_builing == null) return null;
        GameObject go = Instantiate(goblin_builing);
        go.transform.position = PickSpawnPosition();
        go.transform.localScale = Vector3.one;
        return go;
    }

    Vector3 PickSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform pt = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (pt != null) return pt.position;
        }
        Vector3 pos = transform.position;
        return new Vector3(pos.x, pos.y + transform.localScale.y / 2f, pos.z);
    }
}
