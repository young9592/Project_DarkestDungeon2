using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CEnemyBattleFormation : MonoBehaviour
{
    [Header("Battle Formation")]
    [SerializeField] private CEnemyParty _party;
    [SerializeField] private Transform[] _rankSlots = new Transform[4];

    private readonly List<CEnemy> _members = new List<CEnemy>();

    private void LateUpdate()
    {
        RefreshFormation();
    }

    public bool RefreshFormation()
    {
        if (!isActiveAndEnabled || _party == null || _party.EnemyList == null || !HasValidSlots()) return false;
        for (int i = _members.Count - 1; i >= 0; i--)
        {
            if (_members[i] != null) continue;
            _members.RemoveAt(i);
        }
        for (int i = 0; i < _party.EnemyList.Count; i++)
        {
            CEnemy enemy = _party.EnemyList[i];
            if (enemy == null || !enemy.isActiveAndEnabled || _members.Contains(enemy)) continue;
            _members.Add(enemy);
        }
        for (int i = 0; i < _members.Count; i++)
        {
            CEnemy enemy = _members[i];
            if (enemy.FormationMovement == null || enemy.FormationMovement.IsConfigured) continue;
            enemy.FormationMovement.Configure(_members, _rankSlots);
            enemy.FormationMovement.SnapToRank();
        }
        return _members.Count > 0;
    }

    private bool HasValidSlots()
    {
        if (_rankSlots == null || _rankSlots.Length != 4) return false;
        for (int i = 0; i < _rankSlots.Length; i++)
        {
            if (_rankSlots[i] == null) return false;
            for (int j = 0; j < i; j++)
            {
                if (_rankSlots[i] == _rankSlots[j]) return false;
            }
        }
        return true;
    }

    private void OnDisable()
    {
        for (int i = 0; i < _members.Count; i++)
        {
            CEnemy enemy = _members[i];
            if (enemy == null || enemy.FormationMovement == null) continue;
            enemy.FormationMovement.CancelPendingRequests();
            if (enemy.FormationMovement.IsMoving)
            {
                enemy.FormationMovement.SnapToRank();
            }
            enemy.FormationMovement.Configure(null, null);
        }
        // 전투 목록에서 빠진 시체도 다시 연결할 수 있게 멤버는 유지함
    }
}
