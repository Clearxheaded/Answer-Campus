using UnityEngine;
using UnityEngine.UI;
using TMPro;
public enum Direction { Up, Left, Right }

public class RhythmGame : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform movingCircle;  // The moving circle UI element
    public RectTransform targetZone;    // The target zone UI element (should be centered)
    public TextMeshProUGUI directionText;          // UI Text element to display target direction
    public Cheer cheer;
    [Header("Movement Settings")]
    public float speed = 200f;          // Movement speed in pixels per second
    public float hitZoneRadius = 50f;   // Acceptable distance (in pixels) for a valid hit

    private Direction currentDirection;

    private RectTransform canvasRect;
    private float canvasWidth;

    // Calculated x positions for reset
    private float startX;
    private float endX;
    private bool hasCheered = false;
    void Start()
    {
        
        // Get the Canvas's RectTransform (assumes the movingCircle is under a Canvas)
        canvasRect = movingCircle.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        canvasWidth = canvasRect.rect.width;

        // For a Canvas with pivot at (0.5, 0.5):
        // The left edge is at -canvasWidth/2 and the right edge is at canvasWidth/2.
        // With the movingCircle pivot at center, its left edge is (anchoredPosition.x - width/2)
        // We set startX so that the entire movingCircle is off the left side,
        // and endX so that it resets only after it has moved completely off the right side.
        float margin = 10f; // extra margin if needed
        startX = -canvasWidth / 2 - movingCircle.rect.width / 2 - margin;
        endX = canvasWidth / 2 + movingCircle.rect.width / 2 + margin;

        // Set the movingCircle's initial position
        movingCircle.anchoredPosition = new Vector2(startX, movingCircle.anchoredPosition.y);
        SetRandomDirection();
    }

    void Update()
    {
        // Move the movingCircle to the right
        movingCircle.anchoredPosition += Vector2.right * speed * Time.deltaTime;

        // Reset once the movingCircle has completely left the right side of the screen
        // Here, we check if its left edge is past the right edge of the canvas.
        if (movingCircle.anchoredPosition.x > canvasWidth)
        {
            movingCircle.anchoredPosition = new Vector2(startX, movingCircle.anchoredPosition.y);
            SetRandomDirection();
            hasCheered = false;
        }
        float centerX = canvasRect.rect.width / 2;
        float horizontalDistance = Mathf.Abs(movingCircle.anchoredPosition.x - centerX);

        if(horizontalDistance <= hitZoneRadius && !hasCheered) {
            Debug.Log("CHEER! " + currentDirection);
            cheer.Move(currentDirection);
            hasCheered = true;

        }

        // Check for arrow key input
        Direction? pressedDirection = null;
        if (Input.GetKeyDown(KeyCode.UpArrow))
            pressedDirection = Direction.Up;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            pressedDirection = Direction.Left;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            pressedDirection = Direction.Right;

        if (pressedDirection.HasValue)
        {
            // Calculate distance between the movingCircle and the target zone (center)
            
            Debug.Log($"{horizontalDistance} from center");
            if (horizontalDistance <= hitZoneRadius)

            {
                if (pressedDirection.Value == currentDirection)
                {
                    Debug.Log("Perfect! Correct direction!");
                    // Add animations, score updates, sound effects, etc.
                }
                else
                {
                    Debug.Log("Wrong direction!");
                }
            }
            else
            {
                Debug.Log("Too early or too late!");
            }
        }
    }

    // Choose a new random direction and update the UI text
    void SetRandomDirection()
    {
        int randomIndex = Random.Range(0, 3); // 0, 1, or 2
        currentDirection = (Direction)randomIndex;
        if (directionText != null)
        {
            directionText.text = currentDirection.ToString();
        }
    }
}
