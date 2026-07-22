using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;

public class SettingPanel : basePlane
{
        [Header("BGM")]
    [SerializeField] private Toggle bgmToggle;
    [SerializeField] private Slider bgmSlider;
    [Header("SFX")]
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button closeBtn;

    private float lastBgmVolume = 0.8f;   
    private float lastSfxVolume = 0.8f;
    override protected void Init()
    {
        bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        bgmToggle.onValueChanged.AddListener(OnBgmToggleChanged);
        sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);
        closeBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<SettingPanel>();
        });
    }

    private void OnBgmSliderChanged(float value)
    {
        lastBgmVolume = value;
        if (bgmToggle.isOn)
        {
            AudioMgr.Instance.bgmVolume = value;
        }
        //SaveSettings();
    }

    private void OnSfxSliderChanged(float value)
    {
        lastSfxVolume = value;
        if (sfxToggle.isOn)
        {
            AudioMgr.Instance.sfxVolume = value;
        }
        //SaveSettings();
    }

    private void OnBgmToggleChanged(bool isOn)
    {
        if (isOn)
            AudioMgr.Instance.bgmVolume = lastBgmVolume;
        else
            AudioMgr.Instance.bgmVolume = 0f;
        UpdateUI();
        //SaveSettings();
    }

    private void OnSfxToggleChanged(bool isOn)
    {
        if (isOn)
            AudioMgr.Instance.sfxVolume = lastSfxVolume;
        else
            AudioMgr.Instance.sfxVolume = 0f;
        UpdateUI();
        //SaveSettings();
    }

        private void UpdateUI()
    {
        bgmSlider.interactable = bgmToggle.isOn;
        sfxSlider.interactable = sfxToggle.isOn;
    }

    /// <summary>
    /// 按钮绑定这个方法
    /// 安全、无报错、彻底清理
    /// </summary>
    void GoBackToStart()
    {
        ClearMgrInstance<UIManager>();

        // 1. 销毁所有跨场景残留的对象（最关键）
        DestroyAllDontDestroyOnLoad();

        // 2. 加载初始场景 = 回到刚打开游戏的状态
        SceneManager.LoadScene("BeginScene");
    }

    void ClearMgrInstance<T>() where T : class
    {
        var type = typeof(T);
        var field = type.BaseType.GetField("instance",
            BindingFlags.NonPublic | BindingFlags.Static);

        if (field != null)
        {
            field.SetValue(null, null);
        }
    }

    /// <summary>
    /// 安全清理所有DontDestroyOnLoad，不报错
    /// </summary>
    void DestroyAllDontDestroyOnLoad()
    {
        // 找到所有DontDestroy对象
        GameObject[] objs = DontDestroyOnLoadObjects();

        foreach (var go in objs)
        {
            // 不销毁自己（避免执行中断）
            if (go == gameObject) continue;

            Destroy(go);
        }
    }

    /// <summary>
    /// 安全获取DontDestroyOnLoad对象（Unity官方方法，无报错）
    /// </summary>
    GameObject[] DontDestroyOnLoadObjects()
    {
        var temp = new GameObject();
        DontDestroyOnLoad(temp);

        var scene = temp.scene;
        DestroyImmediate(temp);

        return scene.GetRootGameObjects();
    }



}
