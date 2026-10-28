
using TMPro;
using UnityEngine;

public class CTheInnUINavigationBar : MonoBehaviour
{
    #region Inspector
    [SerializeField] private CTheInnUIBiome _biome;

    [SerializeField] private TMP_Text _navigationTitleText;
    [SerializeField] private GameObject _restText;
    #endregion

    private string _originalTitleText = "Torch and Crown";
    private bool _isBiomeSelected = false;
    private bool _isUIOpen;

    public bool IsBiomeSelected => _isBiomeSelected;
    public bool ISUIOpen => _isUIOpen;

    private void Update()
    {
        _restText.SetActive(_isBiomeSelected && !_isUIOpen);
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
        ResetBiomeSelection();
        Debug.Log("여정 시작");
    }

    public void SelectBiome()
    {
        _isBiomeSelected = true;
    }

    public void ResetBiomeSelection()
    {
        _isBiomeSelected = false;
    }

    public void SetUIOpen(bool isOpen)
    {
        _isUIOpen = isOpen;
    }

    public void ToggleUI(GameObject targetUI, string titleText)
    {
        targetUI.SetActive(!targetUI.activeSelf);

        if (targetUI.activeSelf)
        {
            SetUIOpen(true);
            UpdateNavigationTitleText(titleText);
        }
        else
        {
            SetUIOpen(false);
            RestoreNavigationTitleText();
        }
    }
    public void UpdateNavigationTitleText(string text)
    {
        _navigationTitleText.text = text;
    }
    public void RestoreNavigationTitleText()
    {
        UpdateNavigationTitleText(_originalTitleText);
    }
}
