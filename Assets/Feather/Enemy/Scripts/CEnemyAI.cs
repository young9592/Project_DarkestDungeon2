using System.Collections.Generic;
using UnityEngine;

public class CEnemyAI : MonoBehaviour
{
    private CEnemy _enemy;
    private CSkillDataSO _plannedSkill;
    private CEntity _plannedTarget;
    private readonly List<CEntity> _plannedTargets = new List<CEntity>();
    private readonly List<CEntity> _heroes = new List<CEntity>();
    private readonly List<CEnemy> _allies = new List<CEnemy>();
    private CSkillDataSO _retrySkill;
    private bool _hasRequestedAction;

    public CSkillDataSO PlannedSkill { get { return _plannedSkill; } }
    public CEntity PlannedTarget { get { return _plannedTarget; } }
    public IReadOnlyList<CEntity> PlannedTargets { get { return _plannedTargets; } }
    public IReadOnlyList<CEntity> HeroTargets { get { return _heroes; } }
    public bool IsExecuting { get { return _hasRequestedAction && _enemy.ActionState == EEntityActionState.Executing; } }

    private void Awake()
    {
        _enemy = GetComponent<CEnemy>();
        if (_enemy == null)
        {
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        if (_enemy == null) return;
        _enemy.OnActionCompleted += HandleActionEnded;
        _enemy.OnActionInterrupted += HandleActionEnded;
    }

    private void OnDisable()
    {
        if (_enemy != null)
        {
            if (_hasRequestedAction) _enemy.CancelAction();
            _enemy.OnActionCompleted -= HandleActionEnded;
            _enemy.OnActionInterrupted -= HandleActionEnded;
        }
        ClearPlan();
    }

    private void LateUpdate()
    {
        if (_plannedSkill == null) return;
        if (_hasRequestedAction)
        {
            if (!_enemy.IsActionActive) ClearPlan();
            else if (!IsPlanValid())
            {
                bool finishingHit = _enemy.ActionState == EEntityActionState.Executing
                    && !_enemy.IsAttackPresentationPlaying
                    && _plannedTarget is CPlayer target && target.IsDeathPresented
                    && target.DeathPresentation.WasHitReacting;
                if (!finishingHit)
                {
                    _enemy.CancelAction();
                }
            }
            return;
        }
        if (!IsPlanValid() || _enemy.AttackAnimation.IsBusy)
        {
            CSkillDataSO skill = _plannedSkill;
            ClearPlan();
            _retrySkill = skill;
            if (!_enemy.AttackAnimation.IsBusy) RefreshPlan();
        }
    }

    public bool TryPlanAction(IReadOnlyList<CEntity> heroes)
    {
        return TryPlanAction(heroes, null);
    }

    public bool TryPlanAction(IReadOnlyList<CEntity> heroes, IReadOnlyList<CEnemy> allies)
    {
        if (!isActiveAndEnabled || _enemy == null || !_enemy.isActiveAndEnabled || _enemy.IsDeathPresented
            || _enemy.AttackAnimation == null || _enemy.AttackAnimation.IsBusy || _hasRequestedAction
            || (_enemy.IsActionActive && _enemy.ActionState != EEntityActionState.Selecting)) return false;

        if (_enemy.CurrentHp <= 0)
        {
            ClearPlan();
            return false;
        }

        _heroes.Clear();
        if (heroes != null)
        {
            for (int i = 0; i < heroes.Count; i++)
            {
                CEntity hero = heroes[i];
                if (hero != null && hero.EntityType == EEntityType.Hero && !_heroes.Contains(hero)) _heroes.Add(hero);
            }
        }
        _allies.Clear();
        if (allies != null)
        {
            for (int i = 0; i < allies.Count; i++)
            {
                CEnemy ally = allies[i];
                if (ally != null && ally != _enemy && !_allies.Contains(ally)) _allies.Add(ally);
            }
        }
        // 유효한 의도는 다시 추첨하지 않습니다.
        if (IsPlanValid()) return true;
        CSkillDataSO previousSkill = _plannedSkill != null ? _plannedSkill : _retrySkill;
        ClearPlan();

        List<CSkillDataSO> candidates = _enemy.GetSkillCandidates();
        List<CSkillDataSO> usableSkills = new List<CSkillDataSO>();
        for (int i = 0; i < candidates.Count; i++)
        {
            CSkillDataSO skill = candidates[i];
            if (skill == null || usableSkills.Contains(skill)) continue;
            if (CEnemyTargetFilter.GetTargets(skill, _heroes).Count > 0) usableSkills.Add(skill);
        }
        // 대상 수 때문에 스킬의 비중이 커지지 않도록 스킬을 먼저 고릅니다.
        if (previousSkill != null && usableSkills.Contains(previousSkill)
            && CEnemySkillSelector.GetSelectionWeight(_enemy.EnemyData, previousSkill, _enemy.CurrentHp) > 0)
        {
            _plannedSkill = previousSkill;
        }
        else
        {
            bool hasAlly = false;
            bool allyInFront = false;
            for (int i = 0; i < _allies.Count; i++)
            {
                CEnemy ally = _allies[i];
                if (!ally.isActiveAndEnabled || ally.IsDead || ally.IsDeathPresented || ally.CurrentHp <= 0) continue;
                hasAlly = true;
                if ((int)ally.CurrentRank < (int)_enemy.CurrentRank) allyInFront = true;
            }
            _plannedSkill = CEnemySkillSelector.SelectPatternSkill(_enemy.EnemyData, usableSkills,
                _enemy.CurrentHp, _enemy.CurrentRank, hasAlly, allyInFront);
        }
        if (_plannedSkill == null) return false;
        List<CEntity> targets = CEnemyTargetFilter.GetTargets(_plannedSkill, _heroes);
        if (targets.Count == 0) return false;
        _plannedTarget = targets[Random.Range(0, targets.Count)];
        EEnemyTargetSelection selection = _enemy.EnemyData.TargetSelection;
        for (int i = 0; i < targets.Count; i++)
        {
            if ((selection == EEnemyTargetSelection.Frontmost && (int)targets[i].CurrentRank < (int)_plannedTarget.CurrentRank)
                || (selection == EEnemyTargetSelection.Rearmost && (int)targets[i].CurrentRank > (int)_plannedTarget.CurrentRank))
            {
                _plannedTarget = targets[i];
            }
        }
        if (_plannedSkill.IsGroupTarget) _plannedTargets.AddRange(targets);
        else _plannedTargets.Add(_plannedTarget);
        return true;
    }

    public bool IsPlanValid()
    {
        return _enemy != null && _enemy.isActiveAndEnabled && !_enemy.IsDeathPresented
            && _enemy.CurrentHp > 0 && _plannedSkill != null
            && IsHeroAvailable(_plannedTarget) && _heroes.Contains(_plannedTarget)
            && (IsExecuting || CSkillRankUtility.CanTargetSkill(_plannedSkill, _plannedTarget.CurrentRank))
            && (IsExecuting || CEnemySkillFilter.GetCandidates(_enemy.EnemyData, _enemy.CurrentRank).Contains(_plannedSkill))
            && (IsExecuting || _enemy.GetRemainingSkillCooldown(_plannedSkill) == 0)
            && (IsExecuting || ArePlannedTargetsValid())
            && CEnemySkillSelector.GetSelectionWeight(_enemy.EnemyData, _plannedSkill, _enemy.CurrentHp) > 0;
    }

    private bool ArePlannedTargetsValid()
    {
        List<CEntity> targets = CEnemyTargetFilter.GetTargets(_plannedSkill, _heroes);
        if (!targets.Contains(_plannedTarget)) return false;
        if (!_plannedSkill.IsGroupTarget) return _plannedTargets.Count == 1;
        if (targets.Count != _plannedTargets.Count) return false;
        for (int i = 0; i < targets.Count; i++)
        {
            if (!_plannedTargets.Contains(targets[i])) return false;
        }
        return true;
    }

    private bool RefreshPlan()
    {
        // 다시 선택할 때 보관한 후보 목록이 비워지지 않도록 복사합니다.
        return TryPlanAction(new List<CEntity>(_heroes), new List<CEnemy>(_allies));
    }

    public bool TryRequestPlannedAction()
    {
        if (!isActiveAndEnabled || _hasRequestedAction || _enemy == null
            || _enemy.ActionState != EEntityActionState.Selecting) return false;
        if (!IsPlanValid())
        {
            if (!RefreshPlan())
            {
                ClearPlan();
                return false;
            }
        }
        _hasRequestedAction = true;
        bool sent = _enemy.TryRequestSkillAction(_plannedTargets, _plannedTarget, _plannedSkill);
        if (!sent) _hasRequestedAction = false;
        return sent;
    }

    public void CancelPlan()
    {
        if (_hasRequestedAction && _enemy != null) _enemy.CancelAction();
        ClearPlan();
    }

    private bool IsHeroAvailable(CEntity hero)
    {
        // 체력이 0이어도 죽음의 문 상태일 수 있어서 체력만으로 제외하지 않습니다.
        return CEnemyTargetFilter.IsAvailable(hero);
    }

    private void HandleActionEnded(CEntity actor)
    {
        ClearPlan();
    }

    private void ClearPlan()
    {
        _retrySkill = null;
        _plannedSkill = null;
        _plannedTarget = null;
        _plannedTargets.Clear();
        _hasRequestedAction = false;
    }
}
