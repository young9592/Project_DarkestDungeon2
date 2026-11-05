using System;
using System.Collections.Generic;

public static class CEnemyGroupGenerator
{
    public static bool TrySelectEncounter(IReadOnlyList<CEnemyGroupSO> encounters, int seed,
        out CEnemyGroupSO group)
    {
        group = null;
        if (encounters == null) return false;
        List<CEnemyGroupSO> valid = new List<CEnemyGroupSO>();
        for (int i = 0; i < encounters.Count; i++)
        {
            CEnemyGroupSO encounter = encounters[i];
            if (encounter != null && !valid.Contains(encounter) && encounter.TryValidate(out _))
            {
                valid.Add(encounter);
            }
        }
        if (valid.Count == 0) return false;
        group = valid[new Random(seed).Next(valid.Count)];
        return true;
    }

    // 생성한 조합은 사용이 끝나면 호출한 쪽에서 제거함
    public static bool TryGenerate(IReadOnlyList<CEnemy> prefabs, int enemyCount, int seed,
        out CEnemyGroupSO group, bool allowRepeatedEntries = false)
    {
        group = null;
        if (prefabs == null || enemyCount < 1 || enemyCount > 4) return false;

        List<CEnemy> candidates = new List<CEnemy>();
        for (int i = 0; i < prefabs.Count; i++)
        {
            CEnemy enemy = prefabs[i];
            if (enemy == null || enemy.EnemyData == null
                || (!allowRepeatedEntries && candidates.Contains(enemy))) continue;
#if UNITY_EDITOR
            if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(enemy)) continue;
#endif
            candidates.Add(enemy);
        }
        if (candidates.Count < enemyCount) return false;

        Random random = new Random(seed);
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int index = random.Next(i + 1);
            CEnemy enemy = candidates[i];
            candidates[i] = candidates[index];
            candidates[index] = enemy;
        }

        List<CEnemyGroupEntry> entries = new List<CEnemyGroupEntry>();
        if (!TryPlace(candidates, enemyCount, 0, entries)) return false;
        group = CEnemyGroupSO.CreateRuntime(entries);
        return group != null;
    }

    private static bool TryPlace(List<CEnemy> candidates, int count, int nextRank,
        List<CEnemyGroupEntry> entries)
    {
        if (entries.Count == count) return true;
        if (nextRank + count - entries.Count > 4) return false;

        ERank rank = (ERank)(1 << nextRank);
        for (int i = 0; i < candidates.Count; i++)
        {
            CEnemy enemy = candidates[i];
            if (enemy == null) continue;
            CEnemyGroupEntry entry = new CEnemyGroupEntry(enemy, rank);
            if (!entry.TryGetOccupiedRanks(out _)
                || CEnemySkillFilter.GetCandidates(enemy.EnemyData, rank).Count == 0) continue;

            // 뒤쪽 배치가 막히면 앞에서 고른 적을 바꿈
            candidates[i] = null;
            entries.Add(entry);
            bool placed = TryPlace(candidates, count, nextRank + enemy.EnemyData.FormationSize, entries);
            candidates[i] = enemy;
            if (placed) return true;
            entries.RemoveAt(entries.Count - 1);
        }
        return false;
    }
}
