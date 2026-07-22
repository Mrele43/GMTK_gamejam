using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeginPanel : basePlane
{
    public Button startBtn;

    public Button settingBtn;

    public Button exitBtn;
    protected override void Init()
    {
        startBtn.onClick.AddListener(() =>
        {
            SceneMgr.Instance.LoadScene("LowPolyInterior_Demo");
        });

        settingBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel<SettingPanel>();
        });

        exitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

}
