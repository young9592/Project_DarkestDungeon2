using System.Collections.Generic;

public static class CEnemySkillFilter
{
    // 현재 열 조건만 검사합니다. 쿨타임과 실제 대상 유무는 전투에서 판단합니다.
    public static List<CSkillDataSO> GetCandidates(CEnemyDataSO enemyData, ERank currentRank)
    {
        List<CSkillDataSO> candidates = new List<CSkillDataSO>();
        if (enemyData == null) return candidates;

        if (currentRank != ERank.Rank1 && currentRank != ERank.Rank2
            && currentRank != ERank.Rank3 && currentRank != ERank.Rank4)
        {
            return candidates;
        }

        IReadOnlyList<CSkillDataSO> skills = enemyData.AvailableSkills;
        if (skills == null) return candidates;

        for (int i = 0; i < skills.Count; i++)
        {
            CSkillDataSO skill = skills[i];
            if (skill == null) continue;
            if ((skill.UsableRank & currentRank) == ERank.None) continue;

            candidates.Add(skill);
        }

        return candidates;
    }

    // 사용자 열과 대상 열을 검사합니다. 쿨타임과 진영 및 생존 여부는 전투에서 판단합니다.
    public static List<CSkillDataSO> GetCandidates(CEnemyDataSO enemyData, ERank currentRank, ERank targetRank)
    {
        List<CSkillDataSO> candidates = new List<CSkillDataSO>();
        if (targetRank != ERank.Rank1 && targetRank != ERank.Rank2
            && targetRank != ERank.Rank3 && targetRank != ERank.Rank4)
        {
            return candidates;
        }

        List<CSkillDataSO> rankCandidates = GetCandidates(enemyData, currentRank);
        for (int i = 0; i < rankCandidates.Count; i++)
        {
            CSkillDataSO skill = rankCandidates[i];
            if ((skill.TargetableRank & targetRank) == ERank.None) continue;

            candidates.Add(skill);
        }

        return candidates;
    }
}
