using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coffee : ConsumableItem
{
    [Header("咖啡效果")]
    [SerializeField] private float sleepinessDecrease = -0.1f; // -10%

    protected override void OnUseEffect()
    {
        SleepinessManager.Instance.ModifySleepiness(sleepinessDecrease);
        Debug.Log($"使用咖啡，困意 -{Mathf.Abs(sleepinessDecrease):P0}");
    }
}
