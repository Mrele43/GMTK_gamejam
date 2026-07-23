using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DayConfig", menuName = "Game/DayConfig")]
public class DayConfig : ScriptableObject
{
    public int dayNumber;
    public string dayTitle;
    public List<TaskData> tasks;
    public List<MonsterReplacementData> monsterReplacements;
    public AudioClip dayIntroSound;
}
