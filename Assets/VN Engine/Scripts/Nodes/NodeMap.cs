using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace VNEngine
{
    // Not used in real code. Merely a template to copy and paste from when creating new nodes.
    public class NodeMap : Node
    {
        public Character character;
        public string locationScene;
        public bool addLocationToMap = true;

        // Called initially when the node is run, put most of your logic here
        public override void Run_Node()
        {

            List<CharacterLocation> characterLocations = PlayerPrefsExtra.GetList<CharacterLocation>("characterLocations", new List<CharacterLocation>());
            CharacterLocation characterLocation = new CharacterLocation
            {
                character = character,
                location = locationScene
            };
            if (!characterLocations.Contains(characterLocation))
            {
                Debug.Log("Attempting to add character pin to Map : " + character + " at " + locationScene);
                if (addLocationToMap)
                {
                    Debug.Log($"{character} added to Map");
                    characterLocations.Add(characterLocation);
                }
                else
                {
                    for (int i = 0; i < characterLocations.Count; i++)
                    {
                        if(locationScene == characterLocations[i].location && character == characterLocations[i].character)
                        {
                            Debug.LogWarning($"{character} already added to map");
                            characterLocations.RemoveAt(i);
                            break;
                        }
                    }
                }
                Debug.Log($"Setting list of character locations");
                PlayerPrefsExtra.SetList("characterLocations", characterLocations);
            }
            else
            {
                Debug.Log("Duplicate Character Location " + characterLocation);

            }

            Finish_Node();
        }


        // What happens when the user clicks on the dialogue text or presses spacebar? Either nothing should happen, or you call Finish_Node to move onto the next node
        public override void Button_Pressed()
        {
            //Finish_Node();
        }


        // Do any necessary cleanup here, like stopping coroutines that could still be running and interfere with future nodes
        public override void Finish_Node()
        {
            StopAllCoroutines();

            base.Finish_Node();
        }
    }
}