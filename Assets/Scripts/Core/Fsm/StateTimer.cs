

using System;

public class StateTimer
{
    private readonly StateMachine machine;
    public float Elapsed { get; private set; }

    public StateTimer(StateMachine machine)
    {
        this.machine = machine ?? throw new ArgumentNullException(nameof(machine));
        this.machine.OnStateChanged += HandleStateChanged;
    }


    public void Tick(float deltaTime)
    {
        Elapsed += deltaTime;
    }


    public bool HasElapsed(float seconds)
    {
        return Elapsed >= seconds;
    }

    public void Reset()
    {
        Elapsed = 0f;
    }

    public void Detach()
    {
        machine.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(IState oldState, IState newState)
    {
        Elapsed = 0f;
    }
}