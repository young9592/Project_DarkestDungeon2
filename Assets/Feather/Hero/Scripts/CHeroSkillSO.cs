using UnityEngine;

public enum EHeroSkillTarget
{
    Enemy,
    Ally,
    Self,
    AllEnemies,
    AllyOrSelf,
    AllAllies
}

[CreateAssetMenu(menuName = "Data/Hero/Skill", fileName = "HeroSkillSO_")]
public class CHeroSkillSO : CSkillDataSO
{
    [Header("Battle Limits")]
    [SerializeField, Min(0)] private int _usesPerBattle;

    public int UsesPerBattle
    {
        get { return BaseSkill._usesPerBattle; }
    }

    [Header("Mastery")]
    [SerializeField] private CHeroSkillSO _baseSkill;

    public CHeroSkillSO BaseSkill
    {
        get { return _baseSkill != null ? _baseSkill : this; }
    }

    public static CSkillDataSO GetBaseSkill(CSkillDataSO skill)
    {
        CHeroSkillSO heroSkill = skill as CHeroSkillSO;
        return heroSkill != null ? heroSkill.BaseSkill : skill;
    }

    [Header("Hero Target")]
    [SerializeField] private EHeroSkillTarget _target;
    [Header("Presentation")]
    [SerializeField] private string _startState;
    [SerializeField] private string _endState;
    [SerializeField] private string _presentationNote;
    [SerializeField] private bool _requiresCorpse;
    [SerializeField] private bool _clearsCorpses;
    [Header("Selection Presentation")]
    [SerializeField] private string _selectionStartState;
    [SerializeField] private string _selectionHoldState;
    [SerializeField] private string _executionStartState;
    [Header("Skill Feedback")]
    [SerializeField] private CHeroSkillFeedbackSO _feedback;

    public CHeroSkillFeedbackSO Feedback
    {
        get { return _feedback; }
    }
    public bool IsGroupTarget
    {
        get { return _target == EHeroSkillTarget.AllEnemies || _target == EHeroSkillTarget.AllAllies; }
    }

    public EHeroSkillTarget Target
    {
        get { return _target; }
    }

    public string StartState
    {
        get { return _startState; }
    }

    public string EndState
    {
        get { return _endState; }
    }

    public string SelectionStartState
    {
        get { return _selectionStartState; }
    }

    public string SelectionHoldState
    {
        get { return _selectionHoldState; }
    }

    public string ExecutionStartState
    {
        get { return _executionStartState; }
    }

    public string PresentationNote
    {
        get { return _presentationNote; }
    }

    public bool RequiresCorpse
    {
        get { return _requiresCorpse; }
    }

    public bool ClearsCorpses { get { return _clearsCorpses || _requiresCorpse; } }

    public bool CanChooseTarget(CEntity actor, CEntity target)
    {
        if (actor == null || target == null)
        {
            return false;
        }

        if (target is CEnemyCorpse corpse)
        {
            if (_requiresCorpse) return corpse.IsAvailable;
            return corpse.IsAvailable
                && (_target == EHeroSkillTarget.Enemy || _target == EHeroSkillTarget.AllEnemies)
                && (_clearsCorpses || (EffectList != null && HasDamageEffect()));
        }

        if ((actor is CPlayer heroActor && heroActor.IsDeathPresented)
            || (target is CPlayer heroTarget && heroTarget.IsDeathPresented)
            || (target is CEnemy enemyTarget && enemyTarget.IsDeathPresented))
        {
            return false;
        }

        CEnemy enemy = target as CEnemy;
        if (enemy != null && enemy.CurrentHp <= 0)
        {
            return false;
        }

        // 사망 시스템 연결 전까지 시체 대상 스킬 사용 막기
        if (_requiresCorpse)
        {
            return false;
        }

        if (_target == EHeroSkillTarget.Self)
        {
            return actor == target;
        }

        if (_target == EHeroSkillTarget.AllyOrSelf || _target == EHeroSkillTarget.AllAllies)
        {
            return target.EntityType == actor.EntityType && target.EntityType != EEntityType.None;
        }

        if (actor == target)
        {
            return false;
        }

        if (_target == EHeroSkillTarget.Ally)
        {
            return target.EntityType == actor.EntityType && target.EntityType != EEntityType.None;
        }

        return target.EntityType == EEntityType.Enemy;
    }

    private bool HasDamageEffect()
    {
        for (int i = 0; i < EffectList.Count; i++)
        {
            if (EffectList[i] != null && EffectList[i].EffectType == EEffectType.Damage) return true;
        }
        return false;
    }
}
