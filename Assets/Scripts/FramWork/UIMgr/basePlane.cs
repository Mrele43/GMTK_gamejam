using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class basePlane : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    private float alphaSpeed = 10;

    private bool isHide = false;


    //面板淡出后外界传入的方法
    private UnityAction HideCallBack;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null )
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        Init();
    }


    protected abstract void Init();

    public virtual void HideMe(UnityAction callUnit)
    {
        canvasGroup.alpha = 1;
        isHide = true;
        HideCallBack = callUnit;
    }

    public virtual void ShwoMe()
    {
        canvasGroup.alpha = 0;
        isHide = false;

    }

    protected virtual void Update()
    {
        if (isHide && canvasGroup.alpha != 0 )
        {
            canvasGroup.alpha -= alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha <= 0 )
            {
                canvasGroup.alpha = 0;
                //面板淡出后，可继续完成的逻辑
                if (HideCallBack != null)
                    HideCallBack?.Invoke();
            }
        }
        else if (!isHide && canvasGroup.alpha != 1)
        {
            canvasGroup.alpha += alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha > 1)
            {
                canvasGroup.alpha = 1;
            }
        }
    }
}
