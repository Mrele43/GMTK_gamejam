using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : basePlane
{
    public Text InteractTxt;

    [Header("困意环形进度条")]
    public Image radialSleepImg;

    [Header("右上角困意眼睛UI")]
    public Image eyeImg;
    public Text sleepPercentText;
    [Header("眼睛帧序列顺序：0~60｜70~79｜80~89｜90~100")]
    public Sprite[] eyeSprites;

    [Header("生命值红心【从左到右拖拽】")]
    public Image[] heartImages;

    // 平滑进度变量
    private float targetSleepValue;
    private float smoothSpeed = 3f;

    //任务相关
    [Header("UI 引用")]
    public GameObject taskPanel;
    [SerializeField] private Transform taskContainer;
    [SerializeField] private GameObject taskItemPrefab;
    private Dictionary<string, TaskItemUI> taskItemMap = new Dictionary<string, TaskItemUI>();

    protected override void Init()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskCompleted += OnTaskCompleted;
            TaskManager.Instance.OnAllTasksCompleted += OnAllTasksCompleted;
        }

        InteractTxt.gameObject.SetActive(false);
        taskPanel.gameObject.SetActive(true);

        // 注册全局事件
        EventCenter.Instance.AddEventListener<float>(E_EventType.UpdateUISleepBar, UpdateBar);
        EventCenter.Instance.AddEventListener<bool>(E_EventType.ShowInteractTxt, ShowInteractTxt);
        EventCenter.Instance.AddEventListener<string>(E_EventType.UpdateInteractTip, UpdateInteractTip);
        EventCenter.Instance.AddEventListener<int>(E_EventType.UpdateHPUI, UpdateHeartUI);

        // 运行时强制校正环形进度参数
        if (radialSleepImg != null)
        {
            radialSleepImg.type = Image.Type.Filled;
            radialSleepImg.fillMethod = Image.FillMethod.Radial360;
            radialSleepImg.fillOrigin = (int)Image.Origin360.Top;
            radialSleepImg.fillClockwise = true;
            radialSleepImg.fillAmount = 0f;
        }

        // 初始状态
        targetSleepValue = 0f;
        UpdateEyeAndPercent(targetSleepValue);
    }

    protected override void Update()
    {
        base.Update();

        // 环形进度平滑插值
        if (radialSleepImg != null)
        {
            radialSleepImg.fillAmount = Mathf.Lerp(radialSleepImg.fillAmount, targetSleepValue, Time.deltaTime * smoothSpeed);
        }

        // 每帧同步更新眼睛与百分比，跟随平滑进度
        UpdateEyeAndPercent(radialSleepImg.fillAmount);

        // Tab开关任务面板
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (taskPanel.activeSelf)
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

    /// <summary>
    /// 接收外部困意数值，赋值平滑目标
    /// </summary>
    private void UpdateBar(float value)
    {
        targetSleepValue = Mathf.Clamp01(value);
    }

    /// <summary>
    /// 更新红心生命值显示
    /// </summary>
    private void UpdateHeartUI(int hp)
    {
        if (heartImages == null || heartImages.Length == 0) return;
        hp = Mathf.Clamp(hp, 0, heartImages.Length);
        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].gameObject.SetActive(i < hp);
        }
    }

    /// <summary>
    /// 根据0~1困意值切换眼睛贴图 + 更新百分比文本
    /// 分段规则：0~60｜70~79｜80~89｜90~100
    /// </summary>
    private void UpdateEyeAndPercent(float sleepVal)
    {
        // 百分比文字
        if (sleepPercentText != null)
        {
            int percent = Mathf.RoundToInt(sleepVal * 100);
            sleepPercentText.text = $"{percent}%";
        }

        if (eyeImg == null || eyeSprites == null || eyeSprites.Length < 4)
            return;

        int p = Mathf.RoundToInt(sleepVal * 100);
        Sprite targetEyeSprite;

        if (p <= 60)
        {
            targetEyeSprite = eyeSprites[0];
        }
        else if (p >= 70 && p <= 79)
        {
            targetEyeSprite = eyeSprites[1];
        }
        else if (p >= 80 && p <= 89)
        {
            targetEyeSprite = eyeSprites[2];
        }
        else // 90 ~ 100
        {
            targetEyeSprite = eyeSprites[3];
        }

        // 61~69 强制使用睁眼贴图
        if (p > 60 && p < 70)
        {
            targetEyeSprite = eyeSprites[0];
        }

        if (eyeImg.sprite != targetEyeSprite)
        {
            eyeImg.sprite = targetEyeSprite;
            eyeImg.SetNativeSize();
        }
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
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.UpdateUISleepBar, UpdateBar);
        EventCenter.Instance.RemoveEventListener<bool>(E_EventType.ShowInteractTxt, ShowInteractTxt);
        EventCenter.Instance.RemoveEventListener<string>(E_EventType.UpdateInteractTip, UpdateInteractTip);
        EventCenter.Instance.RemoveEventListener<int>(E_EventType.UpdateHPUI, UpdateHeartUI);

        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskCompleted -= OnTaskCompleted;
            TaskManager.Instance.OnAllTasksCompleted -= OnAllTasksCompleted;
        }
    }
}
