using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private StateMachine gameStateMachine;
    private GameContext context;

    // ==================== 初始化 ====================

void Awake()
{
    // ----- 1. 强制初始化所有单例管理器 -----
    var mono = MonoMgr.Instance;
    var sleepMgr = SleepinessManager.Instance;
    var taskMgr = TaskManager.Instance;
    var dayMgr = DayManager.Instance;
    var postMgr = PostProcessManager.Instance;

    // ----- 2. 初始化后处理 -----
    Camera mainCam = Camera.main;
    if (mainCam != null)
        postMgr.Initialize(mainCam);
    else
        Debug.LogError("未找到 MainCamera！后处理将无法工作");

    // ----- 3. 获取当前天配置 -----
    DayConfig initialConfig = dayMgr.GetCurrentDayConfig();

    // ----- 4. 获取玩家引用 -----
    PlayerController player = FindObjectOfType<PlayerController>();
    if (player == null)
    {
        GameObject playerObj = new GameObject("Player_Temp");
        player = playerObj.AddComponent<PlayerController>();
        playerObj.tag = "Player";
        Debug.LogWarning("未找到 Player 对象，已自动创建临时替代");
    }


    // ----- 6. 初始化任务管理器 -----
    if (initialConfig != null)
    taskMgr.InitializeFromDayConfig(initialConfig);

    // ----- 7. 填充上下文 -----
    context = new GameContext
    {
        StateMachine = new StateMachine(),
        SleepinessMgr = sleepMgr,
        TaskMgr = taskMgr,
        PostProcessMgr = postMgr,
        Player = player,
        IsInBed = false,
        Lives = 3,
        CurrentDay = dayMgr.CurrentDay,
        MaxDays = dayMgr.MaxDays,
        CurrentDayConfig = initialConfig
    };

    // ----- 7. 初始化怪物替换管理器（订阅事件） -----
    var replacementMgr = MonsterReplacementManager.Instance;
    // 订阅：怪物出现在场景（等待注视）时只记录引用，不切换状态
    replacementMgr.OnMonsterActivated += (ai) =>
    {
        context.CurrentMonster = ai;
        Debug.Log($"怪物 {ai.name} 出现在场景中");
    };
    // 订阅：怪物消失时，如果当前处于 ChaseState，则切回 GameplayState
    replacementMgr.OnMonsterDeactivated += (ai) =>
    {
        context.CurrentMonster = null;
        Debug.Log("怪物消失，还原物品");
        // 如果当前状态是 ChaseState，切回 GameplayState
        if (gameStateMachine.CurrentState is ChaseState)
        {
            gameStateMachine.SetState<GameplayState>();
        }
    };

    // ----- 8. 订阅事件 -----
    dayMgr.OnDayChanged += OnDayChanged;
    sleepMgr.OnThresholdReached += OnThresholdReached;

    // ----- 9. 注册所有状态 -----
    gameStateMachine = context.StateMachine;
    gameStateMachine.AddState(new BootState(context));
    gameStateMachine.AddState(new GameplayState(context));
    gameStateMachine.AddState(new ChaseState(context));
    gameStateMachine.AddState(new SafeZoneState(context));
    gameStateMachine.AddState(new DayTransitionState(context));
    gameStateMachine.AddState(new FinalWinState(context));
    gameStateMachine.AddState(new LoseState(context));
    gameStateMachine.AddState(new WinState(context));

    // ----- 10. 启动状态机 -----
    gameStateMachine.SetState<BootState>();
    Debug.Log($"状态机已启动，当前天数: {dayMgr.CurrentDay}");
}

    // ==================== 事件回调 ====================

    private void OnDayChanged(int newDay)
    {
        context.CurrentDay = newDay;
        context.CurrentDayConfig = DayManager.Instance.GetCurrentDayConfig();
        context.Lives = 3; // 新一天满血

        // 重置所有怪物（还原物品）
        MonsterReplacementManager.Instance?.DebugDespawnAllMonsters();
        context.CurrentMonster = null;

        // 更新任务和怪物配置（由 DayTransitionState 处理）
        Debug.Log($"GameManager 响应天变化: 第 {newDay} 天");
    }

    private void OnThresholdReached(float threshold)
    {
        // 由 MonsterSpawnManager 处理怪物生成
        // 这里只做日志记录，具体生成在 MonsterSpawnManager 内部
        Debug.Log($"困意达到阈值 {threshold:P0}，MonsterSpawnManager 将处理");
    }

    // ==================== Unity 生命周期 ====================

    void Update()
    {
        gameStateMachine?.Update();
    }

    void FixedUpdate()
    {
        gameStateMachine?.FixedUpdate();
    }

    void OnDestroy()
    {
        // 清理事件订阅，防止内存泄漏
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged -= OnDayChanged;

        if (SleepinessManager.Instance != null)
            SleepinessManager.Instance.OnThresholdReached -= OnThresholdReached;

        // 清理各管理器
        SleepinessManager.Instance?.Cleanup();
        TaskManager.Instance?.Cleanup();
        PostProcessManager.Instance?.Cleanup();
        DayManager.Instance?.Cleanup();
        MonsterReplacementManager.Instance?.Cleanup();
        Debug.Log("GameManager 清理完成");
    }

    // ==================== 公共接口（供状态机和外部调用） ====================

    /// <summary>
    /// 玩家进入被窝
    /// </summary>
    public void PlayerEnterBed()
    {
        if (context.IsInBed) return;
        context.IsInBed = true;

        if (gameStateMachine.CurrentState is GameplayState ||
            gameStateMachine.CurrentState is ChaseState)
        {
            gameStateMachine.SetState<SafeZoneState>();
        }
        Debug.Log("玩家进入被窝");
    }

    /// <summary>
    /// 玩家离开被窝
    /// </summary>
    public void PlayerExitBed()
    {
        if (!context.IsInBed) return;
        context.IsInBed = false;

        if (gameStateMachine.CurrentState is SafeZoneState)
        {
            gameStateMachine.SetState<GameplayState>();
        }
        Debug.Log("玩家离开被窝");
    }

    /// <summary>
    /// 玩家受到攻击
    /// </summary>
    public void PlayerTakesDamage()
    {
        context.Lives--;
        Debug.Log($"玩家受伤，剩余生命: {context.Lives}");

        // 受击时掉落手中物品（由 PlayerController 处理）
        context.Player?.DropCurrentItem();

        if (context.Lives <= 0 && !(gameStateMachine.CurrentState is LoseState))
        {
            gameStateMachine.SetState<LoseState>();
        }
    }

    /// <summary>
    /// 推进到下一天（由 WinState 调用）
    /// </summary>
    public void AdvanceToNextDay()
    {
        // 先还原所有怪物
        MonsterReplacementManager.Instance?.DebugDespawnAllMonsters();
        context.CurrentMonster = null;
        if (DayManager.Instance.AdvanceToNextDay())
        {
            // 更新上下文（OnDayChanged 会处理，但这里再手动确保）
            context.CurrentDay = DayManager.Instance.CurrentDay;
            context.CurrentDayConfig = DayManager.Instance.GetCurrentDayConfig();

            // 进入天过渡
            context.StateMachine.SetState<DayTransitionState>();
        }
        else
        {
            // 已到最后一天 → 通关
            context.StateMachine.SetState<FinalWinState>();
        }
    }

    /// <summary>
    /// 重玩当天（由 LoseState 调用）
    /// </summary>
    public void RestartCurrentDay()
    {
        // 还原所有怪物
        MonsterReplacementManager.Instance?.DebugDespawnAllMonsters();
        context.CurrentMonster = null;
        DayManager.Instance.RestartCurrentDay();

        // 重置生命
        context.Lives = 3;

        // 确保玩家不在被窝
        context.IsInBed = false;

        // 重新从 Boot 开始
        context.StateMachine.SetState<BootState>();
        Debug.Log($"重玩第 {context.CurrentDay} 天");
    }

    /// <summary>
    /// 切换到追逐状态（由怪物调用）
    /// </summary>
    public void TriggerChaseState()
    {
        // 只有 GameplayState 和 SafeZoneState 可以进入追逐
        // 注意：SafeZoneState 中怪物已消失，理论上不会触发，但保留判断
        if (gameStateMachine.CurrentState is GameplayState)
        {
            gameStateMachine.SetState<ChaseState>();
            Debug.Log("GameManager: 切换到 ChaseState");
        }
        else if (gameStateMachine.CurrentState is SafeZoneState)
        {
            // 被窝安全区不应触发追逐，但若发生则忽略
            Debug.LogWarning("被窝内触发追逐，忽略");
        }
    }

    /// <summary>
    /// 查询玩家是否在被窝中
    /// </summary>
    public bool IsPlayerInBed() => context.IsInBed;

    /// <summary>
    /// 获取当前状态名称
    /// </summary>
    public string GetCurrentStateName() =>
        gameStateMachine.CurrentState?.GetType().Name ?? "Null";
    
    /// <summary>
    /// 获取当前怪物引用（供其他脚本使用）
    /// </summary>
    public EnemyAI GetCurrentMonster() => context.CurrentMonster;
}
