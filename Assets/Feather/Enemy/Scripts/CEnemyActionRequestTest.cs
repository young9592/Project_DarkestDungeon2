using UnityEngine;

public class CEnemyActionRequestTest : MonoBehaviour
{
    [Header("Temp_Lee Action Request")]
    [SerializeField] private CEnemy[] _enemies;
    [SerializeField] private int _selectedActorIndex = 1;
    [SerializeField] private int _selectedTargetIndex;

    private readonly string[] _rankLabels = { "1", "2", "3", "4" };
    private string[] _enemyLabels;
    private CEnemy _subscribedEnemy;
    private CEnemy _lastActor;
    private CEntity _lastTarget;
    private CSkillDataSO _lastSkill;
    private int _receivedCount;
    private string _lastRequest = "No request yet";

    private void OnEnable()
    {
        if (_enemies == null || _enemies.Length == 0) return;

        _enemyLabels = new string[_enemies.Length];
        for (int i = 0; i < _enemies.Length; i++)
        {
            CEnemy enemy = _enemies[i];
            _enemyLabels[i] = enemy != null && enemy.EnemyData != null ? enemy.EnemyData.Name : "Not assigned";
        }

        _selectedTargetIndex = Mathf.Clamp(_selectedTargetIndex, 0, _enemies.Length - 1);
        SelectActor(Mathf.Clamp(_selectedActorIndex, 0, _enemies.Length - 1));
    }

    private void OnDisable()
    {
        UnsubscribeEnemy();
    }

    public void DrawControls()
    {
        if (!isActiveAndEnabled || _enemyLabels == null || _enemyLabels.Length == 0)
        {
            GUILayout.Label("Assign and enable the action request test.");
            return;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Actor", GUILayout.Width(45.0f));
        int actorIndex = GUILayout.Toolbar(_selectedActorIndex, _enemyLabels);
        GUILayout.EndHorizontal();
        if (actorIndex != _selectedActorIndex) SelectActor(actorIndex);

        CEnemy actor = GetEnemy(_selectedActorIndex);
        DrawRankControls("Actor rank", actor);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Target", GUILayout.Width(45.0f));
        _selectedTargetIndex = GUILayout.Toolbar(_selectedTargetIndex, _enemyLabels);
        GUILayout.EndHorizontal();

        CEnemy target = GetEnemy(_selectedTargetIndex);
        DrawRankControls("Target rank", target);
        if (actor == null || target == null)
        {
            GUILayout.Label("Assign actor and target enemies.");
            return;
        }

        GUILayout.Label("Rank matches: " + actor.GetSkillCandidates(target.CurrentRank).Count);
        if (GUILayout.Button("Send Request (no combat)", GUILayout.Height(30.0f))) RequestAction();
        GUILayout.Label(_lastRequest);
        GUILayout.Label("Requests received: " + _receivedCount);
        GUILayout.Label("Last actor: " + (_lastActor != null ? _lastActor.name : "-"));
        GUILayout.Label("Last target: " + (_lastTarget != null ? _lastTarget.name : "-"));
        GUILayout.Label("Last skill: " + (_lastSkill != null ? _lastSkill.SkillName : "-"));
    }

    private void DrawRankControls(string label, CEnemy enemy)
    {
        if (enemy == null) return;

        int rankIndex = 0;
        switch (enemy.CurrentRank)
        {
            case ERank.Rank2: rankIndex = 1; break;
            case ERank.Rank3: rankIndex = 2; break;
            case ERank.Rank4: rankIndex = 3; break;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(75.0f));
        int selectedRank = GUILayout.Toolbar(rankIndex, _rankLabels, GUILayout.Height(25.0f));
        GUILayout.EndHorizontal();
        if (selectedRank != rankIndex) enemy.TrySetRank((ERank)(1 << selectedRank));
    }

    private CEnemy GetEnemy(int index)
    {
        if (_enemies == null || index < 0 || index >= _enemies.Length) return null;
        return _enemies[index];
    }

    private void SelectActor(int index)
    {
        if (_enemies == null || index < 0 || index >= _enemies.Length) return;

        UnsubscribeEnemy();
        _selectedActorIndex = index;
        _lastActor = null;
        _lastTarget = null;
        _lastSkill = null;
        _receivedCount = 0;
        _lastRequest = "No request yet";
        CEnemy actor = GetEnemy(index);
        if (actor == null || !isActiveAndEnabled) return;

        _subscribedEnemy = actor;
        _subscribedEnemy.OnSkillActionRequested += HandleSkillActionRequested;
    }

    private void UnsubscribeEnemy()
    {
        if (_subscribedEnemy != null) _subscribedEnemy.OnSkillActionRequested -= HandleSkillActionRequested;
        _subscribedEnemy = null;
    }

    private void RequestAction()
    {
        if (!isActiveAndEnabled) return;

        CEnemy actor = GetEnemy(_selectedActorIndex);
        CEnemy target = GetEnemy(_selectedTargetIndex);
        bool sent = actor != null && target != null && actor.TryRequestSkillAction(target, target.CurrentRank);
        _lastRequest = sent ? "Request sent" : "Not sent: check actor / target / ranks";
    }

    private void HandleSkillActionRequested(CEnemy actor, CEntity target, CSkillDataSO skill)
    {
        if (!isActiveAndEnabled || actor != _subscribedEnemy) return;

        // 전달받은 요청만 기록하고 전투 실행이나 피해 처리는 하지 않습니다.
        _lastActor = actor;
        _lastTarget = target;
        _lastSkill = skill;
        _receivedCount++;
    }
}
