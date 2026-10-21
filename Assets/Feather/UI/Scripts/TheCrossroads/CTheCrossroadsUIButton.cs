
using UnityEngine;

public class CTheCrossroadsUIButton : MonoBehaviour
{
    #region Inspector
    [SerializeField] private CTheCrossroadsHeroSetUI _heroSetUI;
    [SerializeField] private CTheCrossroadsUIText _textUI;
    [SerializeField] private CTheCrossroadsUIPartyMenu _partyMenuUI;

    [SerializeField] private int _currentindex;
    #endregion

    public void ClickButton()
    {
        if (_heroSetUI != null)
        {
            _heroSetUI.UpdateHeroUI(_currentindex);
        }
        if (_textUI != null)
        {
            _textUI.UpdateTextUI(_currentindex);
        }
    }
    public void HeroEquipped()
    {
       if(_partyMenuUI != null)
        {
            _partyMenuUI.UpdateHeroEquippedUI(_currentindex);
        }
    }

}
