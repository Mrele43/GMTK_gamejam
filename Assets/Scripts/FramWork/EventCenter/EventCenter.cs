using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseEventInfo { };

/// <summary>
/// 用于需要传参数的委托
/// </summary>
/// <typeparam name="T"></typeparam>
public class EventInfo<T> : BaseEventInfo
{
    
    public UnityAction<T> unityAction;

    public EventInfo(UnityAction<T>action)
    {
        unityAction += action;
    }    
}

/// <summary>
/// 用于不需要传入参数的委托
/// </summary>
public class EventInfo : BaseEventInfo
{
    public UnityAction unityAction;

    public EventInfo(UnityAction action)
    {
        unityAction += action;
    }
}

/// <summary>
/// 事件中心(弃用版本，多亏它我造了一坨坨新鲜的史)
/// 添加新的事件需要在E_EventType里面添加对应的枚举标识
/// 增加了事件必定要删除事件，否则会造成内存泄漏
/// </summary>
public class EventCenter : BaseMgr<EventCenter>
{
    private EventCenter() { }

    //存储所有事件的字典容器
    public Dictionary<E_EventType,BaseEventInfo> eventDic = new Dictionary<E_EventType, BaseEventInfo>();
    //发生事件
    public void EventTrigger<T>(E_EventType eventType,T obj)
    {
        if(eventDic.ContainsKey(eventType))
        {
            if(obj == null)
            {
                Debug.LogError("传入的对象为null");
            }
            (eventDic[eventType] as EventInfo<T>).unityAction?.Invoke(obj);
        }
    }

    public void EventTrigger(E_EventType eventType)
    {
        if (eventDic.ContainsKey(eventType))
        {
            (eventDic[eventType] as EventInfo).unityAction?.Invoke();
        }
    }

    //注册事件
    public void AddEventListener<T>(E_EventType eventType, UnityAction<T> action)
    {
        if(eventDic.ContainsKey(eventType))
        {
            (eventDic[eventType] as EventInfo<T>).unityAction += action;
        }
        else
        {
            eventDic.Add(eventType, new EventInfo<T>(action));
        }
    }

    public void AddEventListener(E_EventType eventType, UnityAction action)
    {
        if (eventDic.ContainsKey(eventType))
        {
            (eventDic[eventType] as EventInfo).unityAction += action;
        }
        else
        {
            eventDic.Add(eventType, new EventInfo(action));
        }
    }

    //删除事件
    public void RemoveEventListener<T>(E_EventType eventType, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(eventType))
        {
            (eventDic[eventType] as EventInfo<T>).unityAction -= action;
        }
    }
    public void RemoveEventListener(E_EventType eventType, UnityAction action)
    {
        if (eventDic.ContainsKey(eventType))
        {
            (eventDic[eventType] as EventInfo).unityAction -= action;
        }
    }

    //清空全部事件
    public void Clear()
    {
        eventDic.Clear();
    }

    public void Clear(E_EventType eventType)
    {
        if(eventDic.ContainsKey(eventType))
        eventDic.Remove(eventType);
    }

}
