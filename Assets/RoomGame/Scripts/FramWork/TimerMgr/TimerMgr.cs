using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerMgr : BaseMgr<TimerMgr>
{
    //每个计时器的单个唯一ID
    private int KEY_ID = 0;

    //每隔多少时间进行一次计时
    private const float intervalTime = 0.1f;
    //记录协程变量用于开启关闭
    Coroutine coroutineTime;
    Coroutine coroutineRealTime;
    //存储计时器的字典容器
    Dictionary<int,TimeItem> timeItemDic = new Dictionary<int,TimeItem>();
    //存储不受TimeScale影响的计时器字典容器
    Dictionary<int,TimeItem> realTimeItemDic = new Dictionary<int,TimeItem>();
    //待移除的计时器
    List<TimeItem> timeItemList = new List<TimeItem>();
    private TimerMgr()
    {
        //默认计时器开启
        Start();
    }

    //开启计时器管理器
    public void Start()
    {
        coroutineTime = MonoMgr.Instance.StartCoroutine(Timing(false, timeItemDic));
        coroutineRealTime = MonoMgr.Instance.StartCoroutine(Timing(true, realTimeItemDic));
    }

    //关闭计时器
    public void Stop()
    {
        MonoMgr.Instance.StopCoroutine(coroutineTime);
        MonoMgr.Instance.StopCoroutine(coroutineRealTime);
    }

    private WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(intervalTime);
    private WaitForSeconds waitForSeconds = new WaitForSeconds(intervalTime);
    IEnumerator Timing(bool isRealTime, Dictionary<int, TimeItem> timerDic)
    {
        while(true)
        {
            if (isRealTime)
                yield return new WaitForSecondsRealtime(intervalTime);
            else
                yield return new WaitForSeconds(intervalTime);
            foreach(TimeItem item in timerDic.Values)
            {
                //如果停止运行，跳过这个计时器
                if(!item.isRunning)
                {
                    continue;
                }

                //如果有间隔时间的回调函数，进行执行
                if (item.intervalCallBack != null)
                {
                    item.realIntervalTime -= (int)(intervalTime*1000);
                    //间隔计时时间到
                    if(item.realIntervalTime <= 0)
                    {
                        //执行回调
                        item.intervalCallBack.Invoke();
                        //重置间隔时间
                        item.realIntervalTime = item.intervalTime;
                    }
                }

                item.realTotalTime -= (int)(intervalTime*1000);
                //总计时时间到
                if(item.realTotalTime <= 0)
                {
                    //执行回调
                    item.overCallBack.Invoke();
                    //重置时间
                    item.realTotalTime = item.totalTime;

                    //执行完成，将计时器添加到移除列表
                    timeItemList.Add(item);
                }
            }

            //foreach遍历结束，移除待移除列表的对象
            for(int i = 0;i < timeItemList.Count; i++)
            {
                timerDic.Remove(timeItemList[i].ID);
                //放入缓存池当中
                PoolMgr.Instance.PushObj<TimeItem>(timeItemList[i]);
                //清空List
            }

            timeItemList.Clear();

        }
    }

    /// <summary>
    /// 创建单个计时器
    /// </summary>
    /// <param name="isRealTime">是否受TimeScale影响，true为受到影响</param>
    /// <param name="overCallBack">计时器结束回调函数</param>
    /// <param name="totalTime">计时器总时间 1s = 1000ms</param>
    /// <param name="intervalCallBack">计时器间隔时间回调函数</param>
    /// <param name="intervalTime">计时器间隔时间</param>
    /// <returns></returns>
    public int CreatTimeItem(bool isRealTime, UnityAction overCallBack, int totalTime, UnityAction intervalCallBack = null, int intervalTime = 0)
    {
        int keyID = KEY_ID++;

        TimeItem item = PoolMgr.Instance.GetObj<TimeItem>();
        item.Init(keyID, overCallBack, totalTime, intervalCallBack, intervalTime);

        if(isRealTime)
            realTimeItemDic.Add(keyID, item);
        else
            timeItemDic.Add(keyID, item);

        return keyID;
    }

    //移除单个计时器
    public void RemoveTimeItem(int ID)
    {
        if(timeItemDic.ContainsKey(ID))
        {
            //移除对应计时器放入缓存池
            PoolMgr.Instance.PushObj<TimeItem>(timeItemDic[ID]);
            //移除字典中该计时器
            timeItemDic.Remove(ID);
        }
        else if(realTimeItemDic.ContainsKey(ID))
        {
            //移除对应计时器放入缓存池
            PoolMgr.Instance.PushObj<TimeItem>(realTimeItemDic[ID]);
            //移除字典中该计时器
            realTimeItemDic.Remove(ID);
        }
        else
        {
            Debug.LogWarning("ID" + ID + "不存在计时器");
        }
    }

    //重置单个计时器时间
    public void ResetTimeItem(int ID)
    {
        if(timeItemDic.ContainsKey(ID))
        {
            timeItemDic[ID].ResetTime();
        }
        else if(realTimeItemDic.ContainsKey(ID))
        {
            realTimeItemDic[ID].ResetTime();
        }
        else
        {
            Debug.LogWarning("ID" + ID + "不存在");
        }
    }

    //开启单个计时器
    public void OpenTimeItem(int ID)
    {
        if (timeItemDic.ContainsKey(ID))
        {
            TimeItem item = timeItemDic[ID];
            item.isRunning = true;
        }
        else if (realTimeItemDic.ContainsKey(ID))
        {
            TimeItem item = realTimeItemDic[ID];
            item.isRunning = true;
        }
        else 
        {
            Debug.Log("ID" + ID + "不存在");
        }
    }

    //关闭单个计时器
    public void CloseTimeItem(int ID)
    {
        if (timeItemDic.ContainsKey(ID))
        {
            TimeItem item = timeItemDic[ID];
            item.isRunning = false;
        }
        else if (realTimeItemDic.ContainsKey(ID))
        {
            TimeItem item = realTimeItemDic[ID];
            item.isRunning = false;
        }
        else
        {
            Debug.Log("ID" + ID + "不存在");
        }
    }
}
