using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlItem : ConsumableItem
{
    [Header("碗配置")]
    public string bowlName = "脏碗";

    protected override void Start()
    {
        base.Start();
        itemName = bowlName;
        maxUses = 99; // 不会因使用消耗
        currentUses = maxUses;
    }

    // 手持左键点击无效果
    protected override void OnUseEffect()
    {
        Debug.Log("碗需要放入洗碗机，无法直接使用");
    }
}
