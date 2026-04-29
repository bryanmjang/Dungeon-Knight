using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_TextMeshProUGUI;

    const float WAVE_INTERVAL   = 4 * 60f;
    const float SINGLE_INTERVAL = 45f;

    float waveRemaining   = WAVE_INTERVAL;
    float singleRemaining = SINGLE_INTERVAL;

    enum TimerState { Countdown, WaveIncoming, WaveActive }
    TimerState state = TimerState.Countdown;

    List<GameObject> waveGoblins = new List<GameObject>();

    static Timer s_instance;

    public static bool IsWaveActive => s_instance != null &&
        (s_instance.state == TimerState.WaveActive || s_instance.state == TimerState.WaveIncoming);

    void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            Destroy(transform.root.gameObject);
            return;
        }
        s_instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }

    void Update()
    {
        switch (state)
        {
            case TimerState.Countdown:
                UpdateCountdown();
                break;
            case TimerState.WaveIncoming:
                UpdateWaveIncoming();
                break;
            case TimerState.WaveActive:
                UpdateWaveActive();
                break;
        }
    }

    void UpdateCountdown()
    {
        waveRemaining   -= Time.deltaTime;
        singleRemaining -= Time.deltaTime;

        // Single spawn: 1 goblin from a random spawner every 45s.
        if (singleRemaining <= 0f)
        {
            singleRemaining = SINGLE_INTERVAL;
            TriggerSingleSpawn();
        }

        // Wave timer.
        if (waveRemaining <= 0f)
        {
            waveRemaining = 0f;
            TriggerOrHoldWave();
            return;
        }

        int minutes = Mathf.FloorToInt(waveRemaining / 60);
        int seconds  = Mathf.FloorToInt(waveRemaining % 60);
        SetDisplay(string.Format("Next wave: {0:00}:{1:00}", minutes, seconds), Color.white);
    }

    void TriggerSingleSpawn()
    {
        SpawnGoblin[] spawners = Object.FindObjectsByType<SpawnGoblin>(FindObjectsSortMode.None);
        if (spawners.Length == 0) return;

        SpawnGoblin chosen = spawners[Random.Range(0, spawners.Length)];
        GameObject g = chosen.SpawnPlayer();
        Debug.Log($"[Timer] Single goblin spawned from {chosen.name}. Next in {SINGLE_INTERVAL}s");
    }

    void UpdateWaveIncoming()
    {
        SpawnGoblin[] spawners = Object.FindObjectsByType<SpawnGoblin>(FindObjectsSortMode.None);
        if (spawners.Length > 0)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"[Timer] *** WAVE SPAWNING *** in scene \"{sceneName}\" — player returned!");
            waveGoblins = SpawnWave(spawners);
            state = TimerState.WaveActive;
            SetDisplay("WAVE COMING", Color.red);
        }
        else
        {
            SetDisplay("WAVE INCOMING", Color.red);
        }
    }

    void UpdateWaveActive()
    {
        waveGoblins.RemoveAll(g => g == null);

        if (waveGoblins.Count == 0)
        {
            Debug.Log("[Timer] Wave cleared! Restarting timer.");
            waveRemaining = WAVE_INTERVAL;
            state = TimerState.Countdown;
            int minutes = Mathf.FloorToInt(waveRemaining / 60);
            int seconds  = Mathf.FloorToInt(waveRemaining % 60);
            SetDisplay(string.Format("Next wave: {0:00}:{1:00}", minutes, seconds), Color.white);
        }
        else
        {
            SetDisplay("WAVE COMING", Color.red);
        }
    }

    void TriggerOrHoldWave()
    {
        SpawnGoblin[] spawners = Object.FindObjectsByType<SpawnGoblin>(FindObjectsSortMode.None);

        if (spawners.Length > 0)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"[Timer] *** WAVE SPAWNING *** in scene \"{sceneName}\"");
            waveGoblins = SpawnWave(spawners);
            state = TimerState.WaveActive;
            SetDisplay("WAVE COMING", Color.red);
        }
        else
        {
            Debug.Log("[Timer] WAVE INCOMING — player not in goblin scene, holding wave.");
            state = TimerState.WaveIncoming;
            SetDisplay("WAVE INCOMING", Color.red);
        }
    }

    // Always spawns exactly 2 player-attackers + 2 building-attackers,
    // picking spawners randomly regardless of how many there are.
    List<GameObject> SpawnWave(SpawnGoblin[] spawners)
    {
        var all = new List<GameObject>();

        for (int i = 0; i < 2; i++)
        {
            GameObject g = spawners[Random.Range(0, spawners.Length)].SpawnPlayer();
            if (g != null) all.Add(g);
        }

        for (int i = 0; i < 2; i++)
        {
            GameObject g = spawners[Random.Range(0, spawners.Length)].SpawnBuilding();
            if (g != null) all.Add(g);
        }

        Debug.Log($"[Timer] Wave spawned {all.Count} goblins (2 player, 2 building).");
        return all;
    }

    void SetDisplay(string text, Color color)
    {
        if (m_TextMeshProUGUI == null) return;
        m_TextMeshProUGUI.text = text;
        m_TextMeshProUGUI.color = color;
    }
}
