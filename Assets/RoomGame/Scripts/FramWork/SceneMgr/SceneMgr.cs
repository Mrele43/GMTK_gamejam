using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneMgr : BaseMgr<SceneMgr>
{
   private SceneMgr()
    {

    }

    /// <summary>
    /// 同步加载场景
    /// </summary>
    /// <param name="sceneName">场景名字</param>
    /// <param name="callBack">场景加载完后需要执行的事件</param>
    public void LoadScene(string sceneName,UnityAction callBack = null)
    {
        SceneManager.LoadScene(sceneName);
        callBack?.Invoke();
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="sceneName">场景名字</param>
    /// <param name="callBack">场景加载完后需要执行的事件</param>
    public void LoadSceneAsync(string sceneName,UnityAction callBack = null)
    {
        MonoMgr.Instance.StartCoroutine(ReallyLoadSceneAsync(sceneName,callBack));
    }

    IEnumerator ReallyLoadSceneAsync(string name,UnityAction callBack)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(name);
        while(!ao.isDone)
        {
            EventCenter.Instance.EventTrigger<float>(E_EventType.loadProgrees,ao.progress);
            yield return 0;
        }
        EventCenter.Instance.EventTrigger<float>(E_EventType.loadProgrees,1f);
        callBack?.Invoke();
    }
}
