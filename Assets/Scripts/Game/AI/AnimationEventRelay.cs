using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    [SerializeField] private EnemyAI _enemyAI;

    private void Awake()
    {
        if (_enemyAI == null)
        {
            _enemyAI = GetComponentInParent<EnemyAI>();
        }
    }

    public void StaggerEnd()
    {
        if (_enemyAI != null)
        {
            _enemyAI.OnStaggerEnd();
        }
    }

    public void AttackEnd()
    {
        if (_enemyAI != null)
        {
            _enemyAI.OnAttackEnd();
        }
    }

    public void DeathEnd()
    {
        if (_enemyAI != null)
        {
            _enemyAI.OnDeathEnd();
        }
    }
}