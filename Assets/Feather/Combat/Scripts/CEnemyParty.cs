using System.Collections.Generic;
using UnityEngine;

public class CEnemyParty : MonoBehaviour
{
    #region Inspector
    [Header("PartyPerset")]
    [SerializeField] private GameObject _entityRoot;
    [SerializeField] private CEnemyGroupSO _enemyPartyPreset;

    [Header("Option")]
    [SerializeField] private List<GameObject> _enemyRankPosition;
    [SerializeField] private float _entityScale = 150f;
    #endregion

    #region Field
    private List<CEnemy> _enemyList = new List<CEnemy>();
    private Dictionary<ERank, CEnemy> _rank = new Dictionary<ERank, CEnemy>();
    private List<ERank> _checkRank = new List<ERank>() { ERank.Rank1, ERank.Rank2, ERank.Rank3, ERank.Rank4 };

    private static bool _isReady = false;
    private static bool _isDirty = false;
    #endregion

    #region Property
    public static bool IsReady => _isReady;
    public static bool IsDirty => _isDirty;
    public List<CEnemy> EnemyList => _enemyList;
    #endregion
    void Start()
    {
        if (_enemyPartyPreset == null)
        {
            CPrint.Warn($"{gameObject.name} Preset is null.");
            return;
        }

        if (_enemyRankPosition == null)
        {
            CPrint.Warn($"{gameObject.name} Rank Position is null.");
        }

        if (_isReady)
        {
            return;
        }

        for (int i = 0; i < _enemyPartyPreset.Entries.Count; i++)
        {
            var enemyPrefab = Instantiate(_enemyPartyPreset.Entries[i].EnemyPrefab, _entityRoot.transform);

            // 위치 조정
            Vector3 vec = Vector3.zero;
            int size = _enemyPartyPreset.Entries[i].EnemyPrefab.EnemyData.FormationSize;


            switch (_enemyPartyPreset.Entries[i].Rank)
            {
                case ERank.None:
                    break;
                case ERank.Rank1:
                    vec = SizeOffsetPosition(0, size);
                    break;
                case ERank.Rank2:
                    vec = SizeOffsetPosition(1, size);
                    break;
                case ERank.Rank3:
                    vec = SizeOffsetPosition(2, size);
                    break;
                case ERank.Rank4:
                    vec = SizeOffsetPosition(3, size);
                    break;
                default:
                    break;
            }
            enemyPrefab.GetComponent<CEnemy>().TrySetRank(_enemyPartyPreset.Entries[i].Rank);

            enemyPrefab.transform.position = vec;

            // 프리팹 Active
            enemyPrefab.gameObject.SetActive(true);

            // Scale 조정
            enemyPrefab.gameObject.transform.localScale = Vector3.one * _entityScale;

            // Rotaion 조정
            enemyPrefab.gameObject.transform.rotation = Quaternion.Euler(0, 180f, 0);

            // 스크립트 잠시 중단
            enemyPrefab.GetComponent<CEntityHealthView>().enabled = false;

            // 리스트 추가
            _enemyList.Add(enemyPrefab.GetComponent<CEnemy>());
        }

        // 준비 완료
        _isReady = true;
    }

    private Vector3 SizeOffsetPosition(int rank, int size)
    {
        Vector3 vec = Vector3.zero;

        if (rank + size > 4)
        {
            CPrint.Warn("배치 오류 SO 확인필요");
            return Vector3.zero;
        }

        if (size <= 0)
        {
            CPrint.Warn("사이즈 설정 잘못됨");
            return Vector3.zero;
        }

        if (size == 1)
        {
            vec = _enemyRankPosition[rank].transform.position;
        }
        else
        {
            for (int j = rank; j < rank + size; j++)
            {
                vec += _enemyRankPosition[j].transform.position;
            }
            vec /= size;
        }

        return vec;
    }

    public void RemoveEnemy(CEnemy enemy)
    {
        for (int i = _enemyList.Count - 1; i >= 0; i--)
        {
            if (_enemyList[i] == enemy)
            {
                _enemyList.RemoveAt(i);
                _isDirty = true;
                break;
            }
        }
    }

    public void SortRank()
    {
        if (!_isDirty)
        {
            return;
        }

        for (int i = 0; i < _checkRank.Count; i++)
        {
            if (_rank.ContainsKey(_checkRank[i]))
            {
                continue;
            }

            for (int j = i + 1; j < _checkRank.Count; j++)
            {
                if (_rank.TryGetValue(_checkRank[j], out CEnemy enemy))
                {
                    bool _canRankChange = enemy.TrySetRank(_checkRank[i]);

                    if (_canRankChange)
                    {
                        _rank.Add(_checkRank[i], enemy);
                        _rank.Remove(_checkRank[j]);
                    }
                }
            }
        }
        _isDirty = false;
    }
}
