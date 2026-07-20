using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : basePlane
{
    public Text InteractTxt;

    public Scrollbar sleepinessSlider;
    protected override void Init()
    {
        InteractTxt.gameObject.SetActive(false);
        EventCenter.Instance.AddEventListener<float>(E_EventType.UpdateUISleepBar,UpdateBar);
    }


    private void UpdateBar(float value)
    {
        sleepinessSlider.size = value;
    }

    void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.UpdateUISleepBar,UpdateBar);
    }



}
