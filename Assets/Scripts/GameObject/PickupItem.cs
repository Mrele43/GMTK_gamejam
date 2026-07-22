using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour, IInteractable
{

    [SerializeField] private string interactTip = "按E拾取";
    private bool isPickedUp = false;
    private Collider itemCollider;
    private Rigidbody itemRigidbody;

    void Start()
    {
        itemCollider = GetComponent<Collider>();
        itemRigidbody = GetComponent<Rigidbody>();
    }

    public void Interact()
    {
        if (isPickedUp) return;
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            IItem item = GetComponent<IItem>();
            if (item != null)
            {
                if (player.PickupItem(item))
                {
                    SetPickedUp(true);
                }
            }
        }
    }

    public string GetInteractTip()
    {
        return isPickedUp ? "" : interactTip;
    }

     /// <summary>
    /// 设置拾取状态（由 PlayerController 调用）
    /// </summary>
    public void SetPickedUp(bool pickedUp)
    {
        isPickedUp = pickedUp;

        // 切换碰撞体
        if (itemCollider != null)
            itemCollider.enabled = !pickedUp;

        // 切换刚体（拾取时禁用物理，掉落时恢复）
        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = pickedUp;
        }

        // 如果被拾取，移除描边（避免左手持物还高亮）
        Outline outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = !pickedUp;

        Debug.Log($"物品 {gameObject.name} 拾取状态: {pickedUp}");
    }

    public bool IsPickedUp() => isPickedUp;

    // 重置（如果需要重新拾取）
    public void ResetPickup()
    {
        isPickedUp = false;
        // 如果物体被移动后需要复位，需额外处理
    }
}


