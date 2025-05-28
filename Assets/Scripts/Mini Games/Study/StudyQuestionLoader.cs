using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class QuestionWeek {
    public string week;
    public List<QuestionAnswerPair> questions;
}

[System.Serializable]
public class QuestionWeekList {
    public List<QuestionWeek> weeks;
}
public enum GameMode { Solo, Group, Exam }
public class StudyQuestionLoader : MonoBehaviour
{
    public TextAsset questionsJSON;
    public GameMode currentMode = GameMode.Solo;
    public List<QuestionAnswerPair> currentQuestions;
    public bool useDefinitions = true; // optional override
    private QuestionAnswerPair lastQuestion = null;
    private int questionIndex = 0;

    public void LoadQuestionsForCurrentWeek()
    {
        if (questionsJSON == null)
        {
            Debug.LogError("Missing questions JSON.");
            return;
        }

        QuestionWeekList weekList = JsonUtility.FromJson<QuestionWeekList>(questionsJSON.text);
        if (weekList == null || weekList.weeks == null)
        {
            Debug.LogError("Could not parse question data.");
            return;
        }

        int week = 1;
        if (VNEngine.StatsManager.Numbered_Stat_Exists("current_week"))
        {
            week = Mathf.Clamp((int)VNEngine.StatsManager.Get_Numbered_Stat("current_week"), 1, 99);
        }

        foreach (var entry in weekList.weeks)
        {
            if (entry.week == week.ToString())
            {
                currentQuestions = entry.questions;
                return;
            }
        }

        Debug.LogWarning("No questions for this week. Falling back to week 1.");
        currentQuestions = weekList.weeks.Find(w => w.week == "1")?.questions;
    }
    
    public string GetDisplayPrompt(QuestionAnswerPair pair, bool definitions)
    {
        if (definitions)
        {
            if (!string.IsNullOrEmpty(pair.definition))
                return pair.definition;
        }
        return pair.question;
    }

    public QuestionAnswerPair GetRandomQuestion()
    {
        if (currentQuestions == null || currentQuestions.Count == 0)
        {
            Debug.LogWarning("No questions loaded for current week.");
            return null;
        }

        if (currentQuestions.Count == 1)
        {
            lastQuestion = currentQuestions[0];
            return lastQuestion;
        }

        QuestionAnswerPair selected;
        int safety = 0;
        do
        {
            selected = currentQuestions[Random.Range(0, currentQuestions.Count)];
            safety++;
        }
        while (selected == lastQuestion && safety < 10);

        lastQuestion = selected;
        return selected;
    }
}