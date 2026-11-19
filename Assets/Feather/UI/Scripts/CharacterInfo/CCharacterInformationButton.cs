using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class HeroInfo
{
    //[Header("영웅 ")]
    
}


public class CCharacterInformationButton : MonoBehaviour
{
    #region Inspector
    [Header("3D영웅 목록")]
    [SerializeField] private GameObject[] _hero3DModels;

    [Header("캐릭터 정보창 UI")]
    [SerializeField] private GameObject _characterInfoPanel;
    #endregion

    #region Field
    private int _currentHeroIndex = 0;  // 현재 선택된 영웅의 인덱스 몇 번째인지 기억
    #endregion

    private void Awake()
    {
        if (_hero3DModels == null || _hero3DModels.Length == 0)
        {
            Debug.LogError("영웅 3D 모델이 할당X");
        }
    }

    void Start()
    {
        UpdateHeroUI(_currentHeroIndex);    // 초기 UI 갱신

        //if (_characterInfoPanel != null)
        //{
        //    _characterInfoPanel.SetActive(false);  // 시작 시 캐릭터 정보창 비활성화
        //}
    }

    void Update()
    {
        InputQ();
    }

    #region Input Q Key
    public void InputQ()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_characterInfoPanel.activeSelf == true)
            {
                _characterInfoPanel.SetActive(false);
                Debug.Log("캐릭터 정보창 off");
            }
            else
            {
                _characterInfoPanel.SetActive(true);
                Debug.Log("캐릭터 정보창 on");
            }
            Debug.Log("Q 눌림");
        }
    }
    #endregion

    #region Arrow Button Click
    public void OnClickLeftArrow()          // 왼쪽 화살표 클릭
    {
        _currentHeroIndex++;

        if (_currentHeroIndex >= _hero3DModels.Length)
        {
            _currentHeroIndex = 0;
        }
        
        UpdateHeroUI(_currentHeroIndex);    // UI 왼쪽버튼 갱신
        Debug.Log($"왼쪽 화살표 클릭 [{_currentHeroIndex}]");
    }

    public void OnClickRightArrow()        // 오른쪽 화살표 클릭
    {
        _currentHeroIndex--;

        if (_currentHeroIndex < 0)
        {
            _currentHeroIndex = _hero3DModels.Length - 1;
        }

        UpdateHeroUI(_currentHeroIndex);    // UI 오른쪽버튼 갱신
        Debug.Log($"오른쪽 화살표 클릭 [{_currentHeroIndex}]");
    } 
    #endregion

    public void UpdateHeroUI(int index)     // 버튼 누를시 → 영웅 UI 갱신
    {
        if (index < 0 || index >= _hero3DModels.Length)
        {
            return;
        }

        for (int i = 0; i < _hero3DModels.Length; i++)
        {
            bool isActive = (i == index);

            _hero3DModels[i].SetActive(isActive);
        }
    }
}


