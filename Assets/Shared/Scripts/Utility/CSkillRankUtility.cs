public static class CSkillRankUtility
{
    public static bool IsSingleRank(ERank rank)
    {
        return rank == ERank.Rank1 || rank == ERank.Rank2
            || rank == ERank.Rank3 || rank == ERank.Rank4;
    }

    public static bool ContainsRank(ERank allowedRanks, ERank rank)
    {
        // 대상의 현재 열은 하나여야 하고 허용 열에는 여러 열을 넣을 수 있습니다.
        return IsSingleRank(rank) && (allowedRanks & rank) != ERank.None;
    }

    public static bool CanUseSkill(CSkillDataSO skill, ERank currentRank)
    {
        return skill != null && ContainsRank(skill.UsableRank, currentRank);
    }

    public static bool CanTargetSkill(CSkillDataSO skill, ERank targetRank)
    {
        // 열 조건만 검사하고 진영과 생존 여부 및 쿨타임은 검사하지 않습니다.
        return skill != null && ContainsRank(skill.TargetableRank, targetRank);
    }
}
