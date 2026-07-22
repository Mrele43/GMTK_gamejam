using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinState : BaseGameState  // 保持类名，但逻辑改为触发天切换
{
    public WinState(GameContext ctx) : base(ctx) { }

    public override void Enter()
    {
        base.Enter();

        // 锁定困意（GDD要求）
        SleepinessManager.Instance.LockAtMax();

        // 显示完成UI（如“任务全部完成！”）
        // UIMgr.Instance.ShowPanel<TaskCompletePanel>();

        Debug.Log($"第 {context.CurrentDay} 天任务完成！");

    }
}
