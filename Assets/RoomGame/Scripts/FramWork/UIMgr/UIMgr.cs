using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum E_UILayerType
{
    top,
    middle,
    bottom,
    system,
}

/// <summary>
/// UI面板管理器
/// 注意：面板预设体的名字和面板挂载的脚本要相同
/// </summary>
public class UIMgr : BaseMgr<UIMgr>
{
    private EventSystem eventSystem;
    public Canvas canvas;
    public Camera UICamera;

    private Transform topLayer;
    private Transform middleLayer;
    private Transform bottomLayer;
    private Transform systemLayer;

    //储存面板容器
    public Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();
    private UIMgr()
    {
        //加载事件监听
        eventSystem = GameObject.Instantiate(ResourcesMgr.Instance.Load<GameObject>("UI/System/EventSystem")).GetComponent<EventSystem>();
        GameObject.DontDestroyOnLoad(eventSystem);

        //加载UI相机
       
        UICamera = GameObject.Instantiate(ResourcesMgr.Instance.Load<GameObject>("UI/System/UICamera")).GetComponent<Camera>();
        UICamera.name = "UICamera";
        GameObject.DontDestroyOnLoad(UICamera);

        //加载UGUI父组件
        
        canvas = GameObject.Instantiate(ResourcesMgr.Instance.Load<GameObject>("UI/System/Canvas")).GetComponent<Canvas>();
        //设置UI摄像机
        canvas.worldCamera = UICamera;
        GameObject.DontDestroyOnLoad(canvas);

        topLayer = canvas.transform.Find("top");
        middleLayer = canvas.transform.Find("middle");
        bottomLayer = canvas.transform.Find("bottom");
        systemLayer = canvas.transform.Find("system");
    }

  
    /// <summary>
    /// 得到父组件
    /// </summary>
    /// <param name="type">层级类型</param>
    /// <returns></returns>
    public Transform GetFatherLayer(E_UILayerType type)
    {
        switch (type)
        {
            case E_UILayerType.top: return topLayer;
            case E_UILayerType.bottom: return bottomLayer;
            case E_UILayerType.middle: return middleLayer;
            case E_UILayerType.system: return systemLayer;
                default: return null;
        }
    }

    /// <summary>
    /// 显示面板
    /// 注意：异步加载方法没有完善，可能出现bug，使用同步加载就好 isSync = true
    /// </summary>
    /// <typeparam name="T">面板的类型</typeparam>
    /// <param name="layer">需要展示该面板所在的层级</param>
    /// <param name="callBack">委托，用于面板异步加载，默认为null</param>
    /// <param name="isSync">是否同步加载，默认为true</param>
    public void ShowPanel<T>(E_UILayerType layer = E_UILayerType.middle, UnityAction<T> callBack = null, bool isSync = true) where T : BasePanel
    {

        #region
        string panelName = typeof(T).Name;
        //存在面板使用面板
        if (panelDic.ContainsKey(panelName))
        {
            panelDic[panelName].ShowMe();
            //如果存在回调函数返回面板
            callBack?.Invoke(panelDic[panelName] as T);
            return;
        }

        Transform father = GetFatherLayer(layer);

        //如果传入的层级枚举错了，默认为中层
        if (father == null)
            father = GetFatherLayer(E_UILayerType.middle);

        //异步加载
        if (!isSync)
            ResourcesMgr.Instance.LoadAsync<GameObject>("UI/" + panelName, (obj) =>
            {
                GameObject panelObj = GameObject.Instantiate(obj, father, false);
                //显示面板
                T panel = panelObj.GetComponent<T>();
                panel.ShowMe();
                //返回当前面板
                callBack?.Invoke(panel);
                // 添加到字典
                panelDic.Add(panelName, panel);
            });
        //同步加载
        else
        {
            //创建面板
            GameObject obj = ResourcesMgr.Instance.Load<GameObject>("UI/" + panelName);
            //设置到层级
            GameObject panelObj = GameObject.Instantiate(obj, father, false);
            //显示面板
            T panel = panelObj.GetComponent<T>();
            panel.ShowMe();
            //返回当前面板
            callBack?.Invoke(panel);
            //添加到字典
            panelDic.Add(panelName, panel);
        }
        #endregion



    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <typeparam name="T">面板的类型</typeparam>
    /// <typeparam name="isDestroy">销毁面板还是隐藏面板（当频繁使用该面板时选择隐藏，一般是销毁）</typeparam>
    public void HidePanel<T>(bool isDestroy = true) where T : BasePanel
    {
        string panelName = typeof(T).Name;

        if (panelDic.ContainsKey(panelName))
        {
            //销毁面板
            if(isDestroy)
            {
                Debug.Log("销毁面板"+panelName);
                GameObject.Destroy(panelDic[panelName].gameObject);
                panelDic.Remove(panelName);
            }
            else//隐藏面板
                panelDic[panelName].HideMe();
        }
    }
    
    /// <summary>
    /// 获取面板
    /// </summary>
    /// <typeparam name="T">面板的类型</typeparam>
    /// <returns></returns>
    public T GetPanel<T>() where T : BasePanel
    {
        string panelName = typeof(T).Name;

        if (panelDic.ContainsKey(panelName))
            return panelDic[panelName] as T;
        return null;
    }

    /// <summary>
    /// 用于自定义控件的监听事件（如卡牌控件就需要）
    /// </summary>
    /// <typeparam name="T">组件的类型</typeparam>
    /// <param name="control">组件</param>
    /// <param name="type">监听的事件类型</param>
    /// <param name="callBack">执行该事件发生的函数</param>
    public void AddCustomEventListener<T>(T control,EventTriggerType type,UnityAction<BaseEventData> callBack) where T : UIBehaviour
    {
        EventTrigger trigger = control.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = control.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(callBack);

        trigger.triggers.Add(entry);
    }

    public Canvas GetMainCanvas()
    {
         return canvas; // canvas 是 UIMgr 中创建的主 Canvas
    }
}
