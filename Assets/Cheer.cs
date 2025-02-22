using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct CharacterDirection
{
    public Direction direction;
    public Sprite image;

}

public class Cheer : MonoBehaviour
{
    public CharacterDirection[] characterDirections;
    public Image character;
    public void Move (Direction _direction)
    {
        for(int i = 0; i < characterDirections.Length; i++)
        {
            if(characterDirections[i].direction == _direction)
            {
                character.sprite = characterDirections[i].image;
                break;
            }
        }
    }
}
