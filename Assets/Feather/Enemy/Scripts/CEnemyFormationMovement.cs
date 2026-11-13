using System.Collections.Generic;
using UnityEngine;

public class CEnemyFormationMovement : MonoBehaviour
{
    private struct RankRequest
    {
        public CEnemy Enemy;
        public ERank Rank;
    }

    private CEnemy _enemy;
    private IReadOnlyList<CEnemy> _members;
    private Transform[] _slots;
    private Vector3 _startPosition;
    private Vector3 _destination;
    private float _elapsed;
    private float _duration;
    private Vector3 _lastMovementPosition;
    private bool _yieldToExternalPosition;
    private bool _isMoving;
    private bool _hasPendingRank;
    private readonly List<RankRequest> _rankRequests = new List<RankRequest>();
    private readonly List<CEnemy> _pendingMembers = new List<CEnemy>();
    private readonly List<CEnemy> _movedMembers = new List<CEnemy>();
    public bool IsMoving
    {
        get { return _isMoving || _hasPendingRank; }
    }
    public bool IsAnimating
    {
        get { return _isMoving; }
    }
    public bool IsConfigured
    {
        get { return _members != null && HasValidSlots(); }
    }

    internal IReadOnlyList<CEnemy> Members
    {
        get { return _members; }
    }

    private void Awake()
    {
        _enemy = GetComponent<CEnemy>();
    }

    public void Configure(IReadOnlyList<CEnemy> members, Transform[] slots)
    {
        CancelPendingRequests();
        _members = members;
        _slots = slots;
    }

    // 공통 코드가 나눠 보낸 열 변경을 한 번의 진형 이동으로 묶음
    public bool TryQueueRank(ERank rank)
    {
        int destination = GetIndex(rank);
        if (!IsConfigured || _enemy == null || _enemy.IsDead || !_enemy.isActiveAndEnabled
            || _enemy.EnemyData == null || destination < 0
            || destination + _enemy.EnemyData.FormationSize > 4) return false;
        if (rank == _enemy.CurrentRank) return true;
        CEnemyFormationMovement coordinator = GetCoordinator();
        if (coordinator == null) return false;

        coordinator._rankRequests.Add(new RankRequest { Enemy = _enemy, Rank = rank });
        if (!coordinator._pendingMembers.Contains(_enemy))
        {
            coordinator._pendingMembers.Add(_enemy);
        }
        _hasPendingRank = true;
        return true;
    }

    private CEnemyFormationMovement GetCoordinator()
    {
        if (_members == null) return null;
        for (int i = 0; i < _members.Count; i++)
        {
            CEnemy member = _members[i];
            if (member != null && member.isActiveAndEnabled && member.FormationMovement != null
                && member.FormationMovement.isActiveAndEnabled) return member.FormationMovement;
        }
        return null;
    }

    private void LateUpdate()
    {
        if (_rankRequests.Count == 0) return;
        HashSet<CEnemy> processed = new HashSet<CEnemy>();
        for (int i = _rankRequests.Count - 1; i >= 0; i--)
        {
            RankRequest request = _rankRequests[i];
            if (request.Enemy == null || processed.Contains(request.Enemy)) continue;
            CEnemyFormationMovement movement = request.Enemy.FormationMovement;
            CEnemy largeNeighbor = GetLargeSwapNeighbor(i, processed);
            if (movement == null || !movement.TryMoveToRank(request.Rank, largeNeighbor)) break;
            processed.Add(request.Enemy);
            for (int j = 0; j < movement._movedMembers.Count; j++)
            {
                processed.Add(movement._movedMembers[j]);
            }
        }
        ClearRankRequests();
    }

    private CEnemy GetLargeSwapNeighbor(int requestIndex, HashSet<CEnemy> processed)
    {
        if (requestIndex == 0) return null;
        RankRequest request = _rankRequests[requestIndex];
        RankRequest neighbor = _rankRequests[requestIndex - 1];
        if (neighbor.Enemy == null || neighbor.Enemy == request.Enemy || processed.Contains(neighbor.Enemy)
            || neighbor.Rank != request.Enemy.CurrentRank || request.Rank != neighbor.Enemy.CurrentRank
            || Mathf.Abs(GetIndex(request.Rank) - GetIndex(neighbor.Rank)) != 1
            || neighbor.Enemy.EnemyData == null || neighbor.Enemy.EnemyData.FormationSize <= 1) return null;

        return neighbor.Enemy;
    }

