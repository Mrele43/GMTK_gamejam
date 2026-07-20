using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

/// <summary>
/// 玩家控制器（仅处理游戏机制交互，不处理移动/视角）
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private Camera playerCamera;        // 玩家摄像机（如未指定，自动查找）
    [SerializeField] private float interactRange = 3f;   // 交互距离（按E键时用）

    // 拖拽赋值你的玩家物体（身上挂FirstPersonController）
    [SerializeField] private FirstPersonController fpsController;

    // 交互控制标志（由状态机控制）
    private bool interactionEnabled = true;

    // 公开属性
    public Camera PlayerCamera => playerCamera;
    public bool IsInBed { get; private set; }            // 是否在被窝里

    // 内部引用
    private GameManager gameManager;

    void Start()
    {
        // 自动获取摄像机
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>() ?? Camera.main;

        // 获取 GameManager
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
            Debug.LogError("场景中未找到 GameManager！");
    }

    // ==================== 被窝交互（由 BedTrigger 调用） ====================

    public void EnterBed()
    {
        if (IsInBed) return;
        IsInBed = true;
        gameManager?.PlayerEnterBed();
        Debug.Log("玩家进入被窝");
    }

    public void ExitBed()
    {
        if (!IsInBed) return;
        IsInBed = false;
        gameManager?.PlayerExitBed();
        Debug.Log("玩家离开被窝");
    }

    // ==================== 受伤处理（由 MonsterAI 调用） ====================

    public void TakeDamage()
    {
        gameManager?.PlayerTakesDamage();
    }

    // ==================== 交互：按 E 键完成任务（可选） ====================


    /// <summary>
    /// 启用/禁用玩家交互（E键任务交互，不影响移动/视角）
    /// </summary>
    public void EnableInteraction(bool enabled)
    {
        interactionEnabled = enabled;
        Debug.Log($"玩家交互 {(enabled ? "已启用" : "已禁用")}");
    }

    
    void Update()
    {
        // 只有交互启用时才响应 E 键
        if (interactionEnabled && Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            // 检测任务交互物
            // TaskInteractable task = hit.collider.GetComponent<TaskInteractable>();
            // if (task != null && !task.IsCompleted)
            // {
            //     task.Interact();
            //     return;
            // }

            // 检测收音机交互
            // RadioInteract radio = hit.collider.GetComponent<RadioInteract>();
            // if (radio != null)
            // {
            //     radio.ToggleRadio();
            //     return;
            // }

            // 检测药品交互
            // MedicationInteract medication = hit.collider.GetComponent<MedicationInteract>();
            // if (medication != null)
            // {
            //     medication.UseMedication();
            //     return;
            // }
        }
    }

    public void EnableControl(bool canMove)
    {
        fpsController.canMove = canMove;
    }

    public Vector3 GetPosition() => transform.position;

}
