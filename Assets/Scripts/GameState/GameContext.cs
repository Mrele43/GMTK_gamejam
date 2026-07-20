using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameContext
{
    public StateMachine StateMachine { get; set; }
    public SleepinessManager SleepinessMgr { get; set; }
    public TaskManager TaskMgr { get; set; }
    public PlayerController Player { get; set; }
    public MonsterAI CurrentMonster { get; set; }
    public bool IsInBed { get; set; }
    public int Lives { get; set; }
}
