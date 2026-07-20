using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private StateMachine gameStateMachine;
    private GameContext context;

    void Awake()
    {
        // 1. 初始化所有管理器（确保 BaseMgr 单例已创建）
        //    （由于 BaseMgr 是惰性创建，调用一下 Instance 即可触发）
        // var sleepMgr = SleepinessManager.Instance;
        // var taskMgr = TaskManager.Instance;
        // var player = FindObjectOfType<PlayerController>();

        // 2. 构建上下文
        context = new GameContext
        {
            StateMachine = new StateMachine(),
            // SleepinessMgr = sleepMgr,
            // TaskMgr = taskMgr,
            // Player = player,
            IsInBed = false,
            Lives = 3
        };

        // 3. 注册所有状态到状态机
        gameStateMachine = context.StateMachine;
        gameStateMachine.AddState(new BootState(context));
        gameStateMachine.AddState(new GameplayState(context));
        gameStateMachine.AddState(new ChaseState(context));
        gameStateMachine.AddState(new SafeZoneState(context));
        gameStateMachine.AddState(new WinState(context));
        gameStateMachine.AddState(new LoseState(context));

        // 4. 设置起始状态
        gameStateMachine.SetState<BootState>();
    }

    void Update()
    {
        // 驱动状态机的 Update（时间缩放影响）
        gameStateMachine.Update();
    }

    void FixedUpdate()
    {
        // 驱动物理更新（若状态有物理逻辑）
        gameStateMachine.FixedUpdate();
    }

    // 对外提供通用接口（供碰撞器、Trigger 调用）
    public void PlayerEnterBed() => context.IsInBed = true;
    public void PlayerExitBed() => context.IsInBed = false;
    public void PlayerTakesDamage()
    {
        context.Lives--;
        if (context.Lives <= 0 && gameStateMachine.CurrentState is not LoseState)
        {
            gameStateMachine.SetState<LoseState>();
        }
    }
    //public void TaskCompleted() => context.TaskMgr.MarkTaskComplete();
}
