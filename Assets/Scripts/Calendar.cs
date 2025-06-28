using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VNEngine;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using Random = System.Random;
using FMODUnity;
using System.Linq;
using System.Text.RegularExpressions;

[Serializable]

public static class FootballScheduler
{
    static FootballTeam[] opponents = new FootballTeam[]
    {
        new FootballTeam("Northport University", "Grizzlies"),
        new FootballTeam("Central Tech", "Shock"),
        new FootballTeam("Valley State", "Hornets"),
        new FootballTeam("Eastern Pines", "Wolves"),
        new FootballTeam("Bayfront College", "Surge"),
        new FootballTeam("Riverside A&M", "Gators"),
        new FootballTeam("Highland University", "Stags"),
        new FootballTeam("Metro Institute", "Titans")
    };

    public static void GenerateSchedule()
    {
        List<FootballGame> schedule = new List<FootballGame>();
        List<int> possibleWeeks = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9 };
        Shuffle(possibleWeeks);

        for (int i = 0; i < 8; i++)
        {
            schedule.Add(new FootballGame
            {
                week = possibleWeeks[i],
                opponent = opponents[i],
                isHome = true,
                played = false
            });
        }

        string json = JsonUtility.ToJson(new FootballGameListWrapper { games = schedule });
        StatsManager.Set_String_Stat("FootballSchedule", json);
    }
    public static FootballGame GetThisWeeksGame(int currentWeek)
    {
        if (!StatsManager.String_Stat_Exists("FootballSchedule"))
        {
            FootballScheduler.GenerateSchedule(); // only if safe to call
        }

        string json = StatsManager.Get_String_Stat("FootballSchedule");
        if (string.IsNullOrEmpty(json)) return null;

        var wrapper = JsonUtility.FromJson<FootballGameListWrapper>(json);
        if (wrapper?.games == null) return null;

        return wrapper.games.Find(g => g.week == currentWeek);
    }


    static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, list.Count);
            T temp = list[rnd];
            list[rnd] = list[i];
            list[i] = temp;
        }
    }

}

[Serializable]
public class FootballGameListWrapper
{
    public List<FootballGame> games = new List<FootballGame>();
}

public static class SemesterHelper
{
    public const int FinalsWeek = 15;
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
        if (week <= 0) return 0;

        int fullWeeks = Mathf.Min(week - 1, 4);
        int days = fullWeeks * DaysPerWeek;

        if (week <= 5)
        {
            string key = $"Week_{week}_PartialDays";
            int partialDays;

            if (StatsManager.Numbered_Stat_Exists(key))
            {
                partialDays = (int)StatsManager.Get_Numbered_Stat(key);
            }
            else
            {
                int max = Mathf.Min(35 - days, DaysPerWeek);
                partialDays = UnityEngine.Random.Range(1, max + 1);
                StatsManager.Set_Numbered_Stat(key, partialDays);
            }

            days += partialDays;
        }

        return days;
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
[Serializable]
public struct TimeImage
{
    public SpriteRenderer image;
    public Image uiImage;
    public Sprite spriteDay;
    public Sprite spriteNight;
    public enum timeOfDay {DAY, NIGHT}
}
public class Calendar : MonoBehaviour
{
    public TimeImage[] timeImages;
    public TextMeshProUGUI month;
    public TextMeshProUGUI studyPrompt;
    public Transform calendarGrid;
    public GameObject checkmark;
    public int week;
    public string ambientFMODEventName;
    public string musicFMODEventName;
    public Location finalExamLocation;
    private FMOD.Studio.EventInstance bgMusic;
    public Characters characters;
    public GameObject finalReport;
    public TextMeshProUGUI finalText;
    public Image finalCharacterImage;

    private bool isDay = true;
    // Start is called before the first frame update

    public void ToggleDaytime()
    {
        isDay = !isDay;
        if (isDay)
        {
            for (int i = 0; i < timeImages.Length; i++)
            {
                if (timeImages[i].uiImage != null)
                {
                    timeImages[i].uiImage.sprite = timeImages[i].spriteDay;
                }

                if (timeImages[i].image != null)
                {
                    timeImages[0].image.sprite = timeImages[i].spriteDay;
                }
            }
        }
        else
        {
            for (int i = 0; i < timeImages.Length; i++)
            {

                if (timeImages[i].uiImage != null)
                {
                    timeImages[i].uiImage.sprite = timeImages[i].spriteNight;
                }

                if (timeImages[i].image != null)
                {
                    timeImages[i].image.sprite = timeImages[i].spriteNight;
                }
            }
        }
    }
    
