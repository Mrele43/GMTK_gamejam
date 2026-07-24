using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

/// <summary>
/// 第一人称走路镜头晃动（适配Starter Assets官方模板）
/// 挂载到玩家的 CinemachineCameraTarget 物体上
/// </summary>
public class HeadBob : MonoBehaviour
{
    [Header("核心引用")]
    [Tooltip("玩家身上的第一人称控制器组件")]
    [SerializeField] private FirstPersonController playerController;
    
    [Header("行走晃动参数")]
    [Tooltip("垂直晃动幅度，建议0.015~0.04，太大易晕")]
    public float walkAmplitude = 0.02f;
    [Tooltip("晃动频率（次/秒），对应步频")]
    public float walkFrequency = 8f;
    [Tooltip("水平晃动占垂直的比例，模拟左右摆头")]
    public float walkHorizontalRatio = 0.5f;

    [Header("冲刺晃动参数")]
    public float sprintAmplitude = 0.035f;
    public float sprintFrequency = 12f;

    [Header("平滑过渡")]
    [Tooltip("晃动启动/停止的平滑度")]
    public float smoothSpeed = 10f;
    [Tooltip("静止时复位的平滑速度")]
    public float resetSmoothSpeed = 15f;

    // 内部状态
    private Vector3 _initialLocalPos;  // 相机目标初始位置
    private float _bobTimer;           // 晃动计时器
    private float _currentAmplitude;   // 当前实际幅度
    private float _currentFrequency;   // 当前实际频率
    private CharacterController _cc;

    private void Awake()
    {
        // 记录初始本地位置，所有偏移都叠加在初始值上
        _initialLocalPos = transform.localPosition;
        _cc = playerController.GetComponent<CharacterController>();
    }

    private void LateUpdate()
    {
        // 1. 计算水平移动速度，排除垂直方向
        Vector3 velocity = _cc.velocity;
        float horizontalSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
        
        // 只有在地面且有移动速度时才触发晃动
        bool isMoving = horizontalSpeed > 0.1f && playerController.Grounded;

        // 2. 计算目标幅度和频率（区分行走/冲刺）
        float targetAmplitude = 0f;
        float targetFrequency = walkFrequency;

        if (isMoving)
        {
            bool isSprinting = horizontalSpeed > playerController.MoveSpeed + 0.1f;
            targetAmplitude = isSprinting ? sprintAmplitude : walkAmplitude;
            targetFrequency = isSprinting ? sprintFrequency : walkFrequency;
        }

        // 3. 平滑过渡参数，避免启停突兀
        float smooth = isMoving ? smoothSpeed : resetSmoothSpeed;
        _currentAmplitude = Mathf.Lerp(_currentAmplitude, targetAmplitude, Time.deltaTime * smooth);
        _currentFrequency = Mathf.Lerp(_currentFrequency, targetFrequency, Time.deltaTime * smooth);

        // 4. 累加计时器，仅移动时走动，静止时停在当前相位
        if (isMoving)
        {
            _bobTimer += Time.deltaTime * _currentFrequency;

            // 在_bobTimer累加后添加
            float prevSin = Mathf.Sin(_bobTimer - Time.deltaTime * _currentFrequency);
            float currSin = Mathf.Sin(_bobTimer);
            if (prevSin > -0.9f && currSin <= -0.9f && isMoving)
            {
                // 触发脚步声事件
                AudioMgr.Instance.PlaySfxLoop(0);
            }
            
        }
        else
        {
            AudioMgr.Instance.StopSfxLoop();
        }

        // 5. 计算晃动偏移：垂直sin + 水平半频cos，形成自然椭圆轨迹
        float yOffset = Mathf.Sin(_bobTimer) * _currentAmplitude;
        float xOffset = Mathf.Cos(_bobTimer * 0.5f) * _currentAmplitude * walkHorizontalRatio;

        // 6. 叠加偏移到初始位置
        transform.localPosition = _initialLocalPos + new Vector3(xOffset, yOffset, 0);
    }
}

