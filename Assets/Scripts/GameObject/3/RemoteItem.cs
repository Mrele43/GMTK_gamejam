using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteItem : ConsumableItem
{
    [Header("遥控器配置")]
    public string remoteName = "电视遥控器";

    protected override void Start()
    {
        base.Start();
        itemName = remoteName;
        maxUses = 99; // 可重复使用，不会消耗消失
        currentUses = maxUses;
    }

    // 手持左键直接点击无效果，需对准电视按E交互
    protected override void OnUseEffect()
    {
        Debug.Log("请对准电视后按E使用遥控器");
        //TaskManager.Instance.
    }
}
