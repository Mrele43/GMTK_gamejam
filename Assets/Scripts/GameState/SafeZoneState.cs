using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneState : BaseGameState
{
    public SafeZoneState(GameContext ctx) : base(ctx) { }

    public override void Enter()
    {
        base.Enter();
        // 告诉后处理进入“被窝模式”（全屏高斯模糊 + 暗角）
        //PostProcessManager.Instance.SetBedMode(true);
        // 暂停怪物追击（如果怪物存在）
        //context.CurrentMonster?.PauseChase();
        // UI 隐藏所有危险指示器，显示“安全”图标
    }

    public override void Update()
    {
        base.Update();

        // 检测玩家是否离开被窝（碰撞器 Exit 事件会设置 IsInBed = false）
        // if (!context.IsInBed)
        // {
        //     // 恢复困意增长，关闭被窝特效
        //     PostProcessManager.Instance.SetBedMode(false);
        //     // 如果困意依然很高，可能回到 Chase
        //     if (context.SleepinessMgr.CurrentSleepiness >= 0.75f && context.CurrentMonster != null)
        //     {
        //         context.StateMachine.SetState<ChaseState>();
        //     }
        //     else
        //     {
        //         context.StateMachine.SetState<GameplayState>();
        //     }
        // }
    }

    public override void Exit()
    {
        base.Exit();
        // 确保退出时关闭被窝特效（保险）
        //PostProcessManager.Instance.SetBedMode(false);
    }
}
