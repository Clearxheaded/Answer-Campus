using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private int score;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartGame()
    {
        // Initialize game and start rhythm
        RhythmManager.Instance.StartBeat();
    }

    public void EndGame()
    {
        // Handle end game logic
        Debug.Log("Game Over! Final Score: " + score);
    }

    public void UpdateScore(int points)
    {
        score += points;
        UIManager.Instance.UpdateScoreUI(score);
    }
}
