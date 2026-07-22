using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : basePlane
{
    public Text InteractTxt;

    public Scrollbar sleepinessSlider;

    //任务相关

    [Header("UI 引用")]

    public GameObject taskPanel;
    [SerializeField] private Transform taskContainer;
    [SerializeField] private GameObject taskItemPrefab;

    [Header("标题")]
    [SerializeField] private Text titleText;

    private Dictionary<string, TaskItemUI> taskItemMap = new Dictionary<string, TaskItemUI>();


    protected override void Init()
    {

        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskCompleted += OnTaskCompleted;
            TaskManager.Instance.OnAllTasksCompleted += OnAllTasksCompleted;
        }

        if (titleText != null)
            titleText.text = "睡前任务";


        InteractTxt.gameObject.SetActive(false);
        taskPanel.gameObject.SetActive(true);
        EventCenter.Instance.AddEventListener<float>(E_EventType.UpdateUISleepBar,UpdateBar);
        EventCenter.Instance.AddEventListener<bool>(E_EventType.ShowInteractTxt,ShowInteractTxt);
        EventCenter.Instance.AddEventListener<string>(E_EventType.UpdateInteractTip,UpdateInteractTip);
    }
    protected override void Update()
    {
        base.Update();

        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if(taskPanel.activeSelf)
            {
                taskPanel.SetActive(false);
            }
            else
            {
                taskPanel.SetActive(true);
                RefreshAllTasks();
            }
        }
    }



    private void UpdateBar(float value)
    {
        sleepinessSlider.size = value;
    }

    private void RefreshAllTasks()
    {
        if (TaskManager.Instance == null) return;

        foreach (Transform child in taskContainer)
            Destroy(child.gameObject);
        taskItemMap.Clear();

        var tasks = TaskManager.Instance.GetTasks();
        if (tasks == null || tasks.Count == 0)
        {
            ShowEmptyHint("所有任务已完成");
            return;
        }

        foreach (var task in tasks)
        {
            GameObject itemObj = Instantiate(taskItemPrefab, taskContainer);
            TaskItemUI itemUI = itemObj.GetComponent<TaskItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(task);
                taskItemMap[task.taskID] = itemUI;
            }
        }
    }

    private void ShowEmptyHint(string text)
    {
        GameObject hint = new GameObject("EmptyHint");
        hint.transform.SetParent(taskContainer);
        var tmp = hint.AddComponent<Text>();
        tmp.text = text;
        tmp.alignment = TextAnchor.MiddleCenter;
        tmp.fontSize = 18;
        tmp.color = Color.gray;
    }

    private void OnTaskCompleted(TaskData task)
    {
        if (taskItemMap.TryGetValue(task.taskID, out TaskItemUI itemUI))
        {
            itemUI.SetCompleted(true);
        }
        Debug.Log($"TaskPanel: 任务 {task.taskName} 已完成");
    }

    private void OnAllTasksCompleted()
    {
        if (titleText != null)
            titleText.text = "全部完成！";
    }


    private void ShowInteractTxt(bool isShow)
    {
        InteractTxt.gameObject.SetActive(isShow);
    }

    private void UpdateInteractTip(string tipText)
    {
        InteractTxt.text = tipText;
    }

        public override void ShwoMe()
    {
        base.ShwoMe();
        RefreshAllTasks();
    }

    void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.UpdateUISleepBar,UpdateBar);
        EventCenter.Instance.RemoveEventListener<bool>(E_EventType.ShowInteractTxt,ShowInteractTxt);
        EventCenter.Instance.RemoveEventListener<string>(E_EventType.UpdateInteractTip,UpdateInteractTip);

        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskCompleted -= OnTaskCompleted;
            TaskManager.Instance.OnAllTasksCompleted -= OnAllTasksCompleted;
        }
    }



}
