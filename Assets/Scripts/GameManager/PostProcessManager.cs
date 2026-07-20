using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using SCPE; // SC Post Effects 的 URP 特效命名空间（2.4.0 版本）

/// <summary>
/// 后处理管理器（基于 URP Volume 框架）
/// 负责平滑驱动 SC Post Effects 参数，与困意、被窝、怪物攻击联动
/// </summary>
public class PostProcessManager : BaseMgr<PostProcessManager>
{
    private PostProcessManager() { }

    // ---------- 组件引用 ----------
    private Volume volume;
    private VolumeProfile profile;

    // ---------- 缓存的 SC Post Effects 特效组件 ----------
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private RadialBlur radialBlur;
    private Blur blur;
    private DepthOfField depthOfField;

    // ---------- 目标值（业务层设置） ----------
    private float targetVignetteIntensity = 0f;
    private float targetChromaticIntensity = 0f;
    private float targetRadialBlurAmount = 0f;
    private float targetBlurAmount = 0f;

    // ---------- 当前实际值（平滑插值用） ----------
    private float currentVignetteIntensity = 0f;
    private float currentChromaticIntensity = 0f;
    private float currentRadialBlurAmount = 0f;
    private float currentBlurAmount = 0f;

    // ---------- 冲击波临时控制 ----------
    private bool isRadialBlastPlaying = false;
    private float blastRemainingTime = 0f;
    private float blastOriginalTarget = 0f; // 备份原目标值

    // ---------- 初始化（在 GameManager 中调用） ----------
    public void Initialize(Camera playerCamera)
    {
        if (playerCamera == null)
        {
            Debug.LogError("PostProcessManager: 未分配主相机！");
            return;
        }

        // 获取或添加 Volume 组件
        if (!playerCamera.TryGetComponent<Volume>(out volume))
        {
            volume = playerCamera.gameObject.AddComponent<Volume>();
        }

        // 重要：使用 profile（临时拷贝）而非 sharedProfile，避免修改原始资产
        if (volume.profile == null)
        {
            Debug.LogError("PostProcessManager: Volume Profile 为空！请先在相机 Volume 组件中指定一个 Profile。");
            return;
        }
        profile = volume.profile;

        // 尝试获取 SC Post Effects 特效（若 Profile 中未添加，则 TryGet 返回 false）
        profile.TryGet<Vignette>(out vignette);
        profile.TryGet<ChromaticAberration>(out chromaticAberration);
        profile.TryGet<RadialBlur>(out radialBlur);
        profile.TryGet<Blur>(out blur);
        profile.TryGet<DepthOfField>(out depthOfField);

        // 启用各特效的覆盖状态（与示例一致）
        // ========== 关键修正：启用各特效并设置参数覆盖 ==========
        if (vignette != null)
        {
            vignette.active = true;
            vignette.intensity.overrideState = true;
        }
        if (chromaticAberration != null)
        {
            chromaticAberration.active = true;
            chromaticAberration.intensity.overrideState = true;
        }
        if (radialBlur != null)
        {
            radialBlur.active = true;
            radialBlur.amount.overrideState = true;
        }
        if (blur != null)
        {
            blur.active = true;
            blur.amount.overrideState = true;
        }
        if (depthOfField != null)
        {
            depthOfField.active = true;
            // 可按需设置 focusDistance 等参数
        }

        // 注册到 MonoMgr 的 Update 循环
        MonoMgr.Instance.AddInUpdate(UpdateLoop);

        Debug.Log("PostProcessManager 初始化完成");
    }

    // ---------- 业务接口 ----------

    /// <summary>
    /// 根据困意值（0~1）调整基础扭曲特效
    /// </summary>
    public void SetSleepinessEffects(float sleepinessNormalized)
    {
        // 暗角：困意低时微弱，高时强烈
        targetVignetteIntensity = Mathf.Lerp(0f, 0.7f, sleepinessNormalized);

        // 色差（红蓝分离）：困意 > 0.3 时开始出现
        targetChromaticIntensity = Mathf.Lerp(0f, 0.5f, Mathf.Max(0, (sleepinessNormalized - 0.3f) * 1.5f));

        // 径向模糊（中心模糊）：困意 > 0.6 时出现
        targetRadialBlurAmount = Mathf.Lerp(0f, 0.8f, Mathf.Max(0, (sleepinessNormalized - 0.6f) * 2.5f));
    }

