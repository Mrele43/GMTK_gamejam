using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepingPill : ConsumableItem
{
    [Header("安眠药效果")]
    [SerializeField] private float sleepinessIncrease = 0.1f; // +10%

    protected override void OnUseEffect()
    {
        SleepinessManager.Instance.ModifySleepiness(sleepinessIncrease);
        Debug.Log($"使用安眠药，困意 +{sleepinessIncrease:P0}");
    }
}
