using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableSquare : MonoBehaviour
{
    private int index;
    private ShowPath showPath;
    [SerializeField] private SpriteRenderer squareRenderer;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color activeColor = Color.yellow;
    [SerializeField] private Color clickedColor = Color.green; 
    public bool isOnPathThisRound { get; set; } = false;

    public void Initialize(int assignedIndex, ShowPath pathManager)
    {
        index = assignedIndex;
        showPath = pathManager;
        squareRenderer = GetComponent<SpriteRenderer>();
        ResetColor();
        isOnPathThisRound = false; // Always reset at start
    }
    public void ConfirmClick()
    {
        if (squareRenderer != null)
            squareRenderer.color = clickedColor;
    }

    public void Flash()
    {
        if (squareRenderer != null)
            squareRenderer.color = activeColor;
    }

    public void ResetColor()
    {
        if (squareRenderer != null)
            squareRenderer.color = defaultColor;
    }

    private void OnMouseDown()
    {
        showPath.ProcessPlayerInput(index);
    }
}
