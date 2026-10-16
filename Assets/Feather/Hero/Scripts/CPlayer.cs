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
	// 전투에 들고 온 장착할 스킬 리스트
	private List<CSkillDataSO> _equippedSkills = new List<CSkillDataSO>();
	// 유저가 클릭해서 실행 대기 중인 스킬
	private CSkillDataSO _selectedSkill;
	#endregion

	#region Events
	// 스킬 타케팅까지 완료되었을시 BattleCore에 '행동 실행' 요청 이벤트
	public event Action<CPlayer, CEntity, CSkillDataSO> OnSkillActionRequested;
    #endregion

    private bool _isRequesting;
    private bool _isGroupRequest;
    private readonly List<CEntity> _actionTargets = new List<CEntity>();
    public IReadOnlyList<CEntity> ActionTargets { get { return _actionTargets; } }
    public CHeroDataSO HeroData { get { return _heroData; } }
    public override ERank CurrentRank { get { return _currentRank; } }
    public CSkillDataSO SelectedSkill { get { return _selectedSkill; } }
    public IReadOnlyList<CSkillDataSO> EquippedSkills { get { return _equippedSkills; } }

    public override CEntityDataSO EntityData { get { return _heroData; } }
    public override EEntityType EntityType { get { return EEntityType.Hero; } }

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
        return ActionTarget != null && AreActionTargetsValid() && CanChooseTarget(ActionSkill, ActionTarget)
            && ActionSkill != null && _equippedSkills.Contains(ActionSkill)
            && CSkillRankUtility.CanUseSkill(ActionSkill, _currentRank)
            && CanTargetRank(ActionSkill, ActionTarget, ActionTarget.CurrentRank);
    }

    public bool TryEquipLoadout(CHeroSkillLoadoutSO loadout)
    {
        if (!isActiveAndEnabled || _isRequesting || (IsActionActive && ActionState != EEntityActionState.Selecting) || loadout == null
            || !loadout.TryValidate(_heroData, out _)) return false;
        _equippedSkills.Clear();
        for (int i = 0; i < loadout.Skills.Count; i++) _equippedSkills.Add(loadout.Skills[i]);
        _skillLoadout = loadout;
        CancelSkillSelection();
        return true;
    }

    // 전투 쪽에서 효과 처리와 별개로 캐릭터 연출 상태를 확인할 수 있습니다.
    public bool IsPresentationBusy { get { return _actionAnimation != null && _actionAnimation.IsBusy; } }
    public bool IsAttackPresentationPlaying { get { return _actionAnimation != null && _actionAnimation.IsAttacking; } }
    public bool IsHitPresentationPlaying { get { return _actionAnimation != null && _actionAnimation.IsHitReacting; } }

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
        if (_actionAnimation == null) return;
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
        return _actionAnimation == null || !_actionAnimation.IsBusy;
    }

    protected override bool TryStartActionPresentation()
    {
        return _actionAnimation == null || _actionAnimation.TryPlayAttack(ActionSkill);
    }

    protected override void OnActionCancelled()
    {
        if (_actionAnimation != null) _actionAnimation.CancelPlayback();
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
        if (!CSkillRankUtility.IsSingleRank(rank)) return false;
        _currentRank = rank;
        CancelSkillSelection();
        return true;
    }

    protected override void Awake()
    {
        base.Awake();

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
        _selectedSkill = null;
        if (!isActiveAndEnabled || _heroData == null || _equippedSkills == null
            || skillIndex < 0 || skillIndex >= _equippedSkills.Count) return false;
        CSkillDataSO skill = _equippedSkills[skillIndex];
        if (!CSkillRankUtility.CanUseSkill(skill, _currentRank)) return false;
        _selectedSkill = skill;
        return true;
    }

    public void ExecuteSkillToTarget(CEntity target, ERank targetRank)
    {
        TryRequestSkillAction(target, targetRank);
    }

    private bool AreActionTargetsValid()
    {
        if (_actionTargets.Count == 0) return false;
        for (int i = 0; i < _actionTargets.Count; i++)
        {
            CEntity target = _actionTargets[i];
            if (target == null || !target.isActiveAndEnabled || !CanChooseTarget(ActionSkill, target)
                || !CanTargetRank(ActionSkill, target, target.CurrentRank)) return false;
        }
        return true;
    }

    public bool TryRequestSkillAction(IReadOnlyList<CEntity> targets)
    {
        CHeroSkillSO skill = _selectedSkill as CHeroSkillSO;
        if (_isRequesting || _isGroupRequest || skill == null || !skill.IsGroupTarget
            || targets == null || targets.Count == 0) return false;
        List<CEntity> uniqueTargets = new List<CEntity>();
        for (int i = 0; i < targets.Count; i++)
        {
            CEntity target = targets[i];
            if (target == null || !CanRequestSkillAction(target, target.CurrentRank)) return false;
            if (!uniqueTargets.Contains(target)) uniqueTargets.Add(target);
        }
        _actionTargets.Clear();
        _actionTargets.AddRange(uniqueTargets);
        _isGroupRequest = true;
        try
        {
            bool requested = TryRequestSkillAction(uniqueTargets[0], uniqueTargets[0].CurrentRank);
            if (!requested) _actionTargets.Clear();
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
        return isActiveAndEnabled && !_isRequesting && CanIssueActionRequest && _selectedSkill != null
            && target != null && target.isActiveAndEnabled
            && CanChooseTarget(_selectedSkill, target)
            && (OnSkillActionRequested != null || HasCommonRequestReceiver)
            && CSkillRankUtility.CanUseSkill(_selectedSkill, _currentRank)
            && CanTargetRank(_selectedSkill, target, targetRank);
    }

    public override bool TryRequestSkillAction(CEntity target, ERank targetRank)
    {
        if (!isActiveAndEnabled || _isRequesting || !CanIssueActionRequest || _selectedSkill == null
            || target == null || !target.isActiveAndEnabled
            || (OnSkillActionRequested == null && !HasCommonRequestReceiver)) return false;
        if (!CSkillRankUtility.CanUseSkill(_selectedSkill, _currentRank))
        {
            CancelSkillSelection();
            return false;
        }
        if (!CanRequestSkillAction(target, targetRank)) return false;

        // 요청 중 같은 스킬이 다시 전달되지 않도록 선택을 먼저 비웁니다.
        CHeroSkillSO heroSkill = _selectedSkill as CHeroSkillSO;
        if (heroSkill != null && heroSkill.IsGroupTarget && !_isGroupRequest) return false;
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
            if (!NotifyActionRequest(target, skill)) OnSkillActionRequested?.Invoke(this, target, skill);
        }
        finally
        {
            _isRequesting = false;
        }
        return true;
    }

	public void CancelSkillSelection()
	{
		_selectedSkill = null;
		CPrint.Log("스킬 선택을 취소하였습니다.");
	}
	#endregion

	#region CEntity Overrides
	// Effect Core 파이프라인에서 최종 연산된 데미지를 받음
	public override void TakeDamage(int finalDamage)
	{
		_currentHp -= finalDamage;
        if (finalDamage > 0 && _actionAnimation != null) _actionAnimation.TryPlayHit();
		CPrint.Log($"[{_heroData.DefaultName}] 피격 \n 데미지 : {finalDamage}, 남은 체력: {_currentHp}");

		if (_currentHp <= 0)
		{
			_currentHp = 0;
			EnterDeathsDoor();
		}
	}

	// Effect Core 파이프라인에서 필터링시킨 상태이상을 적용받음
    public override void ApplyStatusEffect(CEffect statusEffect)
    {
		CPrint.Log($"[{_heroData.DefaultName}] 에게 상태이상 [{statusEffect.Data.Name}] 적용!");

		// TODO : 전달받은 상태이상 타입인 EffectType을 확인하여 부착시키거나 토큰을 누적시키는 로직 구현 필요
    }

	private void EnterDeathsDoor()
	{
		CPrint.Log($"[{_heroData.DefaultName}] 죽음의 문턱 진입 \n 다음 피격 시 생존 난수를 굴립니다.");
		// TODO : 죽음의 문턱 토글 켜기 및 능력치 감소 디버프등 적용
	}
	#endregion

}
