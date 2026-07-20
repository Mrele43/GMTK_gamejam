using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TaskData
{
    public string taskID;          // 唯一标识（建议与交互物体名对应）
    public string description;      // 任务描述，如“喝一杯水”
    public bool isCompleted = false;
    public float sleepinessReward = 0.15f; // 完成后增加的困意值
}
