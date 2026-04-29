using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    public GameObject[] stateSlots;
    public GameObject statsCanvas;
    private bool isOpen = false;

    public void Start()
    {
        UpdateAll();
    }

    public void ToggleStats()
    {
        if (isOpen)
        {
            statsCanvas.SetActive(false);
        }
        else
        {
            statsCanvas.SetActive(true);
        }
        isOpen = !isOpen;
        
    }
    public void UpdateHealth()
    {
        stateSlots[0].GetComponentInChildren<TMP_Text>().text = "Health: " + Statsmanager.instance.maxHealth;
    }

    public void UpdateStun()
    {
        stateSlots[1].GetComponentInChildren<TMP_Text>().text = "Stun: " + Statsmanager.instance.knockbackStun;
    }

    public void UpdateSpeed()
    {
        stateSlots[2].GetComponentInChildren<TMP_Text>().text = "Speed: " + Statsmanager.instance.speed;
    }

    public void UpdateAttackSpeed()
    {
        stateSlots[3].GetComponentInChildren<TMP_Text>().text = "Attack Speed: " + Statsmanager.instance.attackSpeed;
    }

    public void UpdateAll()
    {
        UpdateHealth();
        UpdateStun();
        UpdateSpeed();
        UpdateAttackSpeed();

    }
}
