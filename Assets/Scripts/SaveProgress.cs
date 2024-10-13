using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VNEngine;


public class SaveProgress : MonoBehaviour
{
    [System.Serializable]
    public struct CharacterRelationship
    {
        public string character;
        public Relationship relationship;

    }

    public string nextScene;

    [SerializeField]
    public CharacterRelationship[] relationships;


    public void StartNewGame(GameObject newGamePanelOverride)
    {
        
        if (PlayerPrefs.HasKey("Leilani"))
        {
            newGamePanelOverride.SetActive(true);
        }
        else
        {
            Debug.Log("Reseting player prefs...");
            Reset();
            Debug.Log("Saving friendship player prefs...");
            Save();
            Debug.Log("Loading Cutscene...");
            GetComponent<MenuOptions>().LoadScene(GetComponent<MenuOptions>().sceneToLoad);
           
        }
    }
    public void SetNextScene()
    {
        PlayerPrefs.SetString("Next Scene", nextScene);
        Debug.Log("Next Scene: " + nextScene);
    }

    public void UpdateFriendship()
    {
        for(int i = 0; i < relationships.Length; i++)
        {
            PlayerPrefs.SetInt(relationships[i].character, (int)relationships[i].relationship);

        }
    }

    public void Reset()
    {
        StatsManager.Clear_All_Stats();
        PlayerPrefs.DeleteAll();
    }

    public void Save()
    {
        //CALLLED WHEN CLICKING ON CHOICE
        Debug.Log("Saving Progress...");
        UpdateFriendship();
        SetNextScene();
    }
}
