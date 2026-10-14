using System.Collections.Generic;
using UnityEngine;

public class CEnemySkillTest : MonoBehaviour
{
    [Header("Temp_Lee Skill Test")]
    [SerializeField] private CEnemyDataSO _enemyData;
    [SerializeField] private CEnemyAttackAnimation _enemy;
    [SerializeField, Range(1, 4)] private int _currentRank = 1;

    private readonly string[] _rankLabels = { "1", "2", "3", "4" };
    private List<CSkillDataSO> _candidates = new List<CSkillDataSO>();
    private Vector2 _scrollPosition;
    private bool _needsRefresh = true;
    private bool _isWaitingForAttack;
    private string _requestedSkillName;
    private string _lastRequest = "No skill request yet";
    private int _completedAttackCount;

    private void OnEnable()
    {
        _needsRefresh = true;
        if (_enemy == null) return;

        _enemy.OnAttackCompleted += HandleAttackCompleted;
    }

    private void OnDisable()
    {
        if (_enemy != null) _enemy.OnAttackCompleted -= HandleAttackCompleted;
        if (!_isWaitingForAttack) return;

        _isWaitingForAttack = false;
        _lastRequest = "Test disabled: tracking cleared";
    }

    private void OnValidate()
    {
        _currentRank = Mathf.Clamp(_currentRank, 1, 4);
        _needsRefresh = true;
    }

    private void LateUpdate()
    {
        if (!_isWaitingForAttack) return;
        if (_enemy != null && _enemy.IsAttacking) return;

        _isWaitingForAttack = false;
        _lastRequest = "Interrupted: no completion";
    }

    private void OnGUI()
    {
        if (_needsRefresh) RefreshCandidates();

        GUILayout.BeginArea(new Rect(360.0f, 20.0f, 300.0f, 340.0f), GUI.skin.box);
        GUILayout.Label("Enemy Skill Candidates");
        GUILayout.Label("Enemy: " + (_enemyData != null ? _enemyData.Name : "Not assigned"));
        GUILayout.Label("Current rank");

        int selectedRank = GUILayout.Toolbar(_currentRank - 1, _rankLabels, GUILayout.Height(30.0f)) + 1;
        if (selectedRank != _currentRank) SetRank(selectedRank);
        if (GUILayout.Button("Refresh Data")) RefreshCandidates();

        GUILayout.Label(_lastRequest);
        GUILayout.Label("Skill attack completions: " + _completedAttackCount);
        GUILayout.Label("Rank candidates: " + _candidates.Count);
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        if (_enemyData == null)
        {
            GUILayout.Label("Assign an EnemySO in the Inspector.");
        }
        else if (_candidates.Count == 0)
        {
            GUILayout.Label("No skills match the current rank.");
        }

        if (_enemy == null) GUILayout.Label("Assign an Enemy animation in the Inspector.");

        for (int i = 0; i < _candidates.Count; i++)
        {
            CSkillDataSO skill = _candidates[i];
            if (skill == null) continue;

            GUILayout.Label(skill.SkillName);
            GUILayout.Label("Targets: " + skill.TargetableRank);
            if (GUILayout.Button("Test Attack", GUILayout.Height(30.0f))) RequestAttack(skill);
            GUILayout.Space(6.0f);
        }
        GUILayout.EndScrollView();
        GUILayout.Label("Rank + animation test only");
        GUILayout.EndArea();
    }

    private void SetRank(int rank)
    {
        _currentRank = Mathf.Clamp(rank, 1, 4);
        RefreshCandidates();
        _scrollPosition = Vector2.zero;
    }

    private void RefreshCandidates()
    {
        ERank rank = (ERank)(1 << (_currentRank - 1));
        _candidates = CEnemySkillFilter.GetCandidates(_enemyData, rank);
        _needsRefresh = false;
    }

    private void RequestAttack(CSkillDataSO skill)
    {
        if (!isActiveAndEnabled) return;
        if (_isWaitingForAttack)
        {
            _lastRequest = "Rejected: attack in progress";
            return;
        }

        RefreshCandidates();
        if (skill == null || !_candidates.Contains(skill))
        {
            _lastRequest = "Rejected: skill unavailable at this rank";
            return;
        }

        if (_enemy == null)
        {
            _lastRequest = "Rejected: Enemy animation missing";
            return;
        }

        // 연출 연결 테스트고 스킬 효과는 적용하지 않습니다.
        if (!_enemy.TryPlayAttack())
        {
            _lastRequest = "Rejected: busy / unavailable";
            return;
        }

        _requestedSkillName = skill.SkillName;
        _isWaitingForAttack = true;
        _lastRequest = "Playing: " + _requestedSkillName;
    }

    private void HandleAttackCompleted()
    {
        // 이 테스트에서 요청한 공격만 완료 횟수에 반영합니다.
        if (!_isWaitingForAttack) return;

        _isWaitingForAttack = false;
        _completedAttackCount++;
        _lastRequest = "Completed: " + _requestedSkillName;
    }
}
