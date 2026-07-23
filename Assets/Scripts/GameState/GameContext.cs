using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameContext
{
    public StateMachine StateMachine { get; set; }
    public SleepinessManager SleepinessMgr { get; set; }
    public TaskManager TaskMgr { get; set; }

    public PlayerController Player { get; set; }
    public PostProcessManager PostProcessMgr { get; set; }
    public EnemyAI CurrentMonster { get; set; }
    public bool IsInBed { get; set; }
    public int Lives { get; set; }

    // --- 新增：外循环相关 ---
    public int CurrentDay { get; set; }          // 当前第几天 (1,2,3)
    public int MaxDays { get; set; } = 3;        // 总天数
    public DayConfig CurrentDayConfig { get; set; } // 当天配置
}
