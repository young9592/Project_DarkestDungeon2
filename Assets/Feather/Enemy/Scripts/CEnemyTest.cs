using System.Collections.Generic;
using UnityEngine;

public class CEnemyTest : MonoBehaviour
{
    [Header("Temp_Lee Enemy Test")]
    [SerializeField] private CEnemy[] _enemies;
    [SerializeField] private int _selectedEnemyIndex;
    [SerializeField, Min(1)] private int _testDamage = 3;

    [Header("Action Request Test")]
    [SerializeField] private CEnemyActionRequestTest _actionRequestTest;

    private readonly string[] _rankLabels = { "1", "2", "3", "4" };
    private readonly string[] _testModeLabels = { "Animation", "Action Request" };
    private int _selectedTestMode = 1;
    private string[] _enemyLabels;
    private CEnemy _selectedEnemy;
    private CEnemyAttackAnimation _subscribedAnimation;
    private List<CSkillDataSO> _candidates = new List<CSkillDataSO>();
    private Vector2 _scrollPosition;
    private bool _isWaitingForAttack;
    private string _requestedSkillName;
    private string _lastRequest = "No skill request yet";
    private int _completedAttackCount;
    private bool _showDetails = true;

    private void OnEnable()
    {
        if (_enemies == null || _enemies.Length == 0) return;

        _enemyLabels = new string[_enemies.Length];
        for (int i = 0; i < _enemies.Length; i++)
        {
            CEnemy enemy = _enemies[i];
            _enemyLabels[i] = enemy != null && enemy.EnemyData != null ? enemy.EnemyData.Name : "Not assigned";
        }

        SelectEnemy(Mathf.Clamp(_selectedEnemyIndex, 0, _enemies.Length - 1));
    }

    private void OnDisable()
    {
        UnsubscribeAnimation();
        _isWaitingForAttack = false;
    }

    private void LateUpdate()
    {
        if (!_isWaitingForAttack) return;
        if (_subscribedAnimation != null && _subscribedAnimation.IsAttacking) return;

        _isWaitingForAttack = false;
        _lastRequest = "Interrupted: no completion";
    }

