using System.Collections.Generic;
using UnityEngine;

public class CEnemyFormationMovement : MonoBehaviour
{
    private CEnemy _enemy;
    private IReadOnlyList<CEnemy> _members;
    private Transform[] _slots;
    private Vector3 _startPosition;
    private Vector3 _destination;
    private float _elapsed;
    private bool _isMoving;
    public bool IsMoving { get { return _isMoving; } }

    private void Awake()
    {
        _enemy = GetComponent<CEnemy>();
    }

    public void Configure(IReadOnlyList<CEnemy> members, Transform[] slots)
    {
        _members = members;
        _slots = slots;
    }

    private void Update()
    {
        if (!_isMoving) return;
        _elapsed += Time.deltaTime;
        float progress = Mathf.SmoothStep(0.0f, 1.0f, Mathf.Clamp01(_elapsed / 0.18f));
        transform.position = Vector3.Lerp(_startPosition, _destination, progress);
        if (_elapsed >= 0.18f) SnapToRank();
    }

    private void MoveToRank()
    {
        int start = GetIndex(_enemy.CurrentRank);
        int end = start + _enemy.EnemyData.FormationSize - 1;
        _startPosition = transform.position;
        _destination = (_slots[start].position + _slots[end].position) * 0.5f;
        _elapsed = 0.0f;
        _isMoving = true;
    }

    public void SnapToRank()
    {
        _isMoving = false;
        if (_enemy == null || _slots == null || _slots.Length != 4) return;
        int start = GetIndex(_enemy.CurrentRank);
        int end = start + _enemy.EnemyData.FormationSize - 1;
        if (start < 0 || end >= 4 || _slots[start] == null || _slots[end] == null) return;
        transform.position = (_slots[start].position + _slots[end].position) * 0.5f;
    }

    private void OnDisable()
    {
        if (_isMoving) SnapToRank();
    }

    public bool TryMove(CSkillDataSO skill)
    {
        if (_enemy == null || skill == null || _enemy.IsDead || !_enemy.isActiveAndEnabled
            || _enemy.ActionState != EEntityActionState.Executing || _enemy.TokenManager.HasToken(EEffectType.Immobilize)
            || _members == null || _slots == null || _slots.Length != 4) return false;
        CEnemySkillRule rule = _enemy.EnemyData.GetSkillRule(skill);
        if (rule == null || rule.RankMovement == 0) return false;
        for (int i = 0; i < _slots.Length; i++) if (_slots[i] == null) return false;

        Dictionary<CEnemy, int> positions = new Dictionary<CEnemy, int>();
        CEnemy[] occupied = new CEnemy[4];
        for (int i = 0; i < _members.Count; i++)
        {
            CEnemy member = _members[i];
            if (member == null || !member.isActiveAndEnabled) continue;
            if (member.IsDead && (member.Corpse == null || member.Corpse.IsRemoved)) continue;
            int start = GetIndex(member.CurrentRank);
            int end = start + member.EnemyData.FormationSize;
            if (start < 0 || end > 4 || positions.ContainsKey(member)) return false;
            for (int rank = start; rank < end; rank++)
            {
                if (occupied[rank] != null) return false;
                occupied[rank] = member;
            }
            positions.Add(member, start);
        }
        if (!positions.ContainsKey(_enemy)) return false;

        int original = positions[_enemy];
        int size = _enemy.EnemyData.FormationSize;
        int direction = rule.RankMovement < 0 ? -1 : 1;
        int remaining = Mathf.Abs(rule.RankMovement);
        while (remaining > 0)
        {
            int current = positions[_enemy];
            int next = direction < 0 ? current - 1 : current + size;
            if (next < 0 || next >= 4) break;
            CEnemy displaced = occupied[next];
            int distance = displaced != null ? displaced.EnemyData.FormationSize : 1;
            if (distance > remaining) break;
            if (displaced != null && (displaced.IsDead || displaced.IsPresentationBusy || displaced.IsActionActive
                || displaced.TokenManager.HasToken(EEffectType.Immobilize))) break;

            for (int rank = current; rank < current + size; rank++) occupied[rank] = null;
            if (displaced != null)
            {
                int old = positions[displaced];
                for (int rank = old; rank < old + distance; rank++) occupied[rank] = null;
                positions[displaced] = direction < 0 ? old + size : current;
                for (int rank = positions[displaced]; rank < positions[displaced] + distance; rank++) occupied[rank] = displaced;
            }
            positions[_enemy] = current + direction * distance;
            for (int rank = positions[_enemy]; rank < positions[_enemy] + size; rank++) occupied[rank] = _enemy;
            remaining -= distance;
        }
        if (positions[_enemy] == original) return false;
        foreach (KeyValuePair<CEnemy, int> pair in positions)
        {
            if (GetIndex(pair.Key.CurrentRank) == pair.Value) continue;
            pair.Key.TrySetRank((ERank)(1 << pair.Value));
            pair.Key.FormationMovement.MoveToRank();
        }
        return true;
    }

    private int GetIndex(ERank rank)
    {
        switch (rank)
        {
            case ERank.Rank1: return 0;
            case ERank.Rank2: return 1;
            case ERank.Rank3: return 2;
            case ERank.Rank4: return 3;
        }
        return -1;
    }
}
