using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FlashlightToggle : MonoBehaviour
{
    [Header("灯光组件绑定")]
    public Light spotLight;

    private bool isLightOn;

    void Awake()
    {
        // 初始化灯光默认开启
        isLightOn = true;
        spotLight.enabled = isLightOn;

    }

    void Update()
    {
        // 鼠标左键切换手电筒开关
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }
    }

    /// <summary>
    /// 切换手电开关状态
    /// </summary>
    public void ToggleFlashlight()
    {
        isLightOn = !isLightOn;
        spotLight.enabled = isLightOn;
    }

    /// <summary>
    /// 外部调用：强制打开手电（供拾取道具脚本调用）
    /// </summary>
    public void ForceLightOn()
    {
        isLightOn = true;
        spotLight.enabled = true;
    }

    /// <summary>
    /// 外部调用：强制关闭手电（怪物靠近自动熄灯逻辑可用）
    /// </summary>
    public void ForceLightOff()
    {
        isLightOn = false;
        spotLight.enabled = false;
    }

    // 获取当前手电状态
    public bool GetLightState()
    {
        return isLightOn;
    }
}
