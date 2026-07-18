using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class AudioMgr : BaseMonoMgr<AudioMgr>
{
    [Header("配置")]
    public AudioConfig config;

    [Header("音量")]
    [Range(0f, 1f)] [SerializeField] private float _masterVolume = 0.8f;
    [Range(0f, 1f)] [SerializeField] private float _bgmVolume = 0.8f;
    [Range(0f, 1f)] [SerializeField] private float _voiceVolume = 0.8f;
    [Range(0f, 1f)] [SerializeField] private float _sfxVolume = 0.8f;

    public float masterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = value;
            UpdateBGMVolume();
            UpdateSFXVolume();
            // Voice and One-shot SFX are ephemeral, but we could update Loop Voice if existed
        }
    }

    public float bgmVolume
    {
        get => _bgmVolume;
        set
        {
            _bgmVolume = value;
            UpdateBGMVolume();
        }
    }

    public float voiceVolume
    {
        get => _voiceVolume;
        set => _voiceVolume = value;
    }

    public float sfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = value;
            UpdateSFXVolume();
        }
    }

    // Track default volumes for currently playing tracks
    private float currentBgmDefaultVol = 1f;
    private float currentLoopSfxDefaultVol = 1f;
    // Track default volumes for one-shot SFX sources
    private Dictionary<int, float> sfxDefaultVolumes = new Dictionary<int, float>();

    [Header("BGM 淡入淡出")]
    public float defaultFadeIn = 0.5f;
    public float defaultFadeOut = 0.5f;

    [Header("SFX Pool")]
    public int sfxPoolSize = 8;

    private AudioSource bgmSource;
    private AudioSource voiceSource;
    private AudioSource loopSfxSource;
    private readonly List<AudioSource> sfxSources = new List<AudioSource>();

    //以id查找的表
    private readonly Dictionary<int, AudioConfigEntry> idLookup = new Dictionary<int, AudioConfigEntry>();

    private readonly Dictionary<string, AudioConfigEntry> nameLookup = new Dictionary<string, AudioConfigEntry>();

    private Coroutine bgmFadeCoroutine;



    protected override void Awake()
    {
        base.Awake();     
        SetupSources();
        BuildLookup();  
    }

    private void Start()
    {
       
    }
    //初始化三种音源，不需要循环播放的音效用对象池创建
    private void SetupSources()
    {
     
        bgmSource = CreateSource("BGM_Source", true);
        voiceSource = CreateSource("Voice_Source", false);
        loopSfxSource = CreateSource("SFX_Loop_Source", true);

        sfxSources.Clear();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            sfxSources.Add(CreateSource($"SFX_Source_{i}", false));
        }
    }

    /// <summary>
    /// 创建AudioSource
    /// </summary>
    /// <param name="name">音效名字</param>
    /// <param name="loop">是否循环</param>
    /// <returns></returns>
    private AudioSource CreateSource(string name, bool loop)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        AudioSource source = go.AddComponent<AudioSource>();
        source.loop = loop;
        source.playOnAwake = false;
        return source;
    }


    private void BuildLookup()
    {
        Debug.Log($"=== BuildLookup 被调用 ===");
        Debug.Log($"config 是否为空: {config == null}");

        if (config == null) return;

        Debug.Log($"config.entries 数量: {config.entries.Count}");

        idLookup.Clear();
        nameLookup.Clear();

        for (int i = 0; i < config.entries.Count; i++)
        {
            AudioConfigEntry entry = config.entries[i];
            if (entry == null)
            {
                Debug.LogWarning($"entry[{i}] 是 null");
                continue;
            }

            // 关键：检查 clip
            Debug.Log($"entry[{i}]: id={entry.audioId}, name={entry.audioName}, clip={entry.clip?.name ?? "NULL"}");

            if (!idLookup.ContainsKey(entry.audioId))
            {
                idLookup.Add(entry.audioId, entry);
            }

            if (!string.IsNullOrWhiteSpace(entry.audioName) && !nameLookup.ContainsKey(entry.audioName))
            {
                nameLookup.Add(entry.audioName, entry);
            }
        }

        Debug.Log($"idLookup 数量: {idLookup.Count}");
    }

    public void PlayBGM(int id, float fadeIn = -1f)
    {
        Debug.Log($"PlayBGM 被调用, id={id}, idLookup.Count={idLookup.Count}");

        if (!TryGetEntry(id, out AudioConfigEntry entry))
        {
            Debug.LogWarning($"找不到 entry id={id}");
            return;
        }

        Debug.Log($"找到 entry: id={entry.audioId}, name={entry.audioName}, clip={entry.clip?.name ?? "NULL"}");
        PlayBGM(entry, fadeIn);
    }




    public void PlayBGM(string name, float fadeIn = -1f)
    {
        Debug.Log($"PlayBGM 被调用, name={name}, idLookup.Count={idLookup.Count}");

        if (!TryGetEntry(name, out AudioConfigEntry entry)) return;
        PlayBGM(entry, fadeIn);
    }

    public void PlayBGM(AudioClip clip, float fadeIn = -1f)
    {
        currentBgmDefaultVol = 1f; // Default for direct clip play
        if (clip == null) return;

        float durationIn = fadeIn >= 0f ? fadeIn : defaultFadeIn;
        float durationOut = defaultFadeOut;

        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);

        if (bgmSource.isPlaying && bgmSource.clip == clip)
        {
            // Already playing this clip, ensure volume is up
            bgmFadeCoroutine = StartCoroutine(FadeSource(bgmSource, masterVolume * bgmVolume, durationIn));
            return;
        }

        if (bgmSource.isPlaying)
        {
            bgmFadeCoroutine = StartCoroutine(SwitchBgmRoutine(clip, durationOut, durationIn));
            return;
        }

        bgmSource.clip = clip;
        bgmSource.volume = 0f;
        bgmSource.Play();
        bgmFadeCoroutine = StartCoroutine(FadeSource(bgmSource, masterVolume * bgmVolume, durationIn));
    }

    private void PlayBGM(AudioConfigEntry entry, float fadeIn)
    {

        if (entry.clip == null)
        {
            Debug.LogWarning("Audio 传入的切片为空");
            return;
        }

        if (entry.type != AudioType.BGM)
        {
            Debug.LogWarning($"Audio entry {entry.audioName} is not BGM.");
            return;
        }

        //Debug.Log($"{nameof(AudioMgr)} PlayBGM: name={entry.audioName}, clip={(entry.clip != null ? entry.clip.name : "null")}, fadeIn={fadeIn}", this);
        //Debug.Log($"{nameof(AudioMgr)} BGM Source id={(bgmSource != null ? bgmSource.GetInstanceID().ToString() : "null")}, currentClip={(bgmSource != null && bgmSource.clip != null ? bgmSource.clip.name : "null")}", this);

        float durationIn = fadeIn >= 0f ? fadeIn : defaultFadeIn;
        float durationOut = defaultFadeOut;

        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);

        currentBgmDefaultVol = entry.defaultVolume;

        if (bgmSource.isPlaying && bgmSource.clip == entry.clip)
        {
            bgmFadeCoroutine = StartCoroutine(FadeSource(bgmSource, GetBGMVolume(entry), durationIn));
            return;
        }

        if (bgmSource.isPlaying)
        {
            bgmFadeCoroutine = StartCoroutine(SwitchBgmRoutine(entry, durationOut, durationIn));
            return;
        }

        bgmSource.clip = entry.clip;
        bgmSource.volume = 0f;
        bgmSource.Play();
        bgmFadeCoroutine = StartCoroutine(FadeSource(bgmSource, GetBGMVolume(entry), durationIn));
    }



    public void StopBGM(float fadeOut = -1f)
    {
        if (!bgmSource.isPlaying) return;

        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        float duration = fadeOut >= 0f ? fadeOut : defaultFadeOut;
        bgmFadeCoroutine = StartCoroutine(FadeOutAndStop(bgmSource, duration));
    }

    public void PlayVoice(int id)
    {
        if (!TryGetEntry(id, out AudioConfigEntry entry)) return;
        PlayVoice(entry);
    }

    public void PlayVoice(string name)
    {
        if (!TryGetEntry(name, out AudioConfigEntry entry)) return;
        PlayVoice(entry);
    }

    private void PlayVoice(AudioConfigEntry entry)
    {
        if (entry.clip == null) return;
        if (entry.type != AudioType.Voice)
        {
            Debug.LogWarning($"Audio entry {entry.audioName} is not Voice.");
            return;
        }

        voiceSource.clip = entry.clip;
        voiceSource.volume = GetVoiceVolume(entry);
        voiceSource.Play();
    }

    public void PlaySFX(int id)
    {
        if (!TryGetEntry(id, out AudioConfigEntry entry)) return;
        PlaySFX(entry);
    }

    public void PlaySFX(string name)
    {
        if (!TryGetEntry(name, out AudioConfigEntry entry)) return;
        PlaySFX(entry);
    }

    /// <summary>
    /// 直接播放 AudioClip（用于 Resources.Load 加载的音效）
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableSfxSource();
        sfxDefaultVolumes[source.GetInstanceID()] = 1f; // Default 1.0 for raw clips
        source.volume = sfxVolume * masterVolume;
        source.clip = clip;
        source.Play();
    }

    public void PlaySfxLoop(int id)
    {
        if (!TryGetEntry(id, out AudioConfigEntry entry)) return;
        PlaySfxLoop(entry);
    }

    public void PlaySfxLoop(string name)
    {
        if (!TryGetEntry(name, out AudioConfigEntry entry)) return;
        PlaySfxLoop(entry);
    }

    /// <summary>
    /// 直接循环播放 AudioClip
    /// </summary>
    public void PlaySfxLoop(AudioClip clip)
    {
        currentLoopSfxDefaultVol = 1f;
        if (clip == null || loopSfxSource == null) return;
        if (loopSfxSource.isPlaying && loopSfxSource.clip == clip) return;

        loopSfxSource.clip = clip;
        loopSfxSource.volume = sfxVolume * masterVolume;
        loopSfxSource.loop = true;
        loopSfxSource.Play();
    }

    private void PlaySFX(AudioConfigEntry entry)
    {
        if (entry.clip == null)
        {
            Debug.LogWarning($"{nameof(AudioMgr)} PlaySFX skipped: clip is null for {entry.audioName}", this);
            return;
        }
        if (entry.type != AudioType.SFX)
        {
            Debug.LogWarning($"Audio entry {entry.audioName} is not SFX.");
            return;
        }

        AudioSource source = GetAvailableSfxSource();
        source.clip = entry.clip;
        
        float defaultVol = entry.defaultVolume;
        sfxDefaultVolumes[source.GetInstanceID()] = defaultVol;

        source.volume = GetSfxVolume(entry);
        source.Play();
        Debug.Log($"{nameof(AudioMgr)} PlaySFX: name={entry.audioName}, clip={entry.clip.name}, volume={source.volume}, sourceId={source.GetInstanceID()}", this);
    }

    private void PlaySfxLoop(AudioConfigEntry entry)
    {
        if (entry.clip == null) return;
        if (entry.type != AudioType.SFX)
        {
            Debug.LogWarning($"Audio entry {entry.audioName} is not SFX.");
            return;
        }

        currentLoopSfxDefaultVol = entry.defaultVolume;

        if (loopSfxSource == null) return;
        if (loopSfxSource.isPlaying && loopSfxSource.clip == entry.clip) return;

        loopSfxSource.clip = entry.clip;
        loopSfxSource.volume = GetSfxVolume(entry);
        loopSfxSource.loop = true;
        loopSfxSource.Play();
        Debug.Log($"{nameof(AudioMgr)} PlaySfxLoop: name={entry.audioName}, clip={entry.clip.name}, volume={loopSfxSource.volume}, sourceId={loopSfxSource.GetInstanceID()}", this);
    }

    public void StopSfxLoop()
    {
        if (loopSfxSource == null || !loopSfxSource.isPlaying) return;
        loopSfxSource.Stop();
        loopSfxSource.clip = null;
    }

    private AudioSource GetAvailableSfxSource()
    {
        for (int i = 0; i < sfxSources.Count; i++)
        {
            if (!sfxSources[i].isPlaying) return sfxSources[i];
        }

        AudioSource extra = CreateSource($"SFX_Source_{sfxSources.Count}", false);
        sfxSources.Add(extra);
        return extra;
    }

    private bool TryGetEntry(int id, out AudioConfigEntry entry)
    {
        if (idLookup.TryGetValue(id, out entry)) return true;
        Debug.LogWarning($"Audio entry id {id} not found.");
        return false;
    }

    private bool TryGetEntry(string name, out AudioConfigEntry entry)
    {
        if (nameLookup.TryGetValue(name, out entry)) return true;
        Debug.LogWarning($"Audio entry name {name} not found.");
        return false;
    }

    private float GetBGMVolume(AudioConfigEntry entry)
    {
        return masterVolume * bgmVolume * entry.defaultVolume;
    }

    private float GetVoiceVolume(AudioConfigEntry entry)
    {
        return masterVolume * voiceVolume * entry.defaultVolume;
    }

    private float GetSfxVolume(AudioConfigEntry entry)
    {
        return masterVolume * sfxVolume * entry.defaultVolume;
    }

    private IEnumerator FadeSource(AudioSource source, float targetVolume, float duration)
    {
        float start = source.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(start, targetVolume, timer / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }

    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        float start = source.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(start, 0f, timer / duration);
            yield return null;
        }
        source.Stop();
        source.volume = 0f;
    }

    private IEnumerator SwitchBgmRoutine(AudioConfigEntry nextEntry, float fadeOutDuration, float fadeInDuration)
    {
        if (bgmSource.isPlaying)
        {
            yield return FadeOutAndStop(bgmSource, fadeOutDuration);
        }

        if (nextEntry.clip == null) yield break;
        bgmSource.clip = nextEntry.clip;
        bgmSource.volume = 0f;
        bgmSource.Play();
        yield return FadeSource(bgmSource, GetBGMVolume(nextEntry), fadeInDuration);
    }

    private IEnumerator SwitchBgmRoutine(AudioClip nextClip, float fadeOutDuration, float fadeInDuration)
    {
        if (bgmSource.isPlaying)
        {
            yield return FadeOutAndStop(bgmSource, fadeOutDuration);
        }

        if (nextClip == null) yield break;
        bgmSource.clip = nextClip;
        bgmSource.volume = 0f;
        bgmSource.Play();
        yield return FadeSource(bgmSource, masterVolume * bgmVolume, fadeInDuration);
    }
    private void UpdateBGMVolume()
    {
        if (bgmSource != null)
        {
            // If fading, stop it to snap to new volume logic, OR let it ride?
            // For Settings slider, immediate snap is better responsiveness.
            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmSource.volume = _masterVolume * _bgmVolume * currentBgmDefaultVol;
        }
    }

    private void UpdateSFXVolume()
    {
        // Update Loop SFX
        if (loopSfxSource != null)
        {
             loopSfxSource.volume = _masterVolume * _sfxVolume * currentLoopSfxDefaultVol;
        }

        // Update One-shot SFX Pool
        for (int i = 0; i < sfxSources.Count; i++)
        {
            AudioSource source = sfxSources[i];
            if (source != null && source.isPlaying)
            {
                float defaultVol = 1f;
                if (sfxDefaultVolumes.TryGetValue(source.GetInstanceID(), out float val))
                {
                    defaultVol = val;
                }
                source.volume = _masterVolume * _sfxVolume * defaultVol;
            }
        }
    }
}
