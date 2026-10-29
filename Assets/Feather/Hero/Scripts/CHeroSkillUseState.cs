using System.Collections.Generic;

public class CHeroSkillUseState
{
    private readonly Dictionary<CSkillDataSO, int> _usedCounts = new Dictionary<CSkillDataSO, int>();
    private readonly Dictionary<CSkillDataSO, int> _cooldowns = new Dictionary<CSkillDataSO, int>();
    private readonly HashSet<CSkillDataSO> _startedCooldowns = new HashSet<CSkillDataSO>();

    public int GetRemainingCooldown(CSkillDataSO skill)
    {
        if (skill == null) return 0;
        _cooldowns.TryGetValue(CHeroSkillSO.GetBaseSkill(skill), out int remaining);
        return remaining;
    }

    public void BeginTurn(bool isExtraAction = false)
    {
        if (isExtraAction) return;
        List<CSkillDataSO> skills = new List<CSkillDataSO>(_cooldowns.Keys);
        for (int i = 0; i < skills.Count; i++)
        {
            CSkillDataSO skill = skills[i];
            // 사용한 턴을 대기 턴으로 세지 않습니다.
            if (_startedCooldowns.Contains(skill)) continue;
            int remaining = _cooldowns[skill] - 1;
            if (remaining <= 0) _cooldowns.Remove(skill);
            else _cooldowns[skill] = remaining;
        }
        _startedCooldowns.Clear();
    }

    public int GetLimit(CSkillDataSO skill)
    {
        CHeroSkillSO heroSkill = skill as CHeroSkillSO;
        return heroSkill != null ? heroSkill.UsesPerBattle : 0;
    }

    public int GetRemaining(CSkillDataSO skill)
    {
        int limit = GetLimit(skill);
        if (limit <= 0) return -1;

        CSkillDataSO baseSkill = CHeroSkillSO.GetBaseSkill(skill);
        _usedCounts.TryGetValue(baseSkill, out int used);
        return System.Math.Max(0, limit - used);
    }

    public bool CanUse(CSkillDataSO skill)
    {
        return skill != null && GetRemaining(skill) != 0 && GetRemainingCooldown(skill) == 0;
    }

    public bool TryUse(CSkillDataSO skill)
    {
        if (!CanUse(skill)) return false;
        CSkillDataSO baseSkill = CHeroSkillSO.GetBaseSkill(skill);
        if (GetLimit(skill) > 0)
        {
            _usedCounts.TryGetValue(baseSkill, out int used);
            _usedCounts[baseSkill] = used + 1;
        }
        if (skill.Cooldown > 0)
        {
            _cooldowns[baseSkill] = skill.Cooldown;
            _startedCooldowns.Add(baseSkill);
        }
        return true;
    }

    public CHeroSkillUseState Copy()
    {
        CHeroSkillUseState copy = new CHeroSkillUseState();
        copy.CopyFrom(this);
        return copy;
    }

    public void CopyFrom(CHeroSkillUseState state)
    {
        if (state == this) return;
        Clear();
        if (state == null) return;

        foreach (KeyValuePair<CSkillDataSO, int> entry in state._usedCounts)
        {
            if (entry.Key != null) _usedCounts.Add(entry.Key, entry.Value);
        }
        foreach (KeyValuePair<CSkillDataSO, int> entry in state._cooldowns)
        {
            if (entry.Key != null) _cooldowns.Add(entry.Key, entry.Value);
        }
        foreach (CSkillDataSO skill in state._startedCooldowns)
        {
            if (skill != null) _startedCooldowns.Add(skill);
        }
    }

    public void Clear()
    {
        _usedCounts.Clear();
        ClearCooldowns();
    }

    public void ClearCooldowns()
    {
        _cooldowns.Clear();
        _startedCooldowns.Clear();
    }
}
