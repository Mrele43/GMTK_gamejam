using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTap : MonoBehaviour, IInteractable
{
    [Header("交互提示")]
    [Tooltip("关闭状态的交互提示")]
    public string closedTip = "按E打开水龙头";
    [Tooltip("开启状态的交互提示")]
    public string openedTip = "按E关闭水龙头";

    [Header("可选效果")]
    [Tooltip("水流粒子/水流模型")]
    public GameObject waterEffect;
    [Tooltip("开关音效")]
    public AudioClip switchSound;

    private bool _isOn = false;
    private AudioSource _audio;

    void Start()
    {
        // 初始默认关闭，隐藏水流
        if (waterEffect != null)
            waterEffect.SetActive(false);
        
        // 自动添加音效组件
        _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
    }

    public void Interact()
    {
        _isOn = !_isOn;

        // 切换水流显示
        if (waterEffect != null)
            waterEffect.SetActive(_isOn);

        // 播放开关音效
        if (switchSound != null && _audio != null)
            _audio.PlayOneShot(switchSound);

        Debug.Log(_isOn ? "水龙头已打开" : "水龙头已关闭");
    }

    // 公开状态，供任务/剧情系统读取
    public bool IsOn => _isOn;

    public string GetInteractTip()
    {
        return _isOn ? openedTip : closedTip;
    }
}
