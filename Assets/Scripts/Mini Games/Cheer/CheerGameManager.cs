using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum CheerDirection { Up, Down, Left, Right }

public enum CheerCombo
{
    Default,
    LeftDown,
    LeftRight,
    LeftUp,
    RightDown,
    RightUp,
    UpDown
}

[System.Serializable]
public class MatchSequence
{
    public Cheer cheerleader;  // Reference to the Cheer controller
}

[System.Serializable]
public class CheerClip
{
    public AudioClip countdownClip;
    public AudioClip gameClip;
    public float[] beatTimes; // One timestamp (in seconds) per cheer
}

public class CheerGameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject playableGameRoot;
    public TextMeshProUGUI comboDisplayText;
    public GameObject scoreboardUI;
    public AudioSource audioSource;
    public AudioClip introClip;
    public TextMeshProUGUI homeScoreText;
    public TextMeshProUGUI awayScoreText;
    public TextMeshProUGUI awayTeamNameText;
    public TextMeshProUGUI quarterText;
    public List<MatchSequence> matchSequences;

    [Header("Crowd Feedback")]
    public float amplification = 0f;
    public float amplificationPerSuccess = 0.2f;
    public float amplificationDecayRate = 0.05f;

    [Header("Visual Feedback")]
    public Image[] leaderImages;
    public RectTransform spotlight;
    public Image glyphA, glyphB;
    public Vector2 glyphOffsetA = new Vector2(-40, 80);
    public Vector2 glyphOffsetB = new Vector2(40, 80);
    public Sprite upGlyph, downGlyph, leftGlyph, rightGlyph;
    public Image[] progressDots;
    public Color dimColor = new Color(1,1,1,0.4f);
    public Color highlightColor = Color.white;
    public Color defaultDotColor = Color.gray;
    public Color successDotColor = Color.green;
    public Color failDotColor = Color.red;

    [Header("Game Variables")]
    public CheerClip[] cheers;
    private int homeScore = 0;
    private int awayScore = 0;
    private int currentQuarter = 1;
    private CheerClip selectedCheerClip;
    private HashSet<KeyCode> usedKeys = new();

    [Header("Round Settings")]
    public float timeBetweenDisplays = 0.8f;
    public float inputFeedbackDuration = 0.4f;
    public int combosToPass = 2;
    private int combosMade = 0;
    private static readonly Dictionary<CheerCombo, CheerDirection[]> comboMap = new()
    {
        { CheerCombo.LeftDown,  new[]{ CheerDirection.Left,  CheerDirection.Down  } },
        { CheerCombo.LeftRight, new[]{ CheerDirection.Left,  CheerDirection.Right } },
        { CheerCombo.LeftUp,    new[]{ CheerDirection.Left,  CheerDirection.Up    } },
        { CheerCombo.RightDown, new[]{ CheerDirection.Right, CheerDirection.Down  } },
        { CheerCombo.RightUp,   new[]{ CheerDirection.Right, CheerDirection.Up    } },
        { CheerCombo.UpDown,    new[]{ CheerDirection.Up,    CheerDirection.Down  } },
    };

    void Start()
    {
        StartCoroutine(GameFlowRoutine());
    }

    IEnumerator GameFlowRoutine()
    {
        yield return StartCoroutine(FadeFromBlack());
        scoreboardUI.SetActive(true);
        if (playableGameRoot != null) playableGameRoot.SetActive(false);

        for (int q = 1; q <= 4; q++)
        {
            currentQuarter = q;
            UpdateScoreboardUI();

            scoreboardUI.SetActive(true);
            if (introClip != null)
            {
                audioSource.PlayOneShot(introClip);
                yield return new WaitForSeconds(introClip.length);
            }

            scoreboardUI.SetActive(false);
            if (playableGameRoot != null) playableGameRoot.SetActive(true);

            selectedCheerClip = cheers[Random.Range(0, cheers.Length)];

            audioSource.PlayOneShot(selectedCheerClip.countdownClip);
            yield return new WaitForSeconds(selectedCheerClip.countdownClip.length);

            audioSource.clip = selectedCheerClip.gameClip;
            audioSource.Play();

            yield return new WaitUntil(() => audioSource.time > 0); // Ensure audio starts before capturing DSP

            double startDSP = AudioSettings.dspTime;
            yield return StartCoroutine(RunBeatSyncedRound(startDSP));

            audioSource.Stop();

            float amplitudeRatio = amplification / amplificationPerSuccess;
            int totalCombos = selectedCheerClip.beatTimes.Length;

            float accuracy = (float)combosMade / totalCombos;

            if (accuracy == 1f) {
                homeScore += 7;
            } else if (accuracy > 0.5f) {
                homeScore += 3;
            } else if (accuracy > 0f) {
                awayScore += 3;
            } else {
                awayScore += 7;
            }

            combosMade = 0;
            yield return new WaitForSeconds(1f);
        }

        EndGame();
    }

    IEnumerator RunBeatSyncedRound(double startDSP)
    {
        int beatCount = selectedCheerClip.beatTimes.Length;
        var combos = new List<CheerCombo>((CheerCombo[])System.Enum.GetValues(typeof(CheerCombo)));
        combos.Remove(CheerCombo.Default);

        for (int i = 0; i < beatCount; i++)
        {
            double targetDSP = startDSP + selectedCheerClip.beatTimes[i];
            CheerCombo combo = combos[Random.Range(0, combos.Count)];

            int leaderIdx = i % matchSequences.Count;
            var leader = matchSequences[leaderIdx];

            while (AudioSettings.dspTime < targetDSP)
                yield return null;

            HighlightLeader(leaderIdx);
            MoveSpotlight(leaderIdx);
            ShowGlyphs(combo, leaderIdx);
            leader.cheerleader.SetCombo(combo);

            yield return new WaitForSeconds(0.6f);

            HideGlyphs();
            leader.cheerleader.SetCombo(CheerCombo.Default);

            CheerDirection first = CheerDirection.Up, second = CheerDirection.Up;
            usedKeys.Clear();
            double inputDeadline = (i + 1 < selectedCheerClip.beatTimes.Length)
                ? startDSP + selectedCheerClip.beatTimes[i + 1]
                : targetDSP + 1.2;

            Debug.Log($"Waiting for first direction input. Expected: {comboMap[combo][0]}");
            yield return StartCoroutine(WaitForDirectionUntilDSP(dir => first = dir, inputDeadline));
            Debug.Log($"Waiting for second direction input. Expected: {comboMap[combo][1]}");
            yield return StartCoroutine(WaitForSecondDirectionUntilDSP(dir => second = dir, first, inputDeadline));

            CheerCombo attempt = GetComboFromDirs(first, second);
            bool success = (attempt == combo);

            if (success)
            {
                amplification += amplificationPerSuccess;
                combosMade++;
            }
            

            Debug.Log($"Beat {i} - {success}, Amplification: {amplification:F2}");
            UnhighlightAll();
            yield return new WaitForSeconds(0.2f);
        }

        comboDisplayText.text = $"Cheers matched: {amplification / amplificationPerSuccess}/{beatCount}";
    }
    IEnumerator WaitForSecondDirectionUntilDSP(System.Action<CheerDirection> callback, CheerDirection alreadyUsed, double deadlineDSP)
    {
        while (AudioSettings.dspTime < deadlineDSP)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && alreadyUsed != CheerDirection.Up) {
                callback(CheerDirection.Up);
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && alreadyUsed != CheerDirection.Down) {
                callback(CheerDirection.Down);
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) && alreadyUsed != CheerDirection.Left) {
                callback(CheerDirection.Left);
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) && alreadyUsed != CheerDirection.Right) {
                callback(CheerDirection.Right);
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator WaitForDirectionUntilDSP(System.Action<CheerDirection> callback, double deadlineDSP)
    {
        while (AudioSettings.dspTime < deadlineDSP)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && usedKeys.Add(KeyCode.UpArrow)) {
                callback(CheerDirection.Up);
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && usedKeys.Add(KeyCode.DownArrow)) {
                callback(CheerDirection.Down);
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) && usedKeys.Add(KeyCode.LeftArrow)) {
                callback(CheerDirection.Left);
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) && usedKeys.Add(KeyCode.RightArrow)) {
                callback(CheerDirection.Right);
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator FadeFromBlack()
    {
        yield return new WaitForSeconds(1f);
    }

    void EndGame() => Debug.Log("Game Over - Final Routine Placeholder");

    void Update()
    {
        amplification = Mathf.Max(0f, amplification - amplificationDecayRate * Time.deltaTime);
    }

    void UpdateScoreboardUI()
    {
        homeScoreText.text = homeScore.ToString();
        awayScoreText.text = awayScore.ToString();
        quarterText.text = $"Q{currentQuarter}";
    }

    CheerCombo GetComboFromDirs(CheerDirection a, CheerDirection b)
    {
        foreach (var kv in comboMap)
            if (kv.Value[0] == a && kv.Value[1] == b)
                return kv.Key;
        Debug.LogWarningFormat("Unknown combo from dirs: {0}, {1}", a, b);
        return CheerCombo.Default;
    }

    void HighlightLeader(int idx)
    {
        if (leaderImages == null) return;
        for (int j = 0; j < leaderImages.Length; j++)
            leaderImages[j].color = (j == idx) ? highlightColor : dimColor;
    }

    void UnhighlightAll()
    {
        if (leaderImages != null)
            foreach (var img in leaderImages) img.color = highlightColor;
        if (spotlight) spotlight.gameObject.SetActive(false);
    }

    void MoveSpotlight(int idx)
    {
        if (spotlight == null || leaderImages == null || idx < 0 || idx >= leaderImages.Length) return;
        spotlight.gameObject.SetActive(true);
        spotlight.position = leaderImages[idx].transform.position;
    }

    void ShowGlyphs(CheerCombo combo, int idx)
    {
        if (glyphA == null || glyphB == null || leaderImages == null || idx < 0 || idx >= leaderImages.Length) return;
        var dirs = comboMap[combo];
        glyphA.sprite = GetGlyphSprite(dirs[0]);
        glyphB.sprite = GetGlyphSprite(dirs[1]);
        Vector3 leaderPos = leaderImages[idx].transform.position;
        glyphA.gameObject.SetActive(true);
        glyphB.gameObject.SetActive(true);
        glyphA.transform.position = leaderPos + (Vector3)glyphOffsetA;
        glyphB.transform.position = leaderPos + (Vector3)glyphOffsetB;
    }

    void HideGlyphs()
    {
        if (glyphA) glyphA.gameObject.SetActive(false);
        if (glyphB) glyphB.gameObject.SetActive(false);
    }

    Sprite GetGlyphSprite(CheerDirection dir)
    {
        return dir switch
        {
            CheerDirection.Up => upGlyph,
            CheerDirection.Down => downGlyph,
            CheerDirection.Left => leftGlyph,
            CheerDirection.Right => rightGlyph,
            _ => null
        };
    }
}
