
using System.Collections.Generic;
using Unity.VisualScripting;
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
    private List<CEntity> _enemyList = new List<CEntity>();
    private static bool _isReady = false;
    #endregion

    #region Property
    public static bool IsReady => _isReady;
    public List<CEntity> EnemyList => _enemyList;
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

            switch (_enemyPartyPreset.Entries[i].Rank)
            {
                case ERank.None:
                    break;
                case ERank.Rank1:
                    if (_enemyPartyPreset.Entries[i].EnemyPrefab.EnemyData.FormationSize == 1)
                    {
                        vec = _enemyRankPosition[0].transform.position;
                    }
                    else
                    {

                    }
                    break;
                case ERank.Rank2:
                    vec = _enemyRankPosition[1].transform.position;
                    break;
                case ERank.Rank3:
                    vec = _enemyRankPosition[2].transform.position;
                    break;
                case ERank.Rank4:
                    vec = _enemyRankPosition[3].transform.position;
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
}
