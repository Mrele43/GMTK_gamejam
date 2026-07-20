using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private StateMachine gameStateMachine;
    private GameContext context;

    void Awake()
    {
        // ----- 第1步：强制初始化所有单例管理器（确保它们在状态机之前准备好） -----
        // 注意：调用 Instance 会触发 BaseMgr 的静态构造函数，创建单例
        var mono = MonoMgr.Instance;          // 确保 MonoMgr 存在（用于 Update 桥接）
        var sleepMgr = SleepinessManager.Instance;
        sleepMgr.Initialize(); // 启动被动增长循环
        //var taskMgr = TaskManager.Instance;

        // 订阅危险阈值事件
        sleepMgr.OnDangerThresholdReached += (value) =>
        {
            // 如果当前没有怪物，生成一个
            if (context.CurrentMonster == null || !context.CurrentMonster.IsActive)
            {
                // 从对象池获取，若没有则实例化（使用 PoolMgr）
                GameObject monsterPrefab = Resources.Load<GameObject>("Monster");
                GameObject monsterObj = PoolMgr.Instance.GetObj("Monster");
                if (monsterObj == null)
                {
                    monsterObj = Instantiate(monsterPrefab);
                    monsterObj.name = "Monster";
                }
                MonsterAI monster = monsterObj.GetComponent<MonsterAI>();
                // 初始化：传入玩家、相机、GameManager自身
                Camera mainCam = Camera.main;
                monster.Init(context.Player.transform, mainCam, this);
                context.CurrentMonster = monster;
            }
        };
        

        // ----- 第2步：获取玩家引用（场景里必须有一个 Tag 为 "Player" 的对象） -----
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null)
        {
            // 如果场景里没有，自己创建一个临时的（演示用）
            GameObject playerObj = new GameObject("Player_Temp");
            player = playerObj.AddComponent<PlayerController>();
            playerObj.tag = "Player";
            Debug.LogWarning("未找到 Player 对象，已自动创建临时替代");
        }

        // ----- 第3步：填充上下文 -----
        context = new GameContext
        {
            StateMachine = new StateMachine(),
            SleepinessMgr = sleepMgr,
            //TaskMgr = taskMgr,
            Player = player,
            IsInBed = false,
            Lives = 3,
            //CurrentMonster = null
        };

        // ----- 第4步：注册所有状态到状态机（顺序无所谓） -----
        gameStateMachine = context.StateMachine;
        gameStateMachine.AddState(new BootState(context));
        gameStateMachine.AddState(new GameplayState(context));
        gameStateMachine.AddState(new ChaseState(context));
        gameStateMachine.AddState(new SafeZoneState(context));
        gameStateMachine.AddState(new WinState(context));
        gameStateMachine.AddState(new LoseState(context));

        // ----- 第5步：启动！跳转到 BootState -----
        gameStateMachine.SetState<BootState>();
        Debug.Log("状态机已启动，当前状态: BootState");
    }

    void Update()
    {
        // 逐帧驱动状态机的逻辑更新（所有状态里的 Update() 会在这里被执行）
        gameStateMachine?.Update();
    }

    void FixedUpdate()
    {
        // 物理帧驱动（如果状态里有物理逻辑）
        gameStateMachine?.FixedUpdate();
    }

    // ==================================================
    // 对外公共接口（供其他脚本调用，触发状态切换）
    // ==================================================

    // 玩家进入被窝（由床的碰撞器调用）
    public void PlayerEnterBed()
    {
        context.IsInBed = true;
        Debug.Log("玩家进入被窝");
    }

    // 玩家离开被窝（由床的碰撞器调用）
    public void PlayerExitBed()
    {
        context.IsInBed = false;
        Debug.Log("玩家离开被窝");
    }

    // 玩家受到攻击（由怪物碰撞器调用）
    public void PlayerTakesDamage()
    {
        context.Lives--;
        Debug.Log($"玩家受伤，剩余生命: {context.Lives}");
        if (context.Lives <= 0 && !(gameStateMachine.CurrentState is LoseState))
        {
            gameStateMachine.SetState<LoseState>();
        }
    }

    // 玩家完成一个任务（由任务交互物调用）
    public void CompleteOneTask()
    {
        //context.TaskMgr.MarkTaskComplete();
        //Debug.Log($"完成任务，当前进度: {context.TaskMgr.IsAllTasksCompleted()}");
    }

    // 供 MonsterAI 调用，切换到 ChaseState
    public void TriggerChaseState()
    {
        if (gameStateMachine.CurrentState is GameplayState || 
            gameStateMachine.CurrentState is SafeZoneState)
        {
            gameStateMachine.SetState<ChaseState>();
        }
    }

    // 供 MonsterAI 查询玩家是否在床中
    public bool IsPlayerInBed()
    {
        return context.IsInBed;
    }

    // 获取当前状态名称（用于调试）
    public string GetCurrentStateName() => gameStateMachine.CurrentState?.GetType().Name ?? "Null";
}
