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
    void Start() {
            characterLocations = PlayerPrefsExtra.GetList<CharacterLocation>("characterLocations", new List<CharacterLocation>());
            Location[] locations = GetComponentsInChildren<Location>();
            for (int i = 0; i < lockableLocations.Length; i++)
            {
                foreach (var characterLocation in characterLocations)
                {
                    // Check if the character's location matches the active scene
                    if (characterLocation.location == lockableLocations[i].name)
                    {
                        for (int j = 0; j < characters.profiles.Length; j++)
                        {
                            if (characters.profiles[j].character == characterLocation.character)
                            {
                                // Assign the profile picture to the location's UI
                                lockableLocations[i].GetComponent<Button>().interactable = true;
                                lockableLocations[i].characterWaiting.sprite = characters.profiles[j].picture;
                                lockableLocations[i].characterWaiting.gameObject.SetActive(true); // Show the profile picture
                            }
                            else
                            {
//                                lockableLocations[i].GetComponent<Button>().interactable = false;
//                                lockableLocations[i].characterWaiting.gameObject.SetActive(false); // Show the profile picture
                                
                            }
                        }
                    }
                }
            }
            foreach (var t in locations)
            {
                for (int j = 0; j < characterLocations.Count; j++)
                {
                    if (t.scene == characterLocations[j].location)
                    {
                        for (int k = 0; k < characters.profiles.Length; k++)
                        {
                            if (characterLocations[j].character == characters.profiles[k].character)
                            {
                                t.characterWaiting.sprite = characters.profiles[k].picture;
                                t.characterWaiting.gameObject.SetActive(true);
                                break;
                            }
                        }
                    }
                }
            }

    }
    
}
