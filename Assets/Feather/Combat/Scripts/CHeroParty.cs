using System.Collections;
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
    private bool _isReady = false;
    #endregion

    #region Property
    public bool IsReady => _isReady;
    #endregion

    private void Start()
    {
        for (int i = 0; i < _heroPartyPreset.Members.Count; i++)
        {
            var heroPrefab = Instantiate(_heroPartyPreset.Members[i].Prefab, _entityRoot.transform);
            
            // 위치 조정
            Vector3 vec = Vector3.zero;

            switch (_heroPartyPreset.Members[i].Rank)
            {
                case ERank.None:
                    break;
                case ERank.Rank1:
                    vec = _heroRankPosition[0].transform.position;
                    break;
                case ERank.Rank2:
                    vec = _heroRankPosition[1].transform.position;
                    break;
                case ERank.Rank3:
                    vec = _heroRankPosition[2].transform.position;
                    break;
                case ERank.Rank4:
                    vec = _heroRankPosition[3].transform.position;
                    break;
                default:
                    break;
            }

            heroPrefab.transform.position = vec;

            // 프리팹 Active
            heroPrefab.gameObject.SetActive(true);
            heroPrefab.gameObject.transform.GetChild(0).gameObject.SetActive(true);

            // Scale 조정
            heroPrefab.gameObject.transform.localScale = Vector3.one * _entityScale;

            // Rotaion 조정
            heroPrefab.gameObject.transform.rotation = Quaternion.Euler(0, 180f, 0);
        }

        // 준비완료
        _isReady = true;
    }
}
