using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private Word currentFallingWord; // Reference to the current word that is falling
    public GameObject sentenceDisplay; // Reference to the UI element displaying the sentence

    private void Update()
    {
        if (currentFallingWord != null && Input.GetKeyDown(KeyCode.Space))
        {
            // Check if the player presses space when the word is at the correct timing
            if (currentFallingWord.IsAlignedWithSentence())
            {
                Debug.Log("Spacebar pressed at the correct time! Word: " + currentFallingWord.wordText);
                GameManager.Instance.UpdateScore(10); // Award points for correct timing
                currentFallingWord.OnCorrectSelection();
            }
            else
            {
                Debug.Log("Spacebar pressed but not at the right time.");
            }
        }
    }

    public void RegisterFallingWord(Word word)
    {
        // Assign the current falling word
        currentFallingWord = word;
    }
}
