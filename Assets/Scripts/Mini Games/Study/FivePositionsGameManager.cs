using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using VNEngine;

[System.Serializable]
public class QuestionAnswerPair {
    public string question;
    public string definition;
    public string answer; // Must be 5 letters
}

public class FivePositionsGameManager : MonoBehaviour
{
    public enum SpawnMode { Sequential, Random }
    public GameMode currentMode;
    public bool useTimer;
    public SpawnMode spawnMode;    private int lastSpawnedColumn = -1;
    public int maxWrongGuesses = 3;

    [Header("Word List")] public List<WordDefinition> possibleWords;
    public StudyQuestionLoader questionLoader;

    [Header("Scene References")]
    public RectTransform[] boxPositions = new RectTransform[5];
    public TextMeshProUGUI[] boxLetterDisplays = new TextMeshProUGUI[5];
    public TextMeshProUGUI targetDefinitionText;
    public TextMeshProUGUI countdownText; // "3-2-1" countdown text
    public TextMeshProUGUI scoreText;
    public ConversationManager endExamConversation;
    private ConversationManager conversationManager;
    [Header("Prefabs/Assets")]
    public GameObject letterPrefab;
    public AudioClip correctClip;
    public AudioClip incorrectClip;
    public GameObject boxSpritePrefab;
    public GameObject eraserPrefab;
    [Header("Offsets")]
    public float spawnYOffset = 100f; // how far above each box the letter should spawn
    public float boxYOffset = 0f;         // Optional adjustment (e.g., -0.5f if needed)
    public float eraserYOffset = 2f;      // How far above the first box to place the eraser
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
    private AudioSource audioSource;
    [Header("Timer Settings")]
    public GameObject timers;
    public float gameDuration = 60f;       // Total game time in seconds
    public TextMeshProUGUI timerText;      // Displays remaining time
    public TextMeshProUGUI penaltyText;    // Briefly shows "-0:20" or similar
    public float penaltyTime = 20f;        // Seconds to remove on incorrect answer
    public GameObject studyGameParent;       // Panel to show when time runs out
    [SerializeField] private GameObject gameStuff;
    public TextMeshProUGUI finalScoreText; // Display final score on game over panel
    private GameObject eraser;
    private float timeLeft;
    private bool gameIsOver = false;
    private Coroutine spawnRoutine;
    private int wrongGuessCount = 0;
    // This bool will pause the timer when true
    private bool isTimerPaused = false;
    private List<int> activeColumns = new List<int>();
    private List<GameObject> boxVisuals = new List<GameObject>();
    
    public void Initialize()
    {
//        audioSource = FMODAudioManager.Instance.GetAudioSource();
        SetMode(currentMode);
        SpawnVisuals();
        StartCoroutine(DelayedGameStart());
    }
    private void SpawnVisuals()
    {
        for (int i = 0; i < boxPositions.Length; i++)
        {
            RectTransform box = boxPositions[i];
            if (box == null) continue;

            // ✅ This respects the entire transform hierarchy, including y = -3
            Vector3 worldBoxCenter = box.position;

            // Add world-space vertical offset if needed
            Vector3 worldPos = worldBoxCenter + new Vector3(0, boxYOffset, 0);
            worldPos.z = 0;

            GameObject visual = Instantiate(boxSpritePrefab, worldPos, Quaternion.identity);
            boxVisuals.Add(visual);
            SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Foreground";
                sr.sortingOrder = 100;
            }

            Debug.Log($"Box visual {i} spawned at world Y = {worldPos.y:F2}");
        }



        // Spawn the eraser above the first column
// Spawn the eraser above the first column
        Transform firstBox = boxPositions[0];
        if (firstBox != null)
        {
            Vector3 eraserPos = firstBox.position + new Vector3(0, eraserYOffset, 0);
            eraser = Instantiate(eraserPrefab, eraserPos, Quaternion.identity);

            // Set sprite rendering layer
            SpriteRenderer sr = eraser.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Foreground";
                sr.sortingOrder = 200;
            }

