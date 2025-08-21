using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VNEngine;

/// <summary>
/// Comprehensive test script for the Enhanced Study Game A/B Testing System
/// </summary>
public class TestABStudySystem : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestsOnStart = true;
    public bool detailedLogging = true;
    
    [Header("Components to Test")]
    public EnhancedStudyQuestionLoader questionLoader;
    public StudyGameLogger studyLogger;
    
    void Start()
    {
        if (runTestsOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }
    
    IEnumerator RunAllTests()
    {
        yield return new WaitForSeconds(1f); // Let other components initialize
        
        LogTest("=== STARTING COMPREHENSIVE A/B STUDY SYSTEM TESTS ===");
        
        // Test 1: JSON File Loading
        yield return StartCoroutine(TestJSONFiles());
        
        // Test 2: Question Length Validation
        yield return StartCoroutine(TestAnswerLengths());
        
        // Test 3: A/B Test Assignment
        yield return StartCoroutine(TestABAssignment());
        
        // Test 4: Question Set Selection
        yield return StartCoroutine(TestQuestionSetSelection());
        
        // Test 5: OGD Logging
        yield return StartCoroutine(TestOGDLogging());
        
        // Test 6: Forced Question Sets
        yield return StartCoroutine(TestForcedQuestionSets());
        
        LogTest("=== ALL TESTS COMPLETED ===");
        LogTest("Check console for any errors or warnings.");
    }
    
    IEnumerator TestJSONFiles()
    {
        LogTest("TEST 1: JSON File Loading");
        
        // Test original TKAM file
        TextAsset tkamFile = Resources.Load<TextAsset>("Data/tkam_minigame_questions");
        if (tkamFile == null)
        {
            LogError("❌ TKAM JSON file not found!");
        }
        else
        {
            LogTest("✅ Original TKAM JSON file loaded successfully");
        }
        
        // Test expanded question sets
        TextAsset expandedFile = Resources.Load<TextAsset>("Data/expanded_question_sets");
        if (expandedFile == null)
        {
            LogError("❌ Expanded question sets JSON file not found!");
        }
        else
        {
            try
            {
                var data = JsonUtility.FromJson<QuestionSetCollection>(expandedFile.text);
                LogTest($"✅ Expanded JSON loaded: {data.sets.Count} question sets found");
                
                foreach (var set in data.sets)
                {
                    LogTest($"   - {set.name} ({set.id}): {set.weeks.Count} weeks");
                }
            }
            catch (System.Exception e)
            {
                LogError($"❌ Error parsing expanded JSON: {e.Message}");
            }
        }
        
        yield return null;
    }
    
    IEnumerator TestAnswerLengths()
    {
        LogTest("TEST 2: Answer Length Validation");
        
        TextAsset expandedFile = Resources.Load<TextAsset>("Data/expanded_question_sets");
        if (expandedFile == null)
        {
            LogError("❌ Cannot test answer lengths - file not found");
            yield break;
        }
        
        try
        {
            var data = JsonUtility.FromJson<QuestionSetCollection>(expandedFile.text);
            bool allValid = true;
            int totalQuestions = 0;
            
            foreach (var set in data.sets)
            {
                foreach (var week in set.weeks)
                {
                    foreach (var question in week.questions)
                    {
                        totalQuestions++;
                        if (question.answer.Length != 5)
                        {
                            LogError($"❌ Invalid answer length: '{question.answer}' ({question.answer.Length} letters) in {set.id}");
                            allValid = false;
                        }
                    }
                }
            }
            
            if (allValid)
            {
                LogTest($"✅ All {totalQuestions} answers are exactly 5 letters!");
            }
            else
            {
                LogError("❌ Some answers are not exactly 5 letters");
            }
        }
        catch (System.Exception e)
        {
            LogError($"❌ Error validating answer lengths: {e.Message}");
        }
        
        yield return null;
    }
    
    IEnumerator TestABAssignment()
    {
        LogTest("TEST 3: A/B Test Assignment");
        
        if (questionLoader == null)
        {
            LogError("❌ EnhancedStudyQuestionLoader not assigned!");
            yield break;
        }
        
        // Test multiple device IDs to see distribution
        string[] testDeviceIds = {
            "test-device-1", "test-device-2", "test-device-3", "test-device-4", 
            "test-device-5", "test-device-6", "test-device-7", "test-device-8"
        };
        
        Dictionary<string, int> groupCounts = new Dictionary<string, int>();
        
        foreach (string deviceId in testDeviceIds)
        {
            int hash = deviceId.GetHashCode();
            int groupIndex = Mathf.Abs(hash) % 2;
            string group = groupIndex == 0 ? "control" : "experimental";
            
            if (!groupCounts.ContainsKey(group))
                groupCounts[group] = 0;
            groupCounts[group]++;
            
            LogTest($"   Device '{deviceId}' → {group} group");
        }
        
        LogTest($"✅ A/B Distribution: Control={groupCounts.GetValueOrDefault("control", 0)}, Experimental={groupCounts.GetValueOrDefault("experimental", 0)}");
        
        yield return null;
    }
    
    IEnumerator TestQuestionSetSelection()
    {
        LogTest("TEST 4: Question Set Selection");
        
        if (questionLoader == null)
        {
            LogError("❌ EnhancedStudyQuestionLoader not assigned!");
            yield break;
        }
        
        // Test loading questions
        questionLoader.LoadQuestionsForCurrentWeek();
        
        if (questionLoader.currentQuestions != null && questionLoader.currentQuestions.Count > 0)
        {
            LogTest($"✅ Questions loaded: {questionLoader.currentQuestions.Count} questions");
            LogTest($"   Question set info: {questionLoader.GetCurrentQuestionSetInfo()}");
            
            // Show sample questions
            for (int i = 0; i < Mathf.Min(3, questionLoader.currentQuestions.Count); i++)
            {
                var q = questionLoader.currentQuestions[i];
                LogTest($"   Sample Q{i+1}: '{q.question}' → '{q.answer}' ({q.answer.Length} letters)");
            }
        }
        else
        {
            LogError("❌ No questions loaded!");
        }
        
        yield return null;
    }
    
    IEnumerator TestOGDLogging()
    {
        LogTest("TEST 5: OGD Logging");
        
        if (StudyGameLogger.Instance == null)
        {
            LogError("❌ StudyGameLogger instance not found!");
            yield break;
        }
        
        // Test logging methods
        try
        {
            StudyGameLogger.Instance.LogStudySessionStart(GameMode.Solo, "test-set", 1);
            LogTest("✅ Session start logging successful");
            
            if (questionLoader.currentQuestions != null && questionLoader.currentQuestions.Count > 0)
            {
                var testQuestion = questionLoader.currentQuestions[0];
                StudyGameLogger.Instance.LogQuestionAttempt(testQuestion, "TEST", true, 2.5f, 1);
                LogTest("✅ Question attempt logging successful");
            }
            
            StudyGameLogger.Instance.LogStudySessionEnd(5, "test_complete", 60f);
            LogTest("✅ Session end logging successful");
            
            StudyGameLogger.Instance.LogABTestData("test-group", "test-set", 0.8f, 45f);
            LogTest("✅ A/B test data logging successful");
        }
        catch (System.Exception e)
        {
            LogError($"❌ OGD Logging error: {e.Message}");
        }
        
        yield return null;
    }
    
    IEnumerator TestForcedQuestionSets()
    {
        LogTest("TEST 6: Forced Question Sets");
        
        if (questionLoader == null)
        {
            LogError("❌ EnhancedStudyQuestionLoader not assigned!");
            yield break;
        }
        
        // Test forcing specific sets
        string[] testSets = { "tkam_themes", "campus_social_issues", "academic_success" };
        
        foreach (string setId in testSets)
        {
            questionLoader.forceQuestionSetId = setId;
            questionLoader.LoadQuestionsForCurrentWeek();
            
            if (questionLoader.currentQuestions != null && questionLoader.currentQuestions.Count > 0)
            {
                LogTest($"✅ Forced set '{setId}': {questionLoader.currentQuestions.Count} questions loaded");
                LogTest($"   Info: {questionLoader.GetCurrentQuestionSetInfo()}");
                
                if (questionLoader.currentQuestions.Count > 0)
                {
                    var firstQuestion = questionLoader.currentQuestions[0];
                    LogTest($"   Sample question: '{firstQuestion.question}' -> '{firstQuestion.answer}'");
                }
            }
            else
            {
                LogError($"❌ Failed to load forced set '{setId}'");
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        // Reset to normal A/B testing
        questionLoader.forceQuestionSetId = "";
        LogTest("✅ Reset to normal A/B testing mode");
        
        yield return null;
    }
    
    void LogTest(string message)
    {
        if (detailedLogging)
        {
            Debug.Log($"[AB_TEST] {message}");
        }
    }
    
    void LogError(string message)
    {
        Debug.LogError($"[AB_TEST] {message}");
    }
    
    // Public method to run tests manually
    [ContextMenu("Run All Tests")]
    public void RunTestsManually()
    {
        StartCoroutine(RunAllTests());
    }
}
