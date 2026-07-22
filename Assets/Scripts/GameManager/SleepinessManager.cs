using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 困意值管理器（GDD v2.0）
/// - 无自动增长，所有变化由外部事件触发
/// - 受击后降至0%并以5%/s恢复至受击前值
/// - 完成全部任务后锁定100%，不可改变
/// </summary>
public class SleepinessManager : BaseMonoMgr<SleepinessManager>
{
    // ---------- 事件 ----------
    public event UnityAction<float> OnSleepinessChanged;           // 当前值变化 (0~1)
    public event UnityAction<float> OnThresholdReached;            // 达到阈值 (参数为阈值)
    public event UnityAction<float> OnThresholdExited;            // 脱离阈值 (参数为阈值)
    public event UnityAction OnLocked;                             // 锁定事件
    public event UnityAction OnRecoveryStart;                      // 开始恢复
    public event UnityAction OnRecoveryComplete;                   // 恢复完成

    // ---------- 配置参数 ----------
    [Header("基础配置")]
    [SerializeField] private float initialSleepiness = 0.2f;      // 初始值 20%
    [SerializeField] private float maxSleepiness = 1f;            // 100%
    [SerializeField] private float minSleepiness = 0f;            // 0%
    [SerializeField] private float recoverySpeed = 0.05f;         // 5%/秒

    [Header("阈值配置")]
    [SerializeField] private float thresholdLivingRoom = 0.7f;    // 70%
    [SerializeField] private float thresholdBathroom = 0.8f;      // 80%
    [SerializeField] private float thresholdBedroom = 0.9f;       // 90%

    // ---------- 运行时状态 ----------
    private float currentSleepiness;
    private float recoveryTarget;          // 受击前值，恢复目标
    private bool isRecovering = false;
    private bool isLocked = false;         // 锁定状态

    // ---------- 公开属性 ----------
    public float CurrentSleepiness => currentSleepiness;
    public bool IsLocked => isLocked;
    public bool IsRecovering => isRecovering;

    // ---------- 初始化 ----------
    protected override void OnInit()
    {
        base.OnInit();
        currentSleepiness = Mathf.Clamp(initialSleepiness, minSleepiness, maxSleepiness);
        isRecovering = false;
        isLocked = false;
        recoveryTarget = 0f;
        OnSleepinessChanged?.Invoke(currentSleepiness);
        CheckAllThresholds();
        Debug.Log($"SleepinessManager 初始化，初始值: {currentSleepiness:P0}");
    }

    // ---------- Update 驱动恢复 ----------
    private void Update()
    {
        // 处理受击恢复
        if (isRecovering && !isLocked)
        {
            // 恢复速度：每秒 recoverySpeed (5%)
            float delta = recoverySpeed * Time.deltaTime;
            currentSleepiness = Mathf.Min(currentSleepiness + delta, recoveryTarget);

            OnSleepinessChanged?.Invoke(currentSleepiness);
            CheckAllThresholds();

            if (Mathf.Approximately(currentSleepiness, recoveryTarget))
            {
                isRecovering = false;
                OnRecoveryComplete?.Invoke();
                Debug.Log($"困意恢复完成，当前值: {currentSleepiness:P0}");
            }
        }
    }

    // ---------- 核心修改接口 ----------

    /// <summary>
    /// 修改困意值（通用接口）
    /// 正数增加，负数减少
    /// 锁定状态下无效
    /// 恢复过程中允许修改，但不会超过恢复目标
    /// </summary>
    public void ModifySleepiness(float amount)
    {
        if (isLocked)
        {
            Debug.LogWarning("困意已锁定，无法修改");
            return;
        }

        float oldValue = currentSleepiness;
        float newValue = currentSleepiness + amount;

        // 如果正在恢复，限制不能超过 recoveryTarget
        if (isRecovering)
        {
            newValue = Mathf.Clamp(newValue, minSleepiness, recoveryTarget);
        }
        else
        {
            newValue = Mathf.Clamp(newValue, minSleepiness, maxSleepiness);
        }

        if (!Mathf.Approximately(oldValue, newValue))
        {
            currentSleepiness = newValue;
            OnSleepinessChanged?.Invoke(currentSleepiness);
            CheckAllThresholds();
        }
    }

