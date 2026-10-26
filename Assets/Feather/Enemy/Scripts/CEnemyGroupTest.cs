using System.Collections.Generic;
using UnityEngine;

public class CEnemyGroupTest : MonoBehaviour
{
    [Header("Temp_Lee Group Test")]
    [SerializeField] private CEnemyGroupSO _group;
    [SerializeField] private CEnemyGroupSO[] _groupOptions;
    [SerializeField] private Transform _spawnRoot;
    [SerializeField] private Transform[] _rankSlots = new Transform[4];
    [SerializeField] private Camera _camera;
    [Header("Existing Tests")]
    [SerializeField] private GameObject[] _existingEnemies;
    [SerializeField] private MonoBehaviour[] _existingPanels;
    private readonly List<CEnemy> _spawnedEnemies = new List<CEnemy>();
    private bool[] _enemyActiveStates;
    private bool[] _panelEnabledStates;
    private bool _isShowingGroup;
    private CEnemy _selectedEnemy;
    private CEnemyIntentTest _selectedIntent;
    private string _message = "Load a group to preview its ranks";
    private int _groupOptionIndex;
    public bool ShowControls { get; set; } = true;

    public IReadOnlyList<CEnemy> SpawnedEnemies
    {
        get { return _spawnedEnemies; }
    }

    public CEnemy SelectedEnemy
    {
        get { return _selectedEnemy; }
    }

    public bool TrySelectEnemy(int index)
    {
        if (!isActiveAndEnabled || index < 0 || index >= _spawnedEnemies.Count)
        {
            return false;
        }

        CEnemy enemy = _spawnedEnemies[index];
        if (enemy == null || !enemy.isActiveAndEnabled || enemy.IsDeathPresented)
        {
            return false;
        }

        _selectedEnemy = enemy;
        _selectedIntent = enemy.GetComponent<CEnemyIntentTest>();
        return _selectedIntent != null;
    }

    private void LateUpdate()
    {
        if (_selectedEnemy != null && _selectedEnemy.IsDeathPresented)
        {
            _selectedEnemy = null;
            _selectedIntent = null;
        }
    }

    public bool TryPlanSelectedSkill()
    {
        if (!isActiveAndEnabled || _selectedIntent == null)
        {
            return false;
        }

        bool accepted = _selectedIntent.TryPrepareIntent();
        _message = accepted ? "Skill planned / ready to play" : "No available skill or enemy is busy";
        return accepted;
    }

    public bool TryPlaySelectedIntent()
    {
        if (!isActiveAndEnabled || _selectedIntent == null)
        {
            return false;
        }

        bool accepted = _selectedIntent.TryExecuteIntent();
        _message = accepted ? "Playing the planned skill" : "Cannot play / check skill and animation binding";
        return accepted;
    }

