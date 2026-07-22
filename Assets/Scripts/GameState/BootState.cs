using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 启动状态（初始化当天配置，重置游戏数据）
/// 适用于：首次启动、重玩当天、天过渡后进入新一天
/// </summary>
public class BootState : BaseGameState
{
    public BootState(GameContext ctx) : base(ctx) { }

    private bool isInitialized = false;

    public override void Enter()
    {
        base.Enter();
        isInitialized = false;

        // 8. 显示UI（主界面 + 当天标题）
        ShowDayUI();

        // 1. 重置玩家生命（满血3条）
        context.Lives = 3;

        // 2. 重置困意（初始值20%）
        SleepinessManager.Instance.ResetForNewDay();

        // 3. 加载当天配置
        DayConfig config = context.CurrentDayConfig;
        if (config == null)
        {
            Debug.LogError($"BootState: 第 {context.CurrentDay} 天的配置为空！");
            // 回退到默认
            context.StateMachine.SetState<GameplayState>();
            return;
        }

        // 4. 初始化任务管理器（加载当天任务列表）
         TaskManager.Instance.InitializeFromDayConfig(config);

        // // 5. 初始化怪物生成管理器（加载当天怪物配置）
        // MonsterSpawnManager.Instance.ApplyDayConfig(config);

        // // 6. 清除残留怪物
        // if (context.CurrentMonster != null)
        // {
        //     context.CurrentMonster.Deactivate();
        //     context.CurrentMonster = null;
        // }

        // 7. 确保玩家不在被窝
        context.IsInBed = false;

        Debug.Log($"BootState: 第 {context.CurrentDay} 天初始化完成");

        // 9. 延迟后进入游戏循环
        int timerId = TimerMgr.Instance.CreatTimeItem(
            false,
            () =>
            {
                context.StateMachine.SetState<GameplayState>();
            },
            800 // 0.8秒
        );
    }

    private void ShowDayUI()
    {
        // 显示主面板（如果还没显示）
         UIManager.Instance.ShowPanel<GamePanel>();

    }
}
