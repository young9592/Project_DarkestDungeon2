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
    private readonly List<CEffect> _activeStatusEffects = new List<CEffect>();
    private bool _isStartingTurn;
    private CEntityActionExecution _actionExecution;
    private CEnemySkillFeedback _skillFeedback;
    private CEntityDeathPresentation _deathPresentation;
    private CEnemyCorpse _corpse;
    public CEnemyCorpse Corpse { get { return _corpse; } }
    public CEntityDeathPresentation DeathPresentation
    {
        get { return _deathPresentation; }
    }
    public bool IsDeathPresented
    {
        get { return _deathPresentation != null && _deathPresentation.IsConfirmed; }
    }
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
        _deathPresentation = GetComponent<CEntityDeathPresentation>();
        if (_deathPresentation == null)
        {
            _deathPresentation = gameObject.AddComponent<CEntityDeathPresentation>();
        }
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
        GameObject corpseObject = new GameObject("Enemy Corpse");
        corpseObject.transform.SetParent(transform, false);
        _corpse = corpseObject.AddComponent<CEnemyCorpse>();
        _corpse.Configure(this);
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
            || (_skillFeedback != null && _skillFeedback.IsBusy)
            || (_deathPresentation != null && _deathPresentation.IsBusy); }
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

    private void LateUpdate()
    {
        if (ActionTarget is CPlayer hero && hero.IsDeathPresented
            && (!hero.DeathPresentation.WasHitReacting || IsAttackPresentationPlaying))
        {
            CancelAction();
        }
    }

    protected override bool CanBeginAction()
    {
        return !IsDeathPresented && _isInitialized && _currentHp > 0 && _attackAnimation != null && _attackAnimation.isActiveAndEnabled && !IsPresentationBusy;
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
        return !IsDeathPresented && _isInitialized
            && isActiveAndEnabled
            && !_isRequestingSkill
            && CanIssueActionRequest
            && target != null
            && target != this
            && (!(target is CPlayer player) || !player.IsDeathPresented)
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
        if (!_isInitialized || !isActiveAndEnabled || _isDead || IsDeathPresented || finalDamage == 0)
        {
            return;
        }

        // 음수 피해는 회복으로 처리하고 최대 체력 제한
        long nextHp = (long)_currentHp - finalDamage;
        _currentHp = (int)Math.Max(0L, Math.Min(_enemyData.MaxHealth, nextHp));
        // 피격에서 사망으로 이어져도 공격자의 완료 대기 유지
        if (finalDamage > 0 && _attackAnimation != null)
        {
            _attackAnimation.TryPlayHit();
        }

        if (_currentHp == 0)
        {
            _isDead = true;
            _isDeathsDoor = false;
            _deathPresentation.TryPlayConfirmedDeath();
            return;
        }

    }

    public override void ApplyStatusEffect(CEffect statusEffect)
    {
        if (!_isInitialized || !isActiveAndEnabled || _isDead || IsDeathPresented || _currentHp <= 0
            || statusEffect == null || statusEffect.Data == null)
        {
            return;
        }

        if (IsDotEffect(statusEffect.EffectType))
        {
            if (statusEffect.RemainingTurns > 0 && statusEffect.Damage > 0
                && !_activeStatusEffects.Contains(statusEffect))
            {
                _activeStatusEffects.Add(statusEffect);
            }
            return;
        }

        EEffectType tokenType = statusEffect.Data.TokenType != EEffectType.None
            ? statusEffect.Data.TokenType : statusEffect.EffectType;
        switch (tokenType)
        {
            case EEffectType.Crit:
            case EEffectType.Riposte:
            case EEffectType.Dodge:
            case EEffectType.DodgePlus:
            case EEffectType.Block:
            case EEffectType.BlockPlus:
            case EEffectType.Guarded:
            case EEffectType.Strength:
            case EEffectType.Speed:
            case EEffectType.Stealth:
            case EEffectType.Daze:
            case EEffectType.Stun:
            case EEffectType.Blind:
            case EEffectType.Weak:
            case EEffectType.Vulnerable:
            case EEffectType.Taunt:
            case EEffectType.Immobilize:
            case EEffectType.Combo:
                TokenManager.AddToken(tokenType, Mathf.Max(1, statusEffect.Damage));
                break;
        }
    }

    public override void CureStatusEffect(EEffectType targetType = EEffectType.None)
    {
        if (!_isInitialized || _isDead || IsDeathPresented) return;

        for (int i = _activeStatusEffects.Count - 1; i >= 0; i--)
        {
            CEffect effect = _activeStatusEffects[i];
            if (effect == null || effect.Data == null
                || targetType == EEffectType.None || effect.EffectType == targetType)
            {
                _activeStatusEffects.RemoveAt(i);
            }
        }
    }

    public override void ResetEntityState()
    {
        if (!_isInitialized || _enemyData == null) return;

        if (_corpse != null) _corpse.ResetEntityState();

        CancelAction();
        if (_attackAnimation != null) _attackAnimation.CancelPlayback();
        if (_deathPresentation != null) _deathPresentation.ResetPresentation();

        _isDead = false;
        _isDeathsDoor = false;
        _currentHp = _enemyData.MaxHealth;
        TokenManager.ClearAllTokens();
        _activeStatusEffects.Clear();
    }

    // 턴 진행 주체가 턴 시작마다 한 번 호출합니다. 행동 재시도에는 호출하지 않습니다.
    public bool OnTurnStart()
    {
        return OnTurnStart(false);
    }

    public bool OnTurnStart(bool isExtraAction)
    {
        if (!_isInitialized || !isActiveAndEnabled || _isDead || IsDeathPresented || _currentHp <= 0
            || _isStartingTurn || IsActionActive || IsPresentationBusy)
        {
            return false;
        }

        _isStartingTurn = true;
        try
        {
            if (!isExtraAction)
            {
                long totalDamage = 0;
                for (int i = _activeStatusEffects.Count - 1; i >= 0; i--)
                {
                    CEffect effect = _activeStatusEffects[i];
                    if (effect == null || effect.Data == null || effect.RemainingTurns <= 0)
                    {
                        _activeStatusEffects.RemoveAt(i);
                        continue;
                    }

                    totalDamage += effect.Damage;
                    effect.DecreaseTurn();
                    if (effect.RemainingTurns <= 0)
                    {
                        _activeStatusEffects.RemoveAt(i);
                    }
                }

                if (totalDamage > 0)
                {
                    TakeDamage((int)Math.Min(int.MaxValue, totalDamage));
                }
            }

            if (!isActiveAndEnabled || _isDead || IsDeathPresented || _currentHp <= 0) return false;
            return !TokenManager.ConsumeToken(EEffectType.Stun);
        }
        finally
        {
            _isStartingTurn = false;
        }
    }

    public int GetTotalDotDamageByType(EEffectType type)
    {
        if (!IsDotEffect(type)) return 0;
        long total = 0;
        for (int i = 0; i < _activeStatusEffects.Count; i++)
        {
            CEffect effect = _activeStatusEffects[i];
            if (effect != null && effect.Data != null && effect.RemainingTurns > 0 && effect.EffectType == type)
            {
                total += effect.Damage;
            }
        }
        return (int)Math.Min(int.MaxValue, total);
    }

    private bool IsDotEffect(EEffectType type)
    {
        return type == EEffectType.Bleed || type == EEffectType.Blight || type == EEffectType.Burn;
    }
}
