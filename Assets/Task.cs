using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Task : MonoBehaviour
{
    public Image checkmark;
    public TextMeshProUGUI title;
    // Start is called before the first frame update

   public  void SetTask(string t, bool complete)
    {
        title.text = t;
        checkmark.enabled = complete;
    }

}
