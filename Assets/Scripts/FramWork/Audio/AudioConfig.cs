using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/AudioConfig", fileName = "AudioConfig")]
public class AudioConfig : ScriptableObject
{
    public List<AudioConfigEntry> entries = new List<AudioConfigEntry>();
}