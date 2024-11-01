using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordSpawner : MonoBehaviour
{
    public static WordSpawner Instance { get; private set; } // Singleton instance

    public GameObject wordPrefab; // Prefab of the word object to be spawned
    public Transform spawnPoint; // Point where words are spawned

    private void Awake()
    {
        // Ensure only one instance of WordSpawner exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to spawn a word object
    public GameObject SpawnWord()
    {
        if (wordPrefab == null || spawnPoint == null)
        {
            Debug.LogError("WordPrefab or SpawnPoint is not set in the WordSpawner.");
            return null;
        }

        // Instantiate the word at the spawn point
        GameObject newWord = Instantiate(wordPrefab, spawnPoint.position, Quaternion.identity);
        newWord.transform.SetParent(this.transform); // Optional: set the parent to keep hierarchy organized

        return newWord;
    }
}
