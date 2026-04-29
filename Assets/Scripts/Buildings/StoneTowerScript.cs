using UnityEngine;

public class StoneTowerScript : MonoBehaviour
{
    private int helath = 20;

    public void ChangeHealth(int amount)
    {
        helath += amount;
        if (helath <= 0)
        {
            Destroy(gameObject);
        }
    }
}
