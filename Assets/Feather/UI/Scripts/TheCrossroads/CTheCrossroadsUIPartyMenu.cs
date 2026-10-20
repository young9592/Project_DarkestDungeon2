
using UnityEngine;

//HeroPlacementUI °ü¸®
public class CTheCrossroadsUIPartyMenu : MonoBehaviour
{
    #region Inspector
    [SerializeField] private GameObject[] _charactericon;
    [SerializeField] private GameObject[] _blankPortrait;
    [SerializeField] private GameObject[] _mountingSlot;
    #endregion
    public void UpdateHeroEquippedUI(int index)
    {
        for(int i = 0; i < _charactericon.Length; i++)
        {
            _charactericon[i].SetActive(i == index);
        }
    }


}
