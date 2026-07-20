using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeginPanel : basePlane
{
    public Button startBtn;
    protected override void Init()
    {
        startBtn.onClick.AddListener(() =>
        {
            SceneMgr.Instance.LoadScene("LowPolyInterior_Demo");
        });
    }

}
