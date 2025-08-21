using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using VNEngine;

/// <summary>
/// Enhanced FivePositionsGameManager with integrated OGD logging and A/B testing support
/// </summary>
public class EnhancedFivePositionsGameManager : MonoBehaviour
{
    [Header("Game Configuration")]
    public GameMode currentMode;
    public bool useTimer;
    public enum SpawnMode { Sequential, Random }
    public SpawnMode spawnMode;
    public int maxWrongGuesses = 3;

    [Header("Question System")]
    public EnhancedStudyQuestionLoader enhancedQuestionLoader;
    
    [Header("UI References")]
    public RectTransform[] boxPositions = new RectTransform[5];
    public TextMeshProUGUI[] boxLetterDisplays = new TextMeshProUGUI[5];
    public TextMeshProUGUI targetDefinitionText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI penaltyText;
    public GameObject studyGameParent;
    public GameObject gameStuff;
    public GameObject timers;
    
    [Header("Game Assets")]
    public GameObject letterPrefab;
    public AudioClip correctClip;
    public AudioClip incorrectClip;
    public GameObject boxSpritePrefab;
    public GameObject eraserPrefab;
    
    [Header("Timing")]
    public float gameDuration = 180f;
    public float penaltyTime = 5f;
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 2f;
    [Range(0f, 1f)] public float chanceOfCorrectLetter = 0.5f;
    
    // Game State
    private string targetWord;
    private char[] targetLetters = new char[5];
    private bool[] boxFilled = new bool[5];
    private int score = 0;
    private float timeLeft;
    private bool gameIsOver = false;
    private bool isTimerPaused = false;
    private Coroutine spawnRoutine;
    public int wrongGuessCount = 0;
    
    // Analytics tracking
    private float questionStartTime;
    private int currentQuestionAttempts = 0;
    private QuestionAnswerPair currentQuestion;
    private List<GameObject> boxVisuals = new List<GameObject>();
    private List<int> activeColumns = new List<int>();
    private ConversationManager conversationManager;
    private ConversationManager endExamConversation;
    
    public void Initialize()
    {
        SetMode(currentMode);
        SpawnVisuals();
        StartCoroutine(DelayedGameStart());
        
        // Initialize logging
        if (StudyGameLogger.Instance == null)
        {
            GameObject loggerObj = new GameObject("StudyGameLogger");
            loggerObj.AddComponent<StudyGameLogger>();
        }
    }
    
    private void SpawnVisuals()
    {
        // Spawn box visuals
        for (int i = 0; i < boxPositions.Length; i++)
        {
            if (boxSpritePrefab != null)
            {
                GameObject box = Instantiate(boxSpritePrefab, boxPositions[i].position, Quaternion.identity);
                box.transform.SetParent(boxPositions[i]);
                boxVisuals.Add(box);
            }
        }
        
        // Spawn eraser if needed
        if (eraserPrefab != null)
        {
            // Position eraser appropriately
        }
    }
    
    private IEnumerator DelayedGameStart()
    {
        yield return new WaitForSeconds(1f);
        StartGame();
    }
    
    public void StartGame()
    {
        // Initialize UI
        UpdateScoreUI();
        if (penaltyText != null) penaltyText.gameObject.SetActive(false);
        if (gameStuff != null) gameStuff.SetActive(true);
        
        // Initialize timer
        timeLeft = gameDuration;
        UpdateTimerUI();
        isTimerPaused = true;
        gameIsOver = false;
        
        // Load questions
        enhancedQuestionLoader.LoadQuestionsForCurrentWeek();
        
        // Start game systems
        StartCoroutine(GameTimerCoroutine());
        StartCoroutine(CountdownCoroutine());
        
        Debug.Log($"Study game started. Question set info: {enhancedQuestionLoader.GetCurrentQuestionSetInfo()}");
    }
    
    private IEnumerator CountdownCoroutine(bool isFirstCountdown = true)
    {
        // Show countdown: 3, 2, 1, GO!
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                countdownText.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(1f);
        }
        
        if (countdownText != null)
        {
            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
            countdownText.gameObject.SetActive(false);
        }
        
