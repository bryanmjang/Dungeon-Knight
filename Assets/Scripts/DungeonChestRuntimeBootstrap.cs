using UnityEngine;
using UnityEngine.SceneManagement;

public static class DungeonChestRuntimeBootstrap
{
    // Only the dungeon scene should receive this automatic chest setup.
    private const string DungeonSceneName = "DungeonLayout";
    private static DungeonChestBootstrapConfig bootstrapConfig;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        // listen for scene loads and also try to configure the current active scene immediately
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryInstallForScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryInstallForScene(scene);
    }

    private static void TryInstallForScene(Scene scene)
    {
        // ignore every scene except the main dungeon scene
        if (!scene.IsValid() || scene.name != DungeonSceneName)
        {
            return;
        }

        // scan the loaded scene for objects named like dungeon chests
        bootstrapConfig = Object.FindObjectOfType<DungeonChestBootstrapConfig>();
        Transform[] sceneTransforms = Object.FindObjectsOfType<Transform>();
        foreach (Transform candidate in sceneTransforms)
        {
            if (candidate == null || !candidate.gameObject.scene.IsValid() || candidate.gameObject.scene.name != DungeonSceneName)
            {
                continue;
            }

            ConfigureChest(candidate.gameObject);
        }
    }

    private static void ConfigureChest(GameObject chestObject)
    {
        if (chestObject == null)
        {
            return;
        }

        string chestName = chestObject.name;
        if (!chestName.Contains("Chest"))
        {
            return;
        }

        // every chest needs a trigger collider so the player can enter its interaction range
        BoxCollider2D boxCollider = chestObject.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = chestObject.AddComponent<BoxCollider2D>();
        }

        boxCollider.isTrigger = true;
        boxCollider.size = GetChestColliderSize(chestObject);

        DungeonChest dungeonChest = chestObject.GetComponent<DungeonChest>();
        if (dungeonChest == null)
        {
            dungeonChest = chestObject.AddComponent<DungeonChest>();
        }

        // weapon chests are configured by hand in the inspector so different weapons can be assigned
        if (chestName == "Weapon Chest")
        {
            dungeonChest.ResetVisual();
            return;
        }
        else if (chestName == "Final Key Chest")
        {
            // special key chests are also handled manually for now
            return;
        }
        else if (chestName == "Fake Chest")
        {
            dungeonChest.Configure(DungeonChest.ChestRewardType.Trap, 1, trapPath: "Goblin");
        }
        else
        {
            // regular money chests get a small random coin reward
            dungeonChest.Configure(DungeonChest.ChestRewardType.Coins, Random.Range(5, 20));
            if (bootstrapConfig != null && bootstrapConfig.CoinItem != null)
            {
                dungeonChest.SetRewardItem(bootstrapConfig.CoinItem);
            }
        }

        dungeonChest.ResetVisual();
    }

    private static Vector2 GetChestColliderSize(GameObject chestObject)
    {
        // size the trigger to the chest sprite so the player can interact from a sensible distance
        SpriteRenderer spriteRenderer = chestObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            return spriteSize * 1.1f;
        }

        return new Vector2(0.3f, 0.3f);
    }
}
