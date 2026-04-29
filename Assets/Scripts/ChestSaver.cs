using UnityEngine;

public class ChestSaver : BasePersistence
{
    // don't ask why I'm doing circumfixes, I only just realized and I'm not changing it.
    protected override string Prefix => "Chest";
    protected override string StateSuffix => "Opened";

    protected override void Start()
    {
        if (string.IsNullOrEmpty(persistenceID)) return;

        if (PlayerPrefs.GetInt(GetPrefsKey(), 0) == 1)
        {
            gameObject.GetComponent<DungeonChest>().OpenChest(null);
        }
    }

    [ContextMenu("Mark As Opened")]
    public void MarkAsOpen() => SetState(true);

    [ContextMenu("Reset Save State")]
    public void ResetState() => SetState(false);
}
