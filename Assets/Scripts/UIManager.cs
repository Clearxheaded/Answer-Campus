using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public TMPro.TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateScoreUI(int newScore)
    {
        scoreText.text = "Score: " + newScore;
    }
}
