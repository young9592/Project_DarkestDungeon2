using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Enemy/Enemy Group", fileName = "EnemyGroupSO_")]
public class CEnemyGroupSO : ScriptableObject
{
    [Header("Enemy Formation")]
    [Tooltip("Rank는 시작 열이고 적 데이터의 Formation Size만큼 자리를 차지합니다.")]
    [SerializeField] private List<CEnemyGroupEntry> _entries = new List<CEnemyGroupEntry>();
    [SerializeField] private string _displayName;
    [SerializeField] private bool _shuffleSingleRankEnemies;
    public string DisplayName
    {
        get { return _displayName; }
    }

    public IReadOnlyList<CEnemyGroupEntry> Entries
    {
        get { return _entries; }
    }

    public IReadOnlyList<CEnemyGroupEntry> GetSpawnEntries()
    {
        if (!_shuffleSingleRankEnemies || _entries == null) return _entries;

        List<CEnemy> prefabs = new List<CEnemy>();
        for (int i = 0; i < _entries.Count; i++)
        {
            CEnemyGroupEntry entry = _entries[i];
            if (entry == null) return _entries;
            CEnemy prefab = entry.EnemyPrefab;
            if (prefab == null || prefab.EnemyData == null || prefab.EnemyData.FormationSize != 1) return _entries;
            prefabs.Add(prefab);
        }

        List<CEnemyGroupEntry> shuffled = new List<CEnemyGroupEntry>();
        for (int i = 0; i < prefabs.Count; i++)
        {
            int selected = Random.Range(i, prefabs.Count);
            CEnemy prefab = prefabs[selected];
            prefabs[selected] = prefabs[i];
            prefabs[i] = prefab;
            shuffled.Add(new CEnemyGroupEntry(prefab, _entries[i].Rank));
        }
        return shuffled;
    }

    internal static CEnemyGroupSO CreateRuntime(List<CEnemyGroupEntry> entries)
    {
        CEnemyGroupSO group = CreateInstance<CEnemyGroupSO>();
        group.hideFlags = HideFlags.DontSave;
        group.name = "Generated Enemy Group";
        group._displayName = "Generated Enemy Group";
        group._entries.AddRange(entries);
        if (group.TryValidate(out _)) return group;
        Destroy(group);
        return null;
    }

    public bool TryValidate(out string message)
    {
        if (_entries == null || _entries.Count == 0)
        {
            message = "적을 한 마리 이상 설정해야 합니다.";
            return false;
        }

        if (_entries.Count > 4)
        {
            message = "적은 최대 네 마리까지 설정할 수 있습니다.";
            return false;
        }

        ERank occupiedRanks = ERank.None;
        for (int i = 0; i < _entries.Count; i++)
        {
            CEnemyGroupEntry entry = _entries[i];
            string prefix = (i + 1) + "번째 항목: ";
            if (entry == null || entry.EnemyPrefab == null)
            {
                message = prefix + "적 프리팹이 필요합니다.";
                return false;
            }

#if UNITY_EDITOR
            if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(entry.EnemyPrefab))
            {
                message = prefix + "씬의 적 대신 프리팹 에셋을 연결해야 합니다.";
                return false;
            }

#endif
            if (entry.EnemyPrefab.EnemyData == null)
            {
                message = prefix + "프리팹에 Enemy Data가 연결되어 있지 않습니다.";
                return false;
            }

            if (!CSkillRankUtility.IsSingleRank(entry.Rank))
            {
                message = prefix + "1열부터 4열 중 하나만 선택해야 합니다.";
                return false;
            }

            if (!entry.TryGetOccupiedRanks(out ERank entryRanks))
            {
                message = prefix + "차지하는 칸 수가 잘못됐거나 배치가 4열을 넘어갑니다.";
                return false;
            }

            if ((occupiedRanks & entryRanks) != ERank.None)
            {
                message = prefix + "다른 적과 배치 열이 겹칩니다.";
                return false;
            }

            occupiedRanks |= entryRanks;
        }

        message = string.Empty;
        return true;
    }

    private void OnValidate()
    {
        // 잘못된 조합 설정 알림
        if (!TryValidate(out string message))
        {
            CPrint.Warn(name + ": " + message);
        }
    }
}
