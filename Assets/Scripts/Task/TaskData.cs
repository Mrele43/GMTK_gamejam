using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TaskData
{
    public string taskID;              // 唯一标识
    public string taskName;            // 任务名称（显示用）
    public string description;         // 描述（可选）
    public TaskConditionType conditionType;  // 条件类型
    public string targetID;            // 关联物体ID
    public float sleepinessReward = 0.1f;   // 完成后增加困意
    public bool isCompleted = false;   // 是否已完成
}
