using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterConfig", menuName = "Game Tools/Monster Config")]
public class MonsterConfig : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("怪物名称")]
    public string monsterName;

    [Tooltip("怪物编号")]
    public string monsterID;

    [Tooltip("所在房间")]
    public string roomName;

    [Tooltip("唤醒阈值(困意达到此值时唤醒)")]
    [Range(0f, 100f)]
    public float awakeningThreshold = 70f;

    [Tooltip("普通物品原型名称(衣架、窗帘、玩偶等)")]
    public string dormantItemName;

    [Tooltip("设计主题")]
    public string designTheme;

    [Tooltip("对应恐惧类型(黑暗、被注视、未知声音等)")]
    public string fearType;

    [Tooltip("玩法定位(追逐型、封路型、观察型等)")]
    public string gameplayRole;

    [Header("Movement")]
    [Tooltip("巡逻速度")]
    public float patrolSpeed = 2f;

    [Tooltip("追逐速度")]
    public float chaseSpeed = 4.5f;

    [Header("Perception")]
    [Tooltip("视觉距离")]
    public float sightRange = 10f;

    [Tooltip("视觉角度(全视角)")]
    [Range(1f, 360f)]
    public float fovAngle = 90f;

    [Tooltip("听觉范围")]
    public float hearingRange = 4f;

    [Tooltip("是否仅听到移动的玩家")]
    public bool hearOnlyMovingPlayer = true;

    [Tooltip("是否使用视觉感知")]
    public bool useVision = true;

    [Tooltip("是否使用听觉感知")]
    public bool useHearing = true;

    [Tooltip("是否使用手电感知")]
    public bool useFlashlightDetection = true;

    [Tooltip("手电感知范围")]
    public float flashlightRange = 8f;

    [Tooltip("是否使用困意感知")]
    public bool useSleepinessDetection = true;

    [Tooltip("是否使用固定事件感知")]
    public bool useFixedEventDetection = true;

    [Header("Attack")]
    [Tooltip("攻击距离")]
    public float attackRange = 1.8f;

    [Tooltip("攻击距离滞后值(防止边界切换)")]
    public float attackRangeHysteresis = 0.4f;

    [Tooltip("攻击伤害")]
    public int attackDamage = 10;

    [Tooltip("攻击冷却时间(秒)")]
    public float attackCooldown = 1.2f;

    [Tooltip("攻击转身速度")]
    public float attackTurnSpeed = 360f;

    [Header("Search & Return")]
    [Tooltip("搜索时间(秒)")]
    public float searchDuration = 5f;

    [Tooltip("丢失目标时间(秒)")]
    public float loseTargetDuration = 3f;

    [Tooltip("重新寻路间隔(秒)")]
    public float repathInterval = 0.2f;

    [Header("Attack & Counter")]
    [Tooltip("攻击前是否有预警")]
    public bool hasAttackWarning = true;

    [Tooltip("预警时间(秒)")]
    public float attackWarningDuration = 0.5f;

    [Tooltip("玩家能否通过奔跑逃脱")]
    public bool canPlayerEscapeByRunning = true;

    [Tooltip("玩家能否利用门阻挡怪物")]
    public bool canDoorBlockMonster = true;

    [Tooltip("玩家能否利用手电反制")]
    public bool canFlashlightCounter = false;

    [Tooltip("怪物是否能进入小孩房")]
    public bool canEnterChildRoom = false;

    [Tooltip("怪物是否能攻击躲在被窝中的玩家")]
    public bool canAttackUnderCovers = false;

    [Tooltip("攻击后是否返回巡逻")]
    public bool returnToPatrolAfterAttack = true;

    [Header("Animation")]
    [Tooltip("待机动画")]
    public AnimationClip idleAnimation;

    [Tooltip("巡逻动画")]
    public AnimationClip patrolAnimation;

    [Tooltip("追逐动画")]
    public AnimationClip chaseAnimation;

    [Tooltip("攻击动画")]
    public AnimationClip attackAnimation;

    [Tooltip("唤醒动画")]
    public AnimationClip awakeningAnimation;

    [Tooltip("死亡动画")]
    public AnimationClip deathAnimation;
}