    void Start()
    {
        if (ambientFMODEventName != null)
        {
            if (FMODAudioManager.Instance != null)
            {
                FMODAudioManager.Instance.PlayMusic(ambientFMODEventName);
            }
        }

        if (musicFMODEventName != null)
        {
            if (FMODAudioManager.Instance != null)
            {
                FMODAudioManager.Instance.PlayMusic(musicFMODEventName);
            }
        }
        
        if(StatsManager.Numbered_Stat_Exists("Week"))
        {
            week = (int)StatsManager.Get_Numbered_Stat("Week");
            if (week <= 1)
            {
                FootballScheduler.GenerateSchedule();
            }
        }
        else
        {
            FootballScheduler.GenerateSchedule();
            week = 1;
        }
        string json = StatsManager.Get_String_Stat("FootballSchedule");
        Debug.Log($"[Schedule JSON] {json}");
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

        if (week == SemesterHelper.FinalsWeek)
        {
            finalExamLocation.GoToLocation();
        }

        if (week >= SemesterHelper.FinalsWeek + 1)
        {
            EndSemester();
        }
    }

    public static Character ParseBestFriendEnum(string rawValue)
    {
        // Normalize: lowercase, remove non-alphanumeric characters, then PascalCase it
        string cleaned = Regex.Replace(rawValue, @"[^a-zA-Z0-9]", ""); // Remove symbols
        cleaned = char.ToUpper(cleaned[0]) + cleaned.Substring(1).ToLower(); // Simple PascalCase

        if (Enum.TryParse(typeof(Character), cleaned, out var result))
        {
            return (Character)result;
        }

        Debug.LogWarning($"Could not parse '{rawValue}' into Character enum. Defaulting.");
        return Character.NONE; // Replace with a safe default in your enum
    }

    void EndSemester()
    {
        string bestFriendRaw = StatsManager.Get_String_Stat("Best Friend");
        Character bestFriendEnum = ParseBestFriendEnum(bestFriendRaw);
        foreach (var profile in characters.profiles)
        {
            if (profile.character == bestFriendEnum)
            {
                finalCharacterImage.sprite = profile.polaroid;
            }
        }
        string json = StatsManager.Get_String_Stat("FootballSchedule");
        string player_name = StatsManager.Get_String_Stat("Player Name");
        var schedule = JsonUtility.FromJson<FootballGameListWrapper>(json);
        int wins = schedule.games.Count(g => g.played && g.won);
        int losses = schedule.games.Count(g => g.played && !g.won);
        float midtermScore = StatsManager.Get_Numbered_Stat("MidtermScore");
        float finalScore = StatsManager.Get_Numbered_Stat("FinalScore");
        float studyBonus = StatsManager.Get_Numbered_Stat("StudyGameScore"); // number of words
// Calculate final GPA
        float examScore = Mathf.Max(midtermScore, finalScore);
        float gpa = examScore; // base GPA
        if (studyBonus > 0) gpa += 0.5f; // bonus bump for studying
        gpa = Mathf.Clamp(gpa, 0f, 4f);
        string finalNarrative = $"{player_name}! Can you believe the semester is over already? You've been an incredible friend. ";
/*
        if (gpa > 3.5f)
        {
            finalNarrative += $"You crushed it this semester, {gpa} GPA? Legend. ";
            if (wins > losses)
            {
                finalNarrative += $"And how about our team? I mean, they're talented but you brought the spirit. ";
            }
            else
            {
                finalNarrative += "But maybe find some time to work on that school spirit, our team could use it! ";
            }

        }
        else
        {
            finalNarrative += $"Don't worry about grades too much, {gpa} is still passing, right? ";
            if (wins > losses)
            {
                if (wins > losses)
                {
                    finalNarrative += $"You did crush it with team spirit. The coach owes you! ";
                }
                else
                {
                    finalNarrative += "And who cares about team spirit anyways? I'd rather go to the club. ";
                }
            }
        }
*/
        finalNarrative += "Can't wait to see what next semester brings.";
        finalText.text = finalNarrative;
        finalReport.SetActive(true);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
