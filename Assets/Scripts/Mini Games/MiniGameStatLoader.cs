using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VNEngine;

public class MiniGameStatLoader : MonoBehaviour
{
    public int gameNumber = 0;

    public int weekNumber = 0;
    string awayTeam; 
    // Start is called before the first frame update
    void Start()
    {
    }

    public string GetAwayTeam()
    {
        return awayTeam;
    }
}
