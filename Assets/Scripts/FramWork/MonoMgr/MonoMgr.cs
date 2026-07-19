using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 单例，用于非继承Mono的类却要使用到继承Mono类中的方法
/// </summary>
public class MonoMgr : BaseMonoMgr<MonoMgr>
{
    private event UnityAction upadateEvent;
    private event UnityAction fixedUpadateEvent;
    private event UnityAction lateUpadateEvent;
    private event UnityAction awakeEvent;
    private event UnityAction destroyEvent;

    // 存储带参数的Awake方法
    private List<System.Delegate> awakeActionsWithParams = new List<System.Delegate>();
    private List<object[]> awakeParams = new List<object[]>();

    public void AddInUpdate(UnityAction updataEvent)
    {
        upadateEvent += updataEvent;
    }
    
    public void AddInFixedUpadate(UnityAction updataEvent)
    {
        fixedUpadateEvent += updataEvent;
    }
    
    public void AddInLateUpadate(UnityAction updataEvent)
    {
        lateUpadateEvent += updataEvent;
    }

    public void AddInAwake(UnityAction updataEvent)
    {
        awakeEvent += updataEvent;
    }

    /// <summary>
    /// 注册带一个参数的Awake方法
    /// </summary>
    public void AddInAwake<T>(UnityAction<T> action, T param)
    {
        if (action != null)
        {
            // 包装成无参委托
            awakeEvent += () => action(param);
        }
    }

    /// <summary>
    /// 注册带两个参数的Awake方法
    /// </summary>
    public void AddInAwake<T1, T2>(UnityAction<T1, T2> action, T1 param1, T2 param2)
    {
        if (action != null)
        {
            awakeEvent += () => action(param1, param2);
        }
    }

    /// <summary>
    /// 注册带三个参数的Awake方法
    /// </summary>
    public void AddInAwake<T1, T2, T3>(UnityAction<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
    {
        if (action != null)
        {
            awakeEvent += () => action(param1, param2, param3);
        }
    }

    /// <summary>
    /// 使用System.Action注册带任意数量参数的Awake方法
    /// </summary>
    public void AddInAwake(System.Delegate action, params object[] parameters)
    {
        if (action != null)
        {
            awakeActionsWithParams.Add(action);
            awakeParams.Add(parameters);
        }
    }

    /// <summary>
    /// 使用自定义类封装参数（最灵活）
    /// </summary>
    public class AwakeActionWithParam
    {
        public System.Delegate action;
        public object[] parameters;
        
        public void Invoke()
        {
            action?.DynamicInvoke(parameters);
        }
    }
    
    private List<AwakeActionWithParam> awakeActions = new List<AwakeActionWithParam>();
    
    public void RegisterAwakeAction(System.Delegate action, params object[] parameters)
    {
        awakeActions.Add(new AwakeActionWithParam 
        { 
            action = action, 
            parameters = parameters 
        });
    }

    public void AddInOnDestroy(UnityAction updataEvent)
    {
        destroyEvent += updataEvent;
    }

    public void RemoveInUpdate(UnityAction updataEvent)
    {
        upadateEvent -= updataEvent;
    }
    
    public void RemoveInFixedUpadate(UnityAction updataEvent)
    {
        fixedUpadateEvent -= updataEvent;
    }
    
    public void RemoveInLateUpadate(UnityAction updataEvent)
    {
        lateUpadateEvent -= updataEvent;
    }

    public void RemoveInAwake(UnityAction updataEvent)
    {
        awakeEvent -= updataEvent;
    }

    /// <summary>
    /// 移除带参数的Awake方法（需要保持注册时的引用）
    /// </summary>
    public void RemoveInAwake(System.Delegate action)
    {
        for (int i = awakeActionsWithParams.Count - 1; i >= 0; i--)
        {
            if (awakeActionsWithParams[i] == action)
            {
                awakeActionsWithParams.RemoveAt(i);
                awakeParams.RemoveAt(i);
            }
        }
        
        for (int i = awakeActions.Count - 1; i >= 0; i--)
        {
            if (awakeActions[i].action == action)
            {
                awakeActions.RemoveAt(i);
            }
        }
    }

    public void RemoveInOnDestroy(UnityAction updataEvent)
    {
        destroyEvent -= updataEvent;
    }


    protected override void Awake()
    {
        base.Awake();

        // 执行无参Awake事件
        awakeEvent?.Invoke();

        // 执行带参数的Awake方法（方案一：使用Delegate.DynamicInvoke）
        for (int i = 0; i < awakeActionsWithParams.Count; i++)
        {
            awakeActionsWithParams[i]?.DynamicInvoke(awakeParams[i]);
        }

        // 执行带参数的Awake方法（方案二：使用封装类）
        foreach (var action in awakeActions)
        {
            action.Invoke();
        }

    }

    void Update()
    {
        upadateEvent?.Invoke();
    }

    void FixedUpdate()
    {
        fixedUpadateEvent?.Invoke();
    }

    private void LateUpdate()
    {
        lateUpadateEvent?.Invoke();
    }

    private void OnDestroy()
    {
        destroyEvent?.Invoke();
    }
}