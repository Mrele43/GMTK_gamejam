using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TV : MonoBehaviour, IInteractable
{
    [Header("电视配置")]
    [Tooltip("关机状态的交互提示")]
    public string closedTip = "按E用遥控器打开电视";
    [Tooltip("开机状态的交互提示")]
    public string openedTip = "电视已开启";

    private bool _isOn = false;
    private PlayerController _player;

    // 可选扩展字段：开机屏幕、音效等
    // public GameObject tvScreen;
    // public AudioClip turnOnClip;

    void Start()
    {
        _player = FindObjectOfType<PlayerController>();
    }

    public void Interact()
    {
        // 已开机则不重复执行
        if (_isOn)
        {
            Debug.Log("电视已经处于开启状态");
            return;
        }

        // 校验：玩家必须手持遥控器
        if (_player == null || !(_player.CurrentHoldItem is RemoteItem))
        {
            Debug.Log("需要手持电视遥控器才能开机");
            return;
        }

        // 执行开机逻辑，遥控器保留在玩家手中
        _isOn = true;
        Debug.Log("电视已成功打开");

        // ===== 扩展效果（自行启用） =====
        // 显示屏幕画面
        // if (tvScreen != null) tvScreen.SetActive(true);
        // 播放开机音效
        // if (turnOnClip != null) AudioSource.PlayClipAtPoint(turnOnClip, transform.position);
        // 通知任务系统
        // EventCenter.Instance.EventTrigger(E_EventType.TVTurnedOn);
    }

    // 公开状态，供外部任务/剧情读取
    public bool IsOn => _isOn;

    public string GetInteractTip()
    {
        return _isOn ? openedTip : closedTip;
    }
}
