using UnityEngine;
using System.Collections;

public class UnlockDungeon2 : MonoBehaviour
{
    public GameObject dungeon2Door;

    void Start()
    {
        // PlayerPrefs.DeleteAll(); // REMOVE ONCE SAVING SYSTEM IS IMPLEMENTED
        if (PlayerPrefs.GetInt("Dungeon1Complete", 0) == 1)
        {
            dungeon2Door.SetActive(true);
        }

        else
        {
            dungeon2Door.SetActive(false);
        }
    }

}