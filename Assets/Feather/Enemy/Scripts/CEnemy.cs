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
    private CEntityActionExecution _actionExecution;
    private CEnemySkillFeedback _skillFeedback;
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

    public override CEntityDataSO EntityData
    {
        get { return _enemyData; }
    }

    public override EEntityType EntityType
    {
        get { return EEntityType.Enemy; }
    }

    public override ERank CurrentRank
    {
        get { return _currentRank; }
    }

    protected override void Awake()
    {
        base.Awake();
        _skillFeedback = GetComponent<CEnemySkillFeedback>();
        _actionExecution = GetComponent<CEntityActionExecution>();
        if (_actionExecution == null)
        {
            _actionExecution = gameObject.AddComponent<CEntityActionExecution>();
        }

        if (_attackAnimation == null)
        {
            _attackAnimation = GetComponent<CEnemyAttackAnimation>();
        }

        if (CPrint.NullUnityObjectCheck(name, _enemyData) || CPrint.NullUnityObjectCheck(name, _attackAnimation))
        {
            enabled = false;
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        _currentHp = _enemyData.MaxHealth;
        if (!CSkillRankUtility.IsSingleRank(_currentRank))
        {
            _currentRank = ERank.Rank1;
        }

        _isInitialized = true;
    }

    // 효과 처리와 별도로 연출 상태 제공
    public bool IsPresentationBusy
    {
        get { return (_attackAnimation != null && _attackAnimation.IsBusy)
            || (_skillFeedback != null && _skillFeedback.IsBusy); }
    }

    public bool IsAttackPresentationPlaying
    {
        get { return _attackAnimation != null && _attackAnimation.IsAttacking; }
    }

    public bool IsHitPresentationPlaying
    {
        get { return _attackAnimation != null && _attackAnimation.IsHitReacting; }
    }

    public event Action<CEntity> OnAttackPresentationStarted;
    public event Action<CEntity> OnAttackPresentationCompleted;
    public event Action<CEntity> OnAttackPresentationInterrupted;
    public event Action<CEntity> OnHitPresentationStarted;
    public event Action<CEntity> OnHitPresentationCompleted;
    public event Action<CEntity> OnHitPresentationInterrupted;
    public bool CanPlayAttackPresentation(CSkillDataSO skill)
    {
        return isActiveAndEnabled && _attackAnimation != null && _attackAnimation.CanPlayAttack(skill);
    }

    public bool CanPlayHitPresentation()
    {
        return isActiveAndEnabled && _attackAnimation != null && _attackAnimation.CanPlayHit();
    }

    private void HandleAttackStarted()
    {
        OnAttackPresentationStarted?.Invoke(this);
    }

    private void HandleHitStarted()
    {
        OnHitPresentationStarted?.Invoke(this);
    }

    private void HandleHitCompleted()
    {
        OnHitPresentationCompleted?.Invoke(this);
    }

    private void HandleHitInterrupted()
    {
        OnHitPresentationInterrupted?.Invoke(this);
    }

    private void OnEnable()
    {
        if (_attackAnimation == null)
        {
            return;
        }

        _attackAnimation.OnAttackStarted += HandleAttackStarted;
        _attackAnimation.OnHitStarted += HandleHitStarted;
        _attackAnimation.OnHitCompleted += HandleHitCompleted;
        _attackAnimation.OnHitInterrupted += HandleHitInterrupted;
        _attackAnimation.OnAttackCompleted += HandleAttackCompleted;
        _attackAnimation.OnAttackInterrupted += HandleAttackInterrupted;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_attackAnimation == null)
        {
            return;
        }

        _attackAnimation.CancelPlayback();
        _attackAnimation.OnAttackStarted -= HandleAttackStarted;
        _attackAnimation.OnHitStarted -= HandleHitStarted;
        _attackAnimation.OnHitCompleted -= HandleHitCompleted;
        _attackAnimation.OnHitInterrupted -= HandleHitInterrupted;
        _attackAnimation.OnAttackCompleted -= HandleAttackCompleted;
        _attackAnimation.OnAttackInterrupted -= HandleAttackInterrupted;
    }

    private void HandleAttackCompleted()
    {
        OnAttackPresentationCompleted?.Invoke(this);
        TryCompleteAction();
    }

    private void HandleAttackInterrupted()
    {
        OnAttackPresentationInterrupted?.Invoke(this);
        CancelAction();
    }

    protected override bool CanBeginAction()
    {
        return _isInitialized && _currentHp > 0 && _attackAnimation != null && _attackAnimation.isActiveAndEnabled && !IsPresentationBusy;
    }

    protected override bool CanCompleteAction()
    {
        return (_actionExecution == null || !_actionExecution.IsRunning)
            && _attackAnimation != null && !IsPresentationBusy;
    }

    protected override bool CanExecuteRequestedAction()
    {
        return _currentHp > 0 && ActionSkill != null
            && GetSkillCandidates(ActionTarget.CurrentRank).Contains(ActionSkill)
            && CEnemySkillSelector.GetSelectionWeight(_enemyData, ActionSkill, _currentHp) > 0;
    }

    protected override bool TryStartActionPresentation()
    {
        return _actionExecution != null && _actionExecution.TryExecute(ActionSkill,
            new CEntity[] { ActionTarget }, StartAttackPresentation);
    }

    private bool StartAttackPresentation()
    {
        return TryPlayAttack(ActionSkill);
    }

    protected override void OnActionCancelled()
    {
        if (_attackAnimation != null)
        {
            _attackAnimation.CancelPlayback();
        }
    }

    public bool TrySetRank(ERank rank)
    {
        if (!CSkillRankUtility.IsSingleRank(rank))
        {
            return false;
        }

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
        if (!_isInitialized || !isActiveAndEnabled)
        {
            return null;
        }

        // 현재 열과 체력으로 스킬 선택
        return CEnemySkillSelector.SelectSkill(_enemyData, GetSkillCandidates(), _currentHp);
    }

    // 전달받은 대상 열로 행동 요청
    public override bool TryRequestSkillAction(CEntity target, ERank targetRank)
    {
        if (!CanRequestSkillAction(target, targetRank))
        {
            return false;
        }

        CSkillDataSO skill = CEnemySkillSelector.SelectSkill(_enemyData, GetSkillCandidates(targetRank), _currentHp);
        return TryRequestSkillAction(target, targetRank, skill);
    }

    public override bool CanRequestSkillAction(CEntity target, ERank targetRank)
    {
        if (!CanSendRequest(target))
        {
            return false;
        }

        List<CSkillDataSO> candidates = GetSkillCandidates(targetRank);
        for (int i = 0; i < candidates.Count; i++)
        {
            if (CEnemySkillSelector.GetSelectionWeight(_enemyData, candidates[i], _currentHp) > 0)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanRequestSkillAction(CEntity target, ERank targetRank, CSkillDataSO skill)
    {
        return CanSendRequest(target)
            && skill != null
            && GetSkillCandidates(targetRank).Contains(skill)
            && CEnemySkillSelector.GetSelectionWeight(_enemyData, skill, _currentHp) > 0;
    }

    private bool CanSendRequest(CEntity target)
    {
        return _isInitialized
            && isActiveAndEnabled
            && !_isRequestingSkill
            && CanIssueActionRequest
            && target != null
            && target != this
            && target.isActiveAndEnabled
            && _attackAnimation != null
            && _attackAnimation.isActiveAndEnabled
            && !IsPresentationBusy
            && (OnSkillActionRequested != null
            || HasCommonRequestReceiver);
    }

    // 의도에 표시한 스킬로 요청
    public bool TryRequestSkillAction(CEntity target, ERank targetRank, CSkillDataSO skill)
    {
        if (!CanRequestSkillAction(target, targetRank, skill))
        {
            return false;
        }

        // 같은 요청이 다시 들어오지 않게 막기
        _isRequestingSkill = true;
        try
        {
            if (!NotifyActionRequest(target, skill))
            {
                OnSkillActionRequested?.Invoke(this, target, skill);
            }
        }
        finally
        {
            _isRequestingSkill = false;
        }

        return true;
    }

    public bool TryPlayAttack(CSkillDataSO skill)
    {
        if (!_isInitialized || !isActiveAndEnabled || _attackAnimation == null)
        {
            return false;
        }

        if (skill == null)
        {
            return false;
        }

        List<CSkillDataSO> candidates = GetSkillCandidates();
        if (!candidates.Contains(skill))
        {
            return false;
        }

        // 현재 열에 맞는 스킬 연출 요청
        return _attackAnimation.TryPlayAttack(skill);
    }

    public override void TakeDamage(int finalDamage)
    {
        if (!_isInitialized || !isActiveAndEnabled || finalDamage == 0)
        {
            return;
        }

        // 음수 피해는 회복으로 처리하고 최대 체력 제한
        long nextHp = (long)_currentHp - finalDamage;
        _currentHp = (int)Math.Max(0L, Math.Min(_enemyData.MaxHealth, nextHp));
        // 체력 반영 후 재생 가능한 경우에만 피격 시작
        if (finalDamage > 0 && _attackAnimation != null)
        {
            _attackAnimation.TryPlayHit();
        }
    // TODO: 전투 규칙에 맞춰 사망과 죽음의 문 연결
    }

    public override void ApplyStatusEffect(CEffect statusEffect)
    {
        if (!_isInitialized || !isActiveAndEnabled || statusEffect == null || statusEffect.Data == null)
        {
            return;
        }

        // TODO: 보관과 갱신 규칙에 맞춰 상태이상 연결
        CPrint.Warn(name + ": Enemy 상태이상 처리는 아직 연결되지 않았습니다.");
    }
}
