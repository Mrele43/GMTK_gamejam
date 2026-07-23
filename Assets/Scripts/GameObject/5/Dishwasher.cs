using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dishwasher : MonoBehaviour, IInteractable
{
    [Header("洗碗机配置")]
    [Tooltip("洗碗机内部的碗模型预制体")]
    public GameObject bowlInsidePrefab;
    [Tooltip("所有碗挂点（拖入5个Transform）")]
    public Transform[] bowlSlots;
    [Tooltip("有空位时的交互提示")]
    public string emptyTip = "按E放入脏碗";
    [Tooltip("全部放满后的交互提示")]
    public string fullTip = "洗碗机已放满";

    private GameObject[] _insideBowls; // 每个槽位对应的碗实例
    private int _filledCount = 0;      // 已放入的碗数量
    private PlayerController _player;

    void Start()
    {
        _player = FindObjectOfType<PlayerController>();
        // 按挂点数量初始化槽位数组
        _insideBowls = new GameObject[bowlSlots.Length];
    }

    public void Interact()
    {
        // 已满，直接终止交互
        if (IsFull)
        {
            Debug.Log("洗碗机已全部放满");
            return;
        }

        // 检查玩家是否手持碗
        if (_player == null || !(_player.CurrentHoldItem is BowlItem))
        {
            Debug.Log("需要手持脏碗才能放入");
            return;
        }

        // 找到第一个空槽
        int emptyIndex = GetFirstEmptySlot();
        if (emptyIndex == -1) return;

        // 消耗玩家手上的碗（无掉落动画，直接销毁）
        _player.ConsumeCurrentItem();

        // 在对应槽位生成碗模型
        if (bowlInsidePrefab != null && bowlSlots[emptyIndex] != null)
        {
            GameObject bowl = Instantiate(bowlInsidePrefab, bowlSlots[emptyIndex]);
            bowl.transform.localPosition = Vector3.zero;
            bowl.transform.localRotation = Quaternion.identity;
            
            // 禁用内部碗的交互和碰撞，避免干扰射线检测
            PickupItem pickup = bowl.GetComponent<PickupItem>();
            if (pickup != null) pickup.enabled = false;
            Collider col = bowl.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            _insideBowls[emptyIndex] = bowl;
        }

        _filledCount++;
        Debug.Log($"放入脏碗，当前进度：{_filledCount}/{bowlSlots.Length}");

        // 全部放满时触发（可在此接入任务完成逻辑）
        if (IsFull)
        {
            Debug.Log("洗碗机已全部放满");
            // 如需通知外部系统，可通过事件中心发送事件
            // TaskManager.Instance.NotifyDishwasherFull();
        }
    }

    // 获取第一个空槽的索引，无空位返回-1
    private int GetFirstEmptySlot()
    {
        for (int i = 0; i < _insideBowls.Length; i++)
        {
            if (_insideBowls[i] == null)
                return i;
        }
        return -1;
    }

    // 公开属性：是否全部放满（供外部任务/逻辑读取）
    public bool IsFull => _filledCount >= bowlSlots.Length;

    // 公开属性：当前已放数量
    public int FilledCount => _filledCount;

    public string GetInteractTip()
    {
        return IsFull ? fullTip : emptyTip;
    }
}
