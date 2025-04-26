using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ColorChanger : MonoBehaviour
{
    [SerializeField] private int squareIndex;
    [SerializeField] private ShowPath showPath;

    private void OnMouseDown()
    {
        showPath.ProcessPlayerInput(squareIndex);
    }
    
}