    public void CancelPendingRequests()
    {
        CEnemyFormationMovement coordinator = GetCoordinator();
        if (_hasPendingRank)
        {
            if (coordinator != null)
            {
                coordinator.ClearRankRequests();
            }
            ClearRankRequests();
            return;
        }
        if (_rankRequests.Count == 0) return;
        if (coordinator != null && coordinator != this)
        {
            // 이동과 무관한 적이 빠지면 나머지 요청을 새 담당에게 넘김
            coordinator._rankRequests.AddRange(_rankRequests);
            for (int i = 0; i < _pendingMembers.Count; i++)
            {
                CEnemy member = _pendingMembers[i];
                if (!coordinator._pendingMembers.Contains(member))
                {
                    coordinator._pendingMembers.Add(member);
                }
            }
            _rankRequests.Clear();
            _pendingMembers.Clear();
        }
        else if (coordinator == null)
        {
            ClearRankRequests();
        }
    }

    private void ClearRankRequests()
    {
        for (int i = 0; i < _pendingMembers.Count; i++)
        {
            CEnemy member = _pendingMembers[i];
            if (member != null && member.FormationMovement != null)
            {
                member.FormationMovement._hasPendingRank = false;
            }
        }
        _pendingMembers.Clear();
        _rankRequests.Clear();
    }

    private void Update()
    {
        if (!_isMoving) return;
        if (_enemy == null || _enemy.IsDead)
        {
            SnapToRank();
            return;
        }
        if (_yieldToExternalPosition && (transform.position - _lastMovementPosition).sqrMagnitude > 0.00000001f)
        {
            // 전투 연출에서 좌표를 잡으면 진형 이동 보간을 멈춤
            _isMoving = false;
            _enemy.TryCompleteAction();
            return;
        }

        _elapsed += Time.deltaTime;
        float progress = Mathf.SmoothStep(0.0f, 1.0f, Mathf.Clamp01(_elapsed / _duration));
        _lastMovementPosition = Vector3.Lerp(_startPosition, _destination, progress);
        transform.position = _lastMovementPosition;
        if (_elapsed >= _duration)
        {
            SnapToRank();
            if (_yieldToExternalPosition) _enemy.TryCompleteAction();
        }
    }

