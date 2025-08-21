using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VNEngine;

[System.Serializable]
public class QuestionSet
{
    public string id;
    public string name;
    public string description;
    public string difficulty;
    public List<QuestionWeek> weeks;
}

[System.Serializable]
public class QuestionSetCollection
{
    public List<QuestionSet> sets;
}

/// <summary>
/// Enhanced StudyQuestionLoader that supports multiple question sets and A/B testing
/// </summary>
public class EnhancedStudyQuestionLoader : MonoBehaviour
{
    [Header("Question Set Configuration")]
    public TextAsset[] questionSetFiles; // Array of different question JSON files
    public bool enableABTesting = true;
    public string forceQuestionSetId = ""; // For debugging - force specific set
    
    [Header("Current Session")]
    public GameMode currentMode = GameMode.Solo;
    public List<QuestionAnswerPair> currentQuestions;
    public bool useDefinitions = true;
    
    private QuestionAnswerPair lastQuestion = null;
    private string currentQuestionSetId;
    private string currentABTestGroup;
    private List<QuestionSetCollection> allQuestionSets = new List<QuestionSetCollection>();
    
    void Awake()
    {
        Debug.Log("=== ENHANCED STUDY QUESTION LOADER AWAKE ===");
    }
    
    void Start()
    {
        Debug.Log("=== ENHANCED STUDY QUESTION LOADER START ===");
        LoadAllQuestionSets();
        DetermineABTestGroup();
        Debug.Log($"Enhanced Study Question Loader initialized. A/B Testing: {enableABTesting}");
    }
    
