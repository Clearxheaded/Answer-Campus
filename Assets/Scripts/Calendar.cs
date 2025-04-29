using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VNEngine;
using TMPro;
using System;
using System.Collections.Generic;

public static class SemesterHelper
{
    public const int FinalsWeek = 16;
    public const int MidtermsWeek = 7;

    public const int MidtermsWarningStart = 4;
    public const int FinalsWarningStart = 5;
    public const int DaysPerWeek = 7;

    public static string GetMonthForWeek(int week)
    {
        if (week <= 2)
            return "August";
        else if (week > 2 && week <= 5)
            return "September";
        else if (week >= 6 && week <= 9)
            return "October";
        else if (week >= 10 && week <= 14)
            return "November";
        else if (week >= 15 && week <= 16)
            return "December";
        else
        Debug.Log($"Week is {week}");
            return "Unknown"; // Safety catch
    }

    public static int GetDaysToCrossOut(int week)
    {
        return week * DaysPerWeek;
    }
    public static string GetStudyPrompt(int currentWeek)
    {
        int weeksUntilMidterms = MidtermsWeek - currentWeek;
        int weeksUntilFinals = FinalsWeek - currentWeek;

        if (weeksUntilMidterms >= 0 && weeksUntilMidterms <= MidtermsWarningStart)
        {
            return GetUrgencyMessage(weeksUntilMidterms, "Midterms");
        }
        else if (weeksUntilFinals >= 0 && weeksUntilFinals <= FinalsWarningStart)
        {
            return GetUrgencyMessage(weeksUntilFinals, "Finals");
        }
        else
        {
            return null; // No prompt needed
        }
    }

    private static string GetUrgencyMessage(int weeksLeft, string examName)
    {
        if (weeksLeft > 2)
        {
            return GetRandomPhrase(new List<string>
            {
                $"{examName} are coming up. Start preparing!",
                $"{examName} are on the horizon. Get ready!",
                $"{examName} are approaching. Plan your study time!"
            });
        }
        else if (weeksLeft == 2)
        {
            return GetRandomPhrase(new List<string>
            {
                $"{examName} are getting closer. Hit the books!",
                $"{examName} are around the corner. Stay sharp!",
                $"Only 2 weeks left until {examName}. Let's focus!"
            });
        }
        else if (weeksLeft == 1)
        {
            return GetRandomPhrase(new List<string>
            {
                $"{examName} are next week. Time to crunch!",
                $"{examName} are just days away. Study hard!",
                $"{examName} are almost here. Finish strong!"
            });
        }
        else // weeksLeft == 0
        {
            return GetRandomPhrase(new List<string>
            {
                $"{examName} are this week. Give it your all!",
                $"{examName} have arrived. Stay focused!",
                $"It's {examName} week. You've got this!"
            });
        }
    }

    private static string GetRandomPhrase(List<string> phrases)
    {
        int index = UnityEngine.Random.Range(0, phrases.Count);
        return phrases[index];
    }
}

public class Calendar : MonoBehaviour
{
    public TextMeshProUGUI month;
    public TextMeshProUGUI studyPrompt;
    public Transform calendarGrid;
    public GameObject checkmark;
    public int week;
// Start is called before the first frame update
    void Start()
    {
        if(StatsManager.Numbered_Stat_Exists("Week"))
        {
            week = (int)StatsManager.Get_Numbered_Stat("Week");
        }
        else
        {
            week = 1;
        }

        month.text = SemesterHelper.GetMonthForWeek(week);
        string prompt = SemesterHelper.GetStudyPrompt(week);
        if (!string.IsNullOrEmpty(prompt))
        {
            studyPrompt.text = prompt;
        }

        for (int i = 0; i < SemesterHelper.GetDaysToCrossOut(week); i++)
        {
            Instantiate(checkmark, calendarGrid);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
