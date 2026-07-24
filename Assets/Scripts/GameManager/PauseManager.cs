using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    // 全局暂停状态
    public bool IsGamePaused { get; private set; }
    private float _cachedTimeScale;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // ESC切换暂停
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// 切换暂停/继续
    /// </summary>
    public void TogglePause()
    {
        if (IsGamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }

    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        // 打开暂停面板
        UIManager.Instance.ShowPanel<PausePanel>();
        IsGamePaused = true;

        // 解锁鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void ResumeGame()
    {
        IsGamePaused = false;
        // 隐藏暂停面板
        UIManager.Instance.HidePanel<PausePanel>();
        // 锁定鼠标（FPS）
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        SceneMgr.Instance.LoadSceneAsync("GameScene");
        ResumeGame();
    }
}