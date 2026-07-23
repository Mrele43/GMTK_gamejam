using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothesItem : ConsumableItem
{
    [Header("衣服配置")]
    public string clothesName = "衣服";

    protected override void Start()
    {
        base.Start();
        itemName = clothesName;
        maxUses = 99; // 可重复拾取搬运，不会消耗消失
        currentUses = maxUses;
    }

    // 手持左键点击无效果，需对准衣柜按E挂放
    protected override void OnUseEffect()
    {
        Debug.Log("请将衣服挂到衣柜中");
    }
}