    /// <summary>
    /// Loads all question set files into memory
    /// </summary>
    private void LoadAllQuestionSets()
    {
        // If no files assigned, try to load from Resources
        if (questionSetFiles == null || questionSetFiles.Length == 0)
        {
            Debug.LogWarning("No question set files assigned. Attempting to load from Resources...");
            
            // Try to load the expanded question sets
            TextAsset expandedSets = Resources.Load<TextAsset>("Data/expanded_question_sets");
            if (expandedSets != null)
            {
                try
                {
                    var collection = JsonUtility.FromJson<QuestionSetCollection>(expandedSets.text);
                    if (collection != null && collection.sets != null)
                    {
                        allQuestionSets.Add(collection);
                        Debug.Log($"Auto-loaded expanded question sets: {collection.sets.Count} sets");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to auto-load expanded question sets: {e.Message}");
                }
            }
            return;
        }
        
        foreach (var file in questionSetFiles)
        {
            if (file != null)
            {
                try
                {
                    var collection = JsonUtility.FromJson<QuestionSetCollection>(file.text);
                    if (collection != null && collection.sets != null)
                    {
                        allQuestionSets.Add(collection);
                        Debug.Log($"Loaded question set file: {file.name} with {collection.sets.Count} sets");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load question set {file.name}: {e.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// Determines which A/B test group the player belongs to
    /// </summary>
    private void DetermineABTestGroup()
    {
        if (!enableABTesting)
        {
            currentABTestGroup = "control";
            return;
        }
        
        // Use player ID or session ID to consistently assign test groups
        string playerId = SystemInfo.deviceUniqueIdentifier;
        int hash = playerId.GetHashCode();
        int groupIndex = Mathf.Abs(hash) % 2; // 2 groups: control vs experimental
        
        switch (groupIndex)
        {
            case 0: currentABTestGroup = "control"; break;
            case 1: currentABTestGroup = "experimental"; break;
        }
        
        Debug.Log($"Player assigned to A/B test group: {currentABTestGroup}");
        
        // Log the A/B test assignment
        if (StudyGameLogger.Instance != null)
        {
            StudyGameLogger.Instance.LogABTestData(currentABTestGroup, currentQuestionSetId, 0f, 0f);
        }
    }
    
    /// <summary>
    /// Selects appropriate question set based on A/B testing group and current week
    /// </summary>
    public void LoadQuestionsForCurrentWeek()
    {
        int week = 1;
        if (StatsManager.Numbered_Stat_Exists("current_week"))
        {
            week = Mathf.Clamp((int)StatsManager.Get_Numbered_Stat("current_week"), 1, 99);
        }
        
        QuestionSet selectedSet = SelectQuestionSetForABTest();
        if (selectedSet == null)
        {
            Debug.LogError("No question set available for current A/B test group");
            return;
        }
        
        currentQuestionSetId = selectedSet.id;
        
        // Find questions for current week
        var weekData = selectedSet.weeks.Find(w => w.week == week.ToString());
        if (weekData != null)
        {
            currentQuestions = new List<QuestionAnswerPair>(weekData.questions);
            Debug.Log($"Loaded {currentQuestions.Count} questions for week {week} from set '{selectedSet.name}'");
        }
        else
        {
            Debug.LogWarning($"No questions found for week {week} in set '{selectedSet.name}'. Using week 1.");
            weekData = selectedSet.weeks.Find(w => w.week == "1");
            currentQuestions = weekData?.questions ?? new List<QuestionAnswerPair>();
        }
        
        // Log the question set selection
        if (StudyGameLogger.Instance != null)
        {
            StudyGameLogger.Instance.LogStudySessionStart(currentMode, currentQuestionSetId, week);
        }
    }
    
    /// <summary>
    /// Selects question set based on A/B testing group
    /// </summary>
    private QuestionSet SelectQuestionSetForABTest()
    {
        // If forcing a specific set (for debugging)
        if (!string.IsNullOrEmpty(forceQuestionSetId))
        {
            return FindQuestionSetById(forceQuestionSetId);
        }
        
        // Select based on A/B test group
        switch (currentABTestGroup)
        {
            case "control":
                // Use original TKAM questions - fall back to original loader if needed
                var tkamSet = FindQuestionSetById("tkam_themes");
                if (tkamSet == null)
                {
                    // Create a set from the original tkam_minigame_questions.json format
                    return CreateTKAMSetFromOriginal();
                }
                return tkamSet;
                
            case "experimental":
                // Use mixed campus + academic questions (randomly pick from expanded sets)
                var expandedSets = new[] { "campus_social_issues", "academic_success" };
                string randomSetId = expandedSets[Random.Range(0, expandedSets.Length)];
                return FindQuestionSetById(randomSetId);
                
            default:
                return FindQuestionSetById("tkam_themes") ?? CreateTKAMSetFromOriginal();
        }
    }
    
    /// <summary>
    /// Creates a QuestionSet from the original tkam_minigame_questions.json format
    /// </summary>
    private QuestionSet CreateTKAMSetFromOriginal()
    {
        // Load the original file
        TextAsset originalTKAM = Resources.Load<TextAsset>("Data/tkam_minigame_questions");
        if (originalTKAM == null) return null;
        
        try
        {
            var originalData = JsonUtility.FromJson<QuestionWeekList>(originalTKAM.text);
            
            // Convert to new format
            var tkamSet = new QuestionSet
            {
                id = "tkam_original",
                name = "To Kill a Mockingbird - Original",
                description = "Original TKAM questions from the base game",
                difficulty = "medium",
                weeks = originalData.weeks
            };
            
            return tkamSet;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load original TKAM questions: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Finds a question set by its ID across all loaded collections
    /// </summary>
    private QuestionSet FindQuestionSetById(string id)
    {
        foreach (var collection in allQuestionSets)
        {
            var set = collection.sets.Find(s => s.id == id);
            if (set != null) return set;
        }
        return null;
    }
    
    /// <summary>
    /// Gets display text for a question based on settings
    /// </summary>
    public string GetDisplayPrompt(QuestionAnswerPair pair, bool definitions)
    {
        if (definitions && !string.IsNullOrEmpty(pair.definition))
        {
            return pair.definition;
        }
        return pair.question;
    }
    
    /// <summary>
    /// Gets a random unused question from current set
    /// </summary>
    public QuestionAnswerPair GetRandomQuestion()
    {
        if (currentQuestions == null || currentQuestions.Count == 0)
        {
            Debug.LogWarning("No questions loaded for current week.");
            return null;
        }
        
        // Filter for unused questions
        var unusedQuestions = currentQuestions.FindAll(q => !q.alreadyUsed);
        
        if (unusedQuestions.Count == 0)
        {
            Debug.Log("All questions used — resetting all.");
            foreach (var q in currentQuestions)
                q.alreadyUsed = false;
            unusedQuestions = new List<QuestionAnswerPair>(currentQuestions);
        }
        
        QuestionAnswerPair selected;
        int safety = 0;
        do
        {
            selected = unusedQuestions[Random.Range(0, unusedQuestions.Count)];
            safety++;
        }
        while (selected == lastQuestion && unusedQuestions.Count > 1 && safety < 10);
        
        selected.alreadyUsed = true;
        lastQuestion = selected;
        return selected;
    }
    
    /// <summary>
    /// Gets information about current question set for analytics
    /// </summary>
    public string GetCurrentQuestionSetInfo()
    {
        return $"Set: {currentQuestionSetId}, A/B Group: {currentABTestGroup}";
    }
    
    /// <summary>
    /// Gets all available question sets for debugging/admin purposes
    /// </summary>
    public List<string> GetAvailableQuestionSetIds()
    {
        var ids = new List<string>();
        foreach (var collection in allQuestionSets)
        {
            ids.AddRange(collection.sets.Select(s => s.id));
        }
        return ids;
    }
}