    private void OnGUI()
    {
        float panelHeight = _showDetails ? 340.0f : 30.0f;
        GUILayout.BeginArea(new Rect(360.0f, 20.0f, 300.0f, panelHeight), GUI.skin.box);
        if (GUILayout.Button("Enemy Runtime Test" + (_showDetails ? " [-]" : " [+]"))) _showDetails = !_showDetails;
        if (!_showDetails)
        {
            GUILayout.EndArea();
            return;
        }

        if (_actionRequestTest != null)
        {
            _selectedTestMode = GUILayout.Toolbar(_selectedTestMode, _testModeLabels);
            if (_selectedTestMode == 1)
            {
                _actionRequestTest.DrawControls();
                GUILayout.EndArea();
                return;
            }
        }

        if (_enemyLabels != null && _enemyLabels.Length > 0)
        {
            int selectedIndex = GUILayout.Toolbar(_selectedEnemyIndex, _enemyLabels);
            if (selectedIndex != _selectedEnemyIndex) SelectEnemy(selectedIndex);
        }

        if (_selectedEnemy == null || _selectedEnemy.EnemyData == null)
        {
            GUILayout.Label("Assign Enemy components in the Inspector.");
            GUILayout.EndArea();
            return;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("HP: " + _selectedEnemy.CurrentHp + " / " + _selectedEnemy.EnemyData.MaxHealth);
        if (GUILayout.Button("Damage " + _testDamage)) RequestDamage();
        GUILayout.EndHorizontal();
        GUILayout.Label("Current rank");
        int currentRank = GetRankNumber(_selectedEnemy.CurrentRank);
        int selectedRank = GUILayout.Toolbar(currentRank - 1, _rankLabels, GUILayout.Height(30.0f)) + 1;
        if (selectedRank != currentRank) SetRank(selectedRank);

        GUILayout.Label(_lastRequest);
        GUILayout.Label("Skill attack completions: " + _completedAttackCount);
        RefreshCandidates();
        GUILayout.Label("Rank candidates: " + _candidates.Count);
        if (GUILayout.Button("Random Skill Attack", GUILayout.Height(30.0f))) RequestRandomAttack();
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        if (_candidates.Count == 0) GUILayout.Label("No skills match the current rank.");
        for (int i = 0; i < _candidates.Count; i++)
        {
            CSkillDataSO skill = _candidates[i];
            if (skill == null) continue;

            if (GUILayout.Button(skill.SkillName, GUILayout.Height(30.0f))) RequestAttack(skill);
            GUILayout.Label("Targets: " + skill.TargetableRank);
        }
        GUILayout.EndScrollView();
        GUILayout.Label("Rank-only selection / shared attack animation");
        GUILayout.EndArea();
    }

    private void SelectEnemy(int index)
    {
        if (_enemies == null || index < 0 || index >= _enemies.Length) return;

        UnsubscribeAnimation();
        _selectedEnemyIndex = index;
        _selectedEnemy = _enemies[index];
        _isWaitingForAttack = false;
        _completedAttackCount = 0;
        _lastRequest = "No skill request yet";
        _scrollPosition = Vector2.zero;
        if (_selectedEnemy != null && _selectedEnemy.AttackAnimation != null && isActiveAndEnabled)
        {
            _subscribedAnimation = _selectedEnemy.AttackAnimation;
            _subscribedAnimation.OnAttackCompleted += HandleAttackCompleted;
        }

        RefreshCandidates();
    }

    private void UnsubscribeAnimation()
    {
        if (_subscribedAnimation != null) _subscribedAnimation.OnAttackCompleted -= HandleAttackCompleted;
        _subscribedAnimation = null;
    }

    private void SetRank(int rank)
    {
        if (_selectedEnemy == null || rank < 1 || rank > 4) return;

        _selectedEnemy.TrySetRank((ERank)(1 << (rank - 1)));
        RefreshCandidates();
        _scrollPosition = Vector2.zero;
    }

    private int GetRankNumber(ERank rank)
    {
        switch (rank)
        {
            case ERank.Rank2: return 2;
            case ERank.Rank3: return 3;
            case ERank.Rank4: return 4;
            default: return 1;
        }
    }

    private void RefreshCandidates()
    {
        _candidates = _selectedEnemy != null ? _selectedEnemy.GetSkillCandidates() : new List<CSkillDataSO>();
    }

    private void RequestAttack(CSkillDataSO skill)
    {
        if (!isActiveAndEnabled || _selectedEnemy == null) return;
        if (_isWaitingForAttack)
        {
            _lastRequest = "Rejected: attack in progress";
            return;
        }

        if (!_selectedEnemy.TryPlayAttack(skill))
        {
            _lastRequest = "Rejected: rank / busy / unavailable";
            return;
        }

        _requestedSkillName = skill.SkillName;
        _isWaitingForAttack = true;
        _lastRequest = "Playing: " + _requestedSkillName;
    }

    private void RequestRandomAttack()
    {
        if (!isActiveAndEnabled || _selectedEnemy == null) return;
        if (_isWaitingForAttack)
        {
            _lastRequest = "Rejected: attack in progress";
            return;
        }

        CSkillDataSO skill = _selectedEnemy.SelectSkill();
        if (skill == null)
        {
            _lastRequest = "No skill selected: no candidates / unavailable";
            return;
        }

        RequestAttack(skill);
    }

    private void RequestDamage()
    {
        if (!isActiveAndEnabled || _selectedEnemy == null) return;

        // 피해 처리 확인용이고 공격 대상에게 스킬 효과를 적용하는 버튼은 아닙니다.
        _selectedEnemy.TakeDamage(_testDamage, null);
    }

    private void HandleAttackCompleted()
    {
        // 이 패널에서 선택한 적에게 요청한 공격만 완료 횟수에 반영합니다.
        if (!_isWaitingForAttack) return;

        _isWaitingForAttack = false;
        _completedAttackCount++;
        _lastRequest = "Completed: " + _requestedSkillName;
    }
}
