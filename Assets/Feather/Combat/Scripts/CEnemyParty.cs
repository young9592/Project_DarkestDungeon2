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
    private Dictionary<ERank, CEnemy> _rankList = new Dictionary<ERank, CEnemy>();
    [SerializeField] private float _entityScale = 150f;
    #endregion

    #region Field
    private List<CEnemy> _enemyList = new List<CEnemy>();

    private static bool _isReady = false;

    #endregion

    #region Property
    public static bool IsReady => _isReady;
    public List<CEnemy> EnemyList => _enemyList;
    public Dictionary<ERank, CEnemy> RankList => _rankList;
    public IReadOnlyList<GameObject> EnemyRankPosition => _enemyRankPosition;
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
            Vector3 vec = _enemyRankPosition[0].transform.position;
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

            // 구울같은 개체는 등을 보여줬어..
            if (enemyPrefab.EntityData.Id == "enemy_ghoul")
            {
                enemyPrefab.gameObject.transform.localScale = new Vector3(enemyPrefab.gameObject.transform.localScale.x, enemyPrefab.gameObject.transform.localScale.y, -enemyPrefab.gameObject.transform.localScale.z);
            }

            // 스크립트 잠시 중단
            enemyPrefab.GetComponent<CEntityHealthView>().enabled = false;

            CEnemy enemy = enemyPrefab.GetComponent<CEnemy>();

            // 리스트 추가
            _enemyList.Add(enemy);

            _rankList.Add(enemy.CurrentRank, enemy);
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
                _rankList.Remove(enemy.CurrentRank);
                _enemyList.RemoveAt(i);
                break;
            }
        }
    }

    public void SortRank()
    {
        // 이거만 수정하면 됨
        for (int i = 0; i < 4; i++)
        {
            if (_rankList.ContainsKey((ERank)(1 << i)))
            {
                _rankList.TryGetValue((ERank)(1 << i), out CEnemy enemy);

                int size = enemy.EnemyData.FormationSize;

                if (size > 1)
                {
                    i = i + size - 1;
                }

                continue;
            }

            for (int j = i + 1; j < 4; j++)
            {
                if (_rankList.TryGetValue((ERank)(1 << j), out CEnemy enemy))
                {
                    if (enemy.TokenManager.HasToken(EEffectType.Immobilize))
                    {
                        continue;
                    }

                    enemy.SetRankImmediate((ERank)(1 << i));
                    _rankList.Remove((ERank)(1 << j));
                    _rankList.Add((ERank)(1 << i), enemy);

                    int size = enemy.EnemyData.FormationSize;

                    i += size - 1;
                    break;
                }
            }
        }

        // Debug
        CPrint.Warn("Enemy Rank Sorting Complited.");

        for (int i = 0; i < 4; i++)
        {
            if (_rankList.TryGetValue((ERank)(1 << i), out CEnemy enemy))
            {
                CPrint.Log($"Rank : [{(ERank)(1 << i)}] / Enemy : [{enemy.EnemyData.Name}]");
            }
        }

        CPrint.Warn("===========================");
    }

    // 현재 제대로 정렬되어 있는가?
    public bool CheckRankValid()
    {
        ERank checkRank = ERank.None;

        for (int i = 0; i < 4; i++)
        {
            bool hasRank = _rankList.TryGetValue((ERank)(1 << i), out CEnemy enemy);

            if (hasRank)
            {
                int size = enemy.EnemyData.FormationSize;

                // 몸뚱이가 이렇게 큰데 왜 여기에 있어요?
                if (i + size > 4)
                {
                    return false;
                }

                for (int j = i; j < i + size; j++)
                {
                    checkRank |= (ERank)(1 << j);
                }

                if (i == 0)
                {
                    i += size - 1;
                    continue;
                }
                else
                {
                    // 고정토큰은 인정입니다.
                    bool hasImmobilize = enemy.TokenManager.HasToken(EEffectType.Immobilize);

                    if (hasImmobilize)
                    {
                        i += size - 1;
                        continue;
                    }
                    else
                    {
                        // 앞 랭크가 비었는데 왜 뒤에있지?
                        for (int j = 0; j < i; j++)
                        {
                            if ((checkRank & (ERank)(1 << j)) == 0)
                            {
                                return false;
                            }
                        }

                    }
                }
            }
        }
        return true;
    }
}
