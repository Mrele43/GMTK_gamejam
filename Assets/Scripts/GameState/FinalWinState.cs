using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalWinState : BaseGameState
{
    public FinalWinState(GameContext ctx) : base(ctx) { }

    public override void Enter()
    {
        base.Enter();

        // 显示通关画面
        // UIMgr.Instance.ShowPanel<FinalWinPanel>();

        Time.timeScale = 0f;

        Debug.Log("恭喜通关！三天全部完成！");
    }

    public override void Exit()
    {
        base.Exit();
        Time.timeScale = 1f;
    }
}
