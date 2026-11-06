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
    private static bool _isReady = false;

    private CHeroExpeditionState _expeditionState = null;
    #endregion

    #region Property
    public static bool IsReady => _isReady;
    public List<CPlayer> HeroList => _heroList;
    public IReadOnlyList<GameObject> HeroRankPosition => _heroRankPosition;
    public CHeroExpeditionState HeroExpeditionState => _expeditionState;
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

        if(_expeditionState == null)
        {
            _expeditionState = gameObject.AddComponent<CHeroExpeditionState>();
        }

        if (_isReady)
        {
            return;
        }

        for (int i = 0; i < _heroPartyPreset.Members.Count; i++)
        {
            var heroPrefab = Instantiate(_heroPartyPreset.Members[i].Prefab, _entityRoot.transform);

            Vector3 vec = _heroRankPosition[0].transform.position;

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

        _expeditionState.TryPrepareNextBattle(_heroList);

        // 위치조정
        Vector3 rankStep = new Vector3(_heroRankPosition[1].transform.position.x - _heroRankPosition[0].transform.position.x, 0, 0);
       

        for (int i = 0; i < _heroList.Count; i++)
        {
            _heroList[i].RankPresentation.Configure(_heroRankPosition[0].transform.position, rankStep, _heroList);
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
                _heroList.RemoveAt(i);
                break;
            }
        }
    }
}
