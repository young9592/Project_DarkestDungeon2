using UnityEngine;

public class CStagecoachPanels : MonoBehaviour
{
    #region Inspector
    [SerializeField] private GameObject _minimap;
    [SerializeField] private GameObject _stagecoach;
    [SerializeField] private GameObject _inventory;
    #endregion
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))    // 미니맵
        {
            OnClickMinimapBtn();
            Debug.Log("M 눌림");
        }
        if (Input.GetKeyDown(KeyCode.Z))    // 마차
        {
            OnClickStageCoachBtn();
            Debug.Log("Z 눌림");
        }
        if (Input.GetKeyDown(KeyCode.I))    // 인벤토리
        {
            OnClickInventoryBtn();
            Debug.Log("I 눌림");
        }
    }

    // 버튼/키(M) → 미니맵: 활성화/비활성화
    public void OnClickMinimapBtn()
    {
        if (_minimap != null)
        {
            bool isActiveM = _minimap.activeSelf;
            _minimap.SetActive(!isActiveM);
            Debug.Log("미니맵 버튼 클릭");
        }
    }

    // 버튼/키(Z) → 마차: 활성화/비활성화
    public void OnClickStageCoachBtn() 
    {
        if (_stagecoach != null)
        {
            bool isActiveZ = _stagecoach.activeSelf;
            _stagecoach.SetActive(!isActiveZ);
        }
        Debug.Log("마차 버튼 클릭");
    }

    // 버튼/키(I) → 인벤토리: 활성화/비활성화
    public void OnClickInventoryBtn()
    {
        if (_inventory != null)
        {
            bool isActiveI = _inventory.activeSelf;
            _inventory.SetActive(!isActiveI);
            Debug.Log("인벤토리 버튼 클릭");
        }
    }
}
