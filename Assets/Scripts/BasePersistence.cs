using UnityEngine;

public abstract class BasePersistence : MonoBehaviour
{
    [Header("Use the format 'scene_number'")]
    [SerializeField] protected string persistenceID;

    // These allow the child classes to define their own string formats
    protected abstract string Prefix { get; }
    protected abstract string StateSuffix { get; }

    protected virtual void Start()
    {
        if (string.IsNullOrEmpty(persistenceID)) return;

        if (PlayerPrefs.GetInt(GetPrefsKey(), 0) == 1)
        {
            gameObject.SetActive(false);
        }
    }

    protected void SetState(bool isActive)
    {
        if (string.IsNullOrEmpty(persistenceID)) return;

        if (isActive)
        {
            PlayerPrefs.SetInt(GetPrefsKey(), 1);
        }
        else
        {
            PlayerPrefs.DeleteKey(GetPrefsKey());
        }

        PlayerPrefs.Save();
    }

    protected string GetPrefsKey() => $"{Prefix}_{persistenceID}_{StateSuffix}";
}