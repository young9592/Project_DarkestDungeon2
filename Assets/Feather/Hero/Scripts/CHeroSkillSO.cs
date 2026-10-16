using UnityEngine;

public enum EHeroSkillTarget
{
    Enemy,
    Ally,
    Self,
    AllEnemies
}

[CreateAssetMenu(menuName = "Data/Hero/Skill", fileName = "HeroSkillSO_")]
public class CHeroSkillSO : CSkillDataSO
{
    [Header("Hero Target")]
    [SerializeField] private EHeroSkillTarget _target;
    [Header("Presentation")]
    [SerializeField] private string _startState;
    [SerializeField] private string _endState;

    public EHeroSkillTarget Target { get { return _target; } }
    public string StartState { get { return _startState; } }
    public string EndState { get { return _endState; } }

    public bool CanChooseTarget(CEntity actor, CEntity target)
    {
        if (actor == null || target == null) return false;
        if (_target == EHeroSkillTarget.Self) return actor == target;
        if (actor == target) return false;
        if (_target == EHeroSkillTarget.Ally) return target.EntityType == actor.EntityType && target.EntityType != EEntityType.None;
        return target.EntityType == EEntityType.Enemy;
    }
}
