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
    [Header("Hero Target")]
    [SerializeField] private EHeroSkillTarget _target;
    [Header("Presentation")]
    [SerializeField] private string _startState;
    [SerializeField] private string _endState;
    [SerializeField] private string _presentationNote;
    [SerializeField] private bool _requiresCorpse;
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

    public string PresentationNote
    {
        get { return _presentationNote; }
    }

    public bool RequiresCorpse
    {
        get { return _requiresCorpse; }
    }

    public bool CanChooseTarget(CEntity actor, CEntity target)
    {
        if (actor == null || target == null)
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
}
