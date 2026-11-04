
using UnityEngine;


public class CTheInnUIUpgrade : MonoBehaviour
{
    #region Inspector
    [SerializeField] CTheInnUINavigationBar _navigationBar;
    [SerializeField] GameObject _upgradeUI;

    [SerializeField] GameObject[] _heroPor = new GameObject[4];
    #endregion

    private int _currentheroPorIndex = 0;
    private string _titleText = "Mastery Trainer";

    public void UpgradeUIOpen()
    {
        _navigationBar.ToggleUI(_upgradeUI, _titleText);
    }

    public void LiftClick()
    {
        _currentheroPorIndex--;

        if(_currentheroPorIndex < 0)
        {
            _currentheroPorIndex = _heroPor.Length -1;
        }
        UpdateHeroProfile();
    }
    public void RightClick()
    {
        _currentheroPorIndex++;

        if(_currentheroPorIndex >= _heroPor.Length)
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
    }
}
