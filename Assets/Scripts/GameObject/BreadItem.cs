using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreadItem : ConsumableItem
{
    [Header("面包配置")]
    public string itemID = "Bread";

    protected override void Start()
    {
        base.Start();
        itemName = "面包";
        maxUses = 1; // 只能吃一次
    }
    protected override void OnUseEffect()
    {
        // ----- 关键：通知任务系统使用了面包 -----
        TaskManager.Instance.NotifyUseItem(itemID);

        // 可以添加其他效果（如恢复体力、播放动画等）
        Debug.Log("吃掉了面包！");

        // 播放食用音效（可选）
        // AudioMgr.Instance.PlaySFX("Eat");
    }

}
