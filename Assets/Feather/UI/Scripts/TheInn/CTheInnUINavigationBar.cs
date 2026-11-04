
using TMPro;
using UnityEngine;

public class CTheInnUINavigationBar : MonoBehaviour
{
    public enum EInnUI
    {
        None,
        Biome,
        Coach,
        Upgrade,
        Store,
        Candle,
        Journal
    }

    #region Inspector
    [SerializeField] private TMP_Text _navigationTitleText;

    [SerializeField] private GameObject _charactorStateUI;
    [SerializeField] private GameObject _selectRestText;
    [SerializeField] private GameObject _restText;
    #endregion

    private GameObject _currentOpenObject;

    private EInnUI _currentOpenUI = EInnUI.None;

    private string _originalTitleText = "Torch and Crown";

    private bool _isBiomeSelected = false;
    private bool _isUIOpen;

    private void Update()
    {
        _restText.SetActive(!_isBiomeSelected && !_isUIOpen);
        _selectRestText.SetActive(_isBiomeSelected && !_isUIOpen);
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
        if (_currentOpenObject == targetUI)
        {
            _currentOpenObject.SetActive(false);
            _currentOpenObject = null;
            SetUIOpen(false);
            _charactorStateUI.SetActive(true);
            RestoreNavigationTitleText();
            return;
        }

        if (_currentOpenObject != null)
        {
            _currentOpenObject.SetActive(false);
        }

        targetUI.SetActive(true);
        _currentOpenObject = targetUI;

        SetUIOpen(true);
        _charactorStateUI.SetActive(false);
        UpdateNavigationTitleText(titleText);

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
