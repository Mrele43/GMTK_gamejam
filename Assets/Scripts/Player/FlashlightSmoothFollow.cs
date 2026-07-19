using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightSmoothFollow : MonoBehaviour
{
    [Header("目标相机")]
    public Transform camTarget;
    
    [Header("左右水平旋转迟滞（越小拖慢越明显，推荐2~5）")]
    public float horizontalSmooth = 3f;
    [Header("上下俯仰迟滞（建议比水平快，7~12）")]
    public float verticalSmooth = 9f;

    [Header("手持偏移位置")]
    public Vector3 handOffset = new Vector3(0, 0, 0);

    private Vector3 targetPos;
    private Vector3 currentEuler;

    void Start()
    {
        if (camTarget == null)
            camTarget = Camera.main.transform;
        // 初始化角度对齐
        currentEuler = camTarget.eulerAngles;
    }

    void LateUpdate()
    {
        if (camTarget == null) return;

        // 1. 位置跟随
        targetPos = camTarget.TransformPoint(handOffset);
        transform.position = Vector3.Lerp(transform.position, targetPos, 12f * Time.deltaTime);

        // 2. 拆分水平Y轴、垂直X轴单独插值，实现左右大幅迟滞
        Vector3 targetEuler = camTarget.eulerAngles;
        
        // Y轴=左右水平旋转，单独慢速插值
        currentEuler.y = Mathf.LerpAngle(currentEuler.y, targetEuler.y, horizontalSmooth * Time.deltaTime);
        // X轴=上下抬头低头，快速跟随
        currentEuler.x = Mathf.LerpAngle(currentEuler.x, targetEuler.x, verticalSmooth * Time.deltaTime);
        // Z轴不旋转，锁定0
        currentEuler.z = 0;

        transform.eulerAngles = currentEuler;
    }
}
