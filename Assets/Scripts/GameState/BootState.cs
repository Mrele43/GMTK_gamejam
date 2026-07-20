using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootState : BaseGameState
{
    public BootState(GameContext ctx) : base(ctx) { }

    public override void Enter()
    {
        base.Enter();
        // 初始化 UI（加载主界面、任务清单面板）
        //UIMgr.Instance.ShowPanel<MainPanel>(E_UILayerType.middle);
        // 重置玩家生命（假设为 3 条命）
        context.Lives = 3;
        // 通过 MonoMgr 延迟 0.5 秒进入游戏（给 UI 渲染时间）
        int timerId = TimerMgr.Instance.CreatTimeItem(false, () =>
        {
            context.StateMachine.SetState<GameplayState>();
        }, 500);
    }
}
