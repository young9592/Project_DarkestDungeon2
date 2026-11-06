using System.Collections.Generic;

public class CEnemySkillCooldownState
{
    private readonly Dictionary<CSkillDataSO, int> _cooldowns = new Dictionary<CSkillDataSO, int>();
    private readonly HashSet<CSkillDataSO> _startedCooldowns = new HashSet<CSkillDataSO>();

    public int GetRemaining(CSkillDataSO skill)
    {
        if (skill == null) return 0;
        _cooldowns.TryGetValue(skill, out int remaining);
        return remaining;
    }

    public bool CanUse(CSkillDataSO skill)
    {
        return skill != null && GetRemaining(skill) == 0;
    }

    public void StartCooldown(CSkillDataSO skill)
    {
        if (skill == null || skill.Cooldown <= 0) return;
        _cooldowns[skill] = skill.Cooldown;
        _startedCooldowns.Add(skill);
    }

    public void BeginTurn(bool isExtraAction)
    {
        if (isExtraAction) return;

        List<CSkillDataSO> skills = new List<CSkillDataSO>(_cooldowns.Keys);
        for (int i = 0; i < skills.Count; i++)
        {
            CSkillDataSO skill = skills[i];
            // 사용한 턴 다음에는 표시된 쿨타임만큼 기다립니다.
            if (_startedCooldowns.Contains(skill)) continue;

            int remaining = _cooldowns[skill] - 1;
            if (remaining <= 0) _cooldowns.Remove(skill);
            else _cooldowns[skill] = remaining;
        }
        _startedCooldowns.Clear();
    }

    public void Clear()
    {
        _cooldowns.Clear();
        _startedCooldowns.Clear();
    }
}
