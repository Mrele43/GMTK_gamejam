using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单个任务项 UI（简化版）
/// </summary>
public class TaskItemUI : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private Text taskNameText;
    [SerializeField] private Image checkmarkImage;
    [SerializeField] private Color completedColor = Color.gray;
    [SerializeField] private Color activeColor = Color.white;

    private TaskData boundTask;

    public void Setup(TaskData task)
    {
        boundTask = task;
        if (taskNameText != null)
            taskNameText.text = task.taskName;
        SetCompleted(task.isCompleted);
    }

    public void SetCompleted(bool completed)
    {
        if (checkmarkImage != null)
            checkmarkImage.gameObject.SetActive(completed);

        if (taskNameText != null)
            taskNameText.color = completed ? completedColor : activeColor;
    }
}
