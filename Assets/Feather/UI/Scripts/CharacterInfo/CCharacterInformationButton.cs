using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class CCharacterInformationButton : MonoBehaviour
{
    #region Inspector
    [Header("3D영웅 목록")]
    [SerializeField] private GameObject[] _hero3DModels;

    [Header("영웅 정보창 UI")]
    [SerializeField] private GameObject _characterInfoPanel;
    [SerializeField] private GameObject[] _skillInfo;
    [SerializeField] private GameObject[] _heroInfo;
    #endregion

    #region Field
    // 현재 선택된 영웅의 인덱스 몇 번째인지 기억
    private int _currentHeroIndex = 0;  
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
        // 초기 UI 갱신
        UpdateHeroUI(_currentHeroIndex);    

        if (_characterInfoPanel != null)
        {
            _characterInfoPanel.SetActive(false);  // 시작 시 캐릭터 정보창 비활성화
        }
    }

    void Update()
    {
        InputQ();
    }
    
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
    
    // 왼쪽 화살표 클릭
    public void OnClickLeftArrow()         
    {
        _currentHeroIndex++;

        if (_currentHeroIndex >= _hero3DModels.Length)
        {
            _currentHeroIndex = 0;
        }

        // UI 왼쪽버튼 갱신
        UpdateHeroUI(_currentHeroIndex);    
        Debug.Log($"왼쪽 화살표 클릭 [{_currentHeroIndex}]");
    }

    // 오른쪽 화살표 클릭
    public void OnClickRightArrow()        
    {
        _currentHeroIndex--;

        if (_currentHeroIndex < 0)
        {
            _currentHeroIndex = _hero3DModels.Length - 1;
        }

        // UI 오른쪽버튼 갱신
        UpdateHeroUI(_currentHeroIndex);    
        Debug.Log($"오른쪽 화살표 클릭 [{_currentHeroIndex}]");
    }

    // 버튼 누를시 → 영웅 UI 갱신
    public void UpdateHeroUI(int index)     
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


