using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class CharacterChoice : MonoBehaviour
{
    public GameObject firstSelectedCharacter;
    // Start is called before the first frame update
    void Start()
    {
        if(firstSelectedCharacter)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedCharacter);
        }
    }

}