    private void MoveToRank(float duration, bool yieldToExternalPosition)
    {
        if (!HasValidSlots()) return;
        int start = GetIndex(_enemy.CurrentRank);
        int end = start + _enemy.EnemyData.FormationSize - 1;
        if (start < 0 || end >= 4) return;
        _startPosition = transform.position;
        _lastMovementPosition = _startPosition;
        _destination = (_slots[start].position + _slots[end].position) * 0.5f;
        _elapsed = 0.0f;
        _duration = duration;
        _yieldToExternalPosition = yieldToExternalPosition;
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

    public bool TryCompactFormation()
    {
        if (!IsConfigured) return false;

        CEnemy[] membersByRank = new CEnemy[_slots.Length];
        int occupied = 0;
        for (int i = 0; i < _members.Count; i++)
        {
            CEnemy member = _members[i];
            if (member == null || !member.isActiveAndEnabled) continue;
            if (member.IsDead && (member.Corpse == null || member.Corpse.IsRemoved)) continue;
            if (member.EnemyData == null || member.IsPresentationBusy
                || member.FormationMovement == null || member.FormationMovement._slots != _slots) return false;

            int start = GetIndex(member.CurrentRank);
            int size = member.EnemyData.FormationSize;
            if (start < 0 || size < 1 || start + size > _slots.Length) return false;
            int mask = ((1 << size) - 1) << start;
            if ((occupied & mask) != 0) return false;
            occupied |= mask;
            membersByRank[start] = member;
        }

        int[] destinations = new int[_slots.Length];
        int nextRank = 0;
        for (int i = 0; i < membersByRank.Length; i++)
        {
            CEnemy member = membersByRank[i];
            if (member == null) continue;

            // 시체나 이동 불가 적이 차지한 칸은 유지함
            bool stays = member.IsDead || member.TokenManager.HasToken(EEffectType.Immobilize);
            destinations[i] = stays ? i : nextRank;
            nextRank = destinations[i] + member.EnemyData.FormationSize;
        }

        // 최종 열을 전부 반영한 뒤 모델 위치를 맞춤
        for (int i = 0; i < membersByRank.Length; i++)
        {
            CEnemy member = membersByRank[i];
            if (member == null || member.IsDead) continue;
            member.SetRankImmediate((ERank)(1 << destinations[i]));
        }
        for (int i = 0; i < membersByRank.Length; i++)
        {
            CEnemy member = membersByRank[i];
            if (member != null && !member.IsDead) member.FormationMovement.SnapToRank();
        }
        return true;
    }

    private void OnDisable()
    {
        CancelPendingRequests();
        if (_isMoving) SnapToRank();
    }

    public bool TryMove(CSkillDataSO skill, float duration = 0.0f)
    {
        if (_enemy == null || skill == null || _enemy.IsDead || !_enemy.isActiveAndEnabled
            || _enemy.ActionState != EEntityActionState.Executing || _enemy.TokenManager.HasToken(EEffectType.Immobilize)
            || _members == null || _slots == null || _slots.Length != 4) return false;
        CEnemySkillRule rule = _enemy.EnemyData.GetSkillRule(skill);
        if (rule == null || rule.RankMovement == 0) return false;
        return TryMoveOffset(rule.RankMovement, -1, duration, duration > 0.0f, countActors: true);
    }

    private bool TryMoveToRank(ERank rank, CEnemy largeNeighbor)
    {
        _movedMembers.Clear();
        if (_enemy == null || _enemy.IsDead || !_enemy.isActiveAndEnabled) return false;
        if (_enemy.CurrentRank == rank) return true;
        int destination = GetIndex(rank);
        if (destination < 0 || destination + _enemy.EnemyData.FormationSize > 4) return false;
        int movement = destination - GetIndex(_enemy.CurrentRank);
        if (largeNeighbor != null)
        {
            // 넉백 한 칸으로 대형 적 한 마리를 넘긴 뒤 실제 열을 맞춤
            destination += movement * (largeNeighbor.EnemyData.FormationSize - 1);
        }
        return TryMoveOffset(movement, destination, 0.18f, false, countActors: largeNeighbor != null);
    }

    private bool HasValidSlots()
    {
        if (_slots == null || _slots.Length != 4) return false;
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null) return false;
        }
        return true;
    }

    private bool TryMoveOffset(int movement, int requiredDestination, float duration, bool yieldToExternalPosition,
        bool countActors)
    {
        _movedMembers.Clear();
        if (!TryPlanMove(movement, countActors, out Dictionary<CEnemy, int> positions)
            || (requiredDestination >= 0 && positions[_enemy] != requiredDestination)) return false;
        foreach (KeyValuePair<CEnemy, int> pair in positions)
        {
            if (GetIndex(pair.Key.CurrentRank) == pair.Value) continue;
            pair.Key.SetRankImmediate((ERank)(1 << pair.Value));
            if (duration > 0.0f) pair.Key.FormationMovement.MoveToRank(duration, yieldToExternalPosition);
            _movedMembers.Add(pair.Key);
        }
        return true;
    }

    public bool CanMoveOffset(int movement)
    {
        return TryPlanMove(movement, true, out _);
    }

    private bool TryPlanMove(int movement, bool countActors, out Dictionary<CEnemy, int> positions)
    {
        positions = null;
        if (!IsConfigured || movement == 0 || _enemy == null || _enemy.EnemyData == null
            || _enemy.IsDead || !_enemy.isActiveAndEnabled
            || _enemy.TokenManager.HasToken(EEffectType.Immobilize)) return false;

        positions = new Dictionary<CEnemy, int>();
        CEnemy[] occupied = new CEnemy[4];
        for (int i = 0; i < _members.Count; i++)
        {
            CEnemy member = _members[i];
            if (member == null || !member.isActiveAndEnabled) continue;
            if (member.EnemyData == null) return false;
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
        int direction = movement < 0 ? -1 : 1;
        int remaining = Mathf.Abs(movement);
        while (remaining > 0)
        {
            int current = positions[_enemy];
            int next = direction < 0 ? current - 1 : current + size;
            if (next < 0 || next >= 4) break;
            CEnemy displaced = occupied[next];
            int distance = displaced != null ? displaced.EnemyData.FormationSize : 1;
            int movementCost = countActors ? 1 : distance;
            if (movementCost > remaining) break;
            if (displaced != null && (displaced.IsDead || displaced.IsCharacterPresentationBusy || displaced.IsActionActive
                || displaced.FormationMovement == null || displaced.FormationMovement.IsAnimating
                || displaced.TokenManager.HasToken(EEffectType.Immobilize))) break;

            for (int rank = current; rank < current + size; rank++)
            {
                occupied[rank] = null;
            }
            if (displaced != null)
            {
                int old = positions[displaced];
                for (int rank = old; rank < old + distance; rank++)
                {
                    occupied[rank] = null;
                }
                positions[displaced] = direction < 0 ? old + size : current;
                for (int rank = positions[displaced]; rank < positions[displaced] + distance; rank++)
                {
                    occupied[rank] = displaced;
                }
            }
            positions[_enemy] = current + direction * distance;
            for (int rank = positions[_enemy]; rank < positions[_enemy] + size; rank++)
            {
                occupied[rank] = _enemy;
            }
            remaining -= movementCost;
        }
        return positions[_enemy] != original;
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
