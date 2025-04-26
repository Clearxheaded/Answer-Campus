using UnityEngine;

public class DirectionalTargeterBehavior : MonoBehaviour {
    // Rotation angles for each direction
    private Quaternion upRotation = Quaternion.Euler(0, 0, 0);
    private Quaternion downRotation = Quaternion.Euler(0, 0, 180);
    private Quaternion leftRotation = Quaternion.Euler(0, 0, 90);
    private Quaternion rightRotation = Quaternion.Euler(0, 0, 270);

    [SerializeField] private float rotationSpeed = 5f; // Speed of rotation
    private Quaternion targetRotation; // Target rotation

    [Header("Detach Settings")]
    [SerializeField] private float maxDist = 3.5f;       // Max distance to move forward
    [SerializeField] private float moveSpeed = 5f;      // Speed to move forward
    [SerializeField] private float returnSpeed = 5f;    // Speed to return to original position

    private Vector3 originalPosition; // To store the original position

    private void Start() {
        // Set the initial target rotation to the current rotation
        targetRotation = transform.rotation;
        // Store the original position
        originalPosition = transform.position;
    }

    private void Update() {
        HandleRotationInput();
        DetachPencil();
    }

    /// <summary>
    /// Handles rotation based on player input.
    /// </summary>
    private void HandleRotationInput() {
        // Check for input and set the appropriate target rotation
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            targetRotation = upRotation;
        } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            targetRotation = downRotation;
        } else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            targetRotation = leftRotation;
        } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            targetRotation = rightRotation;
        }

        // Smoothly interpolate towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Handles the detach and return behavior when holding and releasing the Space key.
    /// </summary>
    private void DetachPencil() {
        if (Input.GetKey(KeyCode.Space)) {
            // Calculate the forward direction based on current rotation
            Vector3 direction = transform.up; // up is forward direction in 2D
            Vector3 targetPosition = originalPosition + direction * maxDist;

            // Move towards the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        } else {
            // Return to the original position smoothly
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, returnSpeed * Time.deltaTime);
        }
    }
}
