using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FivePositionsGameManager : MonoBehaviour {
    [Header("Word List")]
    public List<WordDefinition> possibleWords;

    [Header("Scene References")]
    public Transform[] spawnPositions = new Transform[5];
    public Transform[] boxPositions = new Transform[5];
    public TextMeshProUGUI[] boxLetterDisplays = new TextMeshProUGUI[5];
    public TextMeshProUGUI targetDefinitionText;
    public TextMeshProUGUI countdownText; // "3-2-1" countdown text
    public TextMeshProUGUI scoreText;

    [Header("Prefabs/Assets")]
    public GameObject letterPrefab;
    public AudioClip correctClip;
    public AudioClip incorrectClip;

    [Header("Spawn Settings")]
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 3f;
    [Range(0f, 1f)] public float chanceOfCorrectLetter = 0.3f;
    public float letterSpeed = 2f;

    private string targetWord = "";
    private char[] targetLetters = new char[5];
    private bool[] boxFilled = new bool[5];
    private string alphabet = "abcdefghijklmnopqrstuvwxyz";

    private int score = 0;

    [Header("Timer Settings")]
    public float gameDuration = 60f;       // Total game time in seconds
    public TextMeshProUGUI timerText;      // Displays remaining time
    public TextMeshProUGUI penaltyText;    // Briefly shows "-0:20" or similar
    public float penaltyTime = 20f;        // Seconds to remove on incorrect answer
    public GameObject gameOverPanel;       // Panel to show when time runs out
    [SerializeField] private GameObject gameStuff;
    public TextMeshProUGUI finalScoreText; // Display final score on game over panel

    private float timeLeft;
    private bool gameIsOver = false;
    private Coroutine spawnRoutine;

    // This bool will pause the timer when true
    private bool isTimerPaused = false;

    private void Start() {
        StartGame();
    }

    public void StartGame()
    {
        // Initialize score UI
        UpdateScoreUI();

        // Hide penalty text and game-over panel at start
        if (penaltyText != null) penaltyText.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameStuff != null) gameStuff.SetActive(true);

        // Initialize the timer but don�t let it tick yet
        timeLeft = gameDuration;
        UpdateTimerUI();
        isTimerPaused = true;

        // Start the timer coroutine right away 
        StartCoroutine(GameTimerCoroutine());

        // Start the first countdown
        StartCoroutine(CountdownCoroutine());
        
    }
    /// <summary>
    /// Main game timer. It only decrements timeLeft if isTimerPaused is false.
    /// </summary>
    private IEnumerator GameTimerCoroutine() {
        while (timeLeft > 0 && !gameIsOver) {
            yield return null; // Wait one frame

            if (!isTimerPaused) {
                timeLeft -= Time.deltaTime;
                UpdateTimerUI();

                if (timeLeft <= 0 && !gameIsOver) {
                    timeLeft = 0;
                    UpdateTimerUI();
                    EndGame();
                }
            }
        }
    }

    /// <summary>
    /// Shows a short "3-2-1" countdown, then unpauses the timer and spawns letters.
    /// </summary>
    private IEnumerator CountdownCoroutine() {
        // Start or reset the round�s target word
        StartNewRound();

        if (countdownText != null) {
            countdownText.gameObject.SetActive(true);

            countdownText.text = "3";
            yield return new WaitForSeconds(1f);

            countdownText.text = "2";
            yield return new WaitForSeconds(1f);

            countdownText.text = "1";
            yield return new WaitForSeconds(1f);

            countdownText.gameObject.SetActive(false);
        }

        // Now that the countdown is done, unpause the timer and spawn letters
        if (!gameIsOver) {
            isTimerPaused = false;
            spawnRoutine = StartCoroutine(SpawnLettersRoutine());
        }
    }

    /// <summary>
    /// Spawns letters at random intervals until boxes are filled or game ends.
    /// </summary>
    private IEnumerator SpawnLettersRoutine() {
        while (!AllBoxesFilled() && !gameIsOver) {
            // Which boxes are still unfilled?
            List<int> unfilledIndices = new List<int>();
            for (int i = 0; i < 5; i++) {
                if (!boxFilled[i]) unfilledIndices.Add(i);
            }

            if (unfilledIndices.Count == 0) yield break;

            int randomIndex = unfilledIndices[Random.Range(0, unfilledIndices.Count)];

            // Decide whether to spawn a correct letter or random letter
            char letterToSpawn = Random.value < chanceOfCorrectLetter
                ? targetLetters[randomIndex]
                : alphabet[Random.Range(0, alphabet.Length)];

            // Instantiate the new letter
            Transform spawnPos = spawnPositions[randomIndex];
            GameObject newLetter = Instantiate(letterPrefab, spawnPos.position, Quaternion.identity);

            // Set letter text
            TextMeshPro textComp = newLetter.GetComponentInChildren<TextMeshPro>();
            if (textComp != null) {
                textComp.text = letterToSpawn.ToString();
            }

            // Initialize movement
            LetterMovement letterMovement = newLetter.GetComponent<LetterMovement>();
            letterMovement.Initialize(
                this,
                randomIndex,
                letterToSpawn,
                boxPositions[randomIndex].position,
                letterSpeed
            );

            // Wait before next spawn
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// Selects a random 5-letter word, displays its definition, and resets boxes.
    /// </summary>
    private void StartNewRound() {
        WordDefinition chosenDefinition = SelectRandomFiveLetterWord();
        if (chosenDefinition != null) {
            targetWord = chosenDefinition.word;
            if (targetDefinitionText != null) {
                targetDefinitionText.text = "Definition: " + chosenDefinition.definition;
            }
        } else {
            // Fallback
            targetWord = "abcde";
            if (targetDefinitionText != null) {
                targetDefinitionText.text = "Definition: [No 5-letter words available!]";
            }
        }

        // Reset boxes
        for (int i = 0; i < 5; i++) {
            targetLetters[i] = targetWord[i];
            boxFilled[i] = false;
            if (boxLetterDisplays[i] != null) {
                boxLetterDisplays[i].text = " ";
            }
        }
    }

    /// <summary>
    /// Checks if all 5 boxes are filled.
    /// </summary>
    private bool AllBoxesFilled() {
        foreach (bool filled in boxFilled) {
            if (!filled) return false;
        }
        return true;
    }

    /// <summary>
    /// Called by LetterMovement when a letter reaches its box.
    /// </summary>
    public void OnLetterArrived(int boxIndex, char arrivedLetter, GameObject letterObj) {
        if (gameIsOver) {
            Destroy(letterObj);
            return;
        }

        if (boxFilled[boxIndex]) {
            // Already filled with a correct letter
            Destroy(letterObj);
            return;
        }

        // Check if correct letter
        if (arrivedLetter == targetLetters[boxIndex]) {
            boxFilled[boxIndex] = true;
            if (boxLetterDisplays[boxIndex] != null) {
                boxLetterDisplays[boxIndex].text = arrivedLetter.ToString();
            }

            // Play SFX
            if (AudioManager.Instance != null && correctClip != null) {
                AudioManager.Instance.PlaySFX(correctClip);
            }

            // Destroy any overlapping letters at the same x-position
            DestroyLettersOnSameX(letterObj.transform.position.x);
        } else {
            // Incorrect letter
            if (AudioManager.Instance != null && incorrectClip != null) {
                AudioManager.Instance.PlaySFX(incorrectClip);
            }

            // Apply penalty
            timeLeft -= penaltyTime;
            if (timeLeft < 0) timeLeft = 0;
            UpdateTimerUI();

            // Show penalty text briefly
            if (penaltyText != null) {
                penaltyText.gameObject.SetActive(true);
                penaltyText.text = string.Format("-0:{0:00}", (int)penaltyTime);
                StartCoroutine(HidePenaltyText());
            }
        }

        // Destroy the letter
        Destroy(letterObj);

        // If all boxes are filled, increase score & start next round
        if (AllBoxesFilled()) {
            score++;
            UpdateScoreUI();
            StartCoroutine(RestartGameRoutine());
        }

        // If timer is out, end game
        if (timeLeft <= 0 && !gameIsOver) {
            EndGame();
        }
    }

    /// <summary>
    /// Hides penalty text after a short delay.
    /// </summary>
    private IEnumerator HidePenaltyText() {
        yield return new WaitForSeconds(1f);
        if (penaltyText != null) {
            penaltyText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Destroy all letters at a given x-position (to remove duplicates).
    /// </summary>
    private void DestroyLettersOnSameX(float xPosition) {
        GameObject[] allLetters = GameObject.FindGameObjectsWithTag("Letter");
        foreach (GameObject letter in allLetters) {
            if (Mathf.Abs(letter.transform.position.x - xPosition) < 0.1f) {
                Destroy(letter);
            }
        }
    }

    /// <summary>
    /// After a short delay, pause the timer, clear boxes, do a new countdown, then unpause.
    /// </summary>
    private IEnumerator RestartGameRoutine() {
        // Wait briefly so the player can see the filled boxes
        yield return new WaitForSeconds(2f);

        if (!gameIsOver) {
            // Pause timer during the countdown
            isTimerPaused = true;

            // Clear boxes
            for (int i = 0; i < 5; i++) {
                if (boxLetterDisplays[i] != null) {
                    boxLetterDisplays[i].text = " ";
                }
                boxFilled[i] = false;
            }

            // Run another "3-2-1" countdown, which will unpause the timer again
            StartCoroutine(CountdownCoroutine());
        }
    }

    /// <summary>
    /// Selects a random 5-letter word from 'possibleWords'.
    /// </summary>
    private WordDefinition SelectRandomFiveLetterWord() {
        List<WordDefinition> validFiveLetterWords = new List<WordDefinition>();
        foreach (WordDefinition wd in possibleWords) {
            if (wd.word.Length == 5) {
                validFiveLetterWords.Add(wd);
            }
        }

        if (validFiveLetterWords.Count > 0) {
            int randIndex = Random.Range(0, validFiveLetterWords.Count);
            return validFiveLetterWords[randIndex];
        }
        return null;
    }

    /// <summary>
    /// Updates the UI score label.
    /// </summary>
    private void UpdateScoreUI() {
        if (scoreText != null) {
            scoreText.text = score.ToString();
        }
    }

    /// <summary>
    /// Ends the game, stops coroutines, and shows the Game Over panel.
    /// </summary>
    private void EndGame() {
        gameIsOver = true;
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);

        clearLeftoverLetters();

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameStuff != null) gameStuff.SetActive(false);

        if (finalScoreText != null) {
            finalScoreText.text = score + " Words Studied";
        }
    }

    public void BackToHome()
    {
        //TODO: Adjust Player Stats
        SceneManager.LoadScene("Home");
    }

    /// <summary>
    /// Called by a UI "Retry" button to reset the entire game.
    /// </summary>
    public void RetryGame() {
        gameIsOver = false;
        score = 0;
        UpdateScoreUI();
        timeLeft = gameDuration;
        UpdateTimerUI();

        // Hide game over panel
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameStuff != null) gameStuff.SetActive(true);

        // Clear leftover letters
        clearLeftoverLetters();

        // Pause timer, then show countdown again
        isTimerPaused = true;

        // Start the timer coroutine again (in case we stopped it by ending the game)
        StartCoroutine(GameTimerCoroutine());

        // Start fresh countdown 
        StartCoroutine(CountdownCoroutine());
    }

    /// <summary>
    /// Destroys any leftover letters still on the screen.
    /// </summary>
    private void clearLeftoverLetters() {
        var leftoverLetters = GameObject.FindGameObjectsWithTag("Letter");
        foreach (var letter in leftoverLetters) {
            Destroy(letter);
        }
    }

    private void UpdateTimerUI() {
        if (timerText != null) {
            int minutes = Mathf.FloorToInt(timeLeft / 60f);
            int seconds = Mathf.FloorToInt(timeLeft % 60f);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
    }

}
