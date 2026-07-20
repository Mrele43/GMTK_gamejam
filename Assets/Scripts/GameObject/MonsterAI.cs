using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum MonsterState
{
    Idle,       // 静止，等待被注视
    Watching,   // 正在被玩家注视，计时中
    Chase,      // 追击中
    Attack,     // 攻击中（短暂停顿）
    Deactivated // 已失活（回收或销毁）
}

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterAI : MonoBehaviour, I_DataDrawer
{
    [Header("配置参数")]
    public float watchRequiredTime = 2f;      // 需要持续注视多少秒才触发追击
    public float attackRange = 2f;            // 攻击距离
    public float attackCooldown = 1.5f;       // 攻击冷却（防止连续攻击）
    public float chaseSpeed = 3.5f;
    public float viewAngleThreshold = 45f;    // 玩家视线与怪物方向的夹角阈值（度）

    [Header("组件引用")]
    private NavMeshAgent agent;
    private Transform player;
    private Camera playerCamera;
    private GameManager gameManager;

    // 状态
    private MonsterState currentState = MonsterState.Deactivated;
    private float watchTimer = 0f;
    private float attackTimer = 0f;
    private bool isActive = false;

    // 属性
    public bool IsActive => isActive;
    public MonsterState State => currentState;

    // ----- 初始化（由外部调用） -----
    public void Init(Transform playerTransform, Camera cam, GameManager gm)
    {
        player = playerTransform;
        playerCamera = cam;
        gameManager = gm;

        agent = GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;
        agent.updateRotation = true;

        // 设置为静止
        currentState = MonsterState.Idle;
        isActive = true;
        watchTimer = 0f;
        gameObject.SetActive(true);

        // 随机生成在玩家周围一定距离（避免生成在脸上）
        Vector3 randomDir = Random.insideUnitSphere.normalized;
        randomDir.y = 0; // 保持水平
        Vector3 spawnPos = player.position + randomDir * Random.Range(5f, 10f);
        // 确保不在地形下，简单调整
        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            agent.Warp(spawnPos); // 降级
        }

        Debug.Log($"怪物生成于 {agent.transform.position}，状态：Idle");
    }

    // ----- 每帧更新 -----
    void Update()
    {
        if (!isActive) return;

        switch (currentState)
        {
            case MonsterState.Idle:
                UpdateIdle();
                break;
            case MonsterState.Watching:
                UpdateWatching();
                break;
            case MonsterState.Chase:
                UpdateChase();
                break;
            case MonsterState.Attack:
                UpdateAttack();
                break;
        }
    }

    // ----- 状态逻辑 -----

    private void UpdateIdle()
    {
        // 检测玩家是否看向自己
        if (IsPlayerLookingAtMonster())
        {
            // 开始注视计时
            currentState = MonsterState.Watching;
            watchTimer = 0f;
            Debug.Log("怪物开始被玩家注视...");
        }
    }

    private void UpdateWatching()
    {
        // 如果玩家不再看向怪物，重置计时并回到 Idle
        if (!IsPlayerLookingAtMonster())
        {
            currentState = MonsterState.Idle;
            watchTimer = 0f;
            Debug.Log("玩家转移视线，重置注视计时");
            return;
        }

        // 累加注视时间
        watchTimer += Time.deltaTime;
        if (watchTimer >= watchRequiredTime)
        {
            // 触发追击！
            StartChase();
        }
    }

    private void UpdateChase()
    {
        // 如果玩家进入被窝，停止追击
        if (gameManager != null && gameManager.IsPlayerInBed())
        {
            StopChase();
            return;
        }

        // 向玩家移动
        if (agent.enabled)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            // 无 NavMesh 的简易移动（备选）
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * chaseSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // 检测是否进入攻击距离
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange)
        {
            currentState = MonsterState.Attack;
            attackTimer = 0f;
            agent.isStopped = true;
            Debug.Log("怪物进入攻击范围！");
        }
    }

    private void UpdateAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCooldown)
        {
            // 执行攻击
            PerformAttack();
            // 攻击后回到追击（如果玩家还在范围内，会再次进入攻击；或者稍作后退）
            currentState = MonsterState.Chase;
            agent.isStopped = false;
        }
    }

    // ----- 核心方法 -----

    /// <summary>
    /// 检测玩家是否看向怪物
    /// </summary>
    private bool IsPlayerLookingAtMonster()
    {
        if (playerCamera == null || player == null) return false;

        Vector3 directionToMonster = (transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToMonster);

        // 夹角判断
        if (angle > viewAngleThreshold) return false;

        // 射线检测遮挡（可选）
        RaycastHit hit;
        Vector3 origin = playerCamera.transform.position;
        Vector3 dir = directionToMonster;
        float maxDist = Vector3.Distance(origin, transform.position) + 1f;

        if (Physics.Raycast(origin, dir, out hit, maxDist, ~LayerMask.GetMask("Player", "Monster")))
        {
            // 如果射线打到的是怪物本身，说明可见
            if (hit.transform == transform)
                return true;
            // 否则有遮挡
            return false;
        }
        return true; // 无遮挡则可见
    }

    /// <summary>
    /// 开始追击（由注视计时触发）
    /// </summary>
    private void StartChase()
    {
        if (currentState == MonsterState.Chase) return;

        currentState = MonsterState.Chase;
        agent.isStopped = false;
        Debug.Log("怪物开始追击玩家！");

        // 通知 GameManager 切换到 ChaseState
        if (gameManager != null)
        {
            gameManager.TriggerChaseState();
        }
    }

    /// <summary>
    /// 停止追击（玩家进被窝或怪物失活）
    /// </summary>
    private void StopChase()
    {
        currentState = MonsterState.Idle;
        agent.isStopped = true;
        watchTimer = 0f;
        Debug.Log("怪物停止追击，回到 Idle");
        // 可选：播放消失动画或直接回收
    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    private void PerformAttack()
    {
        if (gameManager == null) return;

        // 扣生命、清空困意值
        gameManager.PlayerTakesDamage(); // 这个方法会扣命并判断是否死亡
        SleepinessManager.Instance.ResetSleepiness();

        // 视觉反馈（冲击波）
        //PostProcessManager.Instance.TriggerRadialBlast(0.8f);

        Debug.Log("怪物攻击命中！");

        // 攻击后怪物消失（或继续追击？策划案未明确，建议消失，被攻击后怪物消失，困意清空）
        Deactivate();
    }

    /// <summary>
    /// 失活（回收或销毁）
    /// </summary>
    public void Deactivate()
    {
        isActive = false;
        currentState = MonsterState.Deactivated;
        agent.isStopped = true;
        gameObject.SetActive(false);
        // 放入对象池（如果使用 PoolMgr）
        // PoolMgr.Instance.PushObj(gameObject); 
        // 注意：如果使用对象池，需要确保资源加载方式匹配
        // 这里简单销毁
        Destroy(gameObject);
    }

    // ----- 接口实现（I_DataDrawer 用于对象池重置） -----
    public void Reset()
    {
        // 重置状态
        isActive = false;
        currentState = MonsterState.Deactivated;
        watchTimer = 0f;
        attackTimer = 0f;
        gameObject.SetActive(false);
    }
}
