using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "TaskListConfig", menuName = "Game/TaskListConfig")]
public class TaskListConfig : ScriptableObject
{
    public List<TaskData> tasks = new List<TaskData>();
}
