using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayTransitionState : BaseGameState
{
    public DayTransitionState(GameContext ctx) : base(ctx) { }

    private bool transitionComplete = false;

    public override void Enter()
    {
        base.Enter();
        transitionComplete = false;

        // 1. 黑幕淡入
         UIManager.Instance.ShowPanel<FadePanel>();

        // 2. 显示 "第 X 天" 文字
         UIManager.Instance.GetPanel<FadePanel>().daytext.text = $"第{context.CurrentDay}天";

        // 3. 播放鸡鸣音效
        // AudioMgr.Instance.Play("Rooster");

        Debug.Log($"=== 第 {context.CurrentDay} 天开始 ===");

        // 4. 应用当天配置（任务列表、怪物配置等）
        ApplyDayConfig();

        // 5. 延迟后切换到 Gameplay
        int timerId = TimerMgr.Instance.CreatTimeItem(
            false,
            () =>
            {
                // 关闭黑幕
                 UIManager.Instance.HidePanel<FadePanel>();
                context.StateMachine.SetState<GameplayState>();
            },
            3000 // 3秒过渡
        );
    }

    private void ApplyDayConfig()
    {
        DayConfig config = context.CurrentDayConfig;
        if (config == null) return;

        // 重新初始化任务管理器
        TaskManager.Instance.InitializeFromDayConfig(config);

        // 通知怪物生成管理器更新当天的配置
        // MonsterSpawnManager.Instance.ApplyConfig(config.monsterConfigs);

        // 重置困意（新一天从20%开始）
        SleepinessManager.Instance.ResetForNewDay();
    }
}
