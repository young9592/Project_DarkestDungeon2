using System.Collections.Generic;
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
    private Dictionary<ERank, CPlayer> _rankList = new Dictionary<ERank, CPlayer>();
    private static bool _isReady = false;

    private CHeroExpeditionState _expeditionState = null;
    private string _runID;
    private string _battleID;
    #endregion

    #region Property
    public static bool IsReady => _isReady;
    public List<CPlayer> HeroList => _heroList;
    public IReadOnlyDictionary<ERank, CPlayer> RankList => _rankList;
    public IReadOnlyList<GameObject> HeroRankPosition => _heroRankPosition;
    public CHeroExpeditionState HeroExpeditionState => _expeditionState;
    public string RunID => _runID;
    public string BattleID => _battleID;
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

        if (_expeditionState == null)
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

            CPlayer hero = heroPrefab.GetComponent<CPlayer>();

            // 리스트 추가
            _heroList.Add(hero);

            _rankList.Add(hero.CurrentRank, hero);
        }

        _expeditionState.TryPrepareNextBattle(_heroList);
        _runID = _expeditionState.RunId;
        _battleID = _expeditionState.BattleId;

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

                _expeditionState.TryRecordHeroDeath(_runID, _battleID, hero);
                _rankList.Remove(hero.CurrentRank);
                _heroList.RemoveAt(i);
                break;
            }
        }
    }

    public void SortRank()
    {
        for (int i = 0; i < 4; i++)
        {
            if (_rankList.ContainsKey((ERank)(1 << i)))
            {
                continue;
            }

            for (int j = i + 1; j < 4; j++)
            {
                if (_rankList.TryGetValue((ERank)(1 << j), out CPlayer hero))
                {
                    if (hero.TokenManager.HasToken(EEffectType.Immobilize))
                    {
                        continue;
                    }

                    bool canRankChange = hero.TrySetRank((ERank)(1 << i));
                    CPrint.Warn($"Rank Change Try {(ERank)(1 << j)} → {(ERank)(1 << i)} [{canRankChange}] : {hero.CurrentRank}");

                    if (canRankChange)
                    {
                        _rankList.Add((ERank)(1 << i), hero);
                        _rankList.Remove((ERank)(1 << j));
                        break;
                    }
                }
            }
        }

        // Debug
        CPrint.Warn("Hero Rank Sorting Complited.");

        for (int i = 0; i < 4; i++)
        {
            if (_rankList.TryGetValue((ERank)(1 << i), out CPlayer hero))
            {
                CPrint.Log($"Rank : [{(ERank)(1 << i)}] / Enemy : [{hero.HeroData.Name}]");
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
            bool hasRank = _rankList.TryGetValue((ERank)(1 << i), out CPlayer hero);

            if (hasRank)
            {
                checkRank |= (ERank)(1 << i);

                if (i == 0)
                {
                    continue;
                }
                else
                {
                    // 고정토큰은 인정입니다.
                    bool hasImmobilize = hero.TokenManager.HasToken(EEffectType.Immobilize);

                    if (hasImmobilize)
                    {
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
    public void UpdateCurrentRank()
    {
        _rankList.Clear();

        CPrint.Warn("Current Hero State");

        for (int i = 0; i < _heroList.Count; i++)
        {
            ERank rank = _heroList[i].CurrentRank;
            CPlayer hero = _heroList[i];

            _rankList.Add(rank, hero);
            CPrint.Log($"[{rank.ToString()}] : {hero.HeroData.Name}");
        }
    }
}
