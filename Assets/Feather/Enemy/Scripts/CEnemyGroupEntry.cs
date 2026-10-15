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
}
