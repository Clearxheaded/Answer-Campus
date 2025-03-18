/*
 * FollowMouseAroundCircle - allows player to control the targeter's rotation according to their mouse's position  
 */

using UnityEngine;

public class FollowMouseAroundCircle : MonoBehaviour
{
    public Transform mainCircle;           // Center Circle to rotate around
    public float distanceFromCenter = 0f;  // Radius of the circle, set to 0 because center circle and targeter are aligned

    void Update()
    {
        // Get the mouse position in world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;  // Set z to 0 for 2D

        // Calculate the direction from the main circle to the mouse
        Vector3 direction = (mousePosition - mainCircle.position).normalized;

        // Position the targeter at the correct distance along the calculated direction
        transform.position = mainCircle.position + direction * distanceFromCenter;

        // Rotate the targeter to face outward from the center
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90); 
    }
}
