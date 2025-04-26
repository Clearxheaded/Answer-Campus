using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.InputSystem;

public class ShowPath : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text progressText;
    [SerializeField] public GameObject squarePrefab;
    [SerializeField] private int gridRows = 7;
    [SerializeField] private int gridCols = 7;
    [SerializeField] private float spacing = 1.4f;

    private GameObject[] allSquares;
    [SerializeField] private GameObject clickableGrid;
    [SerializeField] private SpriteRenderer backgroundColor;
    [SerializeField] private GameObject background;
    [SerializeField] private RandomMover move;
    [SerializeField] private CountdownTimer countdown;
    [SerializeField] private PathGenerator pathGenerator;

    [Header("Game State")]
    private int[] currentPath;
    private int currentStep = 0;
    private int successfulRounds = 0;
    public List<GameObject> activatedSquares = new();

    private void Awake()
    {
        Debug.Log("ShowPath: Awake - Building grid...");
        allSquares = new GameObject[gridRows * gridCols];
        Vector3 startPos = new Vector3(-(gridCols - 1) * spacing / 2f, (gridRows - 1) * spacing / 2f, 0f);

        int index = 0;
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                Vector3 pos = startPos + new Vector3(col * spacing, -row * spacing, 0f);
                GameObject square = Instantiate(squarePrefab, pos, Quaternion.identity, clickableGrid.transform);

                var click = square.GetComponent<ClickableSquare>();
                click.Initialize(index, this);

                allSquares[index] = square;
                square.SetActive(true);
                index++;
            }
        }

        move.InitializeGridSettings(gridRows, gridCols, spacing);
        Debug.Log("ShowPath: Awake - Grid complete.");
    }

    public void StartNewRound()
    {
        Debug.Log("ShowPath: Starting new round...");
        countdown.OnTimerFinished -= FailRound;
        countdown.OnTimerFinished += FailRound;

        currentStep = 0;
        currentPath = pathGenerator.GetRandomPath();
        Debug.Log("ShowPath: Generated path: " + string.Join(", ", currentPath));

        StartCoroutine(ActivateSquares(currentPath));

//        countdown.StartTimer(10f);
        move.StartMoving();
    }

    public void FailRound()
    {
        Debug.Log("ShowPath: Timer expired - failing round.");
        HandleRoundEnd(false);
    }
    private IEnumerator ActivateSquares(int[] path)
    {
        Debug.Log("ShowPath: Activating path...");

        foreach (int index in path)
        {
            if (index < 0 || index >= allSquares.Length)
            {
                Debug.LogError("ShowPath: Invalid path index: " + index);
                continue;
            }

            Debug.Log("ShowPath: Flashing square index: " + index);

            var click = allSquares[index].GetComponent<ClickableSquare>();
            if (click != null)
            {
                click.Flash();
                click.isOnPathThisRound = true; // ✅ Mark this square as part of the current path
            }

            yield return new WaitForSeconds(0.5f);

            if (click != null)
            {
                click.ResetColor();
            }
        }

        yield return new WaitForSeconds(1f);

        foreach (GameObject square in allSquares)
        {
            square.SetActive(true);
        }

        clickableGrid.SetActive(true);
        Debug.Log("ShowPath: Player input enabled.");
    }

    public void ProcessPlayerInput(int clickedIndex)
    {
        Debug.Log("ShowPath: Player clicked index: " + clickedIndex);
        var click = allSquares[clickedIndex].GetComponent<ClickableSquare>();

        if (click != null && click.isOnPathThisRound)
        {
            Debug.Log("ShowPath: Clicked a correct path square.");
        }
        else
        {
            Debug.Log("ShowPath: Clicked a non-path square.");
        }

        if (currentPath[currentStep] == clickedIndex)
        {
            click?.ConfirmClick(); // ✅ Visually mark the square clicked
            activatedSquares.Add(allSquares[clickedIndex]);
            currentStep++;

            if (currentStep == currentPath.Length)
            {
                Debug.Log("ShowPath: Player completed path!");
                HandleRoundEnd(true);
            }
        }    }

    private void HandleRoundEnd(bool success)
    {
        Debug.Log("ShowPath: Round ended. Success: " + success);

        if (success)
        {
            successfulRounds++;
            progressText.text = $"Completed: {successfulRounds}";
        }

        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        Debug.Log("ShowPath: Restarting round after delay...");

        foreach (GameObject square in activatedSquares)
        {
            square.SetActive(false);
        }
        activatedSquares.Clear();

        // 🔥 Reset ALL squares
        foreach (GameObject square in allSquares)
        {
            if (square != null)
            {
                square.SetActive(true); // Squares visible again
                var click = square.GetComponent<ClickableSquare>();
                if (click != null)
                {
                    click.isOnPathThisRound = false;
                    click.ResetColor();
                }
            }
        }

        // 🔥 IMPORTANT: Reactivate the clickable grid parent
        clickableGrid.SetActive(true);

        // ✅ Hide background feedback (after the win/loss screen fades)
        yield return new WaitForSeconds(3f);
        background.SetActive(false);

        yield return new WaitForSeconds(2f);

        StartNewRound(); // Or not, depending on debug mode
    }
    
    public bool IsSquareOccupied(Vector3 position)
    {
        foreach (var square in activatedSquares)
        {
            if (Vector3.Distance(square.transform.position, position) < 0.1f)
            {
                return true;
            }
        }
        return false;
    }
    
 }