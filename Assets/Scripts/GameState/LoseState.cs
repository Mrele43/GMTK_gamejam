using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseState : BaseGameState
{
    public LoseState(GameContext ctx) : base(ctx) { }

    public override void Enter()
    {
        base.Enter();

        // 显示失败UI
        // UIMgr.Instance.ShowPanel<LosePanel>();

        Debug.Log($"玩家死亡，重玩第 {context.CurrentDay} 天");

        // 延迟后重玩当天
        int timerId = TimerMgr.Instance.CreatTimeItem(
            false,
            () =>
            {
                GameManager gm = Object.FindObjectOfType<GameManager>();
                if (gm != null)
                    gm.RestartCurrentDay();
            },
            2000 // 2秒
        );
    }
}
