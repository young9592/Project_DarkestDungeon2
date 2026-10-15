using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Enemy/Enemy Group", fileName = "EnemyGroupSO_")]
public class CEnemyGroupSO : ScriptableObject
{
    [Header("Enemy Formation")]
    [Tooltip("한 칸을 차지하는 적을 최대 네 마리까지 설정합니다. 목록 순서 대신 Rank를 배치 열로 사용합니다.")]
    [SerializeField] private List<CEnemyGroupEntry> _entries = new List<CEnemyGroupEntry>();

    public IReadOnlyList<CEnemyGroupEntry> Entries
    {
        get { return _entries; }
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

            if ((occupiedRanks & entry.Rank) != ERank.None)
            {
                message = prefix + "다른 적과 배치 열이 겹칩니다.";
                return false;
            }

            occupiedRanks |= entry.Rank;
        }

        message = string.Empty;
        return true;
    }

    private void OnValidate()
    {
        // 잘못된 항목을 임의로 지우거나 열을 바꾸지 않고 설정 문제만 알려줍니다.
        if (!TryValidate(out string message)) CPrint.Warn(name + ": " + message);
    }
}
