using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : BaseGameState
{
    public ChaseState(GameContext ctx) : base(ctx) { }

    private float escapeTimer = 0f;
    private const float ESCAPE_DURATION = 8f; // 持续 8 秒未被击中则自动脱险

    public override void Enter()
    {
        base.Enter();
        // 触发怪物追击 AI
        //context.CurrentMonster.StartChase();
        // 视觉冲击：触发后处理 RadialBlast
        //PostProcessManager.Instance.TriggerRadialBlast(1.0f);
        // UI 提示：出现红色警告边框
        //UIMgr.Instance.ShowPanel<WarningPanel>(E_UILayerType.system);
        // 禁用玩家主动交互（不能做任务，只能跑）
        //context.Player.EnableControl(true); // 移动保留，但禁用任务交互
    }

    public override void Update()
    {
        base.Update();

        // 1. 生命归零 → Lose（可能在追逐中被击中）
        if (context.Lives <= 0)
        {
            context.StateMachine.SetState<LoseState>();
            return;
        }

        // 2. 成功躲进被窝 → SafeZone（紧急避险）
        if (context.IsInBed)
        {
            context.StateMachine.SetState<SafeZoneState>();
            return;
        }

        // 3. 逃离计时（如果没被攻击，且一直奔跑，8 秒后怪物消失）
        escapeTimer += Time.deltaTime;
        if (escapeTimer >= ESCAPE_DURATION)
        {
            // 逃离成功，怪物消失
            //context.CurrentMonster.Deactivate();
            // 困意值略微下降（喘口气）
            //context.SleepinessMgr.ModifySleepiness(-0.1f);
            context.StateMachine.SetState<GameplayState>();
            return;
        }

        // 4. 玩家被怪物攻击逻辑（由碰撞器触发外部回调）
        //    （这里假设外部通过 GameContext.Player.OnHit 触发）
    }

    public override void Exit()
    {
        base.Exit();
        escapeTimer = 0f;
        // 隐藏警告 UI
        //UIMgr.Instance.HidePanel<WarningPanel>();
        // 恢复玩家交互能力
        //context.Player.EnableControl(true);
    }
}
