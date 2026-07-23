using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterReplacementManager : BaseMonoMgr<MonsterReplacementManager>
{
    [Header("怪物替换列表（每个房间一个）")]
    [SerializeField] private List<MonsterReplacementData> _replacements = new List<MonsterReplacementData>();

    // 当前激活的怪物列表（可同时激活多个）
    private List<MonsterReplacementData> _activeMonsters = new List<MonsterReplacementData>();

    // 事件
    public System.Action<EnemyAI> OnMonsterActivated;     // 怪物出现在场景中（等待激活）
    public System.Action<EnemyAI> OnMonsterDeactivated;   // 怪物消失，还原为物品

    protected override void OnInit()
    {
        base.OnInit();

        // 预实例化所有怪物到地图外
        PreInstantiateMonsters();

        // 订阅困意变化事件
        if (SleepinessManager.Instance != null)
        {
            SleepinessManager.Instance.OnSleepinessChanged += OnSleepinessChanged;
        }
    }

    private void PreInstantiateMonsters()
    {
        foreach (var data in _replacements)
        {
            if (data.monsterPrefab == null || data.offscreenPoint == null)
            {
                Debug.LogWarning($"怪物替换数据缺少预制体或隐藏点: {data.room}");
                continue;
            }

            // 实例化怪物到地图外，初始隐藏
            GameObject monster = Instantiate(data.monsterPrefab, data.offscreenPoint.position, Quaternion.identity);
            monster.SetActive(false);
            data.currentMonsterInstance = monster;

            // 应用配置
            EnemyAI ai = monster.GetComponent<EnemyAI>();
            if (ai != null && data.monsterConfig != null)
            {
                ai.SetConfig(data.monsterConfig);
            }

            data.isActive = false;
        }
        Debug.Log($"预实例化 {_replacements.Count} 个怪物到地图外");
    }

    private void OnSleepinessChanged(float sleepiness)
    {
        foreach (var data in _replacements)
        {
            // 如果已经激活则跳过
            if (data.isActive) continue;

            float threshold = data.awakeningThreshold / 100f;
            if (sleepiness >= threshold)
            {
                // 执行替换
                ReplaceItemWithMonster(data);
            }
        }
    }

    private void ReplaceItemWithMonster(MonsterReplacementData data)
    {
        if (data.dormantItem == null || data.currentMonsterInstance == null)
        {
            Debug.LogWarning($"替换失败: {data.room} 的物品或怪物实例为空");
            return;
        }

        // ---- 1. 保存物品状态 ----
        Transform itemT = data.dormantItem.transform;
        data.itemParent = itemT.parent;
        data.itemLocalPos = itemT.localPosition;
        data.itemLocalRot = itemT.localRotation;

        // ---- 2. 将物品移到地图外 ----
        itemT.SetParent(data.offscreenPoint);
        itemT.localPosition = Vector3.zero;
        itemT.localRotation = Quaternion.identity;
        data.dormantItem.SetActive(false);

        // ---- 3. 将怪物移到物品位置 ----
        GameObject monster = data.currentMonsterInstance;
        monster.transform.SetParent(data.itemParent);
        monster.transform.localPosition = data.itemLocalPos;
        monster.transform.localRotation = data.itemLocalRot;
        monster.SetActive(true);

        // ---- 4. 初始化怪物AI ----
        EnemyAI ai = monster.GetComponent<EnemyAI>();
        if (ai != null)
        {
            // 设置玩家和 GameManager 引用
            PlayerController pc = FindObjectOfType<PlayerController>();
            GameManager gm = FindObjectOfType<GameManager>();
            ai.SetPlayer(pc);
            ai.SetGameManager(gm);

            // 设置为“等待玩家注视”状态
            ai.SetStateToWaiting();

            // 订阅怪物消失事件（当怪物攻击后或玩家进被窝时触发）
            ai.OnMonsterDeactivated += () => RestoreItemFromMonster(data);
        }

        data.isActive = true;
        _activeMonsters.Add(data);

        OnMonsterActivated?.Invoke(ai);
        Debug.Log($"? [{data.room}] 物品替换为怪物 (困意: {SleepinessManager.Instance.CurrentSleepiness:P0})");
    }

    public void RestoreItemFromMonster(MonsterReplacementData data)
    {
        if (!data.isActive) return;

        // ---- 1. 怪物移回地图外 ----
        GameObject monster = data.currentMonsterInstance;
        monster.SetActive(false);
        monster.transform.SetParent(data.offscreenPoint);
        monster.transform.localPosition = Vector3.zero;
        monster.transform.localRotation = Quaternion.identity;

        EnemyAI ai = monster.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.ResetToDormant();
            ai.OnMonsterDeactivated -= () => RestoreItemFromMonster(data);
        }

        // ---- 2. 物品移回原位 ----
        data.dormantItem.SetActive(true);
        data.dormantItem.transform.SetParent(data.itemParent);
        data.dormantItem.transform.localPosition = data.itemLocalPos;
        data.dormantItem.transform.localRotation = data.itemLocalRot;

        data.isActive = false;
        _activeMonsters.Remove(data);

        OnMonsterDeactivated?.Invoke(ai);
        Debug.Log($"? [{data.room}] 怪物消失，还原为物品");
    }

    // 查询接口
    public bool IsMonsterActive(RoomType room)
    {
        var data = _replacements.Find(d => d.room == room);
        return data != null && data.isActive;
    }

    public EnemyAI GetActiveMonster(RoomType room)
    {
        var data = _replacements.Find(d => d.room == room);
        if (data != null && data.isActive && data.currentMonsterInstance != null)
            return data.currentMonsterInstance.GetComponent<EnemyAI>();
        return null;
    }

    public List<EnemyAI> GetAllActiveMonsters()
    {
        List<EnemyAI> result = new List<EnemyAI>();
        foreach (var data in _activeMonsters)
        {
            if (data.currentMonsterInstance != null)
            {
                var ai = data.currentMonsterInstance.GetComponent<EnemyAI>();
                if (ai != null) result.Add(ai);
            }
        }
        return result;
    }

    public void SetReplacements(List<MonsterReplacementData> dataList)
    {
        _replacements.Clear();
        _replacements.AddRange(dataList);
        // 重新预实例化（清空旧怪物）
        // 注意：需先清理旧实例
        foreach (var data in _replacements)
            if (data.currentMonsterInstance != null) Destroy(data.currentMonsterInstance);
        PreInstantiateMonsters();
    }

    public void Cleanup()
    {
        if (SleepinessManager.Instance != null)
            SleepinessManager.Instance.OnSleepinessChanged -= OnSleepinessChanged;

        // 还原所有激活的怪物
        foreach (var data in _replacements)
        {
            if (data.isActive)
                RestoreItemFromMonster(data);
        }
    }
}
