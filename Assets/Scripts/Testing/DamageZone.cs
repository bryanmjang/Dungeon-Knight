using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    private float timer = 0f;
    private float timeToWait = 1f; // 1 second
    List<HealthTracker> thingsToDamage = new List<HealthTracker>();
    List<HealthTracker> thingsKilled = new List<HealthTracker>();

    void OnTriggerEnter2D(Collider2D other)
    {
        var healthtracker = other.GetComponent<HealthTracker>();
        if (healthtracker != null) // if the object has a healthbar
        {
            thingsToDamage.Add(healthtracker);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var healthtracker = other.GetComponent<HealthTracker>();
        if (healthtracker != null)
        {
            thingsToDamage.Remove(healthtracker);
        }
    }

    void Update()
    {
        // if there's nothing in here, reset count
        if (thingsToDamage.Count == 0)
        {
            timer = 0;
            return;
        } 

        // we've not met the timer yet, do nothing.
        timer += Time.deltaTime;
        if (timer < timeToWait) 
            return;
        
        // reset timer and kill everything
        timer = 0f;
        foreach (var thing in thingsToDamage)
        {
            // if thing is killed
            if (thing.GiveDamage(1))
            {
                thingsKilled.Add(thing);
            }
        }

        foreach (var thing in thingsKilled)
        {
            thingsToDamage.Remove(thing);
        }
        
    }
}
