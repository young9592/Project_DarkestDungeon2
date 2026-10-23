
using UnityEngine;

public class CTheCrossroadsUIButton : MonoBehaviour
{
    #region Inspector
    [SerializeField] private CTheCrossroadsUIHeroRoster _heroRosterUI;
    [SerializeField] private CTheCrossroadsUIHeroDescription _heroDescriptionUI;
    [SerializeField] private CTheCrossroadsUIHeroPlacement _heroPlacementUI;
    [SerializeField] private CTheCrossroadsUICustomName _customNameUI;

    [SerializeField] private int _currentindex;
    #endregion

    private void Update()
    {
        HeroStateUIOpen();
    }
    public void ClickButton()
    {
        if (_heroRosterUI != null)
        {
            _heroRosterUI.UpdateHeroUI(_currentindex);
        }
        if (_heroDescriptionUI != null)
        {
            _heroDescriptionUI.UpdateTextUI(_currentindex);
        }

        if (_customNameUI != null)
        {
            _customNameUI.SelectHero(_currentindex);
        }

    }
    public void HeroEquipped()
    {
        if (_heroPlacementUI != null)
        {
            _heroPlacementUI.SetHeroSlot(_currentindex);
        }
    }

    public void HeroStateUIOpen()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("영웅 상태창 열림");
        }
    }

}
