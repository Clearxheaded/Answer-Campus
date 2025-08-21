using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OGD;
using VNEngine;

/// <summary>
/// Handles OpenGameData logging specifically for the study mini-game
/// Tracks player performance, question difficulty, and learning patterns
/// </summary>
public class StudyGameLogger : MonoBehaviour
{
    public static StudyGameLogger Instance { get; private set; }
    
    private OGDLog m_Logger;
    private bool isLoggingEnabled = true;
    
    // Session tracking
    private string currentSessionId;
    private int questionsAttempted = 0;
    private int questionsCorrect = 0;
    private float sessionStartTime;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLogger();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeLogger()
    {
        // Use the main Logging instance if available, or create our own
        if (Logging.Instance != null)
        {
            // Use existing logger
            StartCoroutine(WaitForMainLogger());
        }
        else
        {
            Debug.LogWarning("Main Logging instance not found. StudyGame logging disabled.");
            isLoggingEnabled = false;
        }
    }
    
    private IEnumerator WaitForMainLogger()
    {
        yield return new WaitUntil(() => Logging.Instance != null);
        Debug.Log("StudyGameLogger connected to main OGD logger");
    }
    
    /// <summary>
    /// Logs when a study session begins
    /// </summary>
    public void LogStudySessionStart(GameMode mode, string questionSet, int currentWeek)
    {
        if (!IsReady()) return;
        
        currentSessionId = System.Guid.NewGuid().ToString();
        questionsAttempted = 0;
        questionsCorrect = 0;
        sessionStartTime = Time.time;
        
        var logger = GetLogger();
        if (logger != null)
        {
            logger.BeginEvent("study_session_start");
            logger.EventParam("session_id", currentSessionId);
            logger.EventParam("game_mode", mode.ToString());
            logger.EventParam("question_set", questionSet);
            logger.EventParam("current_week", currentWeek);
            logger.EventParam("player_level", StatsManager.Get_Numbered_Stat("PlayerLevel"));
            logger.EventParam("timestamp", System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            logger.SubmitEvent();
        }
    }
    
    /// <summary>
    /// Logs each question attempt with detailed metrics
    /// </summary>
    public void LogQuestionAttempt(QuestionAnswerPair question, string playerAnswer, bool isCorrect, float timeToAnswer, int attempts)
    {
        if (!IsReady()) return;
        
        questionsAttempted++;
        if (isCorrect) questionsCorrect++;
        
        var logger = GetLogger();
        if (logger != null)
        {
            logger.BeginEvent("study_question_attempt");
            logger.EventParam("session_id", currentSessionId);
            logger.EventParam("question_id", question.question.GetHashCode().ToString());
            logger.EventParam("question_text", question.question);
            logger.EventParam("correct_answer", question.answer);
            logger.EventParam("player_answer", playerAnswer);
            logger.EventParam("is_correct", isCorrect);
            logger.EventParam("time_to_answer", timeToAnswer);
            logger.EventParam("attempts_on_question", attempts);
            logger.EventParam("question_difficulty", CalculateQuestionDifficulty(question));
            logger.EventParam("session_questions_attempted", questionsAttempted);
            logger.EventParam("session_accuracy", (float)questionsCorrect / questionsAttempted);
            logger.SubmitEvent();
        }
    }
    
    /// <summary>
    /// Logs when a study session ends with summary statistics
    /// </summary>
    public void LogStudySessionEnd(int finalScore, string endReason, float totalTime)
    {
        if (!IsReady()) return;
        
        var logger = GetLogger();
        if (logger != null)
        {
            logger.BeginEvent("study_session_end");
            logger.EventParam("session_id", currentSessionId);
            logger.EventParam("final_score", finalScore);
            logger.EventParam("questions_attempted", questionsAttempted);
            logger.EventParam("questions_correct", questionsCorrect);
            logger.EventParam("accuracy_rate", questionsAttempted > 0 ? (float)questionsCorrect / questionsAttempted : 0f);
            logger.EventParam("total_time", totalTime);
            logger.EventParam("end_reason", endReason); // "completed", "timeout", "quit"
            logger.EventParam("timestamp", System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            logger.SubmitEvent();
        }
    }
    
    /// <summary>
    /// Logs learning progression and mastery indicators
    /// </summary>
    public void LogLearningProgress(string topic, float masteryLevel, int totalAttempts, float averageTime)
    {
        if (!IsReady()) return;
        
        var logger = GetLogger();
        if (logger != null)
        {
            logger.BeginEvent("learning_progress");
            logger.EventParam("session_id", currentSessionId);
            logger.EventParam("topic", topic);
            logger.EventParam("mastery_level", masteryLevel);
            logger.EventParam("total_attempts", totalAttempts);
            logger.EventParam("average_response_time", averageTime);
            logger.EventParam("current_week", StatsManager.Get_Numbered_Stat("current_week"));
            logger.SubmitEvent();
        }
    }
    
    /// <summary>
    /// Logs A/B testing data for different question sets
    /// </summary>
    public void LogABTestData(string testGroup, string questionSetVersion, float engagementScore, float learningOutcome)
    {
        if (!IsReady()) return;
        
        var logger = GetLogger();
        if (logger != null)
        {
            logger.BeginEvent("ab_test_data");
            logger.EventParam("session_id", currentSessionId);
            logger.EventParam("test_group", testGroup); // "control", "experimental_a", "experimental_b"
            logger.EventParam("question_set_version", questionSetVersion);
            logger.EventParam("engagement_score", engagementScore);
            logger.EventParam("learning_outcome", learningOutcome);
            logger.EventParam("player_demographic", GetPlayerDemographic());
            logger.SubmitEvent();
        }
    }
    
    private bool IsReady()
    {
        return isLoggingEnabled && Logging.Instance != null;
    }
    
    private OGDLog GetLogger()
    {
        return Logging.Instance?.GetComponent<Logging>()?.GetLogger();
    }
    
    private string CalculateQuestionDifficulty(QuestionAnswerPair question)
    {
        // Simple heuristic for question difficulty based on word length and complexity
        int wordLength = question.answer.Length;
        bool hasComplexConcepts = question.definition.Contains("metaphor") || 
                                question.definition.Contains("symbolism") || 
                                question.definition.Contains("irony");
        
        if (wordLength <= 4 && !hasComplexConcepts) return "easy";
        if (wordLength == 5 && !hasComplexConcepts) return "medium";
        return "hard";
    }
    
    private string GetPlayerDemographic()
    {
        // You can expand this based on player stats or choices made in the game
        int week = (int)StatsManager.Get_Numbered_Stat("current_week");
        if (week <= 2) return "early_game";
        if (week <= 4) return "mid_game";
        return "late_game";
    }
}

// Extension to make the main Logging class accessible
public static class LoggingExtensions
{
    public static OGDLog GetLogger(this Logging logging)
    {
        return logging.Logger;
    }
}
