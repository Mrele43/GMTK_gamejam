using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConsumableItem : MonoBehaviour, IItem
{
    [Header("消耗品配置")]
    public string itemName = "消耗品";
    public int maxUses = 1;
    public int currentUses { get; protected set; }

    public string ItemName => itemName;

    protected virtual void Start()
    {
        currentUses = maxUses;
    }

    // 使用物品（由玩家调用）
    public void Use()
    {
        if (currentUses <= 0) return;
        OnUseEffect();
        currentUses--;
        if (currentUses <= 0)
        {
            // 用完消失
            Destroy(gameObject);
        }
    }

    // 子类实现具体效果
    protected abstract void OnUseEffect();

    // 丢弃（由玩家调用）
    public void Drop()
    {
        // 清除引用，物体留在场景中（可再拾取）
        // 实际由 PlayerController 处理
    }

   public GameObject GetGameObject()
   {
       // 重要：检查对象是否已被销毁（Unity 重载了 ==）
       if (this == null)
           return null;
       return gameObject;
   }
}
