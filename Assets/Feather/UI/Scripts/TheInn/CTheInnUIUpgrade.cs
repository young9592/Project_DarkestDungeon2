
using TMPro;
using UnityEngine;


public class CTheInnUIUpgrade : MonoBehaviour
{
    #region Inspector
    [SerializeField] CTheInnUINavigationBar _navigationBar;
    [SerializeField] CTheInnUpgrade _theInnUpgrade;
    [SerializeField] GameObject _upgradeUI;

    [SerializeField] GameObject[] _heroPor = new GameObject[4];
    [SerializeField] TMP_Text _heroNameText;
    [SerializeField] string[] _heroName = new string[4];
    #endregion

    private int _currentheroPorIndex = 0;
    private string _titleText = "¼÷·Ã ±³°ü";

    private void Start()
    {
        UpdateHeroProfile();
    }

    public void UpgradeUIOpen()
    {
        if (_upgradeUI.activeSelf)
        {
            _theInnUpgrade.OnCloseUpgradeUI();
            _navigationBar.ToggleUpgradeUI(_upgradeUI, _titleText);
            return;
        }

        if (_theInnUpgrade.TryOpenTraining())
        {
            _navigationBar.ToggleUpgradeUI(_upgradeUI, _titleText);
        }
    }


    public void LiftClick()
    {
        _currentheroPorIndex--;

        if (_currentheroPorIndex < 0)
        {
            _currentheroPorIndex = _heroPor.Length - 1;
        }
        UpdateHeroProfile();
    }
    public void RightClick()
    {
        _currentheroPorIndex++;

        if (_currentheroPorIndex >= _heroPor.Length)
        {
            _currentheroPorIndex = 0;
        }

        UpdateHeroProfile();
    }
    private void UpdateHeroProfile()
    {
        for (int i = 0; i < _heroPor.Length; i++)
        {
            _heroPor[i].SetActive(i == _currentheroPorIndex);
        }
        _heroNameText.text = _heroName[_currentheroPorIndex];
        _theInnUpgrade.UpdateHeroSkill(_currentheroPorIndex);
    }
}
