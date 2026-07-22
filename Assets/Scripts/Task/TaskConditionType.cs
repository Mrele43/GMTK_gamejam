using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TaskConditionType
{
    EnterArea,          // 进入指定区域（Trigger）
    PickupItem,         // 拾取指定物品
    UseItem,            // 使用指定物品（如吃面包）
    InteractObject,     // 与指定物体交互（如开冰箱）
    Delay,              // 等待指定时间
    CustomEvent,        // 自定义事件触发（扩展用）
    None                // 无条件（自动完成）
}
