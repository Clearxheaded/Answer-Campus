using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ChallengeProfile", menuName = "GroupStudy/ChallengeProfile")]
public class ChallengeProfile : ScriptableObject
{
    public string characterName;
    public enum PromptType { Definitions, Questions }
    public PromptType promptType;
    public float timerDuration = 60f;
    public bool showTimer = true;
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 2f;
    [Range(0f, 1f)] public float chanceOfCorrectLetter = 0.5f;
    public bool allowHints = false;
    public List<QuestionAnswerPair> customQuestions;
}