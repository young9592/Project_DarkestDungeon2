using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CInnPanelUiButton : MonoBehaviour
{
    #region Inspector
    [SerializeField] private GameObject _innButton;
    [SerializeField] private GameObject _innTitleButton;
    [SerializeField] private GameObject _inventoryButton;
    [SerializeField] private GameObject _minimapButton;
    [SerializeField] private GameObject _GoalsButton;
    #endregion
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnClickInn()
    {
        if (_innTitleButton != null)
        {

            Debug.Log("마차아이콘버튼 눌림 / 마차 인벤 열림");
        }
        
    }
    public void OnClickInnInventoryTitle()
    {
        
        Debug.Log("마차인벤 제목 수정버튼 눌림");
    }
    public void OnClickInventory()
    {
        Debug.Log("인벤토리 버튼 눌림");
    }
    public void OnClickMinimap()
    {
        Debug.Log("미니맵버튼 눌림");
    }
    public void OnClickGoals()
    {
        Debug.Log("영웅 목표버튼 눌림");
    }
    public void FirstSetupUI()
    {
        _innTitleButton.SetActive(false);
    }
    
}
