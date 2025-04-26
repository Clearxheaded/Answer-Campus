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

public class RhythmGameV3 : MonoBehaviour
{
    [Header("UI References")]
    public List<MatchSequence> matchSequences;  // One entry per leader
    public TextMeshProUGUI comboDisplayText;    // End-of-round feedback text

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

    [Header("Round Settings")]
    public float timeBetweenDisplays = 0.8f;
    public float inputFeedbackDuration = 0.4f;
    public int combosToPass = 2;

    // Combo mapping
    private static readonly Dictionary<CheerCombo, CheerDirection[]> comboMap = new Dictionary<CheerCombo, CheerDirection[]>
    {
        { CheerCombo.LeftDown,  new[]{ CheerDirection.Left,  CheerDirection.Down  } },
        { CheerCombo.LeftRight, new[]{ CheerDirection.Left,  CheerDirection.Right } },
        { CheerCombo.LeftUp,    new[]{ CheerDirection.Left,  CheerDirection.Up    } },
        { CheerCombo.RightDown, new[]{ CheerDirection.Right, CheerDirection.Down  } },
        { CheerCombo.RightUp,   new[]{ CheerDirection.Right, CheerDirection.Up    } },
        { CheerCombo.UpDown,    new[]{ CheerDirection.Up,    CheerDirection.Down  } },
    };

    private List<CheerCombo> assignedCombos;

    void Start()
    {
        // Initialize UI states
        if (comboDisplayText != null) comboDisplayText.text = string.Empty;
        if (progressDots != null)
        {
            foreach (var dot in progressDots)
                dot.color = defaultDotColor;
        }
        if (glyphA != null) glyphA.gameObject.SetActive(false);
        if (glyphB != null) glyphB.gameObject.SetActive(false);
        if (leaderImages != null)
        {
            foreach (var img in leaderImages)
                img.color = highlightColor;
        }

        // Start the first round
        StartCoroutine(RunRound());
    }

    IEnumerator RunRound()
    {
        // 1) Assign random combos
        var combos = new List<CheerCombo>((CheerCombo[])System.Enum.GetValues(typeof(CheerCombo)));
        combos.Remove(CheerCombo.Default);
        assignedCombos = new List<CheerCombo>();
        for (int i = 0; i < matchSequences.Count; i++)
            assignedCombos.Add(combos[Random.Range(0, combos.Count)]);

        // Debug: log assigned combos
        Debug.Log("Assigned combos:");
        for (int i = 0; i < assignedCombos.Count; i++)
            Debug.LogFormat(" Leader {0}: {1}", i, assignedCombos[i]);

        // 2) Reset progress dots
        if (progressDots != null)
        {
            int len = Mathf.Min(progressDots.Length, matchSequences.Count);
            for (int i = 0; i < len; i++)
                progressDots[i].color = defaultDotColor;
        }

        // 3) Show Phase
        for (int i = 0; i < matchSequences.Count; i++)
        {
            HighlightLeader(i);
            MoveSpotlight(i);

            CheerCombo pose = assignedCombos[i];
            ShowGlyphs(pose, i);
            matchSequences[i].cheerleader.SetCombo(pose);

            yield return new WaitForSeconds(timeBetweenDisplays);

            matchSequences[i].cheerleader.SetCombo(CheerCombo.Default);
            HideGlyphs();
            UnhighlightAll();
            yield return new WaitForSeconds(0.3f);
        }

        // 4) Input Phase
        int correctCount = 0;
        for (int i = 0; i < matchSequences.Count; i++)
        {
            HighlightLeader(i);
            MoveSpotlight(i);

            CheerDirection first = CheerDirection.Up;
            CheerDirection second = CheerDirection.Up;
            yield return StartCoroutine(WaitForDirection(dir => first = dir));
            Debug.LogFormat("Leader {0} first: {1}", i, first);
            yield return StartCoroutine(WaitForDirection(dir => second = dir));
            Debug.LogFormat("Leader {0} second: {1}", i, second);

            CheerCombo pressed = GetComboFromDirs(first, second);
            Debug.LogFormat("Leader {0} pressed: {1}, expected: {2}", i, pressed, assignedCombos[i]);

            matchSequences[i].cheerleader.SetCombo(pressed);
            yield return new WaitForSeconds(inputFeedbackDuration);

            // Score this combo
            bool correct = (pressed == assignedCombos[i]);
            // Update progress dot if available
            if (progressDots != null && i < progressDots.Length)
                progressDots[i].color = correct ? successDotColor : failDotColor;
            // Always count correctness
            if (correct)
                correctCount++;
            Debug.LogFormat("Leader {0} correct? {1}", i, correct);

            matchSequences[i].cheerleader.SetCombo(CheerCombo.Default);
            UnhighlightAll();
        }

        // 5) End-of-round Feedback
        if (comboDisplayText != null)
            comboDisplayText.text = $"Score: {correctCount}/{matchSequences.Count}";

        bool passed = (correctCount >= combosToPass);
        if (comboDisplayText != null)
            comboDisplayText.text += passed ? "\nNext Round!" : "\nGame Over!";

        if (passed)
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(RunRound());
        }
    }

    IEnumerator WaitForDirection(System.Action<CheerDirection> callback)
    {
        // Wait until no arrow keys are held (avoid double-capture)
        while (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)
            || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            yield return null;

        // Now wait for a new key press
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))    { callback(CheerDirection.Up);    yield break; }
            if (Input.GetKeyDown(KeyCode.DownArrow))  { callback(CheerDirection.Down);  yield break; }
            if (Input.GetKeyDown(KeyCode.LeftArrow))  { callback(CheerDirection.Left);  yield break; }
            if (Input.GetKeyDown(KeyCode.RightArrow)) { callback(CheerDirection.Right); yield break; }
            yield return null;
        }
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
        switch (dir)
        {
            case CheerDirection.Up:    return upGlyph;
            case CheerDirection.Down:  return downGlyph;
            case CheerDirection.Left:  return leftGlyph;
            case CheerDirection.Right: return rightGlyph;
        }
        return null;
    }
}