    /// <summary>
    /// 受击处理（由怪物攻击调用）
    /// </summary>
    public void TakeDamageEffect()
    {
        // 如果锁定状态，只触发事件但不改变数值（但扣命由外部处理）
        if (isLocked)
        {
            Debug.Log("受击时困意已锁定，不发生变化");
            return;
        }

        // 记录受击前值作为恢复目标
        recoveryTarget = currentSleepiness;
        // 立即降至0%
        currentSleepiness = 0f;
        isRecovering = true;

        OnSleepinessChanged?.Invoke(currentSleepiness);
        CheckAllThresholds();
        OnRecoveryStart?.Invoke();

        Debug.Log($"受击！困意降至0%，开始恢复至 {recoveryTarget:P0}");
    }

    /// <summary>
    /// 锁定困意值为最大值（完成全部任务时调用）
    /// </summary>
    public void LockAtMax()
    {
        if (isLocked) return;

        isLocked = true;
        currentSleepiness = maxSleepiness;
        isRecovering = false; // 停止恢复

        OnSleepinessChanged?.Invoke(currentSleepiness);
        CheckAllThresholds();
        OnLocked?.Invoke();

        Debug.Log("困意锁定100%");
    }

    /// <summary>
    /// 重置困意（新一天开始时调用）
    /// </summary>
    public void ResetForNewDay()
    {
        isLocked = false;
        isRecovering = false;
        currentSleepiness = initialSleepiness;
        recoveryTarget = 0f;
        OnSleepinessChanged?.Invoke(currentSleepiness);
        CheckAllThresholds();
        Debug.Log($"新一天，困意重置为 {currentSleepiness:P0}");
    }

    // ---------- 阈值检测 ----------
    private void CheckAllThresholds()
    {

        EventCenter.Instance.EventTrigger(E_EventType.UpdateUISleepBar, currentSleepiness);
        // 检测三个阈值是否达到
        CheckThreshold(thresholdLivingRoom, "客厅");
        CheckThreshold(thresholdBathroom, "厕所");
        CheckThreshold(thresholdBedroom, "卧室");
    }

    // 检测单个阈值（每次变化时触发，但只触发一次进入/离开）
    private float lastThresholdValue = -1f; // 用于记录上次阈值状态（简化：用三个bool字段）
    private bool[] thresholdReached = new bool[3]; // 0客厅,1厕所,2卧室

    private void CheckThreshold(float threshold, string name)
    {
        int index = -1;
        if (Mathf.Approximately(threshold, thresholdLivingRoom)) index = 0;
        else if (Mathf.Approximately(threshold, thresholdBathroom)) index = 1;
        else if (Mathf.Approximately(threshold, thresholdBedroom)) index = 2;
        if (index < 0) return;

        bool reached = currentSleepiness >= threshold;
        if (reached && !thresholdReached[index])
        {
            thresholdReached[index] = true;
            OnThresholdReached?.Invoke(threshold);
            Debug.Log($"达到阈值 {threshold:P0}，唤醒{name}怪物");
        }
        else if (!reached && thresholdReached[index])
        {
            thresholdReached[index] = false;
            OnThresholdExited?.Invoke(threshold);
            Debug.Log($"脱离阈值 {threshold:P0}，{name}怪物休眠");
        }
    }

    // ---------- 清空事件（场景切换时） ----------
    public void Cleanup()
    {
        OnSleepinessChanged = null;
        OnThresholdReached = null;
        OnThresholdExited = null;
        OnLocked = null;
        OnRecoveryStart = null;
        OnRecoveryComplete = null;
    }
}