        // Unpause the timer and start the round
        isTimerPaused = false;
        if (isFirstCountdown)
        {
            StartNewRound();
            StartLetterSpawning();
        }
    }
    
    private void StartNewRound()
    {
        currentQuestion = enhancedQuestionLoader.GetRandomQuestion();
        if (currentQuestion != null)
        {
            targetWord = currentQuestion.answer.ToLower();
            if (targetDefinitionText != null)
            {
                targetDefinitionText.text = enhancedQuestionLoader.GetDisplayPrompt(currentQuestion, enhancedQuestionLoader.useDefinitions);
            }
            
            // Reset tracking
            questionStartTime = Time.time;
            currentQuestionAttempts = 0;
            
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
            Debug.LogError("No question available for current round");
        }
    }
    
    private void StartLetterSpawning()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnLettersRoutine());
    }
    
    private IEnumerator SpawnLettersRoutine()
    {
        while (!gameIsOver)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
            
            if (!gameIsOver && !isTimerPaused)
            {
                SpawnLetter();
            }
        }
    }
    
    private void SpawnLetter()
    {
        // Choose which letter to spawn based on difficulty and chance settings
        char letterToSpawn;
        bool isCorrectLetter = Random.value < chanceOfCorrectLetter;
        
        if (isCorrectLetter)
        {
            // Spawn a letter that belongs in the word
            List<int> emptyPositions = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                if (!boxFilled[i])
                    emptyPositions.Add(i);
            }
            
            if (emptyPositions.Count > 0)
            {
                int randomPos = emptyPositions[Random.Range(0, emptyPositions.Count)];
                letterToSpawn = targetLetters[randomPos];
            }
            else
            {
                letterToSpawn = targetLetters[Random.Range(0, 5)]; // Fallback
            }
        }
        else
        {
            // Spawn a random incorrect letter
            letterToSpawn = (char)Random.Range('a', 'z' + 1);
        }
        
        // Create letter object
        if (letterPrefab != null)
        {
            GameObject letter = Instantiate(letterPrefab);
            // Set up letter with the chosen character
            var letterComponent = letter.GetComponent<TextMeshProUGUI>();
            if (letterComponent != null)
            {
                letterComponent.text = letterToSpawn.ToString().ToUpper();
            }
            
            // Set up letter movement/physics
            // (This would need to be implemented based on your letter movement system)
        }
    }
    
    public void OnLetterArrived(int boxIndex, char arrivedLetter, GameObject letterObj)
    {
        currentQuestionAttempts++;
        bool isCorrect = char.ToLower(arrivedLetter) == targetLetters[boxIndex];
        
        if (isCorrect)
        {
            // Correct letter placement
            boxFilled[boxIndex] = true;
            if (boxLetterDisplays[boxIndex] != null)
            {
                boxLetterDisplays[boxIndex].text = arrivedLetter.ToString().ToUpper();
            }
            
            // Play correct sound
            if (correctClip != null)
            {
                AudioSource.PlayClipAtPoint(correctClip, transform.position);
            }
        }
        else
        {
            // Incorrect letter placement
            wrongGuessCount++;
            
            // Apply penalty
            if (useTimer)
            {
                timeLeft -= penaltyTime;
                if (timeLeft < 0) timeLeft = 0;
                UpdateTimerUI();
                
                // Show penalty text
                if (penaltyText != null)
                {
                    penaltyText.gameObject.SetActive(true);
                    penaltyText.text = $"-0:{(int)penaltyTime:00}";
                    StartCoroutine(HidePenaltyText());
                }
            }
            
            // Play incorrect sound
            if (incorrectClip != null)
            {
                AudioSource.PlayClipAtPoint(incorrectClip, transform.position);
            }
        }
        
        // Log the attempt
        if (StudyGameLogger.Instance != null)
        {
            float timeToAnswer = Time.time - questionStartTime;
            string playerAnswer = new string(GetCurrentPlayerAnswer());
            StudyGameLogger.Instance.LogQuestionAttempt(currentQuestion, playerAnswer, isCorrect, timeToAnswer, currentQuestionAttempts);
        }
        
        // Clean up letter object
        Destroy(letterObj);
        
        // Check if word is complete
        if (AllBoxesFilled())
        {
            score++;
            UpdateScoreUI();
            StartCoroutine(RestartGameRoutine());
        }
        
        // Check end conditions
        if ((useTimer && timeLeft <= 0) || wrongGuessCount >= maxWrongGuesses)
        {
            StartCoroutine(EndGame());
        }
    }
    
    private char[] GetCurrentPlayerAnswer()
    {
        char[] answer = new char[5];
        for (int i = 0; i < 5; i++)
        {
            if (boxFilled[i] && boxLetterDisplays[i] != null)
            {
                answer[i] = char.ToLower(boxLetterDisplays[i].text[0]);
            }
            else
            {
                answer[i] = '_';
            }
        }
        return answer;
    }
    
    private bool AllBoxesFilled()
    {
        for (int i = 0; i < 5; i++)
        {
            if (!boxFilled[i]) return false;
        }
        return true;
    }
    
    private IEnumerator HidePenaltyText()
    {
        yield return new WaitForSeconds(2f);
        if (penaltyText != null)
        {
            penaltyText.gameObject.SetActive(false);
        }
    }
    
    private IEnumerator RestartGameRoutine()
    {
        yield return new WaitForSeconds(1f);
        
        // Pause timer for transition
        isTimerPaused = true;
        ClearAllBoxes();
        
        // Start new round
        StartNewRound();
        
        // Quick countdown and resume
        StartCoroutine(CountdownCoroutine(false));
    }
    
    private void ClearAllBoxes()
    {
        for (int i = 0; i < 5; i++)
        {
            boxFilled[i] = false;
            if (boxLetterDisplays[i] != null)
            {
                boxLetterDisplays[i].text = " ";
            }
        }
    }
    
    private IEnumerator GameTimerCoroutine()
    {
        while (timeLeft > 0 && !gameIsOver)
        {
            yield return null;
            
            if (useTimer && !isTimerPaused)
            {
                timeLeft -= Time.deltaTime;
                UpdateTimerUI();
                
                if (timeLeft <= 0)
                {
                    timeLeft = 0;
                    UpdateTimerUI();
                    StartCoroutine(EndGame());
                }
            }
            else if (!useTimer)
            {
                // Check other end conditions for non-timer modes
                if (currentMode == GameMode.Group || currentMode == GameMode.Exam)
                {
                    if (wrongGuessCount >= maxWrongGuesses)
                    {
                        StartCoroutine(EndGame());
                    }
                }
            }
        }
    }
    
    private IEnumerator EndGame()
    {
        gameIsOver = true;
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
        
        // Determine end reason
        string endReason = "completed";
        if (timeLeft <= 0) endReason = "timeout";
        if (wrongGuessCount >= maxWrongGuesses) endReason = "max_wrong_guesses";
        
        // Log session end
        if (StudyGameLogger.Instance != null)
        {
            float totalTime = gameDuration - timeLeft;
            StudyGameLogger.Instance.LogStudySessionEnd(score, endReason, totalTime);
        }
        
        // Save score to stats
        StatsManager.Set_Numbered_Stat("StudyGameScore", score);
        
        // Continue to appropriate next scene/conversation
        if (VNSceneManager.scene_manager != null)
        {
            VNSceneManager.scene_manager.Show_UI(true);
            switch (currentMode)
            {
                case GameMode.Exam:
                    if (endExamConversation != null)
                        VNSceneManager.scene_manager.Start_Conversation(endExamConversation);
                    break;
                case GameMode.Group:
                    if (conversationManager != null)
                        VNSceneManager.scene_manager.Start_Conversation(conversationManager);
                    break;
            }
        }
        else
        {
            targetDefinitionText.text = $"Studied {score} word{(score == 1 ? "" : "s")}.";
            yield return new WaitForSeconds(5f);
        }
        
        // Reset questions for next session
        if (enhancedQuestionLoader != null && enhancedQuestionLoader.currentQuestions != null)
        {
            foreach (var q in enhancedQuestionLoader.currentQuestions)
                q.alreadyUsed = false;
        }
        
        studyGameParent.SetActive(false);
    }
    
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    private void UpdateTimerUI()
    {
        if (timerText != null && useTimer)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60f);
            int seconds = Mathf.FloorToInt(timeLeft % 60f);
            timerText.text = $"{minutes:0}:{seconds:00}";
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
                if (groupStudyManager != null)
                    conversationManager = groupStudyManager.conversationManager;
                spawnMode = SpawnMode.Sequential;
                useTimer = false;
                break;
            case GameMode.Exam:
                spawnMode = SpawnMode.Random;
                useTimer = false;
                break;
        }
        
        if (timerText != null)
            timerText.gameObject.SetActive(useTimer);
    }
    
    public void ConfigureChallenge(ChallengeProfile profile)
    {
        if (profile != null)
        {
            timeLeft = profile.timerDuration;
            minSpawnInterval = profile.minSpawnInterval;
            maxSpawnInterval = profile.maxSpawnInterval;
            chanceOfCorrectLetter = profile.chanceOfCorrectLetter;
            
            if (timers != null)
                timers.SetActive(profile.showTimer);
            
            // Configure question type
            bool useDefinitions = profile.promptType == ChallengeProfile.PromptType.Definitions;
            enhancedQuestionLoader.useDefinitions = useDefinitions;
        }
    }
    
    public void BackToHome()
    {
        SceneManager.LoadScene("Home");
    }
}
