using UnityEngine;

public class Word : MonoBehaviour
{
    public string wordText;
    public float fallSpeed = 2.0f; // Speed of the word falling
    private bool isAligned = false; // Tracks if the word is aligned with the sentence

    private void Update()
    {
        // Make the word fall from top to bottom
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // Check if the word is at the alignment position (e.g., y = 0)
        if (Mathf.Abs(transform.position.y) <= 0.1f)
        {
            isAligned = true; // Word is at the right position
        }
        else
        {
            isAligned = false;
        }

        // Destroy the word if it goes off-screen (e.g., y < -10)
        if (transform.position.y < -10)
        {
            Destroy(gameObject);
        }
    }

    public bool IsAlignedWithSentence()
    {
        return isAligned;
    }

    public void OnCorrectSelection()
    {
        // Logic for when the correct word is selected
        Debug.Log("Correct selection made! Word: " + wordText);
        Destroy(gameObject);
    }

    public void StartFalling()
    {
        // Additional logic can be added here if needed
    }
}
