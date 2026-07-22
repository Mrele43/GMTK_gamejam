using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : BaseGameState
{
    public ChaseState(GameContext ctx) : base(ctx) { }

    public override void Enter()
    {
        base.Enter();

        // 显示危险警告UI（如红色边框、闪烁提示）
        //UIMgr.Instance.ShowPanel<WarningPanel>(E_UILayerType.system);

        // 禁用玩家交互（任务交互、拾取物品等），但保留移动/视角控制
        context.Player.EnableInteraction(false);

        // 如果怪物不存在或已失活，则立即切回游戏状态（防御性）
        // if (context.CurrentMonster == null || !context.CurrentMonster.IsActive)
        // {
        //     Debug.LogWarning("ChaseState 进入时怪物已消失，切回 Gameplay");
        //     context.StateMachine.SetState<GameplayState>();
        //     return;
        // }

        // 确保怪物已进入追击模式（MonsterAI 内部已自动切换，无需额外调用）
        Debug.Log("进入追击状态，怪物正在追逐玩家...");
    }

    public override void Update()
    {
        base.Update();

        // 1. 玩家生命归零 → 失败
        if (context.Lives <= 0)
        {
            context.StateMachine.SetState<LoseState>();
            return;
        }

        // 2. 玩家躲进被窝 → 安全区
        if (context.IsInBed)
        {
            context.StateMachine.SetState<SafeZoneState>();
            return;
        }

        // 3. 怪物已消失（攻击命中后自动失活） → 脱离追击
        // if (context.CurrentMonster == null || !context.CurrentMonster.IsActive)
        // {
        //     // 如果玩家存活，回到正常游戏状态
        //     context.StateMachine.SetState<GameplayState>();
        //     return;
        // }
    }

    public override void Exit()
    {
        base.Exit();

        // 隐藏警告UI
        //UIMgr.Instance.HidePanel<WarningPanel>();

        // 恢复玩家交互能力
        //context.Player.EnableInteraction(true);

        Debug.Log("退出追击状态");
    }
}