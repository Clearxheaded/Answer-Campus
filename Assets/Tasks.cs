using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tasks : MonoBehaviour
{
    public GameObject task;
    // Start is called before the first frame update
    void Start()
    {
        if(TaskManager.Instance)
        {
            List<string> incompleteTasks = TaskManager.Instance.GetAssignedTasks();
            List<string> completedTasks = TaskManager.Instance.GetCompletedTasks();

            for (int i = 0; i < completedTasks.Count; i++)
            {
                GameObject t = Instantiate(task, transform);
                t.GetComponent<Task>().SetTask(completedTasks[i], true);
            }


            for (int i = 0; i < incompleteTasks.Count; i++)
            {
                GameObject t = Instantiate(task, transform);
                t.GetComponent<Task>().SetTask(incompleteTasks[i], false);
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
