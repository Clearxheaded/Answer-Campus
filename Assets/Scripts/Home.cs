using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VNEngine;
using System.Linq;
public class Home : MonoBehaviour
{
    public GameObject phone;
    public Sprite phoneNewMessages;
    public Sprite phoneNoNewMessages;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static (int wins, int losses) GetRecord()
    {
        string json = StatsManager.Get_String_Stat("FootballSchedule");
        var wrapper = JsonUtility.FromJson<FootballGameListWrapper>(json);

        int wins = wrapper.games.Count(g => g.played && g.won == true);
        int losses = wrapper.games.Count(g => g.played && g.won == false);
        return (wins, losses);
    }

    public void Study()
    {
        SceneManager.LoadScene("Study");
    }
    public void Quit()
    {
        SceneManager.LoadScene("Main");
    }
}
