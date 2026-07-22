using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 天数管理器（MonoBehaviour 单例版本）
/// 继承 BaseMonoMgr，可在 Inspector 中配置天数列表
/// </summary>
public class DayManager : BaseMonoMgr<DayManager>
{
    [Header("天数配置")]
    [SerializeField] private List<DayConfig> dayConfigs = new List<DayConfig>();

    [Header("运行时状态（只读）")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int maxDays = 3;

    // 公开属性
    public int CurrentDay => currentDay;
    public int MaxDays => maxDays;

    // 事件
    public System.Action<int> OnDayChanged;          // 参数：新天数
    public System.Action OnAllDaysCompleted;        // 全部完成

    protected override void OnInit()
    {
        base.OnInit();

        currentDay = 1;
        maxDays = dayConfigs.Count;

        Debug.Log($"DayManager 初始化，共 {maxDays} 天");
    }

    /// <summary>
    /// 手动设置配置列表（编辑器拖拽或代码注入）
    /// </summary>
    public void SetConfigs(List<DayConfig> configs)
    {
        dayConfigs = configs ?? new List<DayConfig>();
        maxDays = dayConfigs.Count;
    }



    // ==================== 获取配置 ====================

    /// <summary>
    /// 获取当前天配置
    /// </summary>
    public DayConfig GetCurrentDayConfig()
    {
        if (dayConfigs == null || dayConfigs.Count == 0)
            return null;

        int index = Mathf.Clamp(currentDay - 1, 0, dayConfigs.Count - 1);
        return dayConfigs[index];
    }

    /// <summary>
    /// 获取指定天配置
    /// </summary>
    public DayConfig GetDayConfig(int day)
    {
        if (dayConfigs == null || dayConfigs.Count == 0)
            return null;

        int index = Mathf.Clamp(day - 1, 0, dayConfigs.Count - 1);
        return dayConfigs[index];
    }

    /// <summary>
    /// 获取所有配置（只读）
    /// </summary>
    public IReadOnlyList<DayConfig> GetAllConfigs() => dayConfigs.AsReadOnly();

    // ==================== 天切换 ====================

    /// <summary>
    /// 推进到下一日
    /// </summary>
    /// <returns>是否成功推进（false 表示已到最后一天）</returns>
    public bool AdvanceToNextDay()
    {
        if (currentDay >= maxDays)
            return false;

        currentDay++;
        OnDayChanged?.Invoke(currentDay);
        Debug.Log($"进入第 {currentDay} 天");
        return true;
    }

    /// <summary>
    /// 重玩当天
    /// </summary>
    public void RestartCurrentDay()
    {
        // 重置困意、任务、生命等由各管理器自行处理
        SleepinessManager.Instance?.ResetForNewDay();
        TaskManager.Instance?.ResetAllTasks();

        // 重置玩家生命（由 GameManager 处理）
        // 重置怪物（由 GameManager 处理）

        OnDayChanged?.Invoke(currentDay);
        Debug.Log($"重玩第 {currentDay} 天");
    }

    /// <summary>
    /// 重置到第一天
    /// </summary>
    public void ResetToDay1()
    {
        currentDay = 1;
        SleepinessManager.Instance?.ResetForNewDay();
        TaskManager.Instance?.ResetAllTasks();

        OnDayChanged?.Invoke(currentDay);
        Debug.Log("重置到第一天");
    }

    // ==================== 清理 ====================

    public void Cleanup()
    {
        OnDayChanged = null;
        OnAllDaysCompleted = null;
    }
}