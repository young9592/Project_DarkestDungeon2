using System.Collections.Generic;
using UnityEngine;

public static class CEnemySkillSelector
{
    // 조건 검사를 마친 후보를 받고 여러 개면 임시 규칙으로 무작위 선택합니다.
    public static CSkillDataSO SelectRandom(List<CSkillDataSO> candidates)
    {
        if (candidates == null || candidates.Count == 0) return null;
        if (candidates.Count == 1) return candidates[0];

        int selectedIndex = Random.Range(0, candidates.Count);
        return candidates[selectedIndex];
    }
}
