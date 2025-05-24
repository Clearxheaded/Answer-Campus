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
    public CheerLeader cheerleader;  // Reference to the Cheer controller
}
[System.Serializable]
public struct CheerLeader
{
    public Cheer cheer;         // The cheerleader (pose logic)
    public Image glyphA;        // Left directional arrow
    public Image glyphB;        // Right directional arrow
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
    //public TextMeshProUGUI comboDisplayText;
    public GameObject scoreboardUI;
    public AudioSource audioSource;
    public AudioClip introClip;
    public TextMeshProUGUI homeScoreText;
    public TextMeshProUGUI awayScoreText;
    public TextMeshProUGUI awayTeamNameText;
    public TextMeshProUGUI quarterText;
    [SerializeField] public List<MatchSequence> matchSequences;

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
    public Color dimColor = new Color(1,1,1,0.4f);
    public Color highlightColor = Color.white;
    public Color successColor = Color.green;
    public Color failColor = Color.red;

    
    [Header("Game Variables")]
    public CheerClip[] cheers;
    private int homeScore = 0;
    private int awayScore = 0;
    private int currentQuarter = 1;
    private CheerClip selectedCheerClip;
    

    [Header("Round Settings")]
    public float comboDisplayTime = 1f;
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
            leader.cheerleader.cheer.SetCombo(combo);

            yield return new WaitForSeconds(comboDisplayTime);
            leader.cheerleader.cheer.SetCombo(CheerCombo.Default);

            CheerDirection inputA = CheerDirection.Up;
            CheerDirection inputB = CheerDirection.Up;
            double inputDeadline = (i + 1 < selectedCheerClip.beatTimes.Length)
                ? startDSP + selectedCheerClip.beatTimes[i + 1]
                : targetDSP + 1.2;
            yield return null;
            yield return StartCoroutine(WaitForTwoUniqueDirections((a, b) => {
                inputA = a;
                inputB = b;
            }, inputDeadline));

            CheerCombo attempt = GetComboFromDirs(inputA, inputB);
            bool success = (attempt == combo);

