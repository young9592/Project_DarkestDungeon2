using System.Collections.Generic;
using UnityEngine;

public class CEnemy : CEntity
{
    [Header("Data")]
    [SerializeField] private CEnemyDataSO _enemyData;

    [Header("Animation")]
    [SerializeField] private CEnemyAttackAnimation _attackAnimation;

    [Header("Runtime Status")]
    [SerializeField] private int _currentHp;
    [SerializeField] private ERank _currentRank = ERank.Rank1;

    private bool _isInitialized;

    public CEnemyDataSO EnemyData
    {
        get { return _enemyData; }
    }

    public CEnemyAttackAnimation AttackAnimation
    {
        get { return _attackAnimation; }
    }

    public int CurrentHp
    {
        get { return _currentHp; }
    }

    public ERank CurrentRank
    {
        get { return _currentRank; }
    }

    private void Awake()
    {
        if (_attackAnimation == null)
        {
            _attackAnimation = GetComponent<CEnemyAttackAnimation>();
        }

        if (CPrint.NullUnityObjectCheck(name, _enemyData)
            || CPrint.NullUnityObjectCheck(name, _attackAnimation))
        {
            enabled = false;
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        _currentHp = _enemyData.MaxHealth;
        if (!IsSingleRank(_currentRank)) _currentRank = ERank.Rank1;
        _isInitialized = true;
    }

    public bool TrySetRank(ERank rank)
    {
        if (!IsSingleRank(rank)) return false;

        _currentRank = rank;
        return true;
    }

    public List<CSkillDataSO> GetSkillCandidates()
    {
        return CEnemySkillFilter.GetCandidates(_enemyData, _currentRank);
    }

    public bool TryPlayAttack(CSkillDataSO skill)
    {
        if (!_isInitialized || !isActiveAndEnabled || _attackAnimation == null) return false;
        if (skill == null) return false;

        List<CSkillDataSO> candidates = GetSkillCandidates();
        if (!candidates.Contains(skill)) return false;

        // 현재 열에 맞는 스킬의 연출만 요청하고 효과는 적용하지 않습니다.
        return _attackAnimation.TryPlayAttack();
    }

    public override void TakeDamage(int damage, List<CEffect> effectList)
    {
        if (!_isInitialized || !isActiveAndEnabled || damage <= 0) return;

        _currentHp = Mathf.Max(0, _currentHp - damage);

        // 체력은 바로 반영하고 다른 연출이 재생 중이면 피격 연출은 시작하지 않습니다.
        if (_attackAnimation != null) _attackAnimation.TryPlayHit();

        // TODO: 상태이상과 사망 및 죽음의 문 처리는 전투 규칙을 연결할 때 추가합니다.
    }

    private bool IsSingleRank(ERank rank)
    {
        return rank == ERank.Rank1 || rank == ERank.Rank2
            || rank == ERank.Rank3 || rank == ERank.Rank4;
    }
}
