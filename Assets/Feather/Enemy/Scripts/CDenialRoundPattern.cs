using System.Collections.Generic;
using UnityEngine;

public class CDenialRoundPattern
{
    private readonly Dictionary<CDenialLock, int> _lastRoundByLock = new Dictionary<CDenialLock, int>();
    private CDenialLock _selectedLock;
    private int _lastRoundNumber;

    public EDenialRestriction ActiveRestriction { get; private set; }

    public void EndRound()
    {
        if (_selectedLock != null) _selectedLock.PreferUniqueAttack = false;
        _selectedLock = null;
        ActiveRestriction = EDenialRestriction.None;
    }

    public bool TryBeginRound(int roundNumber, IReadOnlyList<CEnemy> enemies, out CDenialLock selected)
    {
        selected = null;
        if (roundNumber <= _lastRoundNumber || enemies == null) return false;

        List<CDenialLock> livingLocks = new List<CDenialLock>();
        for (int i = 0; i < enemies.Count; i++)
        {
            CEnemy enemy = enemies[i];
            if (enemy == null || !enemy.isActiveAndEnabled || enemy.IsDead || enemy.IsDeathPresented) continue;
            if (!enemy.TryGetComponent(out CDenialLock denialLock) || !denialLock.isActiveAndEnabled
                || denialLock.Restriction == EDenialRestriction.None || livingLocks.Contains(denialLock)) continue;
            if (enemy.IsActionActive) return false;

            livingLocks.Add(denialLock);
        }

        List<CDenialLock> candidates = new List<CDenialLock>();
        for (int i = 0; i < livingLocks.Count; i++)
        {
            CDenialLock denialLock = livingLocks[i];
            // 잠금이 세 개 이상이면 직전 두 라운드에 선택한 잠금을 제외함
            if (livingLocks.Count > 2 && _lastRoundByLock.TryGetValue(denialLock, out int lastRound)
                && roundNumber - lastRound <= 2) continue;

            candidates.Add(denialLock);
        }
        if (candidates.Count == 0) return false;

        selected = candidates[Random.Range(0, candidates.Count)];
        EndRound();
        for (int i = 0; i < livingLocks.Count; i++)
        {
            CDenialLock denialLock = livingLocks[i];
            denialLock.PreferUniqueAttack = denialLock == selected;
            if (denialLock.TryGetComponent(out CEnemyAI ai)) ai.CancelPlan();
        }

        _selectedLock = selected;
        ActiveRestriction = selected.Restriction;
        _lastRoundByLock[selected] = roundNumber;
        _lastRoundNumber = roundNumber;
        return true;
    }
}
