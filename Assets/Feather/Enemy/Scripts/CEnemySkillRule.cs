using System;
using UnityEngine;

public enum EEnemySkillCondition
{
    None,
    AllyInFront,
    NoAllyInFront,
    Alone,
    HasAlly
}

[Serializable]
public class CEnemySkillRule
{
    [SerializeField] private CSkillDataSO _skill;
    [SerializeField, Range(-3, 3)] private int _rankMovement;
    public int RankMovement { get { return Mathf.Clamp(_rankMovement, -3, 3); } }
    [Tooltip("같이 선택할 수 있는 스킬 사이의 비중입니다. 0이면 자동 선택에서 제외합니다.")]
    [SerializeField, Range(0, 100)] private int _weight = 1;

    [Header("Own HP Percent")]
    [SerializeField, Range(0, 100)] private int _minHpPercent;
    [SerializeField, Range(0, 100)] private int _maxHpPercent = 100;

    [Header("Preferred Action")]
    [SerializeField, Min(0)] private int _priority;
    [SerializeField] private ERank _preferredRanks;
    [SerializeField] private EEnemySkillCondition _condition;

    public CSkillDataSO Skill
    {
        get { return _skill; }
    }

    public int GetWeight(int currentHp, int maxHp)
    {
        if (_skill == null || maxHp <= 0) return 0;

        // 정수로 나누면서 체력 조건의 경계가 달라지는 것을 막습니다.
        long scaledHp = (long)Mathf.Clamp(currentHp, 0, maxHp) * 100;
        if (scaledHp < (long)maxHp * _minHpPercent
            || scaledHp > (long)maxHp * _maxHpPercent)
        {
            return 0;
        }

        return Mathf.Clamp(_weight, 0, 100);
    }

    public int GetPriority(ERank currentRank, bool hasAlly, bool allyInFront)
    {
        if (_preferredRanks != ERank.None && (_preferredRanks & currentRank) == 0) return 0;

        switch (_condition)
        {
            case EEnemySkillCondition.AllyInFront:
                if (!allyInFront) return 0;
                break;
            case EEnemySkillCondition.NoAllyInFront:
                if (allyInFront) return 0;
                break;
            case EEnemySkillCondition.Alone:
                if (hasAlly) return 0;
                break;
            case EEnemySkillCondition.HasAlly:
                if (!hasAlly) return 0;
                break;
        }

        return Mathf.Max(0, _priority);
    }

    public void Validate()
    {
        _weight = Mathf.Clamp(_weight, 0, 100);
        _minHpPercent = Mathf.Clamp(_minHpPercent, 0, 100);
        _maxHpPercent = Mathf.Clamp(_maxHpPercent, _minHpPercent, 100);
        _priority = Mathf.Max(0, _priority);
    }
}
