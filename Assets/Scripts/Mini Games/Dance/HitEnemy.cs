using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class HitEnemy : MonoBehaviour
{
    [SerializeField] private ShowPath showPath;

    private void OnMouseDown()
    {
        showPath.FailRound();
    }
}
