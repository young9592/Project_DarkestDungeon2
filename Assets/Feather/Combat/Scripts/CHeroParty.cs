using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CHeroParty : MonoBehaviour
{
    #region Inspector
    [Header("PartyPreset")]
    [SerializeField] private GameObject _entityRoot;
    [SerializeField] private CHeroPartySO _heroPartyPreset;

    [Header("Option")]
    [SerializeField] private List<GameObject> _heroRankPosition;
    [SerializeField] private float _entityScale = 100f;
    #endregion

    #region Field
    private List<CPlayer> _heroList = new List<CPlayer>();
    private Dictionary<ERank, CPlayer> _rank = new Dictionary<ERank, CPlayer>();
    private static bool _isReady = false;
    // 랭크 정리가 필요한 상황인가?
    private static bool _isDirty = false;
    #endregion

    #region Property
    public static bool IsReady => _isReady;
    public static bool IsDirty => _isDirty;
    public List<CPlayer> HeroList => _heroList;
    #endregion

    private void Start()
    {
        if (_heroPartyPreset == null)
        {
            CPrint.Warn($"{gameObject.name} Preset is null.");
            return;
        }

        if (_heroRankPosition == null)
        {
            CPrint.Warn($"{gameObject.name} Rank Position is null.");
        }


        if (_isReady)
        {
            return;
        }

        for (int i = 0; i < _heroPartyPreset.Members.Count; i++)
        {
            var heroPrefab = Instantiate(_heroPartyPreset.Members[i].Prefab, _entityRoot.transform);

            // 위치 조정
            Vector3 vec = Vector3.zero;

            switch (_heroPartyPreset.Members[i].Rank)
            {
                case ERank.Rank1:
                    vec = _heroRankPosition[0].transform.position;
                    _rank.Add(ERank.Rank1, heroPrefab.GetComponent<CPlayer>());
                    break;
                case ERank.Rank2:
                    vec = _heroRankPosition[1].transform.position;
                    _rank.Add(ERank.Rank2, heroPrefab.GetComponent<CPlayer>());
                    break;
                case ERank.Rank3:
                    vec = _heroRankPosition[2].transform.position;
                    _rank.Add(ERank.Rank3, heroPrefab.GetComponent<CPlayer>());
                    break;
                case ERank.Rank4:
                    vec = _heroRankPosition[3].transform.position;
                    _rank.Add(ERank.Rank4, heroPrefab.GetComponent<CPlayer>());
                    break;
            }

            heroPrefab.GetComponent<CPlayer>().TrySetRank(_heroPartyPreset.Members[i].Rank);

            heroPrefab.transform.position = vec;

            // 프리팹 Active
            heroPrefab.gameObject.SetActive(true);

            // Scale 조정
            heroPrefab.gameObject.transform.localScale = Vector3.one * _entityScale;

            // Rotaion 조정
            heroPrefab.gameObject.transform.rotation = Quaternion.Euler(0, 180f, 0);

            // 스크립트 잠시 중단
            heroPrefab.GetComponent<CEntityHealthView>().enabled = false;

            // 리스트 추가
            _heroList.Add(heroPrefab.GetComponent<CPlayer>());
        }

        // 준비완료
        _isReady = true;
    }

    public void RemoveHero(CPlayer hero)
    {
        for (int i = _heroList.Count - 1; i >= 0; i--)
        {
            if (_heroList[i] == hero)
            {
                _rank.Remove(hero.CurrentRank);
                _heroList.RemoveAt(i);
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

        for (int i = 0; i < 4; i++)
        {
            if (_rank.ContainsKey((ERank)(1 << i)))
            {
                continue;
            }

            for (int j = i + 1; j < 4; j++)
            {
                
                if (_rank.TryGetValue((ERank)(1 << j), out CPlayer hero))
                {
                    bool _canRankChange = hero.TrySetRank((ERank)(1 << i));

                    if (_canRankChange)
                    {
                        _rank.Add((ERank)(1 << i), hero);
                        _rank.Remove((ERank)(1 << j));
                    }
                    break;
                }
            }
        }
        _isDirty = false;
    }
}
