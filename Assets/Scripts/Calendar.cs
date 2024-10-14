using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VNEngine;
using TMPro;

public class Calendar : MonoBehaviour
{
    public TextMeshProUGUI month;
    public int week;
    // Start is called before the first frame update
    void Start()
    {
        if(StatsManager.Numbered_Stat_Exists("week"))
        {
            week = (int)StatsManager.Get_Numbered_Stat("week");
        }
        SetMonth(week);
    }

    void SetMonth(int w)
    {
        switch (w) {
/* ORIENTATION */
            case 0:
                month.text = "August";
                break;
            /* FALL - SEPTEMBER */
            case 1:
            case 2:
            case 3:
                month.text = "September";
                break;
            case 4:
            case 5:
            case 6:
/* MIDTERMS */
            case 7:
                month.text = "October";
                break;
            case 8:
            case 9:
            case 10:
                month.text = "October";
                break;
            case 11:
            case 12:
            case 13:
            case 14:
                month.text = "November";
                break;
            case 15:

/* FINALS - DECEMBER */
            case 16:
                month.text = "December";
                break;

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
