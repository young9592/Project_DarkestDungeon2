using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CValleyUIBtn : MonoBehaviour
{
    #region Inspector
    [Header("UI버튼 키")]
    [SerializeField] private GameObject _minimapM;
    [SerializeField] private GameObject _inventoryI;
    [SerializeField] private GameObject _StagecoachZ;
    #endregion

    void Start()
    {
        CloseAllUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            MinimapBtnKeyM();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            InventoryBtnKeyI();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            StageCoachBtnKeyZ();
        }
    }

    public void MinimapBtnKeyM()            // 미니맵 버튼 클릭 시
    {
        bool isActiveM = _minimapM.activeSelf;
        _minimapM.SetActive(!isActiveM);
        Debug.Log($"미니맵 버튼 클릭{!isActiveM}");
    }

    public void InventoryBtnKeyI()          // 인벤토리 버튼 클릭 시
    {
        bool isActiveI = _inventoryI.activeSelf;
        _inventoryI.SetActive(!isActiveI);
        Debug.Log($"인벤토리 버튼 클릭{!isActiveI}");
    }

    public void StageCoachBtnKeyZ()         // 마차 버튼 클릭 시
    {
        bool isActiveZ = _StagecoachZ.activeSelf;
        _StagecoachZ.SetActive(!isActiveZ);
        Debug.Log($"마차 버튼 클릭{!isActiveZ}");
    }

    public void CloseAllUI()                // 모든 UI 닫기
    {
        _minimapM.SetActive(false);
        _inventoryI.SetActive(false);
        _StagecoachZ.SetActive(false);
        Debug.Log("모든 UI 닫기");
    }
}
