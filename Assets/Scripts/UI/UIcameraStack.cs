using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UIcameraStack : MonoBehaviour
{
    public Camera uiCamera;
    // Start is called before the first frame update
    void Start()
    {
        // 获取主相机URP数据
        UniversalAdditionalCameraData baseCamData = Camera.main.GetUniversalAdditionalCameraData();
        // 将Overlay UI相机加入堆栈
        baseCamData.cameraStack.Add(uiCamera);
        
    }

}
