using System;
using UnityEngine;

[Serializable]
public class CEnemyGroupEntry
{
    [SerializeField] private CEnemy _enemyPrefab;
    [SerializeField] private ERank _rank = ERank.Rank1;

    public CEnemy EnemyPrefab
    {
        get { return _enemyPrefab; }
    }

    public ERank Rank
    {
        get { return _rank; }
    }

    public bool TryGetOccupiedRanks(out ERank ranks)
    {
        ranks = ERank.None;
        if (_enemyPrefab == null || _enemyPrefab.EnemyData == null
            || !CSkillRankUtility.IsSingleRank(_rank)) return false;
        int size = _enemyPrefab.EnemyData.FormationSize;
        if (size < 1 || size > 4) return false;
        int start = 0;
        while ((1 << start) != (int)_rank) start++;
        if (start + size > 4) return false;
        for (int i = 0; i < size; i++) ranks |= (ERank)(1 << (start + i));
        return true;
    }
}
