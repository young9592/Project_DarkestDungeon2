
using TMPro;
using UnityEngine;

public class CTheInnNavigationBarUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _navigationTitleText;
    [SerializeField] private GameObject _restText;


    private bool _isBiomeSelected = false;

    public bool IsBiomeSelected => _isBiomeSelected;

    private void Update()
    {
        _restText.SetActive(_isBiomeSelected);
    }
    public void OpenJournalIcon()
    {

    }

    public void OpenCandleIcon()
    {

    }

    public void OpenStoreIcon()
    {

    }

    public void OpenUpgradeIcon()
    {

    }

    public void OpenCoachIcon()
    {

    }

    public void OpenBiomeIcon()
    {

    }

    public void ClickRestText()
    {

    }

    public void UpdateNavigationTitleText(string text)
    {
        _navigationTitleText.text = text;
    }

}
