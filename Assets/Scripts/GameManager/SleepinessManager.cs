using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 困意值管理器（核心压力系统）
/// 职责：数值增减、被动增长、调节倍率、阈值事件推送、冻结控制
/// </summary>
public class SleepinessManager : BaseMgr<SleepinessManager>
{
    // ---------- 私有构造（配合 BaseMgr 反射） ----------
    private SleepinessManager() { }

    // ---------- 事件定义（供 UI 和 状态机 订阅） ----------
    /// <summary>困意值变化事件（参数：当前值 0~1）</summary>
    public event UnityAction<float> OnSleepinessChanged;
    /// <summary>困意值达到危险阈值事件（参数：当前值）</summary>
    public event UnityAction<float> OnDangerThresholdReached;
    /// <summary>困意值脱离危险阈值事件</summary>
    public event UnityAction OnDangerThresholdExited;

    // ---------- 核心数值 ----------
    private float currentSleepiness = 0f;
    public float CurrentSleepiness => currentSleepiness;

    // ---------- 配置参数（可序列化，方便调参） ----------
    [Header("基础增长配置")]
    public float basePassiveRate = 0.015f;   // 每秒自然增长（约 67 秒从 0 涨到 1）
    public float dangerThreshold = 0.75f;    // 危险阈值（触发 Chase）

    [Header("调节倍率")]
    public float medicationMultiplier = 2.5f; // 服药后增长加速倍率
    public float radioMultiplier = 0.3f;     // 听收音机时增长减速倍率（降到 30%）

    // ---------- 运行时状态 ----------
    private bool isFrozen = false;           // 是否冻结增长（被窝安全区）
    private float currentMultiplier = 1f;    // 当前综合倍率（1 = 正常）
    private bool wasDangerous = false;       // 上一帧是否处于危险状态（用于边缘触发事件）

    // ---------- 外部调节接口（供任务、药物、收音机调用） ----------

    /// <summary>
    /// 直接修改困意值（用于任务完成增加、药物加速、收音机减速）
    /// </summary>
    /// <param name="amount">正数增加困意，负数减少困意</param>
    public void ModifySleepiness(float amount)
    {
        if (isFrozen) return; // 被窝里冻结一切数值变化（策划案要求）

        float oldValue = currentSleepiness;
        currentSleepiness = Mathf.Clamp01(currentSleepiness + amount);
        
        if (!Mathf.Approximately(oldValue, currentSleepiness))
        {
            OnSleepinessChanged?.Invoke(currentSleepiness);
            CheckThreshold();
        }
    }

    /// <summary>
    /// 强制重置困意值为 0（被怪物袭击后清空）
    /// </summary>
    public void ResetSleepiness()
    {
        float oldValue = currentSleepiness;
        currentSleepiness = 0f;
        if (!Mathf.Approximately(oldValue, currentSleepiness))
        {
            OnSleepinessChanged?.Invoke(currentSleepiness);
            CheckThreshold();
        }
    }

    /// <summary>
    /// 服用助眠药：加速困意增长（持续 10 秒）
    /// </summary>
    public void TakeMedication()
    {
        // 用 TimerMgr 创建一个 10 秒的临时倍率覆盖
        int timerId = TimerMgr.Instance.CreatTimeItem(
            false, 
            () => { currentMultiplier = 1f; }, // 结束回调：恢复默认倍率
            10000, // 10秒 = 10000ms
            null,
            0
        );
        
        // 立即应用倍率（如果当前有其他倍率，叠加计算）
        currentMultiplier = medicationMultiplier;
        Debug.Log($"服用助眠药，困意增长加速至 {medicationMultiplier}x，持续 10 秒");
    }

    /// <summary>
    /// 打开收音机：降低/延缓困意增长（持续开启期间一直有效，由状态机开关）
    /// </summary>
    public void SetRadioMode(bool isOn)
    {
        if (isOn)
        {
            // 收音机开启：降低倍率（注意：如果吃药中，叠加效果？采用乘法叠加避免极端）
            // 这里策略是：降低基础倍率，但保留药物效果（更合理）
            currentMultiplier = radioMultiplier;
            Debug.Log($"打开收音机，困意增长减缓至 {radioMultiplier}x");
        }
        else
        {
            // 关闭收音机：恢复为正常（如果有药物效果则恢复为药物倍率，简化处理直接恢复 1）
            // 实际生产中可设计为栈式覆盖，这里简单恢复 1
            currentMultiplier = 1f;
            Debug.Log("关闭收音机，困意增长恢复正常");
        }
    }

    // ---------- 冻结控制（由 SafeZoneState 调用） ----------

    /// <summary>
    /// 冻结/解冻困意变化（被窝专用）
    /// </summary>
    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;
        Debug.Log($"困意系统 {(frozen ? "已冻结" : "已解冻")}");
    }

    // ---------- 生命周期（由 MonoMgr 驱动） ----------

    // 初始化方法（在 GameManager.Awake 中调用一次）
    public void Initialize()
    {
        // 将被动增长注册到 MonoMgr 的 Update 循环中（因为 BaseMgr 没有 Update）
        MonoMgr.Instance.AddInUpdate(OnPassiveUpdate);
        // 确保初始状态触发一次 UI 更新
        EventCenter.Instance.EventTrigger(E_EventType.UpdateUISleepBar,currentSleepiness);
        CheckThreshold();
    }

    // 被动增长循环（每帧执行）
    private void OnPassiveUpdate()
    {
        // 如果冻结 或 已经满了，不增长
        if (isFrozen || currentSleepiness >= 1f) return;

        // 计算增量：基础速率 * 当前综合倍率 * Time.deltaTime
        float delta = basePassiveRate * currentMultiplier * Time.deltaTime;
        if (delta <= 0) return;

        float oldValue = currentSleepiness;
        currentSleepiness = Mathf.Clamp01(currentSleepiness + delta);

        // 数值变化超过 0.001 才触发事件（防止高频无意义刷新）
        if (Mathf.Abs(currentSleepiness - oldValue) > 0.001f)
        {
            OnSleepinessChanged?.Invoke(currentSleepiness);
            EventCenter.Instance.EventTrigger(E_EventType.UpdateUISleepBar,currentSleepiness);
            CheckThreshold();
        }
    }

    // ---------- 阈值检测（内部逻辑） ----------

    private void CheckThreshold()
    {
        bool isDangerous = currentSleepiness >= dangerThreshold;

        // 刚进入危险区
        if (isDangerous && !wasDangerous)
        {
            wasDangerous = true;
            OnDangerThresholdReached?.Invoke(currentSleepiness);
            Debug.Log($"?? 困意值达到 {currentSleepiness:F2}，触发危险阈值！");
        }
        // 刚脱离危险区
        else if (!isDangerous && wasDangerous)
        {
            wasDangerous = false;
            OnDangerThresholdExited?.Invoke();
            Debug.Log($"? 困意值降至 {currentSleepiness:F2}，脱离危险区");
        }
    }

    // ---------- 清理（防止内存泄漏） ----------

    /// <summary>
    /// 场景切换或游戏结束时调用
    /// </summary>
    public void Cleanup()
    {
        MonoMgr.Instance.RemoveInUpdate(OnPassiveUpdate);
        OnSleepinessChanged = null;
        OnDangerThresholdReached = null;
        OnDangerThresholdExited = null;
    }
}
