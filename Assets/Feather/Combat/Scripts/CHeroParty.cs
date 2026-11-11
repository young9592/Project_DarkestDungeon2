using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityEngine.EventSystems.EventTrigger;

public class CHeroParty : MonoBehaviour
{
    #region Inspector
    [Header("PartyPreset")]
    [SerializeField] private GameObject _entityRoot;
    [SerializeField] private CHeroPartySO _heroPartyPreset;

    [Header("Option")]
    [SerializeField] private List<GameObject> _heroRankPosition;
    // 엔티티들의 모델링이 너무 작다보니..
    [SerializeField] private float _entityScale = 150f;
    #endregion

    #region Field
    private List<CPlayer> _heroList = new List<CPlayer>();
    private Dictionary<ERank, CPlayer> _rankList = new Dictionary<ERank, CPlayer>();
    // 영웅 생성이 완료되었는지 체크
    private static bool _isReady = false;
    // 전투 정보
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
            return;
        }

        if (_expeditionState == null)
        {
            _expeditionState = gameObject.AddComponent<CHeroExpeditionState>();
        }

        if (_isReady)
        {
            return;
        }

        // 영웅 생성 및 배치
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

        // 배틀 인풋에 정보 전달
        for (int i = 0; i < _heroList.Count; i++)
        {
            _heroList[i].RankPresentation.Configure(_heroRankPosition[0].transform.position, rankStep, _heroList);
        }

        // 준비완료
        _isReady = true;
    }
    // 영웅 제거
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
    // 엔티티의 랭크 정렬
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
                CPrint.Log($"Rank : [{(ERank)(1 << i)}] / Name : [{hero.HeroData.Name}]");
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
    // 변경된 랭크에 맞게 랭크 리스트 딕셔너리 업데이트
    public void UpdateCurrentRank()
    {
        _rankList.Clear();

        CPrint.Warn("Current Hero State");


        for (int i = 0; i < _heroList.Count; i++)
        {
            ERank rank = _heroList[i].CurrentRank;
            CPlayer hero = _heroList[i];

            if (!_rankList.ContainsKey(rank))
            {
                _rankList.Add(rank, hero);
            }
            else
            {
                // 동일한 위치의 개체가 존재
                CPrint.Error("동일한 위치의 개체가 존재합니다. 체크 필요.");

                bool forceInput = false;

                for (int j = 0; j < 4; j++)
                {
                    forceInput = _rankList.ContainsKey((ERank)(1 << j));

                    if (forceInput)
                    {
                        _rankList.Add((ERank)(1 << j), hero);
                        CPrint.Error($"강제로 {((ERank)(1 << j))} 위치에 {hero.HeroData.Name}을 추가하였습니다.");
                    }
                }

                if (!forceInput)
                {
                    CPrint.Error("강제로 위치 넣기에 실패하였습니다. 확인바람.");
                }
            }
        }




        for (int i = 0; i < 4; i++)
        {
            if (_rankList.TryGetValue((ERank)(1 << i), out CPlayer hero))
            {
                CPrint.Log($"Rank[{i}] : {hero.HeroData.Name}");
            }
        }
    }
    // 자리 이동 시 정렬
    public void RankChange(CPlayer hero, ERank rank)
    {
        // 영웅의 현재 위치
        int rankIdx = (int)System.MathF.Log((float)hero.CurrentRank, 2);
        // 이동시킬 위치
        int moveIdx = (int)System.MathF.Log((float)rank, 2);

        CPlayer[] heroList = new CPlayer[4];

        // 캐릭터의 랭크 변경만 시도해서 외부에서 정렬하도록 유도 
        for (int i = 0; i < 4; i++)
        {
            if (_rankList.TryGetValue((ERank)(1 << i), out CPlayer inputHero))
            {
                heroList[i] = inputHero;
            }
        }

        // 영웅의 현재 위치는 지워준다.
        if (heroList[rankIdx] != null)
        {
            heroList[rankIdx] = null;
        }
        else
        {
            CPrint.Error("랭크 변경 실패");
            return;
        }

        // 앞으로 이동
        if (moveIdx < rankIdx)
        {
            for (int i = rankIdx; i >= moveIdx; i--)
            {
                // 해당 위치가 비어있는지 체크
                if (heroList[i] != null)
                {
                    continue;
                }

                for (int j = i - 1; j >= moveIdx; j--)
                {
                    if (heroList[j] != null)
                    {
                        if (heroList[j].TokenManager.HasToken(EEffectType.Immobilize))
                        {
                            continue;
                        }

                        heroList[j].TrySetRank((ERank)(1 << i));
                        heroList[i] = heroList[j];
                        heroList[j] = null;
                        break;
                    }
                }
            }
        }
        // 뒤로 이동
        else
        {
            for (int i = rankIdx; i <= moveIdx; i++)
            {
                // 해당 위치가 비어있는지 체크
                if (heroList[i] != null)
                {
                    continue;
                }

                for (int j = i + 1; j <= moveIdx; j++)
                {
                    if (heroList[j] != null)
                    {
                        if (heroList[j].TokenManager.HasToken(EEffectType.Immobilize))
                        {
                            continue;
                        }

                        heroList[j].TrySetRank((ERank)(1 << i));
                        heroList[i] = heroList[j];
                        heroList[j] = null;
                        break;
                    }
                }
            }
        }

        // 자리이동 요청한 영웅 해당 랭크로 변경
        hero.TrySetRank(rank);

        // 정리는 외부에서 루틴에 의해 정렬됨

        /*
        // 앞으로 가기
        if (rankIdx > moveIdx)
        {
            for (int i = moveIdx; i < rankIdx; i++)
            {
                if (_rankList.TryGetValue((ERank)(1 << i), out CPlayer moveHero))
                {
                    moveHero.TrySetRank((ERank)(1 << (i + 1)));
                }
            }
        }
        // 뒤로가기
        else if (rankIdx < moveIdx)
        {
            for (int i = rankIdx + 1; i <= moveIdx; i++)
            {
                if (_rankList.TryGetValue((ERank)(1 << i), out CPlayer moveHero))
                {
                    moveHero.TrySetRank((ERank)(1 << (i - 1)));
                }
            }
        }
        */
    }
}
