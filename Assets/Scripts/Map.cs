using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Map : MonoBehaviour
{
    public Location[] lockableLocations;
    private List<CharacterLocation> characterLocations;
    public Characters characters;
    // Start is called before the first frame update
    void Start()
    {
        characterLocations = PlayerPrefsExtra.GetList<CharacterLocation>("characterLocations", new List<CharacterLocation>());
        var allLocations = GetComponentsInChildren<Location>();

        // Step 1: Lock all lockable locations by default
        foreach (var location in lockableLocations)
        {
            location.GetComponent<Button>().interactable = false;
            location.characterWaiting.gameObject.SetActive(false);
        }

        // Step 2: Place character images and unlock matching locations
        foreach (var characterLocation in characterLocations)
        {
            // Find matching profile for the character
            var profile = Array.Find(characters.profiles, p => p.character == characterLocation.character);
            if (profile.picture == null) continue; // skip if no picture found

            foreach (var location in allLocations)
            {
                // Match either by scene or name (support both use cases)
                if (location.scene == characterLocation.location || location.name == characterLocation.location)
                {
                    location.characterWaiting.sprite = profile.picture;
                    location.characterWaiting.gameObject.SetActive(true);

                    // If this location is lockable, unlock it
                    if (Array.Exists(lockableLocations, l => l == location))
                    {
                        location.GetComponent<Button>().interactable = true;
                        Debug.Log($"Unlocked Location {location.name} with character {profile.character}.");
                    }

                    break; // match found, no need to check other locations
                }
            }
        }
    }


    
}
