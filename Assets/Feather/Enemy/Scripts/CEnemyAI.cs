using System.Collections.Generic;
using UnityEngine;

public class CEnemyAI : MonoBehaviour
{
    private CEnemy _enemy;
    private CSkillDataSO _plannedSkill;
    private CEntity _plannedTarget;
    private readonly List<CEntity> _heroes = new List<CEntity>();
    private bool _hasRequestedAction;

    public CSkillDataSO PlannedSkill { get { return _plannedSkill; } }
    public CEntity PlannedTarget { get { return _plannedTarget; } }
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
        if (!IsPlanValid() || _enemy.AttackAnimation.IsBusy) ClearPlan();
    }

    public bool TryPlanAction(IReadOnlyList<CEntity> heroes)
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
                if (IsHeroAvailable(hero) && !_heroes.Contains(hero)) _heroes.Add(hero);
            }
        }
        // 유효한 의도는 다시 추첨하지 않습니다.
        if (IsPlanValid()) return true;
        ClearPlan();

        List<CSkillDataSO> candidates = _enemy.GetSkillCandidates();
        List<CSkillDataSO> usableSkills = new List<CSkillDataSO>();
        for (int i = 0; i < candidates.Count; i++)
        {
            CSkillDataSO skill = candidates[i];
            if (skill == null || usableSkills.Contains(skill)) continue;
            for (int j = 0; j < _heroes.Count; j++)
            {
                if (!CSkillRankUtility.CanTargetSkill(skill, _heroes[j].CurrentRank)) continue;
                usableSkills.Add(skill);
                break;
            }
        }
        // 대상 수 때문에 스킬의 비중이 커지지 않도록 스킬을 먼저 고릅니다.
        _plannedSkill = CEnemySkillSelector.SelectSkill(_enemy.EnemyData, usableSkills, _enemy.CurrentHp);
        if (_plannedSkill == null) return false;
        List<CEntity> targets = new List<CEntity>();
        for (int i = 0; i < _heroes.Count; i++)
            if (CSkillRankUtility.CanTargetSkill(_plannedSkill, _heroes[i].CurrentRank)) targets.Add(_heroes[i]);
        _plannedTarget = targets[Random.Range(0, targets.Count)];
        return true;
    }

    public bool IsPlanValid()
    {
        return _enemy != null && _enemy.isActiveAndEnabled && !_enemy.IsDeathPresented
            && _enemy.CurrentHp > 0 && _plannedSkill != null
            && IsHeroAvailable(_plannedTarget) && _heroes.Contains(_plannedTarget)
            && _enemy.GetSkillCandidates(_plannedTarget.CurrentRank).Contains(_plannedSkill)
            && CEnemySkillSelector.GetSelectionWeight(_enemy.EnemyData, _plannedSkill, _enemy.CurrentHp) > 0;
    }

    public bool TryRequestPlannedAction()
    {
        if (!isActiveAndEnabled || _hasRequestedAction || _enemy == null
            || _enemy.ActionState != EEntityActionState.Selecting) return false;
        if (!IsPlanValid())
        {
            ClearPlan();
            return false;
        }
        _hasRequestedAction = true;
        bool sent = _enemy.TryRequestSkillAction(_plannedTarget, _plannedTarget.CurrentRank, _plannedSkill);
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
        return hero != null && hero.isActiveAndEnabled && hero.EntityType == EEntityType.Hero
            && (!(hero is CPlayer player) || !player.IsDeathPresented)
            && CSkillRankUtility.IsSingleRank(hero.CurrentRank);
    }

    private void HandleActionEnded(CEntity actor)
    {
        ClearPlan();
    }

    private void ClearPlan()
    {
        _plannedSkill = null;
        _plannedTarget = null;
        _hasRequestedAction = false;
    }
}
