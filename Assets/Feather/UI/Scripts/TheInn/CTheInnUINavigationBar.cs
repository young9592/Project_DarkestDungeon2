
using TMPro;
using UnityEngine;

public class CTheInnUINavigationBar : MonoBehaviour
{
    #region Inspector
    [SerializeField] private CTheInnUIBiome _biome;

    [SerializeField] private TMP_Text _navigationTitleText;
    [SerializeField] private GameObject _restText;
    #endregion

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
        _biome.BiomeUIOpen();
    }

    public void ClickRestText()
    {

    }

    public void UpdateNavigationTitleText(string text)
    {
        _navigationTitleText.text = text;
    }

}
