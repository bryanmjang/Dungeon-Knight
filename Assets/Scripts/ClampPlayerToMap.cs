using UnityEngine;

public class ClampPlayerToMap : MonoBehaviour
{
    public TerrainGeneration terrain;
    public float padding = 0.5f;

    private void LateUpdate()
    {
        if (terrain == null) return;

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, terrain.MinWorldX + padding, terrain.MaxWorldX - padding);
        pos.y = Mathf.Clamp(pos.y, terrain.MinWorldY + padding, terrain.MaxWorldY - padding);

        transform.position = pos;
    }
}