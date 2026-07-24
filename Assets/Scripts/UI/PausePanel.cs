using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PausePanel : basePlane
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button leaveButton;

    protected override void Init()
    {

        // 新游戏：重启场景
        newGameButton.onClick.AddListener(() =>
        {
            PauseManager.Instance.RestartGame();
        });

        // 继续游戏：关闭暂停、恢复时间
        continueButton.onClick.AddListener(() =>
        {
            PauseManager.Instance.ResumeGame();
        });

        // 设置面板
        settingButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel<SettingPanel>();
        });

        // 退出游戏
        leaveButton.onClick.AddListener(() =>
        {
            PauseManager.Instance.QuitGame();
        });


    }


}