    private void Awake()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }
    }

    private void OnDisable()
    {
        ClearGroup();
    }

    public bool TryLoadGroup(CEnemyGroupSO group)
    {
        if (!isActiveAndEnabled)
        {
            return false;
        }

        if (group == null)
        {
            _message = "Assign an Enemy Group SO";
            return false;
        }

        if (!group.TryValidate(out string message))
        {
            _message = message;
            return false;
        }

        if (_spawnRoot == null || _rankSlots == null || _rankSlots.Length != 4)
        {
            _message = "Assign a spawn root and four rank slots";
            return false;
        }

        for (int i = 0; i < _rankSlots.Length; i++)
        {
            if (_rankSlots[i] == null)
            {
                _message = "A rank slot is missing";
                return false;
            }
        }

        // 배치 검사 통과 후 기존 테스트 숨기고 적 교체
        HideExistingTests();
        DestroySpawnedEnemies();
        for (int i = 0; i < group.Entries.Count; i++)
        {
            CEnemyGroupEntry entry = group.Entries[i];
            Transform slot = _rankSlots[GetRankIndex(entry.Rank)];
            int endIndex = GetRankIndex(entry.Rank) + entry.EnemyPrefab.EnemyData.FormationSize - 1;
            Vector3 center = (slot.position + _rankSlots[endIndex].position) * 0.5f;
            CEnemy enemy = Instantiate(entry.EnemyPrefab, center, slot.rotation, _spawnRoot);
            enemy.name = entry.EnemyPrefab.name + "_" + entry.Rank;
            enemy.TrySetRank(entry.Rank);
            if (enemy.GetComponent<CEnemyIntentTest>() == null)
            {
                enemy.gameObject.AddComponent<CEnemyIntentTest>();
            }

            _spawnedEnemies.Add(enemy);
        }

        TrySelectEnemy(0);
        if (_groupOptions != null)
        {
            for (int i = 0; i < _groupOptions.Length; i++)
            {
                if (_groupOptions[i] == group)
                {
                    _groupOptionIndex = i;
                }
            }
        }

        _message = group.name + " / " + _spawnedEnemies.Count + " enemies";
        return true;
    }

    public void ClearGroup()
    {
        DestroySpawnedEnemies();
        if (!_isShowingGroup)
        {
            return;
        }

        if (_existingEnemies != null)
        {
            for (int i = 0; i < _existingEnemies.Length; i++)
            {
                if (_existingEnemies[i] != null)
                {
                    _existingEnemies[i].SetActive(_enemyActiveStates[i]);
                }
            }
        }

        if (_existingPanels != null)
        {
            for (int i = 0; i < _existingPanels.Length; i++)
            {
                if (_existingPanels[i] != null)
                {
                    _existingPanels[i].enabled = _panelEnabledStates[i];
                }
            }
        }

        _isShowingGroup = false;
        _message = "Group cleared / existing tests restored";
    }

    public void SuspendGroup()
    {
        // 준비 화면으로 돌아갈 때 배치한 적 정리
        DestroySpawnedEnemies();
        _message = "Choose a group to enter again";
    }

    private void HideExistingTests()
    {
        if (_isShowingGroup)
        {
            return;
        }

        if (_existingPanels != null)
        {
            _panelEnabledStates = new bool[_existingPanels.Length];
            for (int i = 0; i < _existingPanels.Length; i++)
            {
                if (_existingPanels[i] == null)
                {
                    continue;
                }

                _panelEnabledStates[i] = _existingPanels[i].enabled;
                _existingPanels[i].enabled = false;
            }
        }

        if (_existingEnemies != null)
        {
            _enemyActiveStates = new bool[_existingEnemies.Length];
            for (int i = 0; i < _existingEnemies.Length; i++)
            {
                if (_existingEnemies[i] == null)
                {
                    continue;
                }

                _enemyActiveStates[i] = _existingEnemies[i].activeSelf;
                _existingEnemies[i].SetActive(false);
            }
        }

        _isShowingGroup = true;
    }

    private void DestroySpawnedEnemies()
    {
        // 조합 교체 전에 선택한 적 해제
        _selectedEnemy = null;
        _selectedIntent = null;
        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            if (_spawnedEnemies[i] == null)
            {
                continue;
            }

            // 프레임 끝에 제거되기 전에 먼저 비활성화
            _spawnedEnemies[i].gameObject.SetActive(false);
            Destroy(_spawnedEnemies[i].gameObject);
        }

        _spawnedEnemies.Clear();
    }

    private int GetRankIndex(ERank rank)
    {
        switch (rank)
        {
            case ERank.Rank2:
                return 1;
            case ERank.Rank3:
                return 2;
            case ERank.Rank4:
                return 3;
            default:
                return 0;
        }
    }

    private void OnGUI()
    {
        if (!CTempLeeTestUI.CanShow(this)) return;

        if (!ShowControls)
        {
            return;
        }

        GUILayout.BeginArea(new Rect(20.0f, Screen.height - 120.0f, 320.0f, 110.0f), GUI.skin.box);
        GUILayout.Label("Enemy Group Test");
        if (_groupOptions != null && _groupOptions.Length > 0)
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < _groupOptions.Length; i++)
            {
                if (_groupOptions[i] != null && GUILayout.Button((_groupOptionIndex == i ? "> " : "") + (i + 1)))
                {
                    _groupOptionIndex = i;
                    _group = _groupOptions[i];
                }
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Group"))
        {
            TryLoadGroup(_group);
        }

        if (GUILayout.Button("Clear Group"))
        {
            ClearGroup();
        }

        GUILayout.EndHorizontal();
        GUILayout.Label(_message);
        GUILayout.EndArea();
        if (!_isShowingGroup || _camera == null)
        {
            return;
        }

        DrawSelectionPanel();
        for (int i = 0; i < _rankSlots.Length; i++)
        {
            if (_rankSlots[i] == null)
            {
                continue;
            }

            Vector3 position = _camera.WorldToScreenPoint(_rankSlots[i].position);
            if (position.z <= 0.0f)
            {
                continue;
            }

            GUI.Box(new Rect(position.x - 32.0f, Screen.height - position.y - 48.0f, 64.0f, 22.0f), "Rank " + (i + 1));
        }
    }

    private void DrawSelectionPanel()
    {
        GUILayout.BeginArea(new Rect(20.0f, 20.0f, 310.0f, 170.0f), GUI.skin.box);
        GUILayout.Label("Group Enemy / select a rank");
        GUILayout.BeginHorizontal();
        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            CEnemy enemy = _spawnedEnemies[i];
            if (enemy == null || enemy.IsDeathPresented)
            {
                continue;
            }

            int startRank = GetRankIndex(enemy.CurrentRank) + 1;
            int size = enemy.EnemyData.FormationSize;
            string label = size == 1 ? "Rank " + startRank : "Rank " + startRank + "-" + (startRank + size - 1);
            if (enemy == _selectedEnemy)
            {
                label = "> " + label;
            }

            if (GUILayout.Button(label))
            {
                TrySelectEnemy(i);
            }
        }

        GUILayout.EndHorizontal();
        bool available = _selectedEnemy != null && _selectedEnemy.isActiveAndEnabled && _selectedIntent != null;
        GUILayout.Label(available ? _selectedEnemy.name : "Select an active enemy");
        CSkillDataSO skill = available ? _selectedIntent.PlannedSkill : null;
        GUILayout.Label(skill == null ? "No planned skill" : "Skill: " + skill.SkillName);
        bool previousEnabled = GUI.enabled;
        GUI.enabled = previousEnabled && available && !_selectedIntent.IsExecuting;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Plan Skill"))
        {
            TryPlanSelectedSkill();
        }

        if (GUILayout.Button("Play Intent"))
        {
            TryPlaySelectedIntent();
        }

        if (GUILayout.Button("Cancel"))
        {
            _selectedIntent.CancelIntent();
        }

        GUILayout.EndHorizontal();
        GUI.enabled = previousEnabled;
        GUILayout.Label(available && _selectedIntent.IsExecuting ? "Playing" : "Ready");
        GUILayout.EndArea();
    }
}
