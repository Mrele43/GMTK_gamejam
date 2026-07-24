using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PausePanel : basePlane
{
    [Header("按钮")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button quitButton;
    GameManager gm;

    protected override void Init()
    {
        gm = FindObjectOfType<GameManager>();
        if (continueButton) continueButton.onClick.AddListener(OnContinueClicked);
        if (newGameButton) newGameButton.onClick.AddListener(OnNewGameClicked);
        if (quitButton) quitButton.onClick.AddListener(OnQuitClicked);
    }


    private void OnContinueClicked()
    {
        gm?.TogglePause(false);
    }

    private void OnNewGameClicked()
    {
        gm?.RestartGame();
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public override void ShwoMe()
    {
        base.ShwoMe();
        // 默认选中继续按钮
        if (continueButton) continueButton.Select();

    }

}
