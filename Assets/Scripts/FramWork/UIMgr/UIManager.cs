using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    private static UIManager instance = new UIManager();

    public static UIManager Instance => instance;

    //存面板的字典
    private Dictionary<string,basePlane> panalDic = new Dictionary<string,basePlane>();

    private Transform canvasTrans;

    GameObject cavesObj;
    private UIManager()
    {
        if (cavesObj == null)
        {
            cavesObj = GameObject.Instantiate(Resources.Load<GameObject>("UI/Canvas"));
        }
        canvasTrans = cavesObj.transform;
        //过场景不移除
        GameObject.DontDestroyOnLoad(cavesObj);
    }

    //显示面板
    public T ShowPanel<T>() where T : basePlane
    {
        string panelName = typeof(T).Name;

        //检查字典是否已经有该面板（已显示的）
        if (panalDic.ContainsKey(panelName))
            return panalDic[panelName] as T;

        //显示面板，动态创建面板
        GameObject panelObj = GameObject.Instantiate(Resources.Load<GameObject>("UI/" + panelName));
        panelObj.transform.SetParent(canvasTrans, false);

        //存储面板获得逻辑
        T panel = panelObj.GetComponent<T>();
        panalDic.Add(panelName, panel);
        panel.ShwoMe();

        return panel;

    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <typeparam name="T">面板类名</typeparam>
    /// <param name="isFade">是否淡出完毕后才删除</param>
    public void HidePanel<T>(bool isFade = true) where T : basePlane
    {
        string panelName = typeof(T).Name;
        //删除面板
        if(isFade)
        {
            panalDic[panelName].HideMe(() =>
            {
                GameObject.Destroy(panalDic[panelName].gameObject);
                panalDic.Remove(panelName);
            });
        }
        else
        {
            GameObject.Destroy(panalDic[panelName].gameObject);
            panalDic.Remove(panelName);
        }

    }

    //得到面板

    public T GetPanel<T>() where T : basePlane
    {
        string panelName = typeof(T).Name;

        if (panalDic.ContainsKey(panelName))
            return panalDic[panelName] as T;

        return null;
    }
}