            if (success)
            {
                amplification += amplificationPerSuccess;
                combosMade++;
            }
            FlashGlyphFeedback(leaderIdx, success);
// 👇 WAIT for the reset coroutine to finish
            yield return new WaitForSecondsRealtime(0.6f);

// 👇 Now hide the glyphs and reset the combo pose
            HideGlyphs(leaderIdx);
            leader.cheerleader.cheer.SetCombo(CheerCombo.Default);
            UnhighlightAll();
            CheerInputBridge.Instance.Clear();
            yield return new WaitForSeconds(0.2f);
        }

    }

    IEnumerator WaitForTwoUniqueDirections(System.Action<CheerDirection, CheerDirection> callback, double deadlineDSP)
    {
        
        CheerDirection? first = null;
        CheerDirection? second = null;

        while (AudioSettings.dspTime < deadlineDSP)
        {
            if (CheerInputBridge.Instance.TryGetNextDirection(out var dir))
            {
                if (!first.HasValue)
                {
                    first = dir;
                    
                }
                else if (!second.HasValue && dir != first.Value)
                {
                    second = dir;
                    
                    break;
                }
            }

            yield return null;
        }

        var finalFirst = first ?? CheerDirection.Up;
        var finalSecond = second ?? CheerDirection.Down;

        Debug.Log($"[FINAL INPUT] Using: {finalFirst}, {finalSecond}");
        callback(finalFirst, finalSecond);
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
        {
            var dirs = kv.Value;
            if ((dirs[0] == a && dirs[1] == b) || (dirs[0] == b && dirs[1] == a))
                return kv.Key;
        }

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
        if (matchSequences == null || idx < 0 || idx >= matchSequences.Count) return;

        var leader = matchSequences[idx];
        if (leader.cheerleader.glyphA == null || leader.cheerleader.glyphB == null || leader.cheerleader.cheer == null) return;

        var dirs = comboMap[combo];

        // Assign sprites
        leader.cheerleader.glyphA.sprite = GetGlyphSprite(dirs[0]);
        leader.cheerleader.glyphB.sprite = GetGlyphSprite(dirs[1]);

        // Set default white color
        leader.cheerleader.glyphA.color = Color.white;
        leader.cheerleader.glyphB.color = Color.white;

        // Position near cheer pose
        Vector3 leaderPos = leader.cheerleader.cheer.transform.position;
        leader.cheerleader.glyphA.transform.position = leaderPos + (Vector3)glyphOffsetA;
        leader.cheerleader.glyphB.transform.position = leaderPos + (Vector3)glyphOffsetB;

        // Show them
        leader.cheerleader.glyphA.gameObject.SetActive(true);
        leader.cheerleader.glyphB.gameObject.SetActive(true);
    }
    void HideGlyphs(int idx)
    {
        if (matchSequences == null || idx < 0 || idx >= matchSequences.Count) return;

        var leader = matchSequences[idx];
        if (leader.cheerleader.glyphA != null) leader.cheerleader.glyphA.gameObject.SetActive(false);
        if (leader.cheerleader.glyphB != null) leader.cheerleader.glyphB.gameObject.SetActive(false);
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
    IEnumerator AnimateGlyphColor(Color targetColor, float duration = 0.3f)
    {
        Image[] glyphs = { glyphA, glyphB };
        Color startColor = Color.white;
        float t = 0f;

        foreach (var g in glyphs)
        {
            if (g != null) g.color = startColor;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            foreach (var g in glyphs)
            {
                if (g != null) g.color = Color.Lerp(startColor, targetColor, t / duration);
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);  // pause before resetting

        foreach (var g in glyphs)
        {
            if (g != null) g.color = Color.white;
        }
    }

    public void FlashGlyphFeedback(int idx, bool success)
    {
        Debug.Log($"[FEEDBACK] Triggered for leader idx {idx}, success: {success}");

        if (idx < 0 || idx >= matchSequences.Count)
        {
            Debug.LogWarning($"[FEEDBACK] Invalid idx: {idx}");
            return;
        }

        var leader = matchSequences[idx];

        if (leader.cheerleader.cheer == null)
        {
            Debug.LogWarning($"[FEEDBACK] Leader at idx {idx} has no cheerleader assigned");
            return;
        }

        if (leader.cheerleader.glyphA == null || leader.cheerleader.glyphB == null)
        {
            Debug.LogWarning($"[FEEDBACK] Missing glyphs on leader {idx}: A is {(leader.cheerleader.glyphA == null ? "null" : "set")}, B is {(leader.cheerleader.glyphB == null ? "null" : "set")}");
            return;
        }

        var color = success ? successColor : failColor;

        leader.cheerleader.glyphA.color = color;
        leader.cheerleader.glyphB.color = color;

        Debug.Log($"[FEEDBACK] Set color to {color} on both glyphs");

        StopCoroutine(nameof(ResetGlyphsAfterDelay));
        StartCoroutine(ResetGlyphsAfterDelay(leader.cheerleader, 0.5f));
    }

    private IEnumerator ResetGlyphsAfterDelay(CheerLeader cheer, float delay)
    {
        Debug.Log($"[RESET] Will reset glyph colors after {delay}s");
        yield return new WaitForSecondsRealtime(delay);

        if (cheer.glyphA != null) cheer.glyphA.color = Color.white;
        if (cheer.glyphB != null) cheer.glyphB.color = Color.white;

        Debug.Log($"[RESET] Glyphs reset to white");
    }


    IEnumerator ResetGlyphsAfterDelay(float delay)
    {
        Debug.Log($"[RESET] Waiting {delay} seconds");
        yield return new WaitForSecondsRealtime(delay); // ⬅️ important: unaffected by Time.timeScale

        if (glyphA != null) glyphA.color = Color.white;
        if (glyphB != null) glyphB.color = Color.white;

        Debug.Log("[RESET] Glyphs reset to white");
    }


}
