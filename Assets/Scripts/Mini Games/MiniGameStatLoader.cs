using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VNEngine;

public class MiniGameStatLoader : MonoBehaviour
{
    public int gameNumber = 0;

    public int weekNumber = 0;
    // Start is called before the first frame update
    void Start()
    {
        if(StatsManager.Numbered_Stat_Exists("Week"))
        {
            weekNumber = (int)StatsManager.Get_Numbered_Stat("Week");
        }
        else
        {
            weekNumber = 0;
        }

        if(StatsManager.Numbered_Stat_Exists("Game"))
        {
            gameNumber = (int)StatsManager.Get_Numbered_Stat("Game");
        }
        else
        {
            gameNumber = 0;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
