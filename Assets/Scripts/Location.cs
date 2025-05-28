using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using VNEngine;
using UnityEngine.UI;

public class Location : MonoBehaviour
{
    public string scene;
//    public string conversation;
    public string timestamp;
    public int minutes;
    public bool played = false;
    public Image characterWaiting;

    private void Start()
    {
    }
    public void GoToLocation()
    {
  
        List<CharacterLocation> characterLocations = PlayerPrefsExtra.GetList<CharacterLocation>("characterLocations", new List<CharacterLocation>());
  /*
        int locationIndex = -1;
  
        for(int i = 0; i < characterLocations.Count; i++) {
            if (characterLocations[i].location == scene)
            {
                locationIndex = i;
            }

        }
        if (locationIndex > -1)
        {
            Debug.Log("Removing character location at index: " + locationIndex + " for scene: " + scene);
            characterLocations.RemoveAt(locationIndex);
        }
        else
        {
            Debug.LogWarning("No matching character location found for scene: " + scene);
        }
*/
        PlayerPrefsExtra.SetList<CharacterLocation>("characterLocations", characterLocations);

        // Retrieve the messages list from PlayerPrefsExtra

        List<TextMessage> messages = PlayerPrefsExtra.GetList<TextMessage>("messages", new List<TextMessage>());

        messages = PlayerPrefsExtra.GetList<TextMessage>("messages", new List<TextMessage>());

        // Find all characters associated with the specified scene
        List<string> charactersToRemove = new List<string>();

        foreach (TextMessage message in messages)
        {
            if (message.location == scene)
            {
                charactersToRemove.Add(message.from.ToString());  // Collect all characters from matching scenes
            }
        }

        // Remove all messages where the character is in the list of characters to remove
        messages.RemoveAll(message => charactersToRemove.Contains(message.from.ToString()));

        // Save the updated list back to PlayerPrefsExtra
        PlayerPrefsExtra.SetList("messages", messages);
        //        PlayerPrefs.SetString("Current Conversation", conversation);
        if (StatsManager.Boolean_Stat_Exists("Minutes Passed"))
        {
            StatsManager.Add_To_Numbered_Stat("Minutes Passed", minutes);
        }
        else
        {
            StatsManager.Set_Numbered_Stat("Minutes Passed", minutes);
        }

        if(StatsManager.String_Stat_Exists("Random Encounter"))
        {
            if(StatsManager.Get_String_Stat("Random Encounter").Contains("None")) {
                //already visited
            }
            else
            {
                scene = StatsManager.Get_String_Stat("Random Encounter");
            }
            StatsManager.Set_String_Stat("Random Encounter", "None");
            Debug.Log("Random Encounter Activated: " + scene + " is new location.");

        }

        SceneManager.LoadScene(scene);

    }
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
    public void SetTimestamp()
    {
        timestamp = DateTime.Now.ToString("f");
    }

    public void ChooseScene(int value)
    {
        Debug.Log("Selected scene: " + value);
     
        switch (value)
        {
            case 0:
                //orientation
                Orientation();
                break;
            case 1:
                //competition
                Competition();
                break;
            case 2:
                //dorm
                Dorm();
                break;
            case 3:
                //in the text
                InTheText();
                break;
            case 4:
                //office hours
                OfficeHours();
                break;
            case 5:
                //eric
                Eric();
                break;
            case 6:
                //limp
                Limp();
                break;
            case 7:
                //eyes
                Eyes();
                break;
            case 8:
                //relationships
                Relationships();
                break;
            case 9:
                //compliment
                JustACompliment();
                break;
            case 10:
                //study hall
                StudyHall();
                break;
            case 11:
                //hair
                CanITouchYourHair();
                break;
            case 12:
                //commercial
                Commercial();
                break;
            case 13:
                //deepak
                DeepakPerforms();
                break;
            case 14:
                //art project
                ArtProject();
                break;
            case 15:
                //bro down
                BroDown();
                break;

        }
        GoToLocation();
    }


    //ALWAYS
    public void Orientation()
    {
        //STUDENT CENTER
        scene = "Student Center";
    }

    public void Competition(bool withFriends = false)
    {
        StatsManager.Set_Boolean_Stat("Competition", true);
        //GREEN
        scene = "Green";
    }

    public void Dorm()
    {
        //HOME
        scene = "Home";
    }


    //ONE OFFS
    public void OfficeHours()
    {
        //LECTURE HALL
        scene = "Lecture Hall";
    }

    public void LeaveWithLeilani()
    {
        StatsManager.Set_Boolean_Stat("Eric", false);
        //OUTSIDE CLASS
        scene = "Outside Class";

    }
    public void Eric()
    {
        StatsManager.Set_Boolean_Stat("Eric", true);
        //OUTSIDE CLASS
        scene = "Outside Class";

    }

    //CONFLICT CONVERSATIONS

    public void InTheText()
    {
        StatsManager.Set_Boolean_Stat("In The Text", true);
        //LECTURE HALL
        scene = "Lecture Hall";

    }

    public void Limp()
    {
        StatsManager.Set_Boolean_Stat("Limp", true);
        //GREEN
        scene = "Green";
    }


    public void Eyes()
    {
        StatsManager.Set_Boolean_Stat("Eyes", true);
        //APARTMENT
        scene = "Apartment";

    }

    public void Relationships()
    {
        StatsManager.Set_Boolean_Stat("Relationships", true);
        //APARTMENT
        scene = "Apartment";

    }

    public void CanITouchYourHair()
    {
        StatsManager.Set_Boolean_Stat("Can I Touch Your Hair", true);
        //DINING HALL
        scene = "Dining Hall";

    }
    public void JustACompliment()
    {
        StatsManager.Set_Boolean_Stat("Just a Compliment", true);
        //LIBRARY
        scene = "Library";

    }

    public void StudyHall()
    {
        StatsManager.Set_Boolean_Stat("Study Hall", true);
        //STUDENT CENTER
        scene = "Student Center";
    }

    public void RoundTable()
    {
        StatsManager.Set_Boolean_Stat("Round Table", true);
        //GREEN
        scene = "Green";
    }

    //ENDINGS

    public void Commercial()
    {
        //COMMERCIAL
        scene = "Commercial";
    }

    public void DeepakPerforms()
    {
        StatsManager.Set_Boolean_Stat("Deepak Performs", true);
        //GREEN
        scene = "Green";

    }

    public void ArtProject()
    {
        StatsManager.Set_Boolean_Stat("Art Project", true);
        //GREEN
        scene = "Green";
    }

    public void BroDown()
    {
        StatsManager.Set_Boolean_Stat("Bro Down", true);
        //DINING HALL
        scene = "Dining Hall";

    }

}
