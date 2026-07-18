using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimeItem: I_DataDrawer
{
    //唯一ID
    public int ID;
    //时间计时结束后委托回调
    public UnityAction overCallBack;
    //间隔时间结束后的委托回调
    public UnityAction intervalCallBack;

    //记录初始化的时间变量
    public int realTotalTime;
    //真正用于计时的变量
    public int totalTime;

    public int realIntervalTime;
    public  int intervalTime;

    public bool isRunning;

    public void Init(int ID,UnityAction overCallBack,int totalTime,UnityAction intervalCallBack = null,int intervalTime = 0)
    {
        this.ID = ID;
        this.overCallBack = overCallBack;
        this.realTotalTime = this.totalTime = totalTime;
        this.intervalCallBack = intervalCallBack;
        this.realIntervalTime = this.intervalTime = intervalTime;
        isRunning = true;
    }

    /// <summary>
    /// 重置时间
    /// </summary>
    public void ResetTime()
    {
         totalTime = 0;
         intervalTime = 0;
        isRunning = true;
    }

    /// <summary>
    /// 重置数据，对应的是I_DataDrawer接口
    /// </summary>
    public void Reset()
    {
        overCallBack = null; 
        intervalCallBack = null;
    }
}
