using System;
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
    private bool _isRequestingSkill;

    public event Action<CEnemy, CEntity, CSkillDataSO> OnSkillActionRequested;

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
        if (!CSkillRankUtility.IsSingleRank(_currentRank)) _currentRank = ERank.Rank1;
        _isInitialized = true;
    }

    public bool TrySetRank(ERank rank)
    {
        if (!CSkillRankUtility.IsSingleRank(rank)) return false;

        _currentRank = rank;
        return true;
    }

    public List<CSkillDataSO> GetSkillCandidates()
    {
        return CEnemySkillFilter.GetCandidates(_enemyData, _currentRank);
    }

    public List<CSkillDataSO> GetSkillCandidates(ERank targetRank)
    {
        return CEnemySkillFilter.GetCandidates(_enemyData, _currentRank, targetRank);
    }

    public CSkillDataSO SelectSkill()
    {
        if (!_isInitialized || !isActiveAndEnabled) return null;

        // 현재 열과 자신의 체력을 기준으로 선택합니다. 대상 열까지 검사할 때는 행동 요청을 사용합니다.
        return CEnemySkillSelector.SelectSkill(_enemyData, GetSkillCandidates(), _currentHp);
    }

    // 대상 열은 호출한 쪽에서 현재 값을 전달합니다. true는 요청 전달 성공을 뜻합니다.
    public bool TryRequestSkillAction(CEntity target, ERank targetRank)
    {
        if (!_isInitialized || !isActiveAndEnabled || _isRequestingSkill) return false;
        if (target == null || target == this || !target.isActiveAndEnabled) return false;
        if (_attackAnimation == null || _attackAnimation.IsBusy) return false;
        if (OnSkillActionRequested == null) return false;

        CSkillDataSO skill = CEnemySkillSelector.SelectSkill(_enemyData, GetSkillCandidates(targetRank), _currentHp);
        if (skill == null) return false;

        // 요청을 받는 코드에서 같은 요청이 다시 들어오는 것을 막습니다.
        _isRequestingSkill = true;
        try
        {
            OnSkillActionRequested.Invoke(this, target, skill);
        }
        finally
        {
            _isRequestingSkill = false;
        }

        return true;
    }

    public bool TryPlayAttack(CSkillDataSO skill)
    {
        if (!_isInitialized || !isActiveAndEnabled || _attackAnimation == null) return false;
        if (skill == null) return false;

        List<CSkillDataSO> candidates = GetSkillCandidates();
        if (!candidates.Contains(skill)) return false;

        // 현재 열에 맞는 스킬의 연출만 요청하고 효과는 적용하지 않습니다.
        return _attackAnimation.TryPlayAttack(skill);
    }

    public override void TakeDamage(int finalDamage)
    {
        if (!_isInitialized || !isActiveAndEnabled || finalDamage == 0) return;

        // 음수는 회복으로 받고 체력은 최대 체력을 넘지 않게 합니다.
        long nextHp = (long)_currentHp - finalDamage;
        _currentHp = (int)Math.Max(0L, Math.Min(_enemyData.MaxHealth, nextHp));

        // 체력은 바로 반영하고 다른 연출이 재생 중이면 피격 연출은 시작하지 않습니다.
        if (finalDamage > 0 && _attackAnimation != null) _attackAnimation.TryPlayHit();

        // TODO: 사망과 죽음의 문 처리는 전투 규칙을 연결할 때 추가합니다.
    }

    public override void ApplyStatusEffect(CEffect statusEffect)
    {
        if (!_isInitialized || !isActiveAndEnabled || statusEffect == null || statusEffect.Data == null) return;

        // TODO: 상태이상 보관과 갱신 규칙을 정한 뒤 연결합니다. 아직 실제 효과는 적용하지 않습니다.
        CPrint.Warn(name + ": Enemy 상태이상 처리는 아직 연결되지 않았습니다.");
    }
}
