using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wardrobe : MonoBehaviour, IInteractable
{
    [Header("衣柜配置")]
    [Tooltip("衣柜内部的衣服展示模型预制体")]
    public GameObject clothesInsidePrefab;
    [Tooltip("衣柜内部衣服挂放挂点")]
    public Transform clothesSlot;
    [Tooltip("空置状态交互提示")]
    public string emptyTip = "按E挂放衣服";
    [Tooltip("已挂衣服后的交互提示")]
    public string fullTip = "衣柜已挂有衣服";

    private bool _hasClothes = false;
    private GameObject _insideClothes;
    private PlayerController _player;

    void Start()
    {
        _player = FindObjectOfType<PlayerController>();
    }

    public void Interact()
    {
        // 已有衣服，直接终止交互
        if (_hasClothes)
        {
            Debug.Log("衣柜里已经挂有衣服了");
            return;
        }

        // 校验：玩家必须手持衣服
        if (_player == null || !(_player.CurrentHoldItem is ClothesItem))
        {
            Debug.Log("需要手持衣服才能挂放");
            return;
        }

        // 消耗玩家手上的衣服（直接销毁，无掉落动画）
        _player.ConsumeCurrentItem();

        // 在衣柜挂点生成衣服展示模型
        if (clothesInsidePrefab != null && clothesSlot != null)
        {
            _insideClothes = Instantiate(clothesInsidePrefab, clothesSlot);
            _insideClothes.transform.localPosition = Vector3.zero;
            _insideClothes.transform.localRotation = Quaternion.identity;
            
            // 禁用内部模型的交互与碰撞，避免干扰射线检测
            PickupItem pickup = _insideClothes.GetComponent<PickupItem>();
            if (pickup != null) pickup.enabled = false;
            Collider col = _insideClothes.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        _hasClothes = true;
        Debug.Log("已将衣服挂入衣柜");
        
        // 如需通知任务系统，可在此发送事件
        // EventCenter.Instance.EventTrigger(E_EventType.ClothesHanged);
    }

    // 公开状态，供外部任务/剧情读取
    public bool HasClothes => _hasClothes;

    public string GetInteractTip()
    {
        return _hasClothes ? fullTip : emptyTip;
    }
}
