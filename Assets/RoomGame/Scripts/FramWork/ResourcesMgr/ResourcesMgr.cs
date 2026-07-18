using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 该类对Resources的异步加载进行了一次封装，使用更加便捷
/// </summary>
public class ResourcesMgr : BaseMgr<ResourcesMgr>
{
    Dictionary<string, UnityEngine.Object> resDic = new Dictionary<string, UnityEngine.Object>();
   private ResourcesMgr()
    {

    }

    /// <summary>
    /// 同步加载资源的方法
    /// </summary>
    public T Load<T>(string name) where T : UnityEngine.Object
    {
        return Resources.Load<T>(name);
    }

    /// <summary>
    /// 异步加载资源泛型方法
    /// </summary>
    /// <typeparam name="T">资源的类型</typeparam>
    /// <param name="path">资源的Resources路径</param>
    /// <param name="callBack">返回加载对象</param>
    public void LoadAsync<T>(string path,UnityAction<T> callBack) where T : UnityEngine.Object
    { 
        MonoMgr.Instance.StartCoroutine(LaodAsynRes(path,callBack));       
    }

    IEnumerator LaodAsynRes<T>(string path,UnityAction<T> callBack) where T : UnityEngine.Object
    {
        ResourceRequest rq = Resources.LoadAsync<T>(path);
        yield return rq;
        callBack(rq.asset as T);
    }

    /// <summary>
    /// 异步加载资源传入type方法
    /// </summary>
    /// <param name="path">资源的Resources路径</param>
    /// <param name="type">资源的type</param>
    /// <param name="callBack">返回加载对象</param>
    public void LoadAsync(string path,Type type, UnityAction<UnityEngine.Object> callBack)  
    {
        MonoMgr.Instance.StartCoroutine(LoadAsynRes(path,type,callBack));
    }

    IEnumerator LoadAsynRes(string path,Type type,UnityAction<UnityEngine.Object> callBack) 
    {
        ResourceRequest rq = Resources.LoadAsync(path,type);
        yield return rq;
        callBack(rq.asset);
    }

    /// <summary>
    /// 异步卸载没有使用的资源
    /// </summary>
    /// <returns></returns>
    public void UnloadUnusedAssets(UnityAction callBack)
    {
        MonoMgr.Instance.StartCoroutine(RealUnloadUnusedAssets(callBack));
    }

    private IEnumerator RealUnloadUnusedAssets(UnityAction callBack)
    {
        AsyncOperation rq = Resources.UnloadUnusedAssets();
        yield return rq;
        callBack();
    }

    /// <summary>
    /// 指定卸载一个资源
    /// </summary>
    public void UnloadAsset(UnityEngine.Object assetToUnLoad)
    {
        Resources.UnloadAsset(assetToUnLoad);
    }
}
