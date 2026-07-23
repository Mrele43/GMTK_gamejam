using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

/// <summary>
/// 玩家控制器（含背包系统）
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private FirstPersonController fpsController;

    [Header("交互标签")]
    public string interactTag = "Interactable";

    [Header("左右手挂点")]
    [SerializeField] private Transform leftHandSocket;   // 左手挂点（持有物品）

    // 背包
    private IItem currentItem;          // 当前持有的道具（非手电）
    private GameObject currentItemModel; // 当前持有物品的模型（用于显示/隐藏）

    // 描边与提示
    private Outline _currentHighlightOutline;
    private bool interactionEnabled = true;
    private bool _lastTipState;
    private string _lastTipText = "";

    // 公开属性
    public Camera PlayerCamera => playerCamera;
    public bool IsInBed { get; private set; }

    public bool IsMakingNoise { get; private set; }

    // 内部引用
    private GameManager gameManager;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>() ?? Camera.main;

        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
            Debug.LogError("场景中未找到 GameManager！");

        // 检查挂点
        if (leftHandSocket == null)
            Debug.LogWarning("PlayerController: 左手挂点未指定！");


        Debug.Log("PlayerController 初始化完成");
    }

    // ==================== 被窝交互 ====================
    public void EnterBed()
    {
        if (IsInBed) return;
        IsInBed = true;
        gameManager?.PlayerEnterBed();
        Debug.Log("玩家进入被窝");
    }

    public void ExitBed()
    {
        if (!IsInBed) return;
        IsInBed = false;
        gameManager?.PlayerExitBed();
        Debug.Log("玩家离开被窝");
    }

    // ==================== 受伤处理 ====================
    public void TakeDamage()
    {
        // 受击打断使用并掉落道具（GDD v2.0）
        DropCurrentItem();
        gameManager?.PlayerTakesDamage();
    }

    // ==================== 背包操作 ====================

    /// <summary>
    /// 拾取物品（由 PickupItem 调用）
    /// </summary>
    public bool PickupItem(IItem item)
    {
        if (item == null) return false;
        if (!interactionEnabled) return false;

        // 如果已有道具，先丢弃原道具
        if (currentItem != null)
        {
            DropCurrentItem();
        }

        // 持有新道具
        currentItem = item;
        currentItemModel = item.GetGameObject();

        PickupItem pickup = currentItemModel.GetComponent<PickupItem>();
        if (pickup != null)
        {
            pickup.SetPickedUp(true);
        }

        // 将物品模型放到左手挂点
        if (leftHandSocket != null && currentItemModel != null)
        {
            currentItemModel.transform.SetParent(leftHandSocket);
            currentItemModel.transform.localPosition = Vector3.zero;
            currentItemModel.transform.localRotation = Quaternion.identity;
            currentItemModel.SetActive(true);
        }

        Debug.Log($"拾取 {item.ItemName}");
        return true;
    }

    /// <summary>
    /// 使用当前道具（鼠标左键）
    /// </summary>
    public void UseCurrentItem()
    {
        if (currentItem == null)
        {
            Debug.Log("当前没有道具");
            return;
        }
        if (!interactionEnabled)
        {
            Debug.Log("交互被禁用");
            return;
        }

        

        currentItem.Use();

        // 如果使用后物品被销毁，清空引用
        if (currentItem.GetGameObject() == null)
        {
            currentItem = null;
            currentItemModel = null;
        }
    }

    /// <summary>
    /// 丢弃当前道具（Q键 或 受击时调用）
    /// </summary>
    public void DropCurrentItem()
    {
        if (currentItem == null) return;

        GameObject itemObj = currentItem.GetGameObject();
        if (itemObj != null)
        {

            // --- 关键修复：先获取 GameObject 并检查是否有效
            if (currentItem.GetGameObject() == null)
            {
                // 对象已不存在（可能已被销毁），清空引用
                currentItem = null;
                currentItemModel = null;
                Debug.Log("丢弃道具失败：对象已销毁，已清空引用");
                return;
            }

            PickupItem pickup = itemObj.GetComponent<PickupItem>();
            if (pickup != null)
            {
                // 先重置状态再解绑，确保碰撞体重新启用
                pickup.SetPickedUp(false);
            }
            // 从左手挂点解绑
            itemObj.transform.SetParent(null);
            // 放到玩家脚下前方一点
            Vector3 dropPos = transform.position + Vector3.up * 0.5f + transform.forward * 1f;
            itemObj.transform.position = dropPos;
            itemObj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            itemObj.SetActive(true);
                
        }

        currentItem = null;
        currentItemModel = null;
        Debug.Log("丢弃道具");
    }


    // ==================== 输入处理 ====================

    private void Update()
    {
        // --- 交互检测（E键拾取/互动） ---
        bool hasValidTarget = RaycastInteractable(out RaycastHit hit, out Outline outline, out IInteractable interactable);
        UpdateOutline(outline);
        UpdateInteractTip(hasValidTarget && interactable != null,interactable);

        // 按E键交互（拾取或使用互动物体）
        if (interactionEnabled && Input.GetKeyDown(KeyCode.E))
        {
            if (hasValidTarget && interactable != null)
            {
                interactable.Interact();
            }
        }

        // --- 使用道具（鼠标左键） ---
        if (interactionEnabled && Input.GetMouseButtonDown(0))
        {
            UseCurrentItem();
        }


        // --- 丢弃道具（Q键） ---
        if (interactionEnabled && Input.GetKeyDown(KeyCode.Q))
        {
            DropCurrentItem();
        }

        var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        IsMakingNoise = input.sqrMagnitude > 0f;

        if (!IsMakingNoise)
        {
            return;
        }
    }

    // ==================== 辅助方法 ====================

    private bool RaycastInteractable(out RaycastHit hit, out Outline outline, out IInteractable interactable)
    {
        hit = default;
        outline = null;
        interactable = null;

        if (playerCamera == null) return false;

        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(centerRay, out hit, interactRange)) return false;

        GameObject hitObj = hit.collider.gameObject;
        if (!hitObj.CompareTag(interactTag)) return false;

        hitObj.TryGetComponent(out outline);
        hitObj.TryGetComponent(out interactable);
        return true;
    }

    private void UpdateOutline(Outline targetOutline)
    {
        if (_currentHighlightOutline == targetOutline) return;
        if (_currentHighlightOutline != null)
            _currentHighlightOutline.enabled = false;
        if (targetOutline != null)
            targetOutline.enabled = true;
        _currentHighlightOutline = targetOutline;
    }

    private void UpdateInteractTip(bool show, IInteractable interactable)
    {
        string tipText = "";
        if (show && interactable != null)
        {
            tipText = interactable.GetInteractTip();
        }
        // 状态或文字变化时才更新
        if (_lastTipState == show && _lastTipText == tipText) return;
    
        _lastTipState = show;
        _lastTipText = tipText;
    
        // 通过事件中心发送提示文字
         EventCenter.Instance.EventTrigger(E_EventType.UpdateInteractTip, tipText);
        // 发送事件（需要事先定义 E_EventType.ShowInteractTxt）
         EventCenter.Instance.EventTrigger(E_EventType.ShowInteractTxt, show);

    }

    

    // ==================== 控制开关 ====================
    public void EnableInteraction(bool enabled)
    {
        interactionEnabled = enabled;
        Debug.Log($"玩家交互 {(enabled ? "已启用" : "已禁用")}");
    }

    public void EnableControl(bool canMove)
    {
        if (fpsController != null)
            fpsController.canMove = canMove;
    }

    public Vector3 GetPosition() => transform.position;

    // 【新增公开属性】供洗碗机读取玩家手持物品
    public IItem CurrentHoldItem => currentItem;

    /// <summary>
    /// 【新增方法】直接移除并销毁手持物品（用于放入容器/上交，无掉落动画）
    /// </summary>
    public void ConsumeCurrentItem()
    {
        if (currentItem == null) return;
    
        GameObject itemObj = currentItem.GetGameObject();
        if (itemObj != null)
        {
            Destroy(itemObj);
        }
    
        currentItem = null;
        currentItemModel = null;
        Debug.Log("已上交手持物品");
    }    
}

// 接口定义（如未在其他地方定义）
public interface IInteractable
{
    void Interact();
    string GetInteractTip(); // 新增：获取交互提示文字
}

public interface IItem
{
    string ItemName { get; }
    void Use();            // 使用物品
    void Drop();           // 丢弃物品（由玩家调用）
    GameObject GetGameObject(); // 返回物品的游戏对象
}