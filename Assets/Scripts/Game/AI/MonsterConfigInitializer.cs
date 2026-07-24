using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class MonsterConfigInitializer
{
#if UNITY_EDITOR
    [MenuItem("Game Tools/Initialize Monster Configs")]
    public static void CreateMonsterConfigs()
    {
        CreateLivingRoomMonsterConfig();
        CreateBathroomMonsterConfig();
        CreateBedroomMonsterConfig();

        Debug.Log("Monster configs created successfully!");
    }

    private static void CreateLivingRoomMonsterConfig()
    {
        MonsterConfig config = ScriptableObject.CreateInstance<MonsterConfig>();

        config.monsterName = "衣架怪";
        config.monsterID = "MON_01";
        config.roomName = "客厅";
        config.awakeningThreshold = 70f;
        config.dormantItemName = "单人立式衣架";
        config.designTheme = "被注视的恐惧";
        config.fearType = "被注视";
        config.gameplayRole = "追逐型";

        config.patrolSpeed = 1.5f;
        config.chaseSpeed = 4f;

        config.sightRange = 10f;
        config.fovAngle = 90f;
        config.hearingRange = 6f;
        config.hearOnlyMovingPlayer = true;
        config.useVision = true;
        config.useHearing = true;
        config.useFlashlightDetection = true;
        config.flashlightRange = 8f;
        config.useSleepinessDetection = true;
        config.useFixedEventDetection = true;
        config.returnToDormantOnFlashlight = true;

        config.attackRange = 2f;
        config.attackRangeHysteresis = 0.5f;
        config.attackDamage = 15;
        config.attackCooldown = 1.5f;
        config.attackTurnSpeed = 360f;

        config.searchDuration = 5f;
        config.loseTargetDuration = 2f;
        config.repathInterval = 0.3f;

        config.hasAttackWarning = true;
        config.attackWarningDuration = 0.6f;
        config.canPlayerEscapeByRunning = true;
        config.canDoorBlockMonster = false;
        config.canFlashlightCounter = false;
        config.canEnterChildRoom = false;
        config.canAttackUnderCovers = false;
        config.returnToPatrolAfterAttack = true;

        string path = "Assets/Configs/LivingRoomMonsterConfig.asset";
        EnsureDirectoryExists(path);
        AssetDatabase.CreateAsset(config, path);
    }

    private static void CreateBathroomMonsterConfig()
    {
        MonsterConfig config = ScriptableObject.CreateInstance<MonsterConfig>();

        config.monsterName = "下水道怪";
        config.monsterID = "MON_02";
        config.roomName = "厕所";
        config.awakeningThreshold = 80f;
        config.dormantItemName = "下水道口/莲蓬";
        config.designTheme = "未知的恐惧";
        config.fearType = "未知声音";
        config.gameplayRole = "封路型";

        config.patrolSpeed = 1.2f;
        config.chaseSpeed = 3.5f;

        config.sightRange = 8f;
        config.fovAngle = 120f;
        config.hearingRange = 8f;
        config.hearOnlyMovingPlayer = false;
        config.useVision = true;
        config.useHearing = true;
        config.useFlashlightDetection = true;
        config.flashlightRange = 6f;
        config.useSleepinessDetection = true;
        config.useFixedEventDetection = true;
        config.returnToDormantOnFlashlight = true;

        config.attackRange = 2.5f;
        config.attackRangeHysteresis = 0.6f;
        config.attackDamage = 20;
        config.attackCooldown = 2f;
        config.attackTurnSpeed = 180f;

        config.searchDuration = 6f;
        config.loseTargetDuration = 2f;
        config.repathInterval = 0.4f;

        config.hasAttackWarning = true;
        config.attackWarningDuration = 0.5f;
        config.canPlayerEscapeByRunning = false;
        config.canDoorBlockMonster = true;
        config.canFlashlightCounter = false;
        config.canEnterChildRoom = false;
        config.canAttackUnderCovers = true;
        config.returnToPatrolAfterAttack = true;

        string path = "Assets/Configs/BathroomMonsterConfig.asset";
        EnsureDirectoryExists(path);
        AssetDatabase.CreateAsset(config, path);
    }

    private static void CreateBedroomMonsterConfig()
    {
        MonsterConfig config = ScriptableObject.CreateInstance<MonsterConfig>();

        config.monsterName = "暗影怪";
        config.monsterID = "MON_03";
        config.roomName = "卧室";
        config.awakeningThreshold = 90f;
        config.dormantItemName = "橱柜/床底的黑暗";
        config.designTheme = "黑暗中的凝视";
        config.fearType = "黑暗";
        config.gameplayRole = "观察型";

        config.patrolSpeed = 0.8f;
        config.chaseSpeed = 3f;

        config.sightRange = 12f;
        config.fovAngle = 180f;
        config.hearingRange = 4f;
        config.hearOnlyMovingPlayer = true;
        config.useVision = true;
        config.useHearing = true;
        config.useFlashlightDetection = true;
        config.flashlightRange = 10f;
        config.useSleepinessDetection = true;
        config.useFixedEventDetection = true;
        config.returnToDormantOnFlashlight = true;

        config.attackRange = 1.8f;
        config.attackRangeHysteresis = 0.4f;
        config.attackDamage = 25;
        config.attackCooldown = 2.5f;
        config.attackTurnSpeed = 360f;

        config.searchDuration = 8f;
        config.loseTargetDuration = 2f;
        config.repathInterval = 0.5f;

        config.hasAttackWarning = true;
        config.attackWarningDuration = 0.8f;
        config.canPlayerEscapeByRunning = false;
        config.canDoorBlockMonster = false;
        config.canFlashlightCounter = true;
        config.canEnterChildRoom = true;
        config.canAttackUnderCovers = false;
        config.returnToPatrolAfterAttack = false;

        string path = "Assets/Configs/BedroomMonsterConfig.asset";
        EnsureDirectoryExists(path);
        AssetDatabase.CreateAsset(config, path);
    }

    private static void EnsureDirectoryExists(string path)
    {
        int lastSlash = path.LastIndexOf('/');
        if (lastSlash > 0)
        {
            string directory = path.Substring(0, lastSlash);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                string parentDir = directory.Substring(0, directory.LastIndexOf('/'));
                string folderName = directory.Substring(directory.LastIndexOf('/') + 1);
                AssetDatabase.CreateFolder(parentDir, folderName);
            }
        }
    }
#endif
}