    /// <summary>
    /// 被窝模式（全屏高斯模糊 + 暗角加强）
    /// </summary>
    public void SetBedMode(bool isInBed)
    {
        if (isInBed)
        {
            targetBlurAmount = 1.0f;       // 最大模糊
            targetVignetteIntensity = 0.9f; // 极暗边缘
            targetChromaticIntensity = 0f;
            targetRadialBlurAmount = 0f;
        }
        else
        {
            targetBlurAmount = 0f;
            // 离开被窝后，Vignette 和色差由 SetSleepinessEffects 重新接管
        }
    }

    /// <summary>
    /// 触发一次冲击波（怪物攻击或突发幻觉）
    /// </summary>
    public void TriggerRadialBlast(float power = 1.0f)
    {
        if (radialBlur == null) return;

        isRadialBlastPlaying = true;
        blastRemainingTime = 0.3f; // 持续 0.3 秒

        // 备份当前目标值（用于恢复）
        blastOriginalTarget = targetRadialBlurAmount;

        // 瞬间拉高强度（SCPE.RadialBlur 参数名为 amount）
        radialBlur.amount.value = Mathf.Lerp(0.2f, 1.5f, power);
        // 色差也瞬间拉满
        if (chromaticAberration != null)
            chromaticAberration.intensity.value = 0.8f;

        Debug.Log("触发径向冲击波！");
    }

    // ---------- 私有 Update 循环（由 MonoMgr 驱动） ----------

    private void UpdateLoop()
    {
        // 1. 冲击波倒计时（覆盖插值）
        if (isRadialBlastPlaying)
        {
            blastRemainingTime -= Time.deltaTime;
            if (blastRemainingTime <= 0f)
            {
                isRadialBlastPlaying = false;
                // 恢复目标值（但目标值可能已被业务修改，这里用备份）
                targetRadialBlurAmount = blastOriginalTarget;
                // 色差恢复目标值（由 SetSleepinessEffects 决定）
                if (chromaticAberration != null)
                    chromaticAberration.intensity.value = targetChromaticIntensity;
                // 径向模糊强度恢复目标值
                if (radialBlur != null)
                    radialBlur.amount.value = targetRadialBlurAmount;
                return;
            }
            // 冲击波播放期间，直接返回，不进行插值
            return;
        }

        // 2. 平滑插值（速度可调，此处使用 5.0f）
        float lerpSpeed = 5.0f * Time.deltaTime;

        // ---- 暗角 ----
        if (vignette != null)
        {
            currentVignetteIntensity = Mathf.Lerp(currentVignetteIntensity, targetVignetteIntensity, lerpSpeed);
            vignette.intensity.value = currentVignetteIntensity;
            vignette.active = currentVignetteIntensity > 0.01f || targetVignetteIntensity > 0.01f;
        }

        // ---- 色差 ----
        if (chromaticAberration != null)
        {
            currentChromaticIntensity = Mathf.Lerp(currentChromaticIntensity, targetChromaticIntensity, lerpSpeed);
            chromaticAberration.intensity.value = currentChromaticIntensity;
            chromaticAberration.active = currentChromaticIntensity > 0.01f;
        }

        // ---- 径向模糊 ----
        if (radialBlur != null)
        {
            currentRadialBlurAmount = Mathf.Lerp(currentRadialBlurAmount, targetRadialBlurAmount, lerpSpeed);
            radialBlur.amount.value = currentRadialBlurAmount;
            radialBlur.active = currentRadialBlurAmount > 0.01f;
        }

        // ---- 高斯模糊（被窝） ----
        if (blur != null)
        {
            currentBlurAmount = Mathf.Lerp(currentBlurAmount, targetBlurAmount, lerpSpeed);
            blur.amount.value = currentBlurAmount;
            blur.active = currentBlurAmount > 0.01f;
        }
    }

    // ---------- 清理 ----------
    public void Cleanup()
    {
        MonoMgr.Instance.RemoveInUpdate(UpdateLoop);
        // 可选：重置特效状态
        if (vignette != null) vignette.intensity.value = 0f;
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0f;
        if (radialBlur != null) radialBlur.amount.value = 0f;
        if (blur != null) blur.amount.value = 0f;
    }
}
