using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MonsterReplacementData
{
    [Header("房间与阈值")]
    public RoomType room;
    [Range(0, 100)] public float awakeningThreshold = 70f;

    [Header("场景物品")]
    public GameObject dormantItem;        // 场景中的普通物品

    [Header("地图外存储点")]
    public Transform offscreenPoint;      // 用于存放物品和怪物的隐藏位置

    [Header("怪物预制体与配置")]
    public GameObject monsterPrefab;
    public MonsterConfig monsterConfig;

    // 运行时数据
    [NonSerialized] public GameObject currentMonsterInstance;
    [NonSerialized] public bool isActive = false;
    [NonSerialized] public Transform itemParent;
    [NonSerialized] public Vector3 itemLocalPos;
    [NonSerialized] public Quaternion itemLocalRot;
    
}

public enum RoomType
{
    LivingRoom,   // 客厅
    Bathroom,     // 厕所
    Bedroom       // 卧室
}


