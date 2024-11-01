using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParagraphManager : MonoBehaviour
{
    public string paragraph = "The cat jumped over the fence. She placed the book on the table. The sun will rise tomorrow morning.";
    public GameObject wordPrefab; // Prefab of the word to drop
    public Transform spawnPoint; // Where the word drops from

    private List<string> sentences = new List<string>();

    private void Start()
    {
        ParseParagraph();
        DisplayAndSpawnWord();
    }

    private void ParseParagraph()
    {
        // Split the paragraph into sentences
        sentences = new List<string>(paragraph.Split(new char[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries));
    }

    private void DisplayAndSpawnWord()
    {
        if (sentences.Count == 0)
        {
            Debug.LogError("No sentences to display.");
            return;
        }

        // Pick a sentence and a word to blank out
        string chosenSentence = sentences[0].Trim() + "."; // For now, pick the first sentence
        string[] wordsInSentence = chosenSentence.Split(' ');

        // Pick a word to blank out (e.g., the middle word)
        int blankIndex = wordsInSentence.Length / 2;
        string wordToBlank = wordsInSentence[blankIndex]; // Local variable

        // Replace the word with a blank
        wordsInSentence[blankIndex] = "___";
        string sentenceWithBlank = string.Join(" ", wordsInSentence);

        Debug.Log("Displaying Sentence: " + sentenceWithBlank); // Display this in your UI

        // Spawn the word at the spawn point
        if (spawnPoint != null && wordPrefab != null)
        {
            GameObject wordObject = Instantiate(wordPrefab, spawnPoint.position, Quaternion.identity);
            Word wordComponent = wordObject.GetComponent<Word>();
            if (wordComponent != null)
            {
                wordComponent.wordText = wordToBlank; // Set the word text
                wordComponent.StartFalling(); // Start the word falling

                // Register the new falling word with InputHandler
                InputHandler inputHandler = FindObjectOfType<InputHandler>();
                if (inputHandler != null)
                {
                    inputHandler.RegisterFallingWord(wordComponent);
                }
            }
        }
    }
}
