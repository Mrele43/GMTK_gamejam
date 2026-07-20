using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayState : BaseGameState
{
    public GameplayState(GameContext ctx) : base(ctx) { }

    private float checkInterval = 1f;
    private float elapsedCheck = 0f;

    public override void Enter()
    {
        base.Enter();
        // 显示任务清单 UI
        //UIMgr.Instance.ShowPanel<TaskPanel>(E_UILayerType.top);
        // 恢复玩家移动控制（若有 Pause 逻辑）
        //context.Player.EnableControl(true);
    }

    public override void Update()
    {
        base.Update();

        // 1. 生命值归零 → Lose
        if (context.Lives <= 0) { context.StateMachine.SetState<LoseState>(); return; }
    
        // 2. 任务完成 → Win
        //if (context.TaskMgr.IsAllTasksCompleted()) { context.StateMachine.SetState<WinState>(); return; }

        // 3. 进入被窝 → SafeZone（由触发器设置 IsInBed）
        if (context.IsInBed) { context.StateMachine.SetState<SafeZoneState>(); return; }

        // 4. 困意值 >= 阈值 且 未被攻击中 → Chase
        //    （阈值检测现在由 SleepinessManager 事件驱动更优雅，但为了兼容原有逻辑，保留轮询作为兜底）
        if (context.SleepinessMgr.CurrentSleepiness >= 0.75f)
        {
            // 确保怪物存在
            // if (context.CurrentMonster == null || !context.CurrentMonster.IsActive)
            // {
            //     GameObject monsterObj = PoolMgr.Instance.GetObj("Monster");
            //     context.CurrentMonster = monsterObj.GetComponent<MonsterAI>();
            //     context.CurrentMonster.Init(context.Player.transform);
            // }
            context.StateMachine.SetState<ChaseState>();
        }
    }

    public override void Exit()
    {
        base.Exit();
        // 清理 UI（隐藏任务面板，但保留健康/困意显示）
    }
}
