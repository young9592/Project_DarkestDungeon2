using System.Collections.Generic;
using UnityEngine;
using System;

public class CPlayer : CEntity
{
    #region Inspector
    [Header("Data")]
    [SerializeField] private CHeroDataSO _heroData;
    [SerializeField] private CHeroSkillLoadoutSO _skillLoadout;
    [SerializeField] private CHeroAnimation _actionAnimation;

    [Header("Runtime Status")]
    [SerializeField] private int _currentHp;
    [SerializeField] private int _currentStress;
    // 현재 플레이어가 위치한 열 테스트로 1로 설정함
    [SerializeField] private ERank _currentRank = ERank.Rank1;
    #endregion

    #region Field
    private const int MAX_STRESS = 10;
    private const int MELTDOWN_RESET_STRESS = 3;

    // 전투에 들고 온 장착할 스킬 리스트
    private List<CSkillDataSO> _equippedSkills = new List<CSkillDataSO>();
    // 유저가 클릭해서 실행 대기 중인 스킬
    private CSkillDataSO _selectedSkill;
    // 몸에 걸리는 상태이상 인스턴스 목록
    private List<CEffect> _activeStatusEffects = new List<CEffect>();
    #endregion

    #region Events
    // 스킬 타케팅까지 완료되었을시 BattleCore에 '행동 실행' 요청 이벤트
    public event Action<CPlayer, CEntity, CSkillDataSO> OnSkillActionRequested;

    // 배틀 상태 변화 이벤트
    public event Action<CPlayer, int, int> OnHealthChanged;
    public event Action<CPlayer, int, int> OnStressChanged;
    public event Action<CPlayer, bool> OnDeathDoorChanged;
    public event Action<CPlayer> OnMeltdownTriggered;
    public event Action<CPlayer> OnPlayerDied;
    #endregion

    private bool _isRequesting;
    private CEntityActionExecution _actionExecution;
    private CHeroSkillFeedback _skillFeedback;
    private bool _isGroupRequest;
    private readonly List<CEntity> _actionTargets = new List<CEntity>();
    public IReadOnlyList<CEntity> ActionTargets
    {
        get { return _actionTargets; }
    }
    public int CurrentHp
    {
        get { return _currentHp; }
    }

    public CHeroDataSO HeroData { get { return _heroData; } }
    public override ERank CurrentRank
    {
        get { return _currentRank; }
    }
    public CSkillDataSO SelectedSkill { get { return _selectedSkill; } }
    public IReadOnlyList<CSkillDataSO> EquippedSkills { get { return _equippedSkills; } }

    public override CEntityDataSO EntityData
    {
        get { return _heroData; }
    }
    public override EEntityType EntityType
    {
        get { return EEntityType.Hero; }
    }

    protected override void OnActionBeginning()
    {
        _actionTargets.Clear();
        CancelSkillSelection();
    }

    protected override void OnActionEnded()
    {
        _actionTargets.Clear();
        CancelSkillSelection();
    }

    protected override bool CanExecuteRequestedAction()
    {
        return ActionTarget != null
            && AreActionTargetsValid()
            && CanChooseTarget(ActionSkill, ActionTarget)
            && ActionSkill != null
            && _equippedSkills.Contains(ActionSkill)
            && CSkillRankUtility.CanUseSkill(ActionSkill, _currentRank)
            && CanTargetRank(ActionSkill, ActionTarget, ActionTarget.CurrentRank);
    }

    public bool TryEquipLoadout(CHeroSkillLoadoutSO loadout)
    {
        if (!isActiveAndEnabled
            || _isRequesting
            || (IsActionActive
            && ActionState != EEntityActionState.Selecting)
            || loadout == null
            || !loadout.TryValidate(_heroData, out _))
        {
            return false;
        }

        _equippedSkills.Clear();
        for (int i = 0; i < loadout.Skills.Count; i++)
        {
            _equippedSkills.Add(loadout.Skills[i]);
        }

        _skillLoadout = loadout;
        CancelSkillSelection();
        return true;
    }

