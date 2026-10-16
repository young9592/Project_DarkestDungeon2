using System;
using UnityEngine;

[Serializable]
public class CEnemySkillRule
{
    [SerializeField] private CSkillDataSO _skill;

    [Tooltip("같이 선택할 수 있는 스킬 사이의 비중입니다. 0이면 자동 선택에서 제외합니다.")]
    [SerializeField, Range(0, 100)] private int _weight = 1;

    [Header("Own HP Percent")]
    [SerializeField, Range(0, 100)] private int _minHpPercent;
    [SerializeField, Range(0, 100)] private int _maxHpPercent = 100;

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

    public void Validate()
    {
        _weight = Mathf.Clamp(_weight, 0, 100);
        _minHpPercent = Mathf.Clamp(_minHpPercent, 0, 100);
        _maxHpPercent = Mathf.Clamp(_maxHpPercent, _minHpPercent, 100);
    }
}
