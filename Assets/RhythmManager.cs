using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance { get; private set; }
    public float beatInterval = 1.0f; // Time between beats in seconds

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartBeat()
    {
        StartCoroutine(BeatCoroutine());
    }

    private IEnumerator BeatCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(beatInterval);
            // Trigger UI or animation to indicate beat
            WordSpawner.Instance.SpawnWord();
        }
    }

    public bool CheckInputTiming(float inputTime)
    {
        // Logic for checking if input is within a valid window
        float timeDifference = Mathf.Abs(Time.time % beatInterval - inputTime);
        return timeDifference <= beatInterval * 0.1f; // Example threshold
    }
}
