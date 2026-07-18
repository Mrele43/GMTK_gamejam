using System;
using UnityEngine;

[Serializable]
public class AudioConfigEntry
{
    public int audioId;
    public string audioName;
    public AudioClip clip;
    public AudioType type = AudioType.SFX;
    [Range(0f, 1f)]
    public float defaultVolume = 1f;
}
