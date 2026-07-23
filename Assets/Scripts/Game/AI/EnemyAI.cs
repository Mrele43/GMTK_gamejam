using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private const string PlayerTag = "Player";

    private static readonly Color WaypointGizmoColor = Color.yellow;
    private static readonly Color SightGizmoColor = Color.white;
    private static readonly Color HearingGizmoColor = Color.blue;
    private static readonly Color FovGizmoColor = Color.green;
    private static readonly Color FlashlightGizmoColor = Color.cyan;

    [Header("Monster Configuration")]
    [Tooltip("怪物配置")]
    [SerializeField] private MonsterConfig _config;

    [Header("Patrol")]
    [Tooltip("巡逻路径点")]
    [SerializeField] private Transform[] _waypoints = new Transform[0];

    [Tooltip("路径点到达容差")]
    [SerializeField] private float _waypointTolerance = 0.5f;

    [Tooltip("初始巡逻位置")]
    [SerializeField] private Transform _initialPatrolPosition;

    [Header("Model Switching")]
    [Tooltip("休眠时的物品模型")]
    [SerializeField] private GameObject _dormantModel;

    [Tooltip("怪物化后的模型")]
    [SerializeField] private GameObject _monsterModel;

    [Tooltip("检测层")]
    [SerializeField] private LayerMask _detectionLayers = ~0;

    [Header("Region Restriction")]
    [Tooltip("怪物活动区域的地面对象")]
    [SerializeField] private Transform _activityRegion;

    [Tooltip("超出区域后返回的延迟时间")]
    [SerializeField] private float _outOfRegionDelay = 2f;

    private float _outOfRegionTimer;

    [Header("Gizmos")]
    [Tooltip("路径点Gizmo半径")]
    [SerializeField] private float _waypointGizmoRadius = 0.3f;

    [Tooltip("状态标签高度")]
    [SerializeField] private float _stateLabelHeight = 2.2f;

    private StateMachine _machine;
    private StateTimer _stateTimer;
    private NavMeshAgent _agent;
    private Animator _animator;
    private int _waypointIndex;
    private Vector3 _lastKnownPosition;
    private Vector3 _initialPosition;

    private readonly Collider[] _overlapBuffer = new Collider[8];

    public string CurrentStateName =>
        _machine?.CurrentState != null ? _machine.CurrentState.GetType().Name : "(none)";

    public float TimeInState => _stateTimer?.Elapsed ?? 0f;

    public string LastTransitionReason { get; private set; } = "(initial)";

    public bool CanSeePlayer { get; private set; }
    public bool CanHearPlayer { get; private set; }
    public bool CanDetectFlashlight { get; private set; }
    public bool CanSenseSleepiness { get; private set; }
    public bool HasDetectedFixedEvent { get; private set; }

    public Transform PlayerTarget { get; private set; }

    public bool IsDead { get; private set; }
    public bool IsAwake { get; private set; }

    public MonsterConfig Config => _config;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _initialPosition = transform.position;

        if (_waypoints == null || _waypoints.Length == 0)
        {
            Debug.LogWarning(
                $"[EnemyAI] '{name}' has no patrol waypoints assigned — it will ping-pong " +
                "between Idle and Patrol in place.", this);
        }

        _machine = new StateMachine();
        _machine.AddState(new DormantState(this));
        _machine.AddState(new AwakeningState(this));
        _machine.AddState(new IdleState(this));
        _machine.AddState(new PatrolState(this));
        _machine.AddState(new AlertState(this));
        _machine.AddState(new ChaseState(this));
        _machine.AddState(new SearchState(this));
        _machine.AddState(new AttackState(this));
        _machine.AddState(new ReturnState(this));
        _machine.AddState(new DeadState(this));

        _stateTimer = new StateTimer(_machine);
    }

    private void Start()
    {
        SetModelActive(true);
        ChangeState<IdleState>("spawned");
    }

    private void Update()
    {
        if (!IsDead)
        {
            UpdatePerception();
        }

        _stateTimer.Tick(Time.deltaTime);
        _machine.Update();

        if (!_agent.isOnNavMesh && _agent.enabled)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
            {
                transform.position = hit.position + Vector3.up * 0.1f;
                _agent.Warp(transform.position);
            }
        }
    }

    private bool MoveToPosition(Vector3 targetPosition, float speed)
    {
        if (_agent.isOnNavMesh)
        {
            _agent.SetDestination(targetPosition);
            return true;
        }

        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0f;

        if (direction.sqrMagnitude < _waypointTolerance * _waypointTolerance)
        {
            return true;
        }

        direction.Normalize();

        Vector3 newPosition = transform.position + direction * speed * Time.deltaTime;

        if (NavMesh.SamplePosition(newPosition, out var hit, 0.5f, NavMesh.AllAreas))
        {
            transform.position = hit.position + Vector3.up * 0.1f;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }

        return false;
    }

    private void FixedUpdate()
    {
        _machine.FixedUpdate();
    }

    public void Kill()
    {
        if (IsDead)
            return;

        IsDead = true;
        ChangeState<DeadState>("killed");
    }

    public void SetModelActive(bool active)
    {
        IsAwake = active;

        if (_dormantModel != null)
            _dormantModel.SetActive(!active);

        if (_monsterModel != null)
            _monsterModel.SetActive(active);

        if (_agent != null)
            _agent.enabled = active;
    }

    public void SetLastKnownPosition(Vector3 position)
    {
        _lastKnownPosition = position;
    }

    public Vector3 GetLastKnownPosition()
    {
        return _lastKnownPosition;
    }

    public Vector3 GetInitialPosition()
    {
        return _initialPatrolPosition != null ? _initialPatrolPosition.position : _initialPosition;
    }

    public bool IsWithinActivityRegion()
    {
        if (_activityRegion == null)
            return true;

        Vector3 pos = transform.position;
        Vector3 regionPos = _activityRegion.position;
        Vector3 regionScale = _activityRegion.localScale;

        float halfWidth = regionScale.x * 0.5f;
        float halfDepth = regionScale.z * 0.5f;

        bool inX = Mathf.Abs(pos.x - regionPos.x) <= halfWidth;
        bool inZ = Mathf.Abs(pos.z - regionPos.z) <= halfDepth;

        return inX && inZ;
    }

    public bool IsPositionWithinActivityRegion(Vector3 position)
    {
        if (_activityRegion == null)
            return true;

        Vector3 regionPos = _activityRegion.position;
        Vector3 regionScale = _activityRegion.localScale;

        float halfWidth = regionScale.x * 0.5f;
        float halfDepth = regionScale.z * 0.5f;

        bool inX = Mathf.Abs(position.x - regionPos.x) <= halfWidth;
        bool inZ = Mathf.Abs(position.z - regionPos.z) <= halfDepth;

        return inX && inZ;
    }

    private void UpdatePerception()
    {
        CanSeePlayer = false;
        CanHearPlayer = false;
        CanDetectFlashlight = false;
        CanSenseSleepiness = false;
        HasDetectedFixedEvent = false;

        if (_config == null)
            return;

        if (!IsAwake && !_config.useSleepinessDetection)
            return;

        if (_config.useSleepinessDetection && SleepinessManager.Instance != null)
        {
            CanSenseSleepiness = SleepinessManager.Instance.CurrentSleepiness >= _config.awakeningThreshold;
        }

        if (_config.useFixedEventDetection)
        {
            CheckFixedEvents();
        }

        if (!IsAwake)
            return;

        if (_config.useVision)
        {
            UpdateVisionPerception();
        }

        if (_config.useHearing)
        {
            UpdateHearingPerception();
        }

        if (_config.useFlashlightDetection)
        {
            UpdateFlashlightPerception();
        }
    }

    private void UpdateVisionPerception()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position, _config.sightRange, _overlapBuffer, _detectionLayers);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _overlapBuffer[i];
            if (!hit.CompareTag(PlayerTag))
                continue;

            Vector3 toPlayer = hit.transform.position - transform.position;
            toPlayer.y = 0f;

            if (Vector3.Angle(transform.forward, toPlayer) <= _config.fovAngle * 0.5f)
            {
                CanSeePlayer = true;
                PlayerTarget = hit.transform;
                _lastKnownPosition = hit.transform.position;
            }

            break;
        }
    }

    private void UpdateHearingPerception()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position, _config.hearingRange, _overlapBuffer, _detectionLayers);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _overlapBuffer[i];
            if (!hit.CompareTag(PlayerTag))
                continue;

            bool audible = true;
            if (_config.hearOnlyMovingPlayer && hit.TryGetComponent(out PlayerController playerController))
            {
                audible = playerController.IsMakingNoise;
            }

            if (audible)
            {
                CanHearPlayer = true;
                PlayerTarget = hit.transform;
                _lastKnownPosition = hit.transform.position;
            }

            break;
        }
    }

    private void UpdateFlashlightPerception()
    {
        if (PlayerPerception.Instance == null || !PlayerPerception.Instance.IsFlashlightOn)
            return;

        if (PlayerPerception.Instance.IsPointInFlashlightCone(transform.position))
        {
            float distance = Vector3.Distance(transform.position, PlayerPerception.Instance.transform.position);
            if (distance <= _config.flashlightRange)
            {
                CanDetectFlashlight = true;
            }
        }
    }

    private void CheckFixedEvents()
    {
        if (PlayerTarget != null)
        {
            float distance = Vector3.Distance(transform.position, PlayerTarget.position);
            if (distance <= _config.sightRange * 0.5f)
            {
                HasDetectedFixedEvent = true;
            }
        }
    }

    private void ChangeState<T>(string reason) where T : IState
    {
        LastTransitionReason = reason;
        _machine.SetState<T>();
    }

    private void SetAnimationBool(string parameterName, bool value)
    {
        if (_animator != null)
        {
            _animator.SetBool(parameterName, value);
        }
    }

    private void SetAnimationTrigger(string parameterName)
    {
        if (_animator != null)
        {
            _animator.SetTrigger(parameterName);
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (_animator != null)
        {
            _animator.CrossFade(animationName, 0.2f);
        }
    }

    private void PlayAnimation(AnimationClip clip)
    {
        if (_animator != null && clip != null)
        {
            _animator.CrossFade(clip.name, 0.2f);
        }
    }

    private void SetIsWalking(bool walking)
    {
        if (walking)
        {
            PlayAnimation("Walking");
        }
        else
        {
            PlayAnimation("Idle");
        }
        SetAnimationBool("IsWalking", walking);
    }

    private void TriggerAttack()
    {
        PlayAnimation("Attack");
        SetAnimationTrigger("IsAttacking");
    }

    private void TriggerAwakening()
    {
        PlayAnimation("Stagger");
        SetAnimationTrigger("IsAwakening");
    }

    private void SetIsDead(bool dead)
    {
        if (dead)
        {
            PlayAnimation("Death");
        }
        SetAnimationBool("IsDead", dead);
    }

    public void OnStaggerEnd()
    {
        Debug.Log($"[EnemyAI] {name} Stagger animation ended");
    }

    public void OnAttackEnd()
    {
        Debug.Log($"[EnemyAI] {name} Attack animation ended");
    }

    public void OnDeathEnd()
    {
        Debug.Log($"[EnemyAI] {name} Death animation ended");
    }

    private abstract class EnemyStateBase : IState
    {
        protected readonly EnemyAI Owner;

        protected EnemyStateBase(EnemyAI owner)
        {
            Owner = owner;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }
    }

    private sealed class DormantState : EnemyStateBase
    {
        public DormantState(EnemyAI owner) : base(owner) { }

        public override void Enter()
        {
            Owner.SetModelActive(false);
        }

        public override void Update()
        {
            if (Owner.CanSenseSleepiness)
            {
                Owner.ChangeState<AwakeningState>("sleepiness reached threshold");
            }
        }
    }

    private sealed class AwakeningState : EnemyStateBase
    {
        private float _awakeningDuration;

        public AwakeningState(EnemyAI owner) : base(owner) { }

        public override void Enter()
        {
            Owner.SetModelActive(true);
            Owner.TriggerAwakening();
            _awakeningDuration = Owner._config.awakeningAnimation != null ?
                Owner._config.awakeningAnimation.length : 2f;
        }

        public override void Update()
        {
            if (Owner._stateTimer.HasElapsed(_awakeningDuration))
            {
                Owner.ChangeState<IdleState>("awakening animation finished");
            }
        }
    }

    private sealed class IdleState : EnemyStateBase
    {
        private float _idleDuration;

        public IdleState(EnemyAI owner) : base(owner) { }

        public override void Enter()
        {
            Owner._agent.isStopped = true;
            Owner._agent.ResetPath();
            Owner.SetIsWalking(false);
            _idleDuration = Random.Range(1f, 3f);
        }

        public override void Update()
        {
            if (Owner.CanSeePlayer || Owner.CanHearPlayer)
            {
                Owner.ChangeState<AlertState>("detected player while idle");
                return;
            }

            if (Owner.HasDetectedFixedEvent)
            {
                Owner.ChangeState<AlertState>("detected fixed event");
                return;
            }

            if (Owner._stateTimer.HasElapsed(_idleDuration))
            {
                Owner.ChangeState<PatrolState>("idle finished");
            }
        }

        public override void Exit()
        {
            Owner._agent.isStopped = false;
        }
    }

    private sealed class PatrolState : EnemyStateBase
    {
        public PatrolState(EnemyAI owner) : base(owner) { }

        public override void Enter()
        {
            Owner._agent.speed = Owner._config.patrolSpeed;
            Owner.SetIsWalking(true);

            if (Owner._waypoints.Length > 0 && Owner._waypoints[Owner._waypointIndex] != null)
            {
                Owner.MoveToPosition(Owner._waypoints[Owner._waypointIndex].position, Owner._config.patrolSpeed);
            }
        }

        public override void Update()
        {
            if (Owner.CanSeePlayer || Owner.CanHearPlayer)
            {
                Owner.ChangeState<AlertState>("detected player while patrolling");
                return;
            }

            if (Owner.HasDetectedFixedEvent)
            {
                Owner.ChangeState<AlertState>("detected fixed event");
                return;
            }

            if (Owner._waypoints.Length == 0)
            {
                Owner.ChangeState<IdleState>("no waypoints to patrol");
                return;
            }

            if (Owner._agent.isOnNavMesh)
            {
                if (Owner._agent.pathStatus != NavMeshPathStatus.PathComplete && !Owner._agent.pathPending)
                {
                    Owner._agent.SetDestination(Owner._waypoints[Owner._waypointIndex].position);
                }

                if (!Owner._agent.pathPending &&
                    Owner._agent.remainingDistance <= Owner._waypointTolerance)
                {
                    Owner._waypointIndex = (Owner._waypointIndex + 1) % Owner._waypoints.Length;
                    Owner.ChangeState<IdleState>("reached waypoint");
                }
            }
            else
            {
                bool reached = Owner.MoveToPosition(Owner._waypoints[Owner._waypointIndex].position, Owner._config.patrolSpeed);
                if (reached)
                {
                    Owner._waypointIndex = (Owner._waypointIndex + 1) % Owner._waypoints.Length;
                    Owner.ChangeState<IdleState>("reached waypoint (manual)");
                }
            }
        }
    }

    private sealed class AlertState : EnemyStateBase
    {
        private float _alertDuration;

        public AlertState(EnemyAI owner) : base(owner) { }

        public override void Enter()
        {
            Owner._agent.isStopped = true;
            Owner._agent.ResetPath();
            Owner.SetIsWalking(false);
            Owner.TriggerAwakening();
            _alertDuration = 1f;
        }

        public override void Update()
        {
            if (Owner.CanSeePlayer || Owner.CanHearPlayer)
            {
                Owner.ChangeState<ChaseState>("confirmed player location");
                return;
            }

            if (Owner._stateTimer.HasElapsed(_alertDuration))
            {
                Owner.ChangeState<SearchState>("alert timeout, searching");
            }
        }

        public override void Exit()
        {
            Owner._agent.isStopped = false;
        }
    }

    private sealed class ChaseState : EnemyStateBase
    {
        private float _timeSinceContact;
        private float _nextRepathTime;

        public ChaseState(EnemyAI owner) : base(owner) { }

        public override void Enter()
        {
            Owner._agent.speed = Owner._config.chaseSpeed;
            Owner.SetIsWalking(true);
            _timeSinceContact = 0f;
            _nextRepathTime = 0f;
        }

        public override void Update()
        {
            if (!Owner.IsWithinActivityRegion())
            {
                Owner.ChangeState<ReturnState>("chase exceeded activity region");
                return;
            }

            if (Owner.CanSeePlayer || Owner.CanHearPlayer)
            {
                _timeSinceContact = 0f;
            }
            else
            {
                _timeSinceContact += Time.deltaTime;
            }

            if (_timeSinceContact >= Owner._config.loseTargetDuration)
            {
                Owner.ChangeState<SearchState>(
                    $"lost player for {Owner._config.loseTargetDuration:F0}s");
                return;
            }

            if (Owner.PlayerTarget == null)
            {
                return;
            }

            if (Time.time >= _nextRepathTime)
            {
                _nextRepathTime = Time.time + Owner._config.repathInterval;
                if (Owner._agent.isOnNavMesh)
                {
                    Owner._agent.SetDestination(Owner.PlayerTarget.position);
                }
            }

            if (!Owner._agent.isOnNavMesh)
            {
                Owner.MoveToPosition(Owner.PlayerTarget.position, Owner._config.chaseSpeed);
            }

            float distanceToPlayer = Vector3.Distance(
                Owner.transform.position, Owner.PlayerTarget.position);

            if (distanceToPlayer <= Owner._config.attackRange)
            {
                Owner.ChangeState<AttackState>("player in attack range");
            }
        }
    }

    private sealed class SearchState : EnemyStateBase
    {
        private float _nextRepathTime;

        public SearchState(EnemyAI owner) : base(owner) { }

        public override void Enter()
        {
            Owner._agent.speed = Owner._config.patrolSpeed;
            Owner.SetIsWalking(true);
            _nextRepathTime = 0f;

            Vector3 searchPosition = Owner.GetLastKnownPosition();
            if (searchPosition == Vector3.zero)
            {
                searchPosition = Owner.GetInitialPosition();
            }

            if (!Owner.IsPositionWithinActivityRegion(searchPosition))
            {
                searchPosition = Owner.GetInitialPosition();
            }

            Owner.MoveToPosition(searchPosition, Owner._config.patrolSpeed);
        }

        public override void Update()
        {
            if (!Owner.IsWithinActivityRegion())
            {
                Owner.ChangeState<ReturnState>("search exceeded activity region");
                return;
            }

            if (Owner.CanSeePlayer || Owner.CanHearPlayer)
            {
                Owner.ChangeState<ChaseState>("found player during search");
                return;
            }

            if (Owner._stateTimer.HasElapsed(Owner._config.searchDuration))
            {
                Owner.ChangeState<ReturnState>("search timeout");
                return;
            }

            if (Owner._agent.isOnNavMesh)
            {
                if (!Owner._agent.pathPending && Owner._agent.remainingDistance <= Owner._waypointTolerance)
                {
                    Vector3 randomOffset = Random.insideUnitSphere * 3f;
                    randomOffset.y = 0f;
                    Vector3 newSearchPos = Owner.GetLastKnownPosition() + randomOffset;

                    if (!Owner.IsPositionWithinActivityRegion(newSearchPos))
                    {
                        newSearchPos = Owner.GetInitialPosition();
                    }

                    Owner._agent.SetDestination(newSearchPos);
                }

                if (Time.time >= _nextRepathTime)
                {
                    _nextRepathTime = Time.time + Owner._config.repathInterval;
                    if (Owner._agent.pathStatus != NavMeshPathStatus.PathComplete)
                    {
                        Owner._agent.SetDestination(Owner.GetLastKnownPosition());
                    }
                }
            }
        }
    }

    private sealed class AttackState : EnemyStateBase
    {
        private float _cooldownRemaining;
        private float _warningElapsed;

        public AttackState(EnemyAI owner) : base(owner) { }

        public override void Enter()
        {
            Owner._agent.isStopped = true;
            Owner._agent.ResetPath();
            Owner.SetIsWalking(false);
            _cooldownRemaining = 0f;
            _warningElapsed = 0f;

            if (Owner._config.hasAttackWarning)
            {
                Owner.TriggerAwakening();
            }
        }

        public override void Update()
        {
            if (Owner.PlayerTarget == null)
            {
                Owner.ChangeState<ChaseState>("attack target vanished");
                return;
            }

            FaceTarget();

            float distanceToPlayer = Vector3.Distance(
                Owner.transform.position, Owner.PlayerTarget.position);

            if (distanceToPlayer > Owner._config.attackRange + Owner._config.attackRangeHysteresis)
            {
                Owner.ChangeState<ChaseState>("player left attack range");
                return;
            }

            if (Owner._config.hasAttackWarning && _warningElapsed < Owner._config.attackWarningDuration)
            {
                _warningElapsed += Time.deltaTime;
                return;
            }

            _cooldownRemaining -= Time.deltaTime;
            if (_cooldownRemaining <= 0f)
            {
                _cooldownRemaining = Owner._config.attackCooldown;
                Owner.TriggerAttack();
                Debug.Log($"[EnemyAI] {Owner.name} attacks the player for {Owner._config.attackDamage} damage!");

                //TODO: Implement actual damage application to the player here.
                //if (SleepinessManager.Instance != null)
                //{
                //    SleepinessManager.Instance.OnDamageTaken();
                //}

                if (Owner._config.returnToPatrolAfterAttack)
                {
                    Owner.ChangeState<ReturnState>("attack completed, returning");
                }
            }
        }

        public override void Exit()
        {
            Owner._agent.isStopped = false;
        }

        private void FaceTarget()
        {
            Vector3 toTarget = Owner.PlayerTarget.position - Owner.transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < Mathf.Epsilon)
                return;

            Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            Owner.transform.rotation = Quaternion.RotateTowards(
                Owner.transform.rotation, desired, Owner._config.attackTurnSpeed * Time.deltaTime);
        }
    }

    private sealed class ReturnState : EnemyStateBase
    {
        public ReturnState(EnemyAI owner) : base(owner) { }

        public override void Enter()
        {
            Owner._agent.speed = Owner._config.patrolSpeed;
            Owner.SetIsWalking(true);
            Owner.MoveToPosition(Owner.GetInitialPosition(), Owner._config.patrolSpeed);
        }

        public override void Update()
        {
            if (Owner.CanSeePlayer || Owner.CanHearPlayer)
            {
                Owner.ChangeState<ChaseState>("detected player while returning");
                return;
            }

            if (Owner._agent.isOnNavMesh)
            {
                if (!Owner._agent.pathPending && Owner._agent.remainingDistance <= Owner._waypointTolerance)
                {
                    Owner.ChangeState<PatrolState>("returned to patrol area");
                }
            }
            else
            {
                bool reached = Owner.MoveToPosition(Owner.GetInitialPosition(), Owner._config.patrolSpeed);
                if (reached)
                {
                    Owner.ChangeState<PatrolState>("returned to patrol area (manual)");
                }
            }
        }
    }

    private sealed class DeadState : EnemyStateBase
    {
        public DeadState(EnemyAI owner) : base(owner) { }

        public override void Enter()
        {
            if (Owner._agent.enabled)
            {
                Owner._agent.isStopped = true;
                Owner._agent.ResetPath();
                Owner._agent.enabled = false;
            }

            Owner.SetIsDead(true);
            Debug.Log($"[EnemyAI] {Owner.name} died.", Owner);
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawWaypointGizmos();

        if (_config != null)
        {
            Gizmos.color = SightGizmoColor;
            Gizmos.DrawWireSphere(transform.position, _config.sightRange);

            Gizmos.color = HearingGizmoColor;
            Gizmos.DrawWireSphere(transform.position, _config.hearingRange);

            if (_config.useFlashlightDetection)
            {
                Gizmos.color = FlashlightGizmoColor;
                Gizmos.DrawWireSphere(transform.position, _config.flashlightRange);
            }

            DrawFovGizmo();
        }

        DrawStateLabel();
    }

    private void DrawWaypointGizmos()
    {
        if (_waypoints == null || _waypoints.Length == 0)
            return;

        Gizmos.color = WaypointGizmoColor;

        for (int i = 0; i < _waypoints.Length; i++)
        {
            if (_waypoints[i] == null)
                continue;

            Gizmos.DrawWireSphere(_waypoints[i].position, _waypointGizmoRadius);

            Transform next = _waypoints[(i + 1) % _waypoints.Length];
            if (next != null)
            {
                Gizmos.DrawLine(_waypoints[i].position, next.position);
            }
        }
    }

    private void DrawFovGizmo()
    {
        const int arcSegments = 24;

        Gizmos.color = FovGizmoColor;

        float halfFov = _config.fovAngle * 0.5f;
        Vector3 origin = transform.position;

        Vector3 previousPoint = origin +
            Quaternion.Euler(0f, -halfFov, 0f) * transform.forward * _config.sightRange;

        Gizmos.DrawLine(origin, previousPoint);

        for (int i = 1; i <= arcSegments; i++)
        {
            float angle = -halfFov + _config.fovAngle * (i / (float)arcSegments);
            Vector3 point = origin +
                Quaternion.Euler(0f, angle, 0f) * transform.forward * _config.sightRange;

            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        Gizmos.DrawLine(origin, previousPoint);
    }

    private void DrawStateLabel()
    {
#if UNITY_EDITOR
        var style = new GUIStyle
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = GetStateLabelColor() }
        };

        UnityEditor.Handles.Label(
            transform.position + Vector3.up * _stateLabelHeight,
            Application.isPlaying ? CurrentStateName : "EnemyAI (not playing)",
            style);
#endif
    }

#if UNITY_EDITOR
    private Color GetStateLabelColor()
    {
        return _machine?.CurrentState switch
        {
            DormantState => new Color(0.5f, 0.5f, 0.5f),
            AwakeningState => new Color(0.7f, 0.3f, 0.3f),
            IdleState => Color.gray,
            PatrolState => Color.yellow,
            AlertState => Color.magenta,
            ChaseState => new Color(1f, 0.5f, 0f),
            SearchState => new Color(0.5f, 0.5f, 1f),
            AttackState => Color.red,
            ReturnState => new Color(0.3f, 0.7f, 0.3f),
            DeadState => Color.black,
            _ => Color.white
        };
    }
#endif
}