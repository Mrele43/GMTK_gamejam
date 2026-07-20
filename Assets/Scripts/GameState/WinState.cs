using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinState : BaseGameState
{
    public WinState(GameContext ctx) : base(ctx) { }
    public override void Enter()
    {
        base.Enter();
        // 播放胜利音乐，显示胜利 UI，控制台输出胜利信息
        //UIMgr.Instance.ShowPanel<WinPanel>(E_UILayerType.system);
        Time.timeScale = 0f; // 暂停游戏
    }
}

public class LoseState : BaseGameState
{
    public LoseState(GameContext ctx) : base(ctx) { }
    public override void Enter()
    {
        base.Enter();
        //UIMgr.Instance.ShowPanel<LosePanel>(E_UILayerType.system);
        Time.timeScale = 0f;
    }
}
