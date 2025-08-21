using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VNEngine;

/// <summary>
/// Comprehensive testing script for the Study Game A/B testing implementation
/// </summary>
public class StudyGameTester : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestsOnStart = false;
    public bool logDetailedOutput = true;
    
    private void Start()
    {
        if (runTestsOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }
    
    [ContextMenu("Run All Tests")]
    public void RunAllTestsFromMenu()
    {
        StartCoroutine(RunAllTests());
    }
    
    private IEnumerator RunAllTests()
    {
        Debug.Log("=== STARTING COMPREHENSIVE STUDY GAME TESTS ===");
        
        yield return StartCoroutine(TestQuestionSetLoading());
        yield return StartCoroutine(TestABTestingAssignment());
        yield return StartCoroutine(TestAnswerLengths());
        yield return StartCoroutine(TestOGDLogging());
        yield return StartCoroutine(TestForceQuestionSets());
        
        Debug.Log("=== ALL TESTS COMPLETED ===");
    }
    
    private IEnumerator TestQuestionSetLoading()
    {
        Debug.Log("--- Testing Question Set Loading ---");
        
        // Find the EnhancedStudyQuestionLoader
        EnhancedStudyQuestionLoader loader = FindObjectOfType<EnhancedStudyQuestionLoader>();
        if (loader == null)
        {
            Debug.LogWarning("⚠️ EnhancedStudyQuestionLoader not found in scene! Make sure you have the enhanced study game components set up.");
            Debug.LogWarning("   You can still test JSON files and answer lengths without the enhanced loader.");
            yield break;
        }
        
        Debug.Log("✅ EnhancedStudyQuestionLoader found");
        
        // Test loading questions
        loader.LoadQuestionsForCurrentWeek();
        yield return new WaitForSeconds(0.1f); // Give it time to load
        
        if (loader.currentQuestions != null && loader.currentQuestions.Count > 0)
        {
            Debug.Log($"✅ Questions loaded successfully: {loader.currentQuestions.Count} questions");
            if (logDetailedOutput)
            {
                foreach (var question in loader.currentQuestions)
                {
                    Debug.Log($"   Question: '{question.question}' -> Answer: '{question.answer}' ({question.answer.Length} letters)");
                }
            }
        }
        else
        {
            Debug.LogError("❌ No questions were loaded!");
        }
        
        yield return null;
    }
    
    private IEnumerator TestABTestingAssignment()
    {
        Debug.Log("--- Testing A/B Testing Assignment ---");
        
        EnhancedStudyQuestionLoader loader = FindObjectOfType<EnhancedStudyQuestionLoader>();
        if (loader == null)
        {
            Debug.LogError("❌ EnhancedStudyQuestionLoader not found!");
            yield break;
        }
        
        // Test multiple assignments with different "device IDs"
        string originalDeviceId = SystemInfo.deviceUniqueIdentifier;
        
        Dictionary<string, int> groupCounts = new Dictionary<string, int>
        {
            {"control", 0},
            {"experimental", 0}
        };
        
        // Simulate multiple device assignments
        for (int i = 0; i < 10; i++)
        {
            // We can't actually change SystemInfo.deviceUniqueIdentifier, but we can test the hash logic
            string testId = $"test_device_{i}";
            int hash = testId.GetHashCode();
            int groupIndex = Mathf.Abs(hash) % 2;
            string group = groupIndex == 0 ? "control" : "experimental";
            groupCounts[group]++;
        }
        
        Debug.Log($"✅ A/B Test Distribution (simulated): Control: {groupCounts["control"]}/10, Experimental: {groupCounts["experimental"]}/10");
        
        // Test actual assignment
        loader.LoadQuestionsForCurrentWeek();
        yield return new WaitForSeconds(0.1f);
        
        string questionSetInfo = loader.GetCurrentQuestionSetInfo();
        Debug.Log($"✅ Current assignment: {questionSetInfo}");
        
        yield return null;
    }
    
    private IEnumerator TestAnswerLengths()
    {
        Debug.Log("--- Testing Answer Lengths (Must be exactly 5 letters) ---");
        
        // Test expanded question sets
        TextAsset expandedSets = Resources.Load<TextAsset>("Data/expanded_question_sets");
        if (expandedSets != null)
        {
            var questionData = JsonUtility.FromJson<QuestionSetCollection>(expandedSets.text);
            bool allCorrectLength = true;
            
            foreach (var set in questionData.sets)
            {
                Debug.Log($"Testing question set: {set.name}");
                foreach (var week in set.weeks)
                {
                    foreach (var question in week.questions)
                    {
                        if (question.answer.Length != 5)
                        {
                            Debug.LogError($"❌ Answer length incorrect: '{question.answer}' ({question.answer.Length} letters) in {set.name}");
                            allCorrectLength = false;
                        }
                        else if (logDetailedOutput)
                        {
                            Debug.Log($"   ✅ '{question.answer}' (5 letters) - {question.question}");
                        }
                    }
                }
            }
            
            if (allCorrectLength)
            {
                Debug.Log("✅ All answers in expanded_question_sets.json are exactly 5 letters!");
            }
        }
        else
        {
            Debug.LogError("❌ Could not load expanded_question_sets.json");
        }
        
        // Test original TKAM questions
        TextAsset tkamQuestions = Resources.Load<TextAsset>("Data/tkam_minigame_questions");
        if (tkamQuestions != null)
        {
            var tkamData = JsonUtility.FromJson<QuestionWeekList>(tkamQuestions.text);
            bool allCorrectLength = true;
            
            Debug.Log("Testing original TKAM questions:");
            foreach (var week in tkamData.weeks)
            {
                foreach (var question in week.questions)
                {
                    if (question.answer.Length != 5)
                    {
                        Debug.LogError($"❌ TKAM Answer length incorrect: '{question.answer}' ({question.answer.Length} letters)");
                        allCorrectLength = false;
                    }
                    else if (logDetailedOutput)
                    {
                        Debug.Log($"   ✅ '{question.answer}' (5 letters) - {question.question}");
                    }
                }
            }
            
            if (allCorrectLength)
            {
                Debug.Log("✅ All answers in tkam_minigame_questions.json are exactly 5 letters!");
            }
        }
        else
        {
            Debug.LogError("❌ Could not load tkam_minigame_questions.json");
        }
        
        yield return null;
    }
    
    private IEnumerator TestOGDLogging()
    {
        Debug.Log("--- Testing OGD Logging ---");
        
        // Find the EnhancedStudyQuestionLoader for testing
        EnhancedStudyQuestionLoader loader = FindObjectOfType<EnhancedStudyQuestionLoader>();
        
        // Test StudyGameLogger
        if (StudyGameLogger.Instance != null)
        {
            Debug.Log("✅ StudyGameLogger instance found");
            
            // Test logging functionality
            StudyGameLogger.Instance.LogStudySessionStart(GameMode.Solo, "campus_social_issues", 1);
            yield return new WaitForSeconds(0.1f);
            
            if (loader != null && loader.currentQuestions != null && loader.currentQuestions.Count > 0)
            {
                var testQuestion = loader.currentQuestions[0];
                StudyGameLogger.Instance.LogQuestionAttempt(testQuestion, "being", true, 2.5f, 1);
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                Debug.LogWarning("⚠️ No question loader found, testing logging with minimal data");
            }
            
            StudyGameLogger.Instance.LogStudySessionEnd(1, "test_completed", 100.0f);
            yield return new WaitForSeconds(0.1f);
            
            Debug.Log("✅ OGD logging test completed (check console for OGD events if debug mode enabled)");
        }
        else
        {
            Debug.LogWarning("⚠️ StudyGameLogger instance not found - create one to test OGD logging");
        }
        
        // Test main logging system
        if (Logging.Instance != null)
        {
            Debug.Log("✅ Main Logging instance found");
            if (Logging.Instance.Logger != null)
            {
                Debug.Log("✅ OGD Logger is accessible from main Logging system");
            }
            else
            {
                Debug.LogWarning("⚠️ OGD Logger is null in main Logging system");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Main Logging instance not found");
        }
        
        yield return null;
    }
    
    private IEnumerator TestForceQuestionSets()
    {
        Debug.Log("--- Testing Force Question Set Feature ---");
        
        EnhancedStudyQuestionLoader loader = FindObjectOfType<EnhancedStudyQuestionLoader>();
        if (loader == null)
        {
            Debug.LogError("❌ EnhancedStudyQuestionLoader not found!");
            yield break;
        }
        
        string[] testSets = { "tkam_themes", "campus_social_issues", "academic_success" };
        
        foreach (string setId in testSets)
        {
            Debug.Log($"Testing force question set: {setId}");
            
            // Set the force ID
            loader.forceQuestionSetId = setId;
            loader.LoadQuestionsForCurrentWeek();
            yield return new WaitForSeconds(0.1f);
            
            if (loader.currentQuestions != null && loader.currentQuestions.Count > 0)
            {
                Debug.Log($"✅ Successfully loaded {loader.currentQuestions.Count} questions for {setId}");
                Debug.Log($"   Info: {loader.GetCurrentQuestionSetInfo()}");
                
                if (logDetailedOutput && loader.currentQuestions.Count > 0)
                {
                    var firstQuestion = loader.currentQuestions[0];
                    Debug.Log($"   Sample question: '{firstQuestion.question}' -> '{firstQuestion.answer}'");
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to load questions for {setId}");
            }
        }
        
        // Reset force ID
        loader.forceQuestionSetId = "";
        Debug.Log("✅ Force question set testing completed, reset to automatic assignment");
        
        yield return null;
    }
}
