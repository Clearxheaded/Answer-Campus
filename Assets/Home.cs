using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void Quit()
    {
        SceneManager.LoadScene("Main");
    }
}
