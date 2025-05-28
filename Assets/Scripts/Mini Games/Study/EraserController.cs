using UnityEngine;

public class EraserController : MonoBehaviour {
    [Header("Box References")]
    [Tooltip("Assign the same box Transforms that letters aim for.")]
    public Transform[] boxPositions;  // We'll take their X, keep y = -1.55

    [Header("Movement Settings")]
    [Tooltip("How fast the eraser moves between positions.")]
    public float moveSpeed = 5f;

    [Tooltip("The Y value where the eraser should remain.")]
    public float eraserZ = 1f;

    private Vector3[] eraserPositions;  // Will be generated from boxPositions
    private int targetIndex = 0;        // Which position we're moving toward

    private void Start() {
        // Auto-generate eraserPositions from boxPositions
        if (boxPositions == null || boxPositions.Length == 0) {
            Debug.LogError("EraserController: No box positions assigned.");
            return;
        }

        eraserPositions = new Vector3[boxPositions.Length];

        // Use each box's X-value, but fix the Y at eraserY and Z at eraserZ
        for (int i = 0; i < boxPositions.Length; i++) {
            float boxX = boxPositions[i].position.x;
            eraserPositions[i] = new Vector3(boxX, transform.position.y, eraserZ);
        }

        // Initialize eraser at the first position
        transform.position = eraserPositions[targetIndex];
    }

    private void Update() {
        if (eraserPositions == null || eraserPositions.Length == 0) return;

        HandleInput();
        MoveToTarget();
    }

    /// <summary>
    /// Checks player input (left/right) and updates the targetIndex accordingly.
    /// </summary>
    private void HandleInput() {
        // Move left (A or LeftArrow)
        if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && targetIndex > 0) {
            targetIndex--;
        }
        // Move right (D or RightArrow)
        else if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && targetIndex < eraserPositions.Length - 1) {
            targetIndex++;
        }
    }

    /// <summary>
    /// Smoothly moves in a straight line from current position to eraserPositions[targetIndex].
    /// </summary>
    private void MoveToTarget() {
        Vector3 currentPos = transform.position;
        Vector3 goalPos = eraserPositions[targetIndex];

        // Move the eraser in a straight line at approximately 'moveSpeed' units/second
        transform.position = Vector3.MoveTowards(
            currentPos,
            goalPos,
            moveSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Called when another object (e.g. a Letter) enters this collider.
    /// If it�s tagged �Letter,� we destroy it.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Letter")) {
            Destroy(other.gameObject);
            // Optional: add SFX or other feedback here
        }
    }
}
