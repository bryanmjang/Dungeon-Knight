using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    public string requiredKeyID;

    public Collider2D doorCollider;
    public GameObject doorVisual;

    public AudioSource audioSource;
    public AudioClip unlockSound;
    public AudioClip lockedSound;

    private bool isUnlocked = false;

    public void TryUnlock(KeyItem key)
    {
        if (isUnlocked) return;

        if (key != null && key.keyID == requiredKeyID)
        {
            Unlock();
        }
        else
        {
            if (audioSource && lockedSound)
                audioSource.PlayOneShot(lockedSound);
        }
    }

    void Unlock()
    {
        isUnlocked = true;
        var isPersistant = GetComponent<DoorPersistence>();
        if (isPersistant != null) isPersistant.MarkAsOpen();

        if (audioSource && unlockSound)
            audioSource.PlayOneShot(unlockSound);

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorVisual != null)
            doorVisual.SetActive(false);
    }
}