using System.Collections.Generic;
using UnityEngine;

public static class CEnemySkillSelector
{
    // 별도 규칙 없이 같은 비중으로 선택할 때 사용합니다.
    public static CSkillDataSO SelectRandom(List<CSkillDataSO> candidates)
    {
        if (candidates == null || candidates.Count == 0) return null;
        if (candidates.Count == 1) return candidates[0];

        int selectedIndex = Random.Range(0, candidates.Count);
        return candidates[selectedIndex];
    }

    public static int GetSelectionWeight(CEnemyDataSO enemyData, CSkillDataSO skill, int currentHp)
    {
        if (enemyData == null || skill == null) return 0;

        CEnemySkillRule rule = enemyData.GetSkillRule(skill);
        return rule != null ? rule.GetWeight(currentHp, enemyData.MaxHealth) : 1;
    }

    // 열 검사를 마친 후보에 체력 조건과 선택 비중을 적용합니다.
    public static CSkillDataSO SelectSkill(CEnemyDataSO enemyData, List<CSkillDataSO> candidates, int currentHp)
    {
        if (enemyData == null || candidates == null || candidates.Count == 0) return null;

        List<CSkillDataSO> selectableSkills = new List<CSkillDataSO>();
        float totalWeight = 0.0f;
        for (int i = 0; i < candidates.Count; i++)
        {
            CSkillDataSO skill = candidates[i];
            if (skill == null || selectableSkills.Contains(skill)) continue;

            int weight = GetSelectionWeight(enemyData, skill, currentHp);
            if (weight <= 0) continue;

            selectableSkills.Add(skill);
            totalWeight += weight;
        }

        if (selectableSkills.Count == 0) return null;
        if (selectableSkills.Count == 1) return selectableSkills[0];

        float selectedWeight = Random.Range(0.0f, totalWeight);
        for (int i = 0; i < selectableSkills.Count; i++)
        {
            CSkillDataSO skill = selectableSkills[i];
            selectedWeight -= GetSelectionWeight(enemyData, skill, currentHp);
            if (selectedWeight < 0.0f) return skill;
        }

        // float 난수가 최댓값을 포함하는 경우 마지막 후보를 선택합니다.
        return selectableSkills[selectableSkills.Count - 1];
    }
}
