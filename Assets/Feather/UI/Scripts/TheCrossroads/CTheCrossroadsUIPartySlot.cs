using UnityEngine;

public class CTheCrossroadsUIPartySlot : MonoBehaviour
{
    #region Inspector
    [SerializeField] private int _slotIndex;
    [SerializeField] private GameObject[] _heroImage;
    [SerializeField] private GameObject _blankPortrait;
    #endregion

    #region Property
    public int SlotIndex => _slotIndex;
    #endregion

    public void SetPartyHero(int heroIndex)
    {
        for (int i = 0; i < _heroImage.Length; i++)
        {
            _heroImage[i].SetActive(i == heroIndex);
        }

        _blankPortrait.SetActive(false);
    }

    public void ClearPartyHero()
    {
        for (int i = 0; i < _heroImage.Length; i++)
        {
            _heroImage[i].SetActive(false);
        }

        _blankPortrait.SetActive(true);
    }
}