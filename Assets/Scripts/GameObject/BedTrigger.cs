using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 床交互（按E钻被窝 / 按E上床睡觉）
/// </summary>
[RequireComponent(typeof(Collider))]
public class BedTrigger : MonoBehaviour, IInteractable
{
    [Header("提示文字")]
    [SerializeField] private string enterBedTip = "按E钻被窝";
    [SerializeField] private string exitBedTip = "按E离开被窝";
    [SerializeField] private string sleepTip = "按E上床睡觉";
    //[SerializeField] private string lockedTip = "真得睡觉了";

    private Collider bedCollider;
    private PlayerController cachedPlayer;
    private bool isPlayerInRange = false;

    void Start()
    {
        bedCollider = GetComponent<Collider>();
        if (bedCollider != null)
            bedCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            cachedPlayer = other.GetComponent<PlayerController>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            cachedPlayer = null;
        }
    }

    /// <summary>
    /// IInteractable 交互方法（按E时调用）
    /// </summary>
    public void Interact()
    {
        if (cachedPlayer == null) return;

        // ===== 情况1：玩家已经在被窝里 → 离开被窝 =====
        if (cachedPlayer.IsInBed)
        {
            cachedPlayer.ExitBed();
            return;
        }

        // ===== 情况2：困意已锁定（完成全部任务） → 上床睡觉 =====
        if (SleepinessManager.Instance.IsLocked)
        {
            // 触发胜利/天过渡
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                // 通知 GameManager 进入胜利状态
                gm.AdvanceToNextDay();
            }
            Debug.Log("上床睡觉！");
            return;
        }

        // ===== 情况3：正常钻被窝 =====
        // 检查困意是否达到90%以上（可上床睡觉，但还没完成任务）
        if (SleepinessManager.Instance.CurrentSleepiness >= 0.9f)
        {
            // GDD: 困意达到90%以上可以上床，但如果没有完成任务，不能结算
            // 可以显示提示 "还没完成所有任务..."
            Debug.Log("还没完成所有任务，还不能睡觉...");
            // TODO: 显示UI提示
            return;
        }

        // 钻被窝
        cachedPlayer.EnterBed();
    }

    /// <summary>
    /// IInteractable 获取交互提示
    /// </summary>
    public string GetInteractTip()
    {
        // 玩家不在范围内，不显示提示
        if (!isPlayerInRange || cachedPlayer == null)
            return "";

        // ===== 情况1：玩家已经在被窝里 → 显示"离开被窝" =====
        if (cachedPlayer.IsInBed)
            return exitBedTip;

        // ===== 情况2：困意已锁定（完成全部任务） → 显示"上床睡觉" =====
        if (SleepinessManager.Instance.IsLocked)
            return sleepTip;

        // ===== 情况3：困意达到90%但任务未完成 =====
        if (SleepinessManager.Instance.CurrentSleepiness >= 0.9f)
            return "还没完成所有任务...";

        // ===== 情况4：困意未锁定，正常显示"钻被窝" =====
        return enterBedTip;
    }

    // 获取玩家是否在床范围内
    public bool IsPlayerInRange => isPlayerInRange;
}
