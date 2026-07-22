using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 任务管理器（简化版）
/// 每个任务只有一步，检测最终操作是否完成
/// </summary>
public class TaskManager : BaseMonoMgr<TaskManager>
{
    // 事件
    public event UnityAction<TaskData> OnTaskCompleted;
    public event UnityAction OnAllTasksCompleted;

    private List<TaskData> currentTasks = new List<TaskData>();
    private int completedCount = 0;

    public int TotalTaskCount => currentTasks.Count;
    public int CompletedCount => completedCount;
    public bool IsAllTasksCompleted => completedCount >= TotalTaskCount && TotalTaskCount > 0;

    // 运行时记录（用于检测条件）
    private HashSet<string> enteredAreas = new HashSet<string>();
    private HashSet<string> pickedUpItems = new HashSet<string>();
    private HashSet<string> usedItems = new HashSet<string>();
    private HashSet<string> interactedObjects = new HashSet<string>();
    private Dictionary<string, float> delayTimers = new Dictionary<string, float>();

    protected override void OnInit()
    {
        base.OnInit();
        ClearAllRecords();
    }

    // ==================== 初始化 ====================

    /// <summary>
    /// 根据 DayConfig 初始化任务列表
    /// </summary>
    public void InitializeFromDayConfig(DayConfig config)
    {
        if (config == null || config.tasks == null)
        {
            Debug.LogWarning("TaskManager: 无任务配置");
            return;
        }

        currentTasks.Clear();
        foreach (var taskData in config.tasks)
        {
            currentTasks.Add(new TaskData
            {
                taskID = taskData.taskID,
                taskName = taskData.taskName,
                description = taskData.description,
                conditionType = taskData.conditionType,
                targetID = taskData.targetID,
                sleepinessReward = taskData.sleepinessReward,
                isCompleted = false
            });
        }

        completedCount = 0;
        ClearAllRecords();
        OnAllTasksCompleted = null;

        Debug.Log($"TaskManager 初始化，共 {TotalTaskCount} 个任务");
    }

    // ==================== 条件检测（由外部事件触发） ====================

    /// <summary>
    /// 在 Update 中调用，检测延迟型任务
    /// </summary>
    public void UpdateTasks()
    {
        if (IsAllTasksCompleted) return;

        // 只检测延迟类型（其他类型由事件触发）
        foreach (var task in currentTasks)
        {
            if (task.isCompleted) continue;

            if (task.conditionType == TaskConditionType.Delay)
            {
                // 延迟需要 floatParam，存储时需要在 TaskData 中添加
                // 这里简化：使用 targetID 作为计时器键
                if (!delayTimers.ContainsKey(task.taskID))
                    delayTimers[task.taskID] = 0f;

                delayTimers[task.taskID] += Time.deltaTime;
                float requiredTime = GetDelayTime(task);
                if (delayTimers[task.taskID] >= requiredTime)
                {
                    CompleteTask(task);
                }
            }
        }
    }

    private float GetDelayTime(TaskData task)
    {
        // 从 task 中获取延迟时间，暂时从 targetID 解析或使用默认值
        // 实际项目中可以添加 floatParam 字段
        return 2f; // 默认2秒
    }

    // ==================== 外部通知接口（由场景物体调用） ====================

    public void NotifyEnterArea(string areaID)
    {
        if (string.IsNullOrEmpty(areaID)) return;
        enteredAreas.Add(areaID);
        CheckTasks(TaskConditionType.EnterArea, areaID);
    }

    public void NotifyPickupItem(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return;
        pickedUpItems.Add(itemID);
        CheckTasks(TaskConditionType.PickupItem, itemID);
    }

    public void NotifyUseItem(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return;
        usedItems.Add(itemID);
        CheckTasks(TaskConditionType.UseItem, itemID);
    }

    public void NotifyInteracted(string objectID)
    {
        if (string.IsNullOrEmpty(objectID)) return;
        interactedObjects.Add(objectID);
        CheckTasks(TaskConditionType.InteractObject, objectID);
    }

    public void NotifyCustomEvent(string eventName)
    {
        if (string.IsNullOrEmpty(eventName)) return;
        CheckTasks(TaskConditionType.CustomEvent, eventName);
    }

    // ==================== 核心检测逻辑 ====================

    private void CheckTasks(TaskConditionType type, string targetID)
    {
        if (IsAllTasksCompleted) return;

        foreach (var task in currentTasks)
        {
            if (task.isCompleted) continue;
            if (task.conditionType != type) continue;
            if (task.targetID != targetID) continue;

            // 条件匹配，完成任务
            CompleteTask(task);
        }
    }

    private void CompleteTask(TaskData task)
    {
        if (task.isCompleted) return;

        task.isCompleted = true;
        completedCount++;

        // 增加困意
        SleepinessManager.Instance.ModifySleepiness(task.sleepinessReward);

        OnTaskCompleted?.Invoke(task);
        Debug.Log($"任务完成: {task.taskName}，困意 +{task.sleepinessReward:P0}");

        // 检测是否全部完成
        if (IsAllTasksCompleted)
        {
            OnAllTasksCompleted?.Invoke();
            Debug.Log("所有任务完成！");
        }
    }

    // ==================== 查询接口 ====================

    public IReadOnlyList<TaskData> GetTasks() => currentTasks.AsReadOnly();

    public bool IsTaskCompleted(string taskID)
    {
        var task = currentTasks.Find(t => t.taskID == taskID);
        return task != null && task.isCompleted;
    }

    public TaskData GetTask(string taskID)
    {
        return currentTasks.Find(t => t.taskID == taskID);
    }

    // ==================== 重置 ====================

    public void ResetAllTasks()
    {
        foreach (var task in currentTasks)
        {
            task.isCompleted = false;
        }
        completedCount = 0;
        ClearAllRecords();
        Debug.Log("任务已全部重置");
    }

    private void ClearAllRecords()
    {
        enteredAreas.Clear();
        pickedUpItems.Clear();
        usedItems.Clear();
        interactedObjects.Clear();
        delayTimers.Clear();
    }

    // ==================== 清理 ====================

    public void Cleanup()
    {
        OnTaskCompleted = null;
        OnAllTasksCompleted = null;
    }
}