            // Connect the eraser to the box positions
            EraserController eraserController = eraser.GetComponent<EraserController>();
            if (eraserController != null)
            {
                eraserController.boxPositions = boxPositions;
            }
            else
            {
                Debug.LogWarning("Eraser prefab is missing EraserController.");
            }
        }
    }

    private IEnumerator DelayedGameStart()
    {
        yield return new WaitForEndOfFrame();
        StartGame();
    }
    
    private Vector3 GetSpawnPosAboveBox(int index)
    {
        if (boxPositions == null || index < 0 || index >= boxPositions.Length)
        {
            Debug.LogError("Invalid box index.");
            return Vector3.zero;
        }

        RectTransform rect = boxPositions[index];
        if (rect == null)
        {
            Debug.LogError("Box is not a RectTransform.");
            return Vector3.zero;
        }

        // World-space center of the box, including parent offset
        Vector3 worldBoxCenter = rect.position;

        // Offset vertically in world units
        Vector3 spawnPos = worldBoxCenter + new Vector3(0, spawnYOffset, 0);
        spawnPos.z = 0;

        return spawnPos;
    }




    public void StartGame()
    {
        // Initialize score UI
        UpdateScoreUI();

        // Hide penalty text and game-over panel at start
        if (penaltyText != null) penaltyText.gameObject.SetActive(false);
        if (gameStuff != null) gameStuff.SetActive(true);

        // Initialize the timer but don�t let it tick yet
        timeLeft = gameDuration;
        UpdateTimerUI();
        isTimerPaused = true;
        gameIsOver = false;
        // Start the timer coroutine right away 
        StartCoroutine(GameTimerCoroutine());
        questionLoader.LoadQuestionsForCurrentWeek();
        // Start the first countdown
        StartCoroutine(CountdownCoroutine());
        
    }
    /// <summary>
    /// Main game timer. It only decrements timeLeft if isTimerPaused is false.
    /// </summary>
    private IEnumerator GameTimerCoroutine() {
        while (timeLeft > 0 && !gameIsOver) {
            yield return null; // Wait one frame
            if (useTimer)
            {
                if (!isTimerPaused)
                {
                    timeLeft -= Time.deltaTime;
                    UpdateTimerUI();

                    if (timeLeft <= 0 && !gameIsOver)
                    {
                        timeLeft = 0;
                        UpdateTimerUI();
                        StartCoroutine(EndGame());

                    }
                }
            }
            else {
                if (currentMode == GameMode.Group || currentMode == GameMode.Exam)
                {
                    if (AllBoxesFilled() || wrongGuessCount >= maxWrongGuesses)
                    {
                        StartCoroutine(EndGame());
                    }
                }

            }
        }
    }

    /// <summary>
    /// Shows a short "3-2-1" countdown, then unpauses the timer and spawns letters.
    /// </summary>
    private IEnumerator CountdownCoroutine(bool skipCountdown = false) {
        // Start or reset the round�s target word
        StartNewRound();
        if (!skipCountdown)
        {
            
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
        }

        // Now that the countdown is done, unpause the timer and spawn letters
        if (!gameIsOver) {
            isTimerPaused = false;
            if (spawnRoutine != null)
            {
                Debug.LogWarning("Spawn routine already running — not starting another.");
                yield break;
            }
            spawnRoutine = StartCoroutine(SpawnLettersRoutine());
        }
    }

    /// <summary>
    /// Spawns letters at random intervals until boxes are filled or game ends.
    /// </summary>
    private IEnumerator SpawnLettersRoutine() {
        while (!AllBoxesFilled() && !gameIsOver)
        {
            List<int> spawnableIndices = new List<int>();
            for (int i = 0; i < boxPositions.Length; i++)
            {
                if (!boxFilled[i] && !LetterInColumn(i))
                    spawnableIndices.Add(i);
            }

            // ✅ Nothing available? Stop trying to spawn and wait for update loop
            if (spawnableIndices.Count == 0)
            {
                // Check if we're just waiting for remaining letters to arrive
                bool waitingForDelivery = false;
                for (int i = 0; i < boxFilled.Length; i++)
                {
                    if (!boxFilled[i] && LetterInColumn(i))
                    {
                        waitingForDelivery = true;
                        break;
                    }
                }

                if (!waitingForDelivery)
                {
                    // Nothing left to do — either all filled or all blocked with no letters en route
                    yield break;
                }

                yield return null;
                continue;
            }

            int selectedIndex;

            if (spawnMode == SpawnMode.Sequential)
            {
                int attempts = 0;
                do
                {
                    lastSpawnedColumn = (lastSpawnedColumn + 1) % boxPositions.Length;
                    selectedIndex = lastSpawnedColumn;
                    attempts++;
                }
                while ((!spawnableIndices.Contains(selectedIndex)) && attempts <= boxPositions.Length);
            }
            else // Random
            {
                selectedIndex = spawnableIndices[Random.Range(0, spawnableIndices.Count)];
            }

            // Decide whether to spawn a correct letter or random letter
            char letterToSpawn = Random.value < chanceOfCorrectLetter
                ? targetLetters[selectedIndex]
                : alphabet[Random.Range(0, alphabet.Length)];

            // Instantiate the new letter
            Vector3 spawnPos = GetSpawnPosAboveBox(selectedIndex);
            GameObject newLetter = Instantiate(letterPrefab, spawnPos, Quaternion.identity);
            RegisterActiveColumn(selectedIndex);
            // Set letter text
            TextMeshPro textComp = newLetter.GetComponentInChildren<TextMeshPro>();
            if (textComp != null) {
                textComp.text = letterToSpawn.ToString();
            }

            // Initialize movement
            LetterMovement letterMovement = newLetter.GetComponent<LetterMovement>();
            letterMovement.Initialize(
                this,
                selectedIndex,
                letterToSpawn,
                boxPositions[selectedIndex].position,
                letterSpeed
            );

            // Wait before next spawn
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }
    

    public void RegisterActiveColumn(int index)
    {
        if (!activeColumns.Contains(index))
            activeColumns.Add(index);
    }

    public void UnregisterActiveColumn(int index)
    {
        activeColumns.Remove(index);
    }

    private bool LetterInColumn(int index)
    {
        return activeColumns.Contains(index);
    }


    private void StartNewRound()
    {
        QuestionAnswerPair question = SelectRandomQuestion();
        if (question != null)
        {
            targetWord = question.answer.ToLower(); // Ensure lowercase for consistency
            if (targetDefinitionText != null)
            {
                targetDefinitionText.text = questionLoader.GetDisplayPrompt(question, questionLoader.useDefinitions);
            }
            else
            {
                targetWord = "error";
                targetDefinitionText.text = "No valid 5-letter questions!";
            }

            // Reset box UI
            for (int i = 0; i < 5; i++)
            {
                targetLetters[i] = targetWord[i];
                boxFilled[i] = false;
                if (boxLetterDisplays[i] != null)
                {
                    boxLetterDisplays[i].text = " ";
                }
            }
        }
        else
        {
            Debug.LogWarning($"Invalid question selected. Length is {question.answer.Length}");
        }
    }

    public void ConfigureChallenge(ChallengeProfile profile)
    {
        timeLeft = profile.timerDuration;
        minSpawnInterval = profile.minSpawnInterval;
        maxSpawnInterval = profile.maxSpawnInterval;
        chanceOfCorrectLetter = profile.chanceOfCorrectLetter;

        timers.SetActive(profile.showTimer);

        // Decide whether we're using definitions or questions
        bool useDefinitions = profile.promptType == ChallengeProfile.PromptType.Definitions;
        questionLoader.useDefinitions = useDefinitions;
        questionLoader.LoadQuestionsForCurrentWeek(); // fallback
    }

    private QuestionAnswerPair SelectRandomQuestion()
    {
        if (questionLoader == null || questionLoader.currentQuestions == null || questionLoader.currentQuestions.Count == 0)
        {
            Debug.LogWarning("No loaded questions available. Returning default.");
            return new QuestionAnswerPair { question = "Missing data", answer = "error" };
        }

        return questionLoader.GetRandomQuestion();
    }

    /// <summary>
    /// Checks if all 5 boxes are filled.
    /// </summary>
    private void ClearAllBoxes()
    {
        for (int i = 0; i < boxFilled.Length; i++)
        {
            boxFilled[i] = false;
        }
    }
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
//            audioSource.PlayOneShot(correctClip);

            DestroyLettersOnSameX(letterObj.transform.position.x);
        } else {
            // Incorrect letter
//            audioSource.PlayOneShot(incorrectClip);

            wrongGuessCount++;
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
        UnregisterActiveColumn(boxIndex);
        Destroy(letterObj);

        // If all boxes are filled, increase score & start next round
        if (AllBoxesFilled()) {
            score++;
            UpdateScoreUI();
            StartCoroutine(RestartGameRoutine());
        }

        // If timer is out, end game
        if (timeLeft <= 0 && !gameIsOver && useTimer) {
            StartCoroutine(EndGame());

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
        //half the time between spawns
        minSpawnInterval *= .5f;
        maxSpawnInterval *= .5f;
        // Wait briefly so the player can see the filled boxes
        yield return new WaitForSeconds(2f);

        if (!gameIsOver) {
            // Pause timer during the countdown
            isTimerPaused = true;
            spawnRoutine = null;
            // Clear boxes
            for (int i = 0; i < 5; i++) {
                if (boxLetterDisplays[i] != null) {
                    boxLetterDisplays[i].text = " ";
                }
                boxFilled[i] = false;
            }
            // Start the timer coroutine right away 
            StartCoroutine(GameTimerCoroutine());
            questionLoader.LoadQuestionsForCurrentWeek();
            // Run another "3-2-1" countdown, which will unpause the timer again
            StartCoroutine(CountdownCoroutine(false));
            
            
            
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
    private IEnumerator EndGame()
    {
        Debug.Log("GAME END TRIGGERED");
        Debug.Log($"Boxes Filled: {AllBoxesFilled()} Wrong Guess: {wrongGuessCount} Max Wrong {maxWrongGuesses} Time Left: {timeLeft}");
        gameIsOver = true;
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
        spawnRoutine = null;
        clearLeftoverLetters();

        // Log score to StatsManager using a consistent key
        StatsManager.Set_Numbered_Stat("StudyGameScore", score);

        // Branch by context
        if (VNSceneManager.scene_manager != null)
        {
            VNSceneManager.scene_manager.Show_UI(true);
            switch (currentMode)
            {
                case GameMode.Exam:
                    VNSceneManager.scene_manager.Start_Conversation(endExamConversation);

                    break;
                    case GameMode.Group:
                    VNSceneManager.scene_manager.Start_Conversation(conversationManager);
                    break;
            }
        }
        else
        {
            targetDefinitionText.text = $"Studied {score} word{(score == 1 ? "" : "s")}.";
            yield return new WaitForSeconds(5f);
        }

        studyGameParent.SetActive(false);
    }

    public void BackToHome()
    {
        //TODO: Adjust Player Stats
        SceneManager.LoadScene("Home");
    }

    /// <summary>
    /// Destroys any leftover letters still on the screen.
    /// </summary>
    private void clearLeftoverLetters() {
        var leftoverLetters = GameObject.FindGameObjectsWithTag("Letter");
        foreach (var letter in leftoverLetters) {
            Destroy(letter);
        }

        foreach (var box in boxVisuals)
        {
            Destroy(box);
        }
        Destroy(eraser);
    }

    private void UpdateTimerUI() {
        if (timerText != null) {
            int minutes = Mathf.FloorToInt(timeLeft / 60f);
            int seconds = Mathf.FloorToInt(timeLeft % 60f);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
    }
    public void SetMode(GameMode mode)
    {
        currentMode = mode;

        switch (mode)
        {
            case GameMode.Solo:
                spawnMode = SpawnMode.Random;
                useTimer = true;
                break;

            case GameMode.Group:
                GroupStudyManager groupStudyManager = FindObjectOfType<GroupStudyManager>();
                conversationManager = groupStudyManager.conversationManager;
                spawnMode = SpawnMode.Sequential;
                useTimer = false;
                break;

            case GameMode.Exam:
                spawnMode = SpawnMode.Random;
                useTimer = false; // or true, depending on your exam design
                break;
        }

        timerText.gameObject.SetActive(useTimer);
    }

}
