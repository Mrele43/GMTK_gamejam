using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameState : IState
{
    protected GameContext context;
    protected StateTimer timer;

    public BaseGameState(GameContext ctx)
    {
        context = ctx;
        timer = new StateTimer(ctx.StateMachine);
    }

    public virtual void Enter() { timer.Reset(); }
    public virtual void Update() { timer.Tick(Time.deltaTime); }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { timer.Detach(); }
}
