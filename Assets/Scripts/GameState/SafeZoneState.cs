using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneState : BaseGameState
{
    public SafeZoneState(GameContext ctx) : base(ctx) { }

    public override void Enter()
    {
        base.Enter();

        // 应用被窝视觉效果
        PostProcessManager.Instance.SetBedMode(true);

        // 清除所有怪物（被窝绝对安全）
        MonsterReplacementManager.Instance?.DebugDespawnAllMonsters();
        context.CurrentMonster = null;

        // 禁用玩家移动和交互（除了与床交互）
        context.Player.EnableControl(false);

        Debug.Log("进入被窝，困意以每秒2%速度降低");
    }

    public override void Update()
    {
        base.Update();

        // 持续降低困意：-2%/秒
        if (!SleepinessManager.Instance.IsLocked)
        {
            SleepinessManager.Instance.ModifySleepiness(-0.02f * Time.deltaTime);
        }

        // 注意：不再检测玩家是否离开被窝，由 BedTrigger 控制退出
        // 当玩家按E离开被窝时，会调用 PlayerController.ExitBed()
        // 然后 GameManager 会触发状态切换
    }

    public override void Exit()
    {
        base.Exit();

        // 关闭被窝视觉效果
        PostProcessManager.Instance.SetBedMode(false);

        // 恢复玩家控制和交互
        context.Player.EnableControl(true);

        Debug.Log("退出被窝");
    }
}