    // 효과 처리와 별도로 연출 상태 제공
    public bool IsPresentationBusy
    {
        get { return (_actionAnimation != null && _actionAnimation.IsBusy)
            || (_skillFeedback != null && _skillFeedback.IsBusy); }
    }
    public bool IsAttackPresentationPlaying
    {
        get { return _actionAnimation != null && _actionAnimation.IsAttacking; }
    }
    public bool IsHitPresentationPlaying
    {
        get { return _actionAnimation != null && _actionAnimation.IsHitReacting; }
    }

    public event Action<CEntity> OnAttackPresentationStarted;
    public event Action<CEntity> OnAttackPresentationCompleted;
    public event Action<CEntity> OnAttackPresentationInterrupted;
    public event Action<CEntity> OnHitPresentationStarted;
    public event Action<CEntity> OnHitPresentationCompleted;
    public event Action<CEntity> OnHitPresentationInterrupted;

    public bool CanPlayAttackPresentation(CSkillDataSO skill)
    {
        return isActiveAndEnabled && _actionAnimation != null && _actionAnimation.CanPlayAttack(skill);
    }

    public bool CanPlayHitPresentation()
    {
        return isActiveAndEnabled && _actionAnimation != null && _actionAnimation.CanPlayHit();
    }

    private void HandleAttackStarted()
    {
        OnAttackPresentationStarted?.Invoke(this);
    }
    private void HandleHitStarted()
    {
        CancelSkillSelection();
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
        if (_actionAnimation == null)
        {
            return;
        }

        _actionAnimation.OnAttackStarted += HandleAttackStarted;
        _actionAnimation.OnHitStarted += HandleHitStarted;
        _actionAnimation.OnHitCompleted += HandleHitCompleted;
        _actionAnimation.OnHitInterrupted += HandleHitInterrupted;
        _actionAnimation.OnAttackCompleted += HandleAttackCompleted;
        _actionAnimation.OnAttackInterrupted += HandleAttackInterrupted;
    }

    protected override bool CanBeginAction()
    {
        return _actionAnimation == null || !_actionAnimation.IsBusy;
    }

    protected override bool CanCompleteAction()
    {
        return (_actionExecution == null || !_actionExecution.IsRunning)
            && (_actionAnimation == null || !_actionAnimation.IsBusy)
            && (_skillFeedback == null || !_skillFeedback.IsBusy);
    }

    protected override bool TryStartActionPresentation()
    {
        return _actionExecution != null && _actionExecution.TryExecute(ActionSkill,
            _actionTargets, StartAttackPresentation);
    }

    private bool StartAttackPresentation()
    {
        return _actionAnimation == null || _actionAnimation.TryPlayAttack(ActionSkill);
    }

    protected override void OnActionCancelled()
    {
        if (_actionAnimation != null)
        {
            _actionAnimation.CancelPlayback();
        }
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

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_actionAnimation != null)
        {
            _actionAnimation.CancelPlayback();
            _actionAnimation.OnAttackStarted -= HandleAttackStarted;
            _actionAnimation.OnHitStarted -= HandleHitStarted;
            _actionAnimation.OnHitCompleted -= HandleHitCompleted;
            _actionAnimation.OnHitInterrupted -= HandleHitInterrupted;
            _actionAnimation.OnAttackCompleted -= HandleAttackCompleted;
            _actionAnimation.OnAttackInterrupted -= HandleAttackInterrupted;
        }

