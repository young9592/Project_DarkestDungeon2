using System.Collections.Generic;

public static class CEnemyTargetFilter
{
    public static bool IsAvailable(CEntity target)
    {
        return target != null && target.isActiveAndEnabled && !target.IsDead
            && target.EntityType == EEntityType.Hero
            && (!(target is CPlayer hero) || !hero.IsDeathPresented)
            && CSkillRankUtility.IsSingleRank(target.CurrentRank);
    }

    public static List<CEntity> GetTargets(CSkillDataSO skill, IReadOnlyList<CEntity> heroes)
    {
        List<CEntity> targets = new List<CEntity>();
        if (skill == null || heroes == null) return targets;

        bool hasVisibleTarget = false;
        for (int i = 0; i < heroes.Count; i++)
        {
            CEntity hero = heroes[i];
            if (!IsAvailable(hero) || targets.Contains(hero)
                || !CSkillRankUtility.CanTargetSkill(skill, hero.CurrentRank)) continue;

            targets.Add(hero);
            if (!hero.TokenManager.HasToken(EEffectType.Stealth)) hasVisibleTarget = true;
        }

        // 광역 공격은 은신 대상도 포함하며 도발로 범위를 좁히지 않습니다.
        if (skill.IsGroupTarget) return targets;

        if (hasVisibleTarget)
        {
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                if (targets[i].TokenManager.HasToken(EEffectType.Stealth)) targets.RemoveAt(i);
            }
        }

        bool hasTauntTarget = false;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].TokenManager.HasToken(EEffectType.Taunt)) hasTauntTarget = true;
        }
        if (hasTauntTarget)
        {
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                if (!targets[i].TokenManager.HasToken(EEffectType.Taunt)) targets.RemoveAt(i);
            }
        }
        return targets;
    }
}
