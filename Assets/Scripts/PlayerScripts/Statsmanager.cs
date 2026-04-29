using UnityEngine;

public class Statsmanager : MonoBehaviour
{
    public static Statsmanager instance;
     private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Stats")]
    public float knockbackStun;
    public float attackSpeed;
    public int maxHealth;
    public float speed;
}