        CancelSkillSelection();
    }

    public bool TrySetRank(ERank rank)
    {
        if (!CSkillRankUtility.IsSingleRank(rank))
        {
            return false;
        }

        _currentRank = rank;
        CancelSkillSelection();
        return true;
    }

    protected override void Awake()
    {
        base.Awake();
        _actionExecution = GetComponent<CEntityActionExecution>();
        if (_actionExecution == null)
        {
            _actionExecution = gameObject.AddComponent<CEntityActionExecution>();
        }


        _skillFeedback = GetComponent<CHeroSkillFeedback>();
        if (_actionAnimation == null) _actionAnimation = GetComponent<CHeroAnimation>();
        if (CPrint.NullUnityObjectCheck(gameObject.name, _heroData))
        {
            enabled = false;
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        _currentHp = _heroData.MaxHealth;
        _currentStress = 0;

        _equippedSkills.Clear();
        if (_skillLoadout != null)
        {
            if (!TryEquipLoadout(_skillLoadout))
            {
                CPrint.Warn(name + ": 장착 스킬 목록을 확인해야 합니다.");
                enabled = false;
            }
            return;
        }

        // 장착 목록이 없는 기존 캐릭터는 보유 스킬을 사용합니다.
        if (_heroData.AvailableSkills == null) return;
        for (int i = 0; i < _heroData.AvailableSkills.Count; i++)
        {
            CSkillDataSO skill = _heroData.AvailableSkills[i];
            if (skill != null && !_equippedSkills.Contains(skill)) _equippedSkills.Add(skill);
        }
    }

    #region Skill Usage
    /// </summary>
    /// 1. 유저가 스킬 버튼 클릭시 호출
    /// </summary>
    public void SelectSkill(int skillIndex)
    {
        TrySelectSkill(skillIndex);
    }

    public bool TrySelectSkill(int skillIndex)
    {
        if (_isRequesting || !CanIssueActionRequest) return false;
        if (!isActiveAndEnabled || _heroData == null || _equippedSkills == null
            || skillIndex < 0 || skillIndex >= _equippedSkills.Count)
        {
            CancelSkillSelection();
            return false;
        }

        CSkillDataSO skill = _equippedSkills[skillIndex];
        if (!CSkillRankUtility.CanUseSkill(skill, _currentRank))
        {
            CancelSkillSelection();
            return false;
        }

        if (_selectedSkill == skill && (_actionAnimation == null || _actionAnimation.IsSelecting)) return true;
        CancelSkillSelection();
        if (_actionAnimation != null && !_actionAnimation.TryPlaySelection(skill)) return false;
        _selectedSkill = skill;
        return true;
    }

    public void ExecuteSkillToTarget(CEntity target, ERank targetRank)
    {
        TryRequestSkillAction(target, targetRank);
    }

    private bool AreActionTargetsValid()
    {
        if (_actionTargets.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < _actionTargets.Count; i++)
        {
            CEntity target = _actionTargets[i];
            if (target == null
                || !target.isActiveAndEnabled
                || !CanChooseTarget(ActionSkill, target)
                || !CanTargetRank(ActionSkill, target, target.CurrentRank))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryRequestSkillAction(IReadOnlyList<CEntity> targets)
    {
        CHeroSkillSO skill = _selectedSkill as CHeroSkillSO;
        if (_isRequesting || _isGroupRequest || skill == null || !skill.IsGroupTarget || targets == null || targets.Count == 0)
        {
            return false;
        }

        List<CEntity> uniqueTargets = new List<CEntity>();
        for (int i = 0; i < targets.Count; i++)
        {
            CEntity target = targets[i];
            if (target == null || !CanRequestSkillAction(target, target.CurrentRank))
            {
                return false;
            }

            if (!uniqueTargets.Contains(target))
            {
                uniqueTargets.Add(target);
            }
        }

        _actionTargets.Clear();
        _actionTargets.AddRange(uniqueTargets);
        _isGroupRequest = true;
        try
        {
            bool requested = TryRequestSkillAction(uniqueTargets[0], uniqueTargets[0].CurrentRank);
            if (!requested)
            {
                _actionTargets.Clear();
            }

            return requested;
        }
        finally
        {
            _isGroupRequest = false;
        }
    }

    private bool CanTargetRank(CSkillDataSO skill, CEntity target, ERank targetRank)
    {
        CEnemy enemy = target as CEnemy;
        if (enemy == null || enemy.EnemyData == null)
        {
            return CSkillRankUtility.CanTargetSkill(skill, targetRank);
        }

        int start = 0;
        for (int i = 0; i < 4; i++)
        {
            if ((int)targetRank == (1 << i))
            {
                start = i + 1;
            }
        }

        if (start == 0)
        {
            return false;
        }

        for (int i = 0; i < enemy.EnemyData.FormationSize && start + i <= 4; i++)
        {
            if (CSkillRankUtility.CanTargetSkill(skill, (ERank)(1 << (start + i - 1))))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanChooseTarget(CSkillDataSO skill, CEntity target)
    {
        CHeroSkillSO heroSkill = skill as CHeroSkillSO;
        return heroSkill == null ? target != this : heroSkill.CanChooseTarget(this, target);
    }

    public override bool CanRequestSkillAction(CEntity target, ERank targetRank)
    {
        return isActiveAndEnabled
            && !_isRequesting
            && CanIssueActionRequest
            && _selectedSkill != null
            && target != null
            && target.isActiveAndEnabled
            && CanChooseTarget(_selectedSkill, target)
            && (OnSkillActionRequested != null
            || HasCommonRequestReceiver)
            && CSkillRankUtility.CanUseSkill(_selectedSkill, _currentRank)
            && CanTargetRank(_selectedSkill, target, targetRank);
    }

    public override bool TryRequestSkillAction(CEntity target, ERank targetRank)
    {
        if (!isActiveAndEnabled
            || _isRequesting
            || !CanIssueActionRequest
            || _selectedSkill == null
            || target == null
            || !target.isActiveAndEnabled
            || (OnSkillActionRequested == null
            && !HasCommonRequestReceiver))
        {
            return false;
        }

        if (!CSkillRankUtility.CanUseSkill(_selectedSkill, _currentRank))
        {
            CancelSkillSelection();
            return false;
        }

        if (!CanRequestSkillAction(target, targetRank))
        {
            return false;
        }

        // 요청 전에 스킬 선택 해제해서 중복 요청 막기
        CHeroSkillSO heroSkill = _selectedSkill as CHeroSkillSO;
        if (heroSkill != null && heroSkill.IsGroupTarget && !_isGroupRequest)
        {
            return false;
        }

        if (!_isGroupRequest)
        {
            _actionTargets.Clear();
            _actionTargets.Add(target);
        }

        CSkillDataSO skill = _selectedSkill;
        _selectedSkill = null;
        _isRequesting = true;
        try
        {
            if (!NotifyActionRequest(target, skill))
            {
                OnSkillActionRequested?.Invoke(this, target, skill);
            }
        }
        finally
        {
            _isRequesting = false;
        }

        return true;
    }

    public void CancelSkillSelection()
    {
        if (_actionAnimation != null) _actionAnimation.CancelSelection();
        _selectedSkill = null;
        CPrint.Log("스킬 선택을 취소하였습니다.");
    }
    #endregion

    #region CEntity Overrides, DOT, Stress & Death's Door Logic
    // Effect Core 파이프라인에서 최종 연산된 데미지를 받음
    public override void TakeDamage(int finalDamage)
    {
        if (!isActiveAndEnabled || _isDead || _heroData == null || finalDamage == 0)
        {
            return;
        }

        if (finalDamage < 0)
        {
            int previousHp = _currentHp;
            long nextHp = (long)_currentHp - finalDamage;
            _currentHp = (int)Math.Max(0L, Math.Min(_heroData.MaxHealth, nextHp));
            
            if (_currentHp != previousHp)
            {
                OnHealthChanged?.Invoke(this, previousHp, _currentHp);
                CPrint.Log($"[{_heroData.DefaultName}] 체력 회복 : +{-finalDamage} (현재 체력: {_currentHp}/{_heroData.MaxHealth})");
            }

            if (_isDeathsDoor && _currentHp > 0)
            {
                ExitDeathDoor();
            }
            return;
        }
        
        // 데미지 피격시 애니메이션 재생
        if (_actionAnimation != null)
        {
            _actionAnimation.TryPlayHit();
        }

        // 죽음의 문턱 상태에서 데미지 입을 경우 즉사 저항 돌리기
        if (_isDeathsDoor)
        {
            RollDeathblow();
            return;
        }

        // 일반 체력 차감 처리
        int prevHp = _currentHp;
        long calculatedHp = (long)_currentHp - finalDamage;
        _currentHp = (int)Mathf.Max(0L, Math.Min(_heroData.MaxHealth, calculatedHp));

        if (_currentHp != prevHp)
        {
            OnHealthChanged?.Invoke(this, prevHp, _currentHp);
            CPrint.Log($"[{_heroData.DefaultName}] 피격" +
                $"데미지: {finalDamage}, 남은 체력: {_currentHp}/{_heroData.MaxHealth}");
        }

        // 체력이 0일시 죽음의 문턱 진입
        if (_currentHp == 0)
        {
            EnterDeathsDoor();
        }
    }

    // 상태이상 부여 (합치지 않고 리스트에 개별 인스턴스로 추가)
    public override void ApplyStatusEffect(CEffect statusEffect)
    {
        if (_isDead || statusEffect == null || statusEffect.Data == null) return;

        if (IsDotEffect(statusEffect.EffectType))
        {
            _activeStatusEffects.Add(statusEffect);

            int currentTotalDamage = GetTotalDotDamageByType(statusEffect.EffectType);
            CPrint.Log($"[{_heroData.DefaultName}] 에게 [{statusEffect.EffectType}] 추가 부여 \n" +
                $"(이번 효과 : {statusEffect.Damage} 데미지 / {statusEffect.RemainingTurns}턴 |" +
                $"다음 턴 예정 {statusEffect.EffectType} 합산 데미지 : {currentTotalDamage})");
        }
    }

    // 스트레스 수치 증감
    public void ModifyStress(int stressAmount)
    {
        if (!isActiveAndEnabled || _isDead || _heroData == null || stressAmount == 0)
        {
            return;
        }

        int previousStress = _currentStress;
        _currentStress = Mathf.Clamp(_currentStress + stressAmount, 0, MAX_STRESS);

        if (_currentStress != previousStress)
        {
            OnStressChanged?.Invoke(this, previousStress, _currentStress);
            CPrint.Log($"[{_heroData.DefaultName}] 스트레스 변화: {previousStress} → {_currentStress}/{MAX_STRESS}");
        }

        if (_currentStress >= MAX_STRESS)
        {
            TriggerMeltdown();
        }
    }


    public void OnTurnStart()
    {
        if (_isDead) return;

        if (_activeStatusEffects.Count > 0)
        {
            int bleedDamage = 0;
            int blightDamage = 0;
            int burnDamage = 0;

            for (int i = _activeStatusEffects.Count - 1 ; i >= 0 ; i--)
            {
                CEffect effect = _activeStatusEffects[i];

                switch (effect.EffectType)
                {
                    case EEffectType.Bleed: bleedDamage += effect.Damage; break;
                    case EEffectType.Blight: blightDamage += effect.Damage; break;
                    case EEffectType.Burn: burnDamage += effect.Damage; break;
                }

                effect.DecreaseTurn();

                if (effect.RemainingTurns <= 0)
                {
                    CPrint.Log($"[{_heroData.DefaultName} 의 [{effect.Data.Name}({effect.Damage}데미지)] 지속 시간이 끝났습니다.");
                    _activeStatusEffects.RemoveAt(i);
                }
            }

            int totlaTickDamage = bleedDamage + blightDamage + burnDamage;
            if (totlaTickDamage > 0)
            {
                CPrint.Log($"[{_heroData.DefaultName}] 턴 시작 도트 피해 정산 → " +
                    $"출혈({bleedDamage}) + 중독({blightDamage}) + 화상({burnDamage}) = 총 [{totlaTickDamage}] 데미지");

                TakeDamage(totlaTickDamage);

                if (_isDead) return;
            }
        }

        if (TokenManager.ConsumeToken(EEffectType.Stun))
        {
            CPrint.Log($"[{_heroData.DefaultName}] 기절 토큰 소모\n" +
                $"이번 턴 행동을 못합니다.");
            // TODO : Battle Core부분에 턴 종료 알리기
        }
    }

    // 특정 도트 합산 데미지 반환
    public int GetTotalDotDamageByType(EEffectType dotType)
    {
        int total = 0;
        for (int i = 0; i < _activeStatusEffects.Count; i++)
        {
            if (_activeStatusEffects[i].EffectType == dotType)
            {
                total += _activeStatusEffects[i].Damage;
            }
        }
        return total;
    }

    private bool IsDotEffect(EEffectType type)
    {
        return type == EEffectType.Bleed || type == EEffectType.Blight || type == EEffectType.Burn;
    }

	private void EnterDeathsDoor()
	{
        if (_isDeathsDoor || _isDead) return;

        _isDeathsDoor = true;
		CPrint.Log($"[{_heroData.DefaultName}] 죽음의 문턱 진입 \n 다음 피격 시 생존 난수를 굴립니다.");

        TokenManager.AddToken(EEffectType.Weak, 1);

        OnDeathDoorChanged?.Invoke(this, true);
	}

    private void ExitDeathDoor()
    {
        if (!_isDeathsDoor) return;

        _isDeathsDoor = false;
        CPrint.Log($"[{_heroData.DefaultName}] 죽음의 문턱에서 벗어났습니다 (현재 체력 : {_currentHp})");

        OnDeathDoorChanged?.Invoke(this, false);
    }

    private void RollDeathblow()
    {
        // TODO : CResistanceStat에 Deathblow 스탯 을 만들어서 진행시 _heroData.Resistance.Deathblow로 교체해서 진행할것
        int deathblowResist = 75;
        int randomRoll = UnityEngine.Random.Range(1, 101);

        CPrint.Log($"[{_heroData.DefaultName}] 죽음의 일격 저항 굴림");

        if (randomRoll <= deathblowResist)
        {
            CPrint.Log($"[{_heroData.DefaultName}] 죽음의 일격을 버텼습니다.");
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        if (_isDead) return;

        _isDead = true;
        _isDeathsDoor = false;
        _currentHp = 0;

        TokenManager.ClearAllTokens();
        _activeStatusEffects.Clear();
        CancelSkillSelection();

        CPrint.Log($"[{_heroData.DefaultName}] 사망.. 전투 불능 상태가 되었습니다.");

        OnPlayerDied?.Invoke(this);
    }

    private void TriggerMeltdown()
    {
        CPrint.Log($"[{_heroData.DefaultName}] 스트레스 10 도달! [붕괴] 발생!");

        // 스트레스 수치 3으로 리셋 및 이벤트
        int prevStress = _currentStress;
        _currentStress = MELTDOWN_RESET_STRESS;
        OnStressChanged?.Invoke(this, prevStress, _currentStress);

        if (_currentHp > 0)
        {
            int prevHp = _currentHp;
            int meltdownDamage = Mathf.Max(1, Mathf.RoundToInt(_heroData.MaxHealth * 0.7f));
            _currentHp = Mathf.Max(0, _currentHp - meltdownDamage);

            OnHealthChanged?.Invoke(this, prevHp, _currentHp);

            if (_currentHp == 0)
            {
                EnterDeathsDoor();
            }
        }

        TokenManager.AddToken(EEffectType.Weak, 1);
        TokenManager.AddToken(EEffectType.Vulnerable, 1);

        OnMeltdownTriggered?.Invoke(this);
    }
	#endregion

}
