using System;
using System.Collections.Generic;


public class StateMachine
{
    private readonly Dictionary<Type, IState> states = new Dictionary<Type, IState>();

    public IState CurrentState { get; private set; }

    public event Action<IState, IState> OnStateChanged;

    public void AddState(IState state)
    {
        if (null == state)
        {
            throw new ArgumentNullException(nameof(state));
        }

        Type key = state.GetType();
        if (!states.TryAdd(key, state))
        {
            throw new InvalidOperationException($"A state of type '{key.Name}' is already registered. Each state type may be added once.");
        }
    }

    public void SetState<T>() where T : IState
    {
        Type key = typeof(T);

        if (!states.TryGetValue(key, out IState nextState))
        {
            throw new InvalidOperationException($"No state of type '{key.Name}' is registered. Call AddState(new {key.Name}(...)) first.");
        }

        if (ReferenceEquals(nextState, CurrentState))
        {
            return;
        }

        IState previousState = CurrentState;
        previousState?.Exit();

        CurrentState = nextState;
        nextState.Enter();

        OnStateChanged?.Invoke(previousState, nextState);
    }


    public bool IsInState<T>() where T : IState
    {
        return CurrentState is T;
    }

    public void Update()
    {
        CurrentState?.Update();
    }

    public void FixedUpdate()
    {
        CurrentState?.FixedUpdate();
    }
}