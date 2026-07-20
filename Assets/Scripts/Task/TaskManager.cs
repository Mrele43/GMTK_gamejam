using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 任务清单管理器
/// 职责：管理任务列表、完成状态、进度事件、与 SleepinessManager 联动
/// </summary>
public class TaskManager : BaseMgr<TaskManager>
{
    private TaskManager() { }

    // ---------- 事件（供 UI 和状态机订阅） ----------
    /// <summary>任务状态变化事件（参数：任务ID, 是否完成）</summary>
    public event UnityAction<string, bool> OnTaskStateChanged;
    /// <summary>全部任务完成事件</summary>
    public event UnityAction OnAllTasksCompleted;

    // ---------- 数据 ----------
    private List<TaskData> taskList = new List<TaskData>();
    private int completedCount = 0;
    public int TotalTaskCount => taskList.Count;
    public int CompletedCount => completedCount;
    public bool IsAllTasksCompleted => completedCount >= TotalTaskCount && TotalTaskCount > 0;

    // ---------- 初始化（从 ScriptableObject 加载） ----------
    public void Initialize(TaskListConfig config)
    {
        if (config == null)
        {
            Debug.LogError("TaskManager: 未提供 TaskListConfig！");
            return;
        }

        // 深拷贝任务列表（避免修改配置资源）
        taskList.Clear();
        foreach (var task in config.tasks)
        {
            taskList.Add(new TaskData
            {
                taskID = task.taskID,
                description = task.description,
                isCompleted = false,
                sleepinessReward = task.sleepinessReward
            });
        }

        completedCount = 0;
        Debug.Log($"TaskManager 初始化，共 {TotalTaskCount} 个任务");
    }

    // ---------- 核心业务接口 ----------

    /// <summary>
    /// 标记某个任务为已完成
    /// </summary>
    /// <param name="taskID">任务唯一ID</param>
    /// <returns>是否成功标记</returns>
    public bool CompleteTask(string taskID)
    {
        if (string.IsNullOrEmpty(taskID))
        {
            Debug.LogWarning("CompleteTask: taskID 为空");
            return false;
        }

        // 查找任务
        TaskData target = taskList.Find(t => t.taskID == taskID);
        if (target == null)
        {
            Debug.LogWarning($"CompleteTask: 未找到任务 ID '{taskID}'");
            return false;
        }

        // 如果已经完成，忽略
        if (target.isCompleted)
        {
            Debug.Log($"任务 '{taskID}' 已经完成，无需重复");
            return false;
        }

        // 标记完成
        target.isCompleted = true;
        completedCount++;

        // 增加困意值（调用 SleepinessManager）
        SleepinessManager.Instance.ModifySleepiness(target.sleepinessReward);

        // 触发事件
        OnTaskStateChanged?.Invoke(taskID, true);
        Debug.Log($"任务完成: {target.description} (ID: {taskID})，困意 +{target.sleepinessReward}");

        // 检测是否全部完成
        if (IsAllTasksCompleted)
        {
            OnAllTasksCompleted?.Invoke();
            Debug.Log("? 所有任务完成！");
        }

        return true;
    }

    /// <summary>
    /// 获取当前任务列表（只读）
    /// </summary>
    public IReadOnlyList<TaskData> GetTaskList() => taskList.AsReadOnly();

    /// <summary>
    /// 获取某个任务的完成状态
    /// </summary>
    public bool IsTaskCompleted(string taskID)
    {
        var task = taskList.Find(t => t.taskID == taskID);
        return task != null && task.isCompleted;
    }

    /// <summary>
    /// 重置所有任务（重新开始游戏时调用）
    /// </summary>
    public void ResetAllTasks()
    {
        foreach (var task in taskList)
        {
            task.isCompleted = false;
        }
        completedCount = 0;
        // 不触发事件，因为重置时通常整个状态机重置
        Debug.Log("任务已全部重置");
    }

    // ---------- 清理 ----------
    public void Cleanup()
    {
        OnTaskStateChanged = null;
        OnAllTasksCompleted = null;
    }
}
