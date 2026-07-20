using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayState : BaseGameState
{
    public GameplayState(GameContext ctx) : base(ctx) { }

    public override void Enter()
    {
        base.Enter();

        // 显示任务面板
        //UIMgr.Instance.ShowPanel<TaskListPanel>(E_UILayerType.top);

        // 启用玩家交互（可能之前被 ChaseState 禁用）
        context.Player.EnableInteraction(true);

        // 确保时间缩放正常（若之前被暂停）
        Time.timeScale = 1f;

        Debug.Log("进入游戏状态 (Gameplay)");
    }

    public override void Update()
    {
        base.Update();

        // 1. 生命归零 → Lose
        if (context.Lives <= 0)
        {
            context.StateMachine.SetState<LoseState>();
            return;
        }

        // 2. 任务全部完成 → Win
        if (context.TaskMgr.IsAllTasksCompleted)
        {
            context.StateMachine.SetState<WinState>();
            return;
        }

        // 3. 玩家进入被窝 → SafeZone
        if (context.IsInBed)
        {
            context.StateMachine.SetState<SafeZoneState>();
            return;
        }

        // 注意：不再检测困意阈值，由 SleepinessManager 事件驱动生成怪物，
        // 怪物自身注视检测并触发 ChaseState（通过 GameManager.TriggerChaseState）
    }

    public override void Exit()
    {
        base.Exit();
        // 可隐藏任务面板（但若进入 SafeZone 或 Win，可能保留，由对应状态处理）
        // 这里不做强制，避免干扰
        Debug.Log("退出游戏状态 (Gameplay)");
    }
}
