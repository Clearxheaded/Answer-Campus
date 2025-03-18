using UnityEngine;

/// <summary>
/// Handles the motion of a single letter from its spawn point to the assigned box.
/// Also listens for collisions with the eraser (if you use the eraser logic).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LetterMovement : MonoBehaviour {
    private FivePositionsGameManager manager;
    private int boxIndex;
    private char letterChar;
    private Vector3 targetPos;
    private float moveSpeed;
    public AudioClip eraserClip;

    /// <summary>
    /// Called right after instantiating a Letter prefab,
    /// sets up everything needed for it to move and know its manager.
    /// </summary>
    public void Initialize(
        FivePositionsGameManager manager,
        int boxIndex,
        char letterChar,
        Vector3 targetPos,
        float moveSpeed
    ) {
        this.manager = manager;
        this.boxIndex = boxIndex;
        this.letterChar = letterChar;
        this.targetPos = targetPos;
        this.moveSpeed = moveSpeed;
    }

    private void Update() {
        // Move letter towards the box
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        // If close enough to the box, inform the manager
        if (Vector3.Distance(transform.position, targetPos) < 0.01f) {
            manager.OnLetterArrived(boxIndex, letterChar, gameObject);
        }
    }

    /// <summary>
    /// If the eraser hits this letter, destroy it immediately.
    /// (Assumes your pencil/eraser flips have colliders with tag "EraserCollider" or similar).
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("EraserCollider")) {

            if (AudioManager.Instance != null && eraserClip != null) {
                AudioManager.Instance.PlaySFX(eraserClip);
            }
            Destroy(gameObject);
        }
    }
}
