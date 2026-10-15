using System.Collections.Generic;
using UnityEngine;

public class CEnemyGroupTest : MonoBehaviour
{
    [Header("Temp_Lee Group Test")]
    [SerializeField] private CEnemyGroupSO _group;
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
    private string _message = "Load a group to preview its ranks";

    public IReadOnlyList<CEnemy> SpawnedEnemies
    {
        get { return _spawnedEnemies; }
    }

    private void Awake()
    {
        if (_camera == null) _camera = Camera.main;
    }

    private void OnDisable()
    {
        ClearGroup();
    }

    public bool TryLoadGroup(CEnemyGroupSO group)
    {
        if (!isActiveAndEnabled) return false;
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

        // 설정 검사가 끝난 뒤 기존 테스트를 숨기고 이번에 만든 적만 교체합니다.
        HideExistingTests();
        DestroySpawnedEnemies();
        for (int i = 0; i < group.Entries.Count; i++)
        {
            CEnemyGroupEntry entry = group.Entries[i];
            Transform slot = _rankSlots[GetRankIndex(entry.Rank)];
            CEnemy enemy = Instantiate(entry.EnemyPrefab, slot.position, slot.rotation, _spawnRoot);
            enemy.name = entry.EnemyPrefab.name + "_" + entry.Rank;
            enemy.TrySetRank(entry.Rank);
            _spawnedEnemies.Add(enemy);
        }
        _message = group.name + " / " + _spawnedEnemies.Count + " enemies";
        return true;
    }

    public void ClearGroup()
    {
        DestroySpawnedEnemies();
        if (!_isShowingGroup) return;
        if (_existingEnemies != null)
        {
            for (int i = 0; i < _existingEnemies.Length; i++)
                if (_existingEnemies[i] != null) _existingEnemies[i].SetActive(_enemyActiveStates[i]);
        }
        if (_existingPanels != null)
        {
            for (int i = 0; i < _existingPanels.Length; i++)
                if (_existingPanels[i] != null) _existingPanels[i].enabled = _panelEnabledStates[i];
        }
        _isShowingGroup = false;
        _message = "Group cleared / existing tests restored";
    }

    private void HideExistingTests()
    {
        if (_isShowingGroup) return;
        if (_existingPanels != null)
        {
            _panelEnabledStates = new bool[_existingPanels.Length];
            for (int i = 0; i < _existingPanels.Length; i++)
            {
                if (_existingPanels[i] == null) continue;
                _panelEnabledStates[i] = _existingPanels[i].enabled;
                _existingPanels[i].enabled = false;
            }
        }
        if (_existingEnemies != null)
        {
            _enemyActiveStates = new bool[_existingEnemies.Length];
            for (int i = 0; i < _existingEnemies.Length; i++)
            {
                if (_existingEnemies[i] == null) continue;
                _enemyActiveStates[i] = _existingEnemies[i].activeSelf;
                _existingEnemies[i].SetActive(false);
            }
        }
        _isShowingGroup = true;
    }

    private void DestroySpawnedEnemies()
    {
        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            if (_spawnedEnemies[i] == null) continue;
            // Destroy는 프레임 끝에 처리되어서 먼저 비활성화합니다.
            _spawnedEnemies[i].gameObject.SetActive(false);
            Destroy(_spawnedEnemies[i].gameObject);
        }
        _spawnedEnemies.Clear();
    }

    private int GetRankIndex(ERank rank)
    {
        switch (rank)
        {
            case ERank.Rank2: return 1;
            case ERank.Rank3: return 2;
            case ERank.Rank4: return 3;
            default: return 0;
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20.0f, Screen.height - 95.0f, 320.0f, 85.0f), GUI.skin.box);
        GUILayout.Label("Enemy Group Test");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Group")) TryLoadGroup(_group);
        if (GUILayout.Button("Clear Group")) ClearGroup();
        GUILayout.EndHorizontal();
        GUILayout.Label(_message);
        GUILayout.EndArea();

        if (!_isShowingGroup || _camera == null) return;
        for (int i = 0; i < _rankSlots.Length; i++)
        {
            if (_rankSlots[i] == null) continue;
            Vector3 position = _camera.WorldToScreenPoint(_rankSlots[i].position);
            if (position.z <= 0.0f) continue;
            GUI.Box(new Rect(position.x - 32.0f, Screen.height - position.y - 24.0f, 64.0f, 22.0f), "Rank " + (i + 1));
        }
    